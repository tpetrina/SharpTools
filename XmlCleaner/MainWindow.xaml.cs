using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using XmlCleaner.AvalonExtensions;
using XmlCleaner.ViewModels;

namespace XmlCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _vm = new MainViewModel();

            // make sure we can start coding immediately
            txtCode.Focus();

            // error checking will be done later on
            _vm.InputText = string.Join(Environment.NewLine, File.ReadAllLines("PBZPodrunicaZagreb.kml"));

            //txtCode.TextArea.TextView.LineTransformers.Add(new ColorizeAvalonEdit());
            txtCode.TextArea.TextView.LineTransformers.Add(new HighlightErrorLine(_vm));
        }

        private async void TxtCode_OnKeyDown(object sender, KeyEventArgs e)
        {
            _vm.BuildErrors = new ReadOnlyArray<CommonDiagnostic>();

            BuildSyntaxTree(txtCode.Text);

            if (e.Key == Key.F5)
            {
                Cursor = Cursors.Wait;
                await _vm.CompileAndRun(txtCode.Text);
                txtCode.TextArea.TextView.Redraw();
                Cursor = Cursors.Arrow;
            }
            else if (e.Key == Key.F6)
            {
                await _vm.CreateSyntaxTree(txtCode.Text);
            }
        }

        private async void BuildSyntaxTree(string code)
        {
            TxtSyntax.Text = String.Empty;
            TxtSyntax.Text = await Task<string>.Run(async () =>
                {
                    try
                    {
                        SyntaxTree compiled = SyntaxTree.ParseText(code);
                        var visitor = new Visitor();
                        visitor.Visit(await compiled.GetRootAsync());
                        return visitor.Text;
                    }
                    catch { }
                    return string.Empty;
                });
        }
    }

    public class Visitor : SyntaxRewriter
    {
        StringBuilder _sb = new StringBuilder();
        private int indent = 0;
        public string Text { get { return _sb.ToString(); } }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            indent++;
            if (node != null)
                _sb.AppendLine(new string(' ', indent * 2) + node.Kind.ToString());

            var newNode = base.Visit(node);
            indent--;
            return newNode;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return base.VisitFieldDeclaration(node);
        }
    }
}
