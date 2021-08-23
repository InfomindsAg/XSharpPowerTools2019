using Community.VisualStudio.Toolkit;
using System.IO;
using System.Threading.Tasks;
using XSharpPowerTools.Helpers;
using XSharpPowerTools.View.Windows;
using File = System.IO.File;

namespace XSharpPowerTools.Commands
{
    public static class CommandBase
    {
        public static async Task ShowBaseWindowAsync(BaseWindow window)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution != null)
            {
                var solutionDirectory = Path.GetDirectoryName(solution.FullPath);
                var dbFile = solutionDirectory + @"\.vs\" + Path.GetFileNameWithoutExtension(solution.FullPath) + @"\X#Model.xsdb";
                if (File.Exists(dbFile))
                {
                    window.XSModel = new XSModel(dbFile);
                    window.SearchTerm = await DocumentHelper.GetEditorSearchTermAsync();
                    try
                    {
                        window.ShowModal();
                    }
                    finally
                    {
                        window.Close();
                        window.XSModel.CloseConnection();
                        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    }
                }
                else
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                    await VS.MessageBox.ShowWarningAsync("X# Code Browser", "Waiting for solution to be fully loaded.");
                }
            }
            else
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                await VS.MessageBox.ShowWarningAsync("X# Code Browser", "X# Code Browser is only available an opened solution.");
            }
        }
    }
}
