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

        private void SetTableColumns(XSModelResultType resultType)
        {
            ResultsDataGrid.Columns[0].Visibility = resultType == XSModelResultType.Type
                ? Visibility.Collapsed
                : Visibility.Visible;

            ResultsDataGrid.Columns[1].Visibility = resultType == XSModelResultType.Procedure
                ? Visibility.Collapsed
                : Visibility.Visible;

            ResultsDataGrid.Columns[0].Width = 0;
            ResultsDataGrid.Columns[1].Width = 0;
            ResultsDataGrid.Columns[2].Width = 0;
            ResultsDataGrid.Columns[3].Width = 0;
            ResultsDataGrid.UpdateLayout();
            ResultsDataGrid.Columns[0].Width = new DataGridLength(3, DataGridLengthUnitType.Star);
            ResultsDataGrid.Columns[1].Width = new DataGridLength(4, DataGridLengthUnitType.Star);
            ResultsDataGrid.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
            ResultsDataGrid.Columns[3].Width = new DataGridLength(9, DataGridLengthUnitType.Star);
        }

        public void SolutionEvents_OnBeforeCloseSolution() =>
            UpdateToolWindowContents(XSModelResultType.Member, new List<XSModelResultItem>());
    }
}
