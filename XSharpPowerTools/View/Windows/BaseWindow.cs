using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows.Input;

namespace XSharpPowerTools.View.Windows
{
    public abstract class BaseWindow : DialogWindow
    {
        public XSModel XSModel { get; set; }
        public abstract string SearchTerm { set; }
        protected bool AllowReturn;

        public BaseWindow() =>
            PreviewKeyDown += BaseWindow_PreviewKeyDown;

        private void BaseWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            try
            {
                //Close();
            }
            catch (InvalidOperationException)
            { }
        }

        protected abstract void OnTextChanged();
    }
}
