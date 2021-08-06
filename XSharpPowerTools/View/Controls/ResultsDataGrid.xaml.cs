using System.Windows.Controls;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for ResultsDataGrid.xaml
    /// </summary>
    public partial class ResultsDataGrid : DataGrid
    {
        public ResultsDataGrid()
        {
            InitializeComponent();
        }

        public void SelectNext()
        {
            var currentItem = SelectedItem;
            if (currentItem == null)
                return;
            var currentIndex = Items.IndexOf(currentItem);
            if (currentIndex >= Items.Count - 1)
                return;
            SelectedItem = Items.GetItemAt(currentIndex + 1);
            UpdateLayout();
            ScrollIntoView(SelectedItem);
        }

        public void SelectPrevious()
        {
            var currentItem = SelectedItem;
            if (currentItem == null)
                return;
            var currentIndex = Items.IndexOf(currentItem);
            if (currentIndex < 1)
                return;
            SelectedItem = Items.GetItemAt(currentIndex - 1);
            UpdateLayout();
            ScrollIntoView(SelectedItem);
        }

    }
}
