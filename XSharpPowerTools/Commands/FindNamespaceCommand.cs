using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using XSharpPowerTools.View.Windows;
using Task = System.Threading.Tasks.Task;

namespace XSharpPowerTools.Commands
{
    [Command("24100142-dc8c-4a86-a29b-99bbaf6bab3c", 0x0110)]
    internal sealed class FindNamespaceCommand : BaseCommand<FindNamespaceCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var window = new FindNamespaceWindow();
            await CommandBase.ShowBaseWindowAsync(window);
        }
    }
}
