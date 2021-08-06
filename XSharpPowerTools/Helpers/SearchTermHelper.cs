using System;
using System.Linq;

namespace XSharpPowerTools.Helpers
{
    public static class SearchTermHelper
    {
        public static (string, string) EvaluateSearchTerm(string searchTerm)
        {
            var methodName = string.Empty;
            var className = string.Empty;
            searchTerm = searchTerm.Trim().Replace(':', '.');
            searchTerm = searchTerm.Replace(' ', '.');
            searchTerm = searchTerm.Replace('*', '%');
            searchTerm = searchTerm.Replace('\'', '"');
            var keyWords = searchTerm.Split('.');
            if (keyWords.Length > 1)
            {
                methodName = keyWords[keyWords.Length - 1];
                className = keyWords[keyWords.Length - 2];
            }
            else if (searchTerm.StartsWith("."))
            {
                methodName = searchTerm.Substring(searchTerm.TakeWhile(q => q == '.').Count());
            }
            else
            {
                className = searchTerm;
            }

            if (className.Contains('"'))
                className = className.Replace("\"", "");
            else if (!string.IsNullOrWhiteSpace(className))
                className = "%" + className + "%";

            if (methodName.Contains('"'))
                methodName = methodName.Replace("\"", "");
            else if (!string.IsNullOrWhiteSpace(methodName))
                methodName = "%" + methodName + "%";

            return (className, methodName);
        }
    }
}
