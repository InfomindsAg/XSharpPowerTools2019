using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XSharpPowerTools.Helpers;
using XSharpPowerTools.View.Windows;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

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

        public static void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menuCommand)
            {
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    menuCommand.Enabled = await ActiveSolutionContainsXsProjectAsync();
                });
            }
        }

        public static async Task<bool> ActiveSolutionContainsXsProjectAsync()
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution != null)
            {
                return await solution.Children.AnyAsync(async q =>
                {
                    if (q.Type == SolutionItemType.Project)
                    {
                        var project = q as Project;
                        return await project.IsKindAsync(XSharpPowerToolsPackage.XSharpProjectTypeGuid);
                    }
                    return false;
                });
            }
            return false;
        }
    }
}
