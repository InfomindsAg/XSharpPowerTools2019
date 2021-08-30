using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.IO;
using XSharpPowerTools.View.Windows;
using Task = System.Threading.Tasks.Task;

namespace XSharpPowerTools.Commands
{
    [Command(PackageIds.CodeBrowserCommand)]
    internal sealed class CodeBrowserCommand : BaseCommand<CodeBrowserCommand>
    {
        protected override async Task InitializeCompletedAsync()
        {
            await base.InitializeCompletedAsync();
            Command.BeforeQueryStatus += CommandBase.BeforeQueryStatus;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution != null)
            {
                var solutionDirectory = Path.GetDirectoryName(solution.FullPath);
                var window = new CodeBrowserWindow(solutionDirectory);
                await CommandBase.ShowBaseWindowAsync(window);
            }
        }
    }
}
