using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for SearchTextBox.xaml
    /// </summary>
    public partial class SearchTextBox : TextBox
    {
        private readonly VisualBrush PlaceholderBrush;

        public new string Text
        {
            get => base.Text;
            set
            {
                if (value == null)
                {
                    base.Text = null;
                    return;
                }

                var lines = value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                base.Text = lines.FirstOrDefault();
            }
        }

        public SearchTextBox()
        {
            InitializeComponent();
            TextWrapping = TextWrapping.Wrap;
            PlaceholderBrush = new VisualBrush
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Center,
                Stretch = Stretch.None,
                Visual = new Label
                {
                    Content = "Search",
                    Foreground = TryFindResource(EnvironmentColors.ControlEditHintTextBrushKey) as Brush
                }
            };
            Foreground = TryFindResource(EnvironmentColors.ComboBoxTextBrushKey) as Brush;
            Background = string.IsNullOrWhiteSpace(Text) ? PlaceholderBrush : TryFindResource(EnvironmentColors.ComboBoxBackgroundBrushKey) as Brush;
            BorderBrush = TryFindResource(EnvironmentColors.ComboBoxBorderBrushKey) as Brush;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Foreground = TryFindResource(EnvironmentColors.ComboBoxFocusedTextBrushKey) as Brush;
            Background = TryFindResource(EnvironmentColors.ComboBoxFocusedBackgroundBrushKey) as Brush;
            BorderBrush = TryFindResource(EnvironmentColors.ComboBoxFocusedBorderBrushKey) as Brush;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Foreground = TryFindResource(EnvironmentColors.ComboBoxTextBrushKey) as Brush;
            Background = string.IsNullOrWhiteSpace(Text) ? PlaceholderBrush : TryFindResource(EnvironmentColors.ComboBoxBackgroundBrushKey) as Brush;
            BorderBrush = TryFindResource(EnvironmentColors.ComboBoxBorderBrushKey) as Brush;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) =>
            Background = string.IsNullOrWhiteSpace(Text) ? PlaceholderBrush : TryFindResource(EnvironmentColors.ComboBoxBackgroundBrushKey) as Brush;

        public IObservable<TextChangedEventArgs> WhenTextChanged =>
            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>
                (
                    h => TextChanged += h,
                    h => TextChanged -= h
                ).Select(x => x.EventArgs);
    }
}
