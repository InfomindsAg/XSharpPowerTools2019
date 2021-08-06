using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.OperationProgress;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using XSharpPowerTools.Commands;
using Task = System.Threading.Tasks.Task;

namespace XSharpPowerTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.XSharpPowerToolsString)]
    [ProvideBindingPath]
    public sealed class XSharpPowerToolsPackage : ToolkitPackage
    {
        public const string UIContextGuid = "10534154-102D-46E2-ABA8-A6BFA25BA0BE"; //Solution opened and fully loaded

        public static XSharpPowerToolsPackage Instance { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await CodeBrowserCommand.InitializeAsync(this);
            await FindNamespaceCommand.InitializeAsync(this);
            Instance = this;

            var operationProgress = await VS.GetServiceAsync<SVsOperationProgress, IVsOperationProgressStatusService>();
            IVsOperationProgressStageStatus intellisenseStatus = operationProgress.GetStageStatus(CommonOperationProgressStageIds.Intellisense);
            await intellisenseStatus.WaitForCompletionAsync();

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }
    }
}