using System.Windows.Controls;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for ResultsDataGrid.xaml
    /// </summary>
    public partial class ResultsDataGrid : DataGrid
    {
        public new IResultsDataGridParent Parent { private get; set; }

        public ResultsDataGrid() =>
            InitializeComponent();

        protected void ResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            Parent?.OnReturn((sender as DataGridRow).Item);

        public void SelectNext()
        {
            object currentItem = SelectedItem;
            if (currentItem == null)
                return;
            int currentIndex = Items.IndexOf(currentItem);
            if (currentIndex >= Items.Count - 1)
                return;
            SelectedItem = Items.GetItemAt(currentIndex + 1);
            UpdateLayout();
            ScrollIntoView(SelectedItem);
        }

        public void SelectPrevious()
        {
            object currentItem = SelectedItem;
            if (currentItem == null)
                return;
            int currentIndex = Items.IndexOf(currentItem);
            if (currentIndex < 1)
                return;
            SelectedItem = Items.GetItemAt(currentIndex - 1);
            UpdateLayout();
            ScrollIntoView(SelectedItem);
        }

    }
}
