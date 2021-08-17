using System.Windows.Controls;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for ResultsDataGrid.xaml
    /// </summary>
    public partial class ResultsDataGrid : DataGrid
    {
        public new IResultsDataGridParent Parent { private get; set; }

        public ResultsDataGrid()
        {
            InitializeComponent();
            PreviewKeyDown += ResultsDataGrid_PreviewKeyDown;
            MouseDoubleClick += ResultsDataGrid_MouseDoubleClick;
        }

        private void ResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => 
            Parent?.OnReturn(SelectedItem);

        private void ResultsDataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) => 
            Parent?.OnReturn(SelectedItem);

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
