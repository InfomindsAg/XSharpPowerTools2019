using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XSharpPowerTools.Helpers
{
    class WithWaitCursor : IDisposable
    {
        private Cursor _PreviousCursor;

        public WithWaitCursor()
        {
            _PreviousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _PreviousCursor;
        }

        #endregion IDisposable Members    }
    }
}