using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XSharpPowerTools.Helpers;

namespace XSharpPowerTools.View.Windows
{
    /// <summary>
    /// Interaction logic for CodeBrowserWindow.xaml
    /// </summary>
    public partial class CodeBrowserWindow : BaseWindow
    {
        private readonly string SolutionDirectory;

        public override string SearchTerm
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    SearchTextBox.Text = value;
            }
        }

        public CodeBrowserWindow(string solutionDirectory) : base()
        {
            InitializeComponent();
            SolutionDirectory = solutionDirectory;

            SearchTextBox.WhenTextChanged
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(x => OnTextChanged());
        }

        private void SetTableColumns(XSModelResultType resultType)
        {
            if (resultType == XSModelResultType.Member)
                ResultsDataGrid.Columns.First().Visibility = Visibility.Visible;
            else
                ResultsDataGrid.Columns.First().Visibility = Visibility.Collapsed;
        }

        protected async Task SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            var currentFile = searchTerm.Trim().StartsWith("..") || searchTerm.Trim().StartsWith("::") ? await DocumentHelper.GetCurrentFileAsync() : null;
            var (results, resultType) = await XSModel.GetSearchTermMatchesAsync(searchTerm, SolutionDirectory, currentFile);

            if (results == null || results.Count < 1)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                return;
            }

            results = resultType == XSModelResultType.Type
                ? results.OrderBy(q => q.TypeName.Length).ThenBy(q => q.TypeName).ToList()
                : results.OrderBy(q => q.MemberName.Length).ThenBy(q => q.MemberName).ThenBy(q => q.TypeName.Length).ThenBy(q => q.TypeName).ToList();

            ResultsDataGrid.ItemsSource = results;
            ResultsDataGrid.SelectedItem = results.FirstOrDefault();
            SetTableColumns(resultType);

            AllowReturn = true;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        private async Task OpenItemAsync(XSModelResultItem item)
        {
            if (item == null)
                return;

            await DocumentHelper.OpenProjectItemAtAsync(item.ContainingFile, item.Line);
            Close();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AllowReturn && e.Key == Key.Return)
            {
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    if (ResultsDataGrid.SelectedItem is XSModelResultItem item && item != null)
                        await OpenItemAsync(item);
                    else
                        await SearchAsync(SearchTextBox.Text);
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

        private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ResultsDataGrid.SelectedItem as XSModelResultItem;
            _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
            {
                await OpenItemAsync(item);
            });
        }

        private void ResultsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AllowReturn && e.Key == Key.Return)
            {
                var item = ResultsDataGrid.SelectedItem as XSModelResultItem;
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    await OpenItemAsync(item);
                });
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e) =>
            HelpControl.Visibility = HelpControl.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => 
            AllowReturn = false;
    }
}