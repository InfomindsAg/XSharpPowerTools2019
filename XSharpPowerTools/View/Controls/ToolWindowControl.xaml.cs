using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XSharpPowerTools.Helpers;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for ToolWindowControl.xaml
    /// </summary>
    public partial class ToolWindowControl : UserControl, IResultsDataGridParent
    {
        public ToolWindowControl()
        {
            InitializeComponent();
            ResultsDataGrid.Parent = this;
        }

        public void OnReturn(object selectedItem) 
        {
            var item = selectedItem as XSModelResultItem;
            _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
            {
                await DocumentHelper.OpenProjectItemAtAsync(item.ContainingFile, item.Line);
            });
        }

        public void UpdateToolWindowContents(XSModelResultType resultType, IEnumerable<XSModelResultItem> results) 
        {
            SetTableColumns(resultType);
            ResultsDataGrid.ItemsSource = results;
        }

        private void SetTableColumns(XSModelResultType resultType) => 
            ResultsDataGrid.Columns.First().Visibility = resultType == XSModelResultType.Member
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
