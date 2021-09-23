using Community.VisualStudio.Toolkit;
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

namespace XSharpPowerTools.View.Windows
{
    /// <summary>
    /// Interaction logic for CodeBrowserWindow.xaml
    /// </summary>
    public partial class CodeBrowserWindow : BaseWindow, IResultsDataGridParent
    {
        private readonly string SolutionDirectory;
        private XSModelResultType DisplayedResultType;

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

        protected async Task SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            var currentFile = searchTerm.Trim().StartsWith("..") || searchTerm.Trim().StartsWith("::")
                ? await DocumentHelper.GetCurrentFileAsync()
                : null;
            var (results, resultType) = await XSModel.GetSearchTermMatchesAsync(searchTerm, SolutionDirectory, currentFile);

            ResultsDataGrid.ItemsSource = results;
            ResultsDataGrid.SelectedItem = results.FirstOrDefault();
            SetTableColumns(resultType);
            DisplayedResultType = resultType;

            NoResultsLabel.Visibility = results.Count < 1
                ? Visibility.Visible
                : Visibility.Collapsed;

            AllowReturn = true;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        private async Task OpenItemAsync(XSModelResultItem item)
        {
            if (item == null)
                return;

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            try
            {
                await DocumentHelper.OpenProjectItemAtAsync(item.ContainingFile, item.Line);
                Close();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
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
            XSharpPowerToolsPackage.Instance.JoinableTaskFactory.Run(() => DoSearchAsync());
        }

        private async Task DoSearchAsync()
        {
            await XSharpPowerToolsPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
            var searchTerm = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
                await SearchAsync(searchTerm);
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
                _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
                {
                    await OpenItemAsync(item);
                });
            }
        }

        private void ResultsViewButton_Click(object sender, RoutedEventArgs e) =>
            SaveResultsToToolWindow();

        private void SaveResultsToToolWindow() =>
            _ = XSharpPowerToolsPackage.Instance.JoinableTaskFactory.RunAsync(async delegate
            {
                if (ResultsDataGrid.Items.Count < 1)
                    return;

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                if (ResultsDataGrid.SelectedItem != null)
                    await OpenItemAsync(ResultsDataGrid.SelectedItem as XSModelResultItem);
                else
                    Close();

                var toolWindowPane = await CodeBrowserResultsToolWindow.ShowAsync();
                (toolWindowPane.Content as ToolWindowControl).UpdateToolWindowContents(DisplayedResultType, ResultsDataGrid.ItemsSource as List<XSModelResultItem>);

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            });
    }
}