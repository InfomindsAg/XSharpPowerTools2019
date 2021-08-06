using System.Windows.Controls;

namespace XSharpPowerTools.View.Controls
{
    /// <summary>
    /// Interaction logic for HelpTextControl.xaml
    /// </summary>
    public partial class HelpTextControl : UserControl
    {
        public HelpTextControl()
        {
            InitializeComponent();

            HelpTextLabel.Content =
@"example      - searches after classes with names similar to 'example'
ex1.ex2       - searches after methods similar to 'ex2' within classes similar to 'ex1' (""."" equal to "":"")
.example     - searches after methods 'example' within all classes
..example    - searches after methods 'example' within current document
" + "\"example\"" + @"   - matches whole word only
ex*Model    - * is a placeholder for multiple characters
p example   - searches after procedures/functions similar to 'example'
d example   - searches after defines similar to 'example'";
        }
    }
}
