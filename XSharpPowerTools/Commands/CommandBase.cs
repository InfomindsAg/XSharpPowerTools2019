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

        private static async Task<bool> ActiveSolutionContainsXsProjectAsync()
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            return solution != null && await ChildrenContainXsProjectAsync(solution.Children);
        }

        private static async Task<bool> ChildrenContainXsProjectAsync(IEnumerable<SolutionItem?> children)
        {
            foreach(var child in children) 
            {
                if (child.Type == SolutionItemType.Project)
                {
                    var project = child as Project;
                    if (await project.IsKindAsync(XSharpPowerToolsPackage.XSharpProjectTypeGuid))
                        return true;
                }
                else if (child.Type == SolutionItemType.SolutionFolder || child.Type == SolutionItemType.PhysicalFolder)
                {
                    if (await ChildrenContainXsProjectAsync(child.Children))
                        return true;
                }
            }
            return false;
        } 
    }
}
