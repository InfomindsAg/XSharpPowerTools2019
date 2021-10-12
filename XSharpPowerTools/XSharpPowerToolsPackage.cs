using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OperationProgress;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using XSharpPowerTools.Commands;
using XSharpPowerTools.Helpers;
using XSharpPowerTools.View.Windows;
using Task = System.Threading.Tasks.Task;

namespace XSharpPowerTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.XSharpPowerToolsString)]
    [ProvideBindingPath]
    [ProvideToolWindow(typeof(CodeBrowserResultsToolWindow.Pane), Style = VsDockStyle.Linked, Window = WindowGuids.ErrorList)]
    public sealed class XSharpPowerToolsPackage : ToolkitPackage
    {
        public const string XSharpProjectTypeGuid = "{aa6c8d78-22ff-423a-9c7c-5f2393824e04}";

        public static XSharpPowerToolsPackage Instance { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            using var waitCursor = new WithWaitCursor();

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await CodeBrowserCommand.InitializeAsync(this);
            await FindNamespaceCommand.InitializeAsync(this);
            this.RegisterToolWindows();
            Instance = this;

            var operationProgress = await VS.GetServiceAsync<SVsOperationProgress, IVsOperationProgressStatusService>();
            IVsOperationProgressStageStatus intellisenseStatus = operationProgress.GetStageStatus(CommonOperationProgressStageIds.Intellisense);
            await intellisenseStatus.WaitForCompletionAsync();
        }
    }
}