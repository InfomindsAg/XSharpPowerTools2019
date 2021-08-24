using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XSharpPowerTools.Helpers;

namespace XSharpPowerTools
{
    public enum XSModelResultType
    {
        Type,
        Member
    }

    public class XSModelResultItem
    {
        public string SolutionDirectory { get; set; }
        public XSModelResultType ResultType { get; set; }
        public string TypeName { get; set; }
        public string MemberName { get; set; }
        public string ContainingFile { get; set; }
        public string Project { get; set; }
        public int Line { get; set; }

        public string RelativePath =>
            string.IsNullOrWhiteSpace(SolutionDirectory) || !ContainingFile.StartsWith(SolutionDirectory) ? ContainingFile : ContainingFile.Substring(SolutionDirectory.Length + 1);
    }

    public class NamespaceResultItem
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
    }

    public class XSModel
    {
        private readonly SqliteConnection Connection;

        public XSModel(string dbFile) =>
            Connection = GetConnection(dbFile) ?? throw new ArgumentNullException();

        public async Task<(List<XSModelResultItem>, XSModelResultType)> GetSearchTermMatchesAsync(string searchTerm, string solutionDirectory, string currentFile = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return (null, 0);

            await Connection.OpenAsync();
            var command = Connection.CreateCommand();

            if (searchTerm.Trim().StartsWith("p ") || searchTerm.Trim().StartsWith("d "))
            {
                command.CommandText =
                @"
                    SELECT Name, FileName, StartLine, TypeName, ProjectFileName
                    FROM ProjectMembers 
                    WHERE";

                command.CommandText += searchTerm.Trim().StartsWith("p ")
                    ? " (Kind = 9 OR Kind = 10)"
                    : " Kind = 23";

                command.CommandText += @" AND LOWER(TRIM(Name)) LIKE $memberName ESCAPE '\' ORDER BY LENGTH(TRIM(Name)), TRIM(Name) LIMIT 100";

                searchTerm = searchTerm.Trim().Substring(2).Trim();
                searchTerm = searchTerm.Replace("_", @"\_");
                searchTerm = searchTerm.ToLower().Replace("*", "%");
                if (!searchTerm.Contains("\""))
                    searchTerm = "%" + searchTerm + "%";

                command.Parameters.AddWithValue("$memberName", searchTerm);

                var reader = await command.ExecuteReaderAsync();

                var results = new List<XSModelResultItem>();
                while (await reader.ReadAsync())
                {
                    if (!reader.GetString(4).Trim().EndsWith("(OrphanedFiles).xsproj"))
                    {
                        var resultItem = new XSModelResultItem
                        {
                            MemberName = reader.GetString(0),
                            ContainingFile = reader.GetString(1),
                            Line = reader.GetInt32(2),
                            TypeName = reader.GetString(3),
                            Project = Path.GetFileNameWithoutExtension(reader.GetString(4)),
                            ResultType = XSModelResultType.Member,
                            SolutionDirectory = solutionDirectory
                        };
                        results.Add(resultItem);
                    }
                }
                Connection.Close();
                return (results, XSModelResultType.Member);

            }
            else if (!string.IsNullOrWhiteSpace(currentFile) && (searchTerm.Trim().StartsWith("..") || searchTerm.Trim().StartsWith("::")))
            {
                var memberName = searchTerm.Trim().Substring(2).Trim();
                if (string.IsNullOrWhiteSpace(memberName))
                    return (null, 0);

                memberName = memberName.Replace("_", @"\_");
                memberName = memberName.ToLower().Replace("*", "%");
                if (!memberName.Contains("\""))
                    memberName = "%" + memberName + "%";

                command.CommandText =
                    @"
                        SELECT Name, FileName, StartLine, TypeName, ProjectFileName
                        FROM ProjectMembers
                        WHERE IdType IN (SELECT Id
                				         FROM ProjectTypes
                				         WHERE Kind = 1
                				         AND LOWER(Sourcecode) LIKE '%class%'
                                         AND LOWER(TRIM(FileName))=$fileName)
                        AND (Kind = 5 OR Kind = 6 OR Kind = 7 OR Kind = 8)
                        AND LOWER(Name) LIKE $memberName  ESCAPE '\'
                        ORDER BY LENGTH(TRIM(Name)), TRIM(Name)
                        LIMIT 100
                    ";

                command.Parameters.AddWithValue("$memberName", memberName).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$fileName", currentFile.Trim().ToLower()).SqliteType = SqliteType.Text;

                var reader = await command.ExecuteReaderAsync();

                var results = new List<XSModelResultItem>();
                while (await reader.ReadAsync())
                {
                    if (!reader.GetString(4).Trim().EndsWith("(OrphanedFiles).xsproj"))
                    {
                        var resultItem = new XSModelResultItem
                        {
                            MemberName = reader.GetString(0),
                            ContainingFile = reader.GetString(1),
                            Line = reader.GetInt32(2),
                            TypeName = reader.GetString(3),
                            Project = Path.GetFileNameWithoutExtension(reader.GetString(4)),
                            ResultType = XSModelResultType.Member,
                            SolutionDirectory = solutionDirectory
                        };
                        results.Add(resultItem);
                    }
                }
                Connection.Close();
                return (results, XSModelResultType.Member);
            }
            else
            {
                var (className, memberName) = SearchTermHelper.EvaluateSearchTerm(searchTerm);
                if (string.IsNullOrWhiteSpace(memberName))
                {
                    className = className.Replace("_", @"\_");

                    command.CommandText =
                    @"
                        SELECT Name, FileName, StartLine, ProjectFileName
                        FROM ProjectTypes 
                        WHERE Kind = 1
                        AND LOWER(Sourcecode) LIKE '%class%'
                        AND LOWER(TRIM(Name)) LIKE $className ESCAPE '\'
                        ORDER BY LENGTH(TRIM(Name)), TRIM(Name)
                        LIMIT 100
                    ";
                    command.Parameters.AddWithValue("$className", className.Trim().ToLower());

                    var reader = await command.ExecuteReaderAsync();

                    var results = new List<XSModelResultItem>();
                    while (await reader.ReadAsync())
                    {
                        if (!reader.GetString(3).Trim().EndsWith("(OrphanedFiles).xsproj"))
                        {
                            var resultItem = new XSModelResultItem
                            {
                                TypeName = reader.GetString(0),
                                MemberName = string.Empty,
                                ContainingFile = reader.GetString(1),
                                Line = reader.GetInt32(2),
                                Project = Path.GetFileNameWithoutExtension(reader.GetString(3)),
                                ResultType = XSModelResultType.Type,
                                SolutionDirectory = solutionDirectory
                            };
                            results.Add(resultItem);
                        }
                    }
                    Connection.Close();
                    return (results, XSModelResultType.Type);
                }
                else
                {
                    memberName = memberName.Replace("_", @"\_");
                    className = className.Replace("_", @"\_");

                    command.CommandText =
                    @"
                        SELECT Name, FileName, StartLine, TypeName, ProjectFileName
                        FROM ProjectMembers 
                        WHERE (Kind = 5 OR Kind = 6 OR Kind = 7 OR Kind = 8)
                        AND LOWER(TRIM(Name)) LIKE $memberName ESCAPE '\'
                    ";
                    command.Parameters.AddWithValue("$memberName", memberName.Trim().ToLower());

                    if (!string.IsNullOrWhiteSpace(className))
                    {
                        command.CommandText += @" AND LOWER(TRIM(TypeName)) LIKE $className  ESCAPE '\'";
                        command.Parameters.AddWithValue("$className", className.Trim().ToLower());
                    }
                    command.CommandText += " ORDER BY LENGTH(TRIM(Name)), TRIM(Name) LIMIT 100";

                    var reader = await command.ExecuteReaderAsync();

                    var results = new List<XSModelResultItem>();
                    while (await reader.ReadAsync())
                    {
                        if (!reader.GetString(4).Trim().EndsWith("(OrphanedFiles).xsproj"))
                        {
                            var resultItem = new XSModelResultItem
                            {
                                MemberName = reader.GetString(0),
                                ContainingFile = reader.GetString(1),
                                Line = reader.GetInt32(2),
                                TypeName = reader.GetString(3),
                                Project = Path.GetFileNameWithoutExtension(reader.GetString(4)),
                                ResultType = XSModelResultType.Member,
                                SolutionDirectory = solutionDirectory
                            };
                            results.Add(resultItem);
                        }
                    }
                    Connection.Close();
                    return (results, XSModelResultType.Member);
                }
            }
        }

        public async Task<List<NamespaceResultItem>> GetContainingNamespaceAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return null;

            await Connection.OpenAsync();
            var command = Connection.CreateCommand();
            command.CommandText =
                    @"
                        SELECT DISTINCT Name, Namespace
                        FROM AssemblyTypes 
                        WHERE Namespace IS NOT NULL
                        AND trim(Namespace) != ''
                        AND LOWER(TRIM(Name)) LIKE $className ESCAPE '\'
                        ORDER BY LENGTH(TRIM(Name)), TRIM(Name)
                        LIMIT 100
                    ";
            searchTerm = searchTerm.Replace("_", @"\_");
            command.Parameters.AddWithValue("$className", "%" + searchTerm.Trim().ToLower() + "%");

            var reader = await command.ExecuteReaderAsync();
            var results = new List<NamespaceResultItem>();
            while (await reader.ReadAsync())
            {
                var result = new NamespaceResultItem
                {
                    ClassName = reader.GetString(0),
                    Namespace = reader.GetString(1)
                };
                results.Add(result);
            }
            Connection.Close();
            return results;
        }

        public async Task<bool> FileContainsUsingAsync(string file, string usingToInsert) 
        {
            if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(usingToInsert))
                return false;

            await Connection.OpenAsync();
            var command = Connection.CreateCommand();
            command.CommandText =
                @"
                    SELECT Usings
					FROM Files
					WHERE TRIM(LOWER(FileName)) = $fileName
                ";
            command.Parameters.AddWithValue("$fileName", file.Trim().ToLower());

            var result = await command.ExecuteScalarAsync() as string;
            var usings = result.Split(new[] { '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return usings.Contains(usingToInsert);
        }

        public void CloseConnection()
        {
            if (Connection?.State != System.Data.ConnectionState.Closed)
                Connection?.Close();
        }

        public static SqliteConnection GetConnection(string dbFile)
        {
            var connectionSB = new SqliteConnectionStringBuilder
            {
                DataSource = dbFile,
                Mode = SqliteOpenMode.ReadOnly
            };
            var connection = new SqliteConnection(connectionSB.ConnectionString);
            return connection;
        }
    }
}
