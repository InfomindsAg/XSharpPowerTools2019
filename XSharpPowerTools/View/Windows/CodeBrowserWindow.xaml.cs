using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XSharpPowerTools.Helpers;
using XSharpPowerTools.View.Controls;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;

namespace XSharpPowerTools.View.Windows
{
    /// <summary>
    /// Interaction logic for CodeBrowserWindow.xaml
    /// </summary>
    public partial class CodeBrowserWindow : BaseWindow, IResultsDataGridParent
    {
        const string FileReference = "vs/XSharpPowerTools/CodeBrowser/";
        readonly string SolutionDirectory;
        XSModelResultType DisplayedResultType;
        volatile bool SearchActive = false;
        volatile bool ReDoSearch = false;

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
            ResultsDataGrid.Parent = this;

            SearchTextBox.WhenTextChanged
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(x => OnTextChanged());
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

        protected async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                return;

            if (SearchActive)
            {
                ReDoSearch = SearchActive;
                return;
            }

            using var waitCursor = new WithWaitCursor();
            SearchActive = true;
            try
            {
                do
                {
                    var searchTerm = SearchTextBox.Text.Trim();
                    ReDoSearch = false;
                    var currentFile = searchTerm.StartsWith("..") || searchTerm.StartsWith("::") ? await DocumentHelper.GetCurrentFileAsync() : null;
                    var (results, resultType) = await XSModel.GetSearchTermMatchesAsync(searchTerm, SolutionDirectory, currentFile);

                    ResultsDataGrid.ItemsSource = results;
                    ResultsDataGrid.SelectedItem = results.FirstOrDefault();
                    SetTableColumns(resultType);
                    DisplayedResultType = resultType;

                    NoResultsLabel.Visibility = results.Count < 1 ? Visibility.Visible : Visibility.Collapsed;

                } while (ReDoSearch);
            }
            finally
            {
                SearchActive = false;
                AllowReturn = true;
            }
        }

        private async Task OpenItemAsync(XSModelResultItem item)
        {
            if (item == null)
                return;

            using var waitCursor = new WithWaitCursor();
            await DocumentHelper.OpenProjectItemAtAsync(item.ContainingFile, item.Line);
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (AllowReturn && e.Key == Key.Return && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SaveResultsToToolWindow();
            }
            else if (AllowReturn && e.Key == Key.Return)
            {
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    if (ResultsDataGrid.SelectedItem is XSModelResultItem item && item != null)
                        await OpenItemAsync(item);
                    else
                        await SearchAsync();
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
                    await SearchAsync();
                });
                SearchTextBox.CaretIndex = int.MaxValue;
            }
            try
            {
                SearchTextBox.Focus();
            }
            catch (Exception)
            { }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e) =>
            HelpControl.Visibility = HelpControl.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        protected override void OnTextChanged()
        {
            XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async () => await DoSearchAsync()).FileAndForget($"{FileReference}OnTextChange");
        }

        private async Task DoSearchAsync()
        {
            await XSharpPowerToolsPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
            await SearchAsync();
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
                var item = selectedItem as XSModelResultItem;
                XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async () => await OpenItemAsync(item)).FileAndForget($"{FileReference}OnReturn");
            }
        }

        private void ResultsViewButton_Click(object sender, RoutedEventArgs e) =>
            SaveResultsToToolWindow();

        private void SaveResultsToToolWindow()
        {
            if (ResultsDataGrid.Items.Count < 1)
                return;

            XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async () => 
            {
                using (var waitCursor = new WithWaitCursor())
                {

                    if (ResultsDataGrid.SelectedItem != null)
                        await OpenItemAsync(ResultsDataGrid.SelectedItem as XSModelResultItem);
                    else
                        Close();

                    var toolWindowPane = await CodeBrowserResultsToolWindow.ShowAsync();
                    (toolWindowPane.Content as ToolWindowControl).UpdateToolWindowContents(DisplayedResultType, ResultsDataGrid.ItemsSource as List<XSModelResultItem>);
                }
            }).FileAndForget($"{FileReference}SaveResultsToToolWindow");
        }
    }
}