using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class Results : List<XSModelResultItem> //required for DataBinding for grouping
    { }

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
            if (selectedItem == null)
                return;
            var item = selectedItem as XSModelResultItem;
            _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
            {
                await DocumentHelper.OpenProjectItemAtAsync(item.ContainingFile, item.Line);
            });
        }

        public void UpdateToolWindowContents(XSModelResultType resultType, List<XSModelResultItem> results)
        {
            SetTableColumns(resultType);

            var _results = Resources["Results"] as Results;
            _results.Clear();
            _results.AddRange(results);

            var cvResults = CollectionViewSource.GetDefaultView(ResultsDataGrid.ItemsSource);
            if (cvResults != null && cvResults.CanGroup)
            {
                cvResults.GroupDescriptions.Clear();
                cvResults.GroupDescriptions.Add(new PropertyGroupDescription("Project"));
            }
        }

        private void SetTableColumns(XSModelResultType resultType) => 
            ResultsDataGrid.Columns.First().Visibility = resultType == XSModelResultType.Member
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
