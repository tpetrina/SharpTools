using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace SyntaxTreeVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            txtCode.Focus();
        }

        private async void TxtCode_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var code = txtCode.Text;
            var root = (await Task.Run(async () =>
                {
                    try
                    {
                        var compiled = Roslyn.Compilers.CSharp.SyntaxTree.ParseText(code);
                        return await compiled.GetRootAsync();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                    }

                    return null;
                })).ToEnumerable();

            SyntaxTree.ItemsSource = root;
        }
    }

    public static class Extensions
    {
        public static EnumerableSyntaxNode ToEnumerable(this SyntaxNode @this)
        {
            return new EnumerableSyntaxNode(@this);
        }
    }
}
