using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using XSharpPowerTools.View.Controls;

namespace XSharpPowerTools.View.Windows
{
    public class CodeBrowserResultsToolWindow : BaseToolWindow<CodeBrowserResultsToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "X# Code Browser Results";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken) 
        {
            var toolWindowControl = new ToolWindowControl();
            VS.Events.SolutionEvents.OnBeforeCloseSolution += toolWindowControl.SolutionEvents_OnBeforeCloseSolution;
            return System.Threading.Tasks.Task.FromResult<FrameworkElement>(toolWindowControl);
        }

        // Give this a new unique guid
        [Guid("d3b3ebd9-87d1-41cd-bf84-268d88953417")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                // Set an image icon for the tool window
                BitmapImageMoniker = KnownMonikers.StatusInformation;
            }
        }
    }
}