using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XSharpPowerTools.Helpers;
using XSharpPowerTools.View.Controls;

namespace XSharpPowerTools.View.Windows
{
    /// <summary>
    /// Interaction logic for FindNamespaceWindow.xaml
    /// </summary>
    public partial class FindNamespaceWindow : BaseWindow, IResultsDataGridParent
    {
        public override string SearchTerm
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    SearchTextBox.Text = value;
            }
        }

        public FindNamespaceWindow() : base()
        {
            InitializeComponent();
            ResultsDataGrid.Parent = this;

            SearchTextBox.WhenTextChanged
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(x => OnTextChanged());
        }

        private async Task SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;


            var results = await XSModel.GetContainingNamespaceAsync(searchTerm.Trim());
            ResultsDataGrid.ItemsSource = results;
            ResultsDataGrid.SelectedItem = results.FirstOrDefault();

            AllowReturn = true;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        private async Task InsertUsingAsync(NamespaceResultItem item)
        {
            if (item == null)
                return;
            await DocumentHelper.InsertUsingAsync(item.Namespace, XSModel);
            Close();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AllowReturn && e.Key == Key.Return)
            {
                var item = ResultsDataGrid.SelectedItem as NamespaceResultItem;
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    await InsertUsingAsync(item);
                });
            }
            else if (e.Key == Key.Down)
            {
                ResultsDataGrid.SelectNext();
            }
            else if (e.Key == Key.Up)
            {
                ResultsDataGrid.SelectPrevious();
            }
        }

        protected override async void OnTextChanged()
        {
            await XSharpPowerToolsPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
            await Dispatcher.Invoke(async delegate
            {
                var searchTerm = SearchTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                    await SearchAsync(searchTerm);
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    await SearchAsync(SearchTextBox.Text);
                });
            }
            try
            {
                SearchTextBox.Focus();
            }
            catch (Exception)
            { }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            AllowReturn = false;

        public void OnReturn(object selectedItem) 
        {
            if (AllowReturn)
            {
                var item = selectedItem as NamespaceResultItem;
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    await InsertUsingAsync(item);
                });
            }
        }
    }
}
