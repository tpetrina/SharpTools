using System;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common.AvalonExtensions;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting.CSharp;

namespace SyntaxLearner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SolidColorBrush _salmonBrush = new SolidColorBrush(Colors.Salmon);
        private readonly SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);

        private readonly ScriptEngine _engine;
        private HighlightErrorLine _highlightErrorLine;

        public MainWindow()
        {
            InitializeComponent();
            txtCode.Text =
@"using System;
                
int x = 5;
                
void foo(bool b)
{
                
}
                
Console.WriteLine(x);";

            txtVisitorCode.Text =
@"class Visitor : SyntaxRewriter
{
    private int _indent;
    private readonly StringBuilder _sb = new StringBuilder();

    public string Tree
    {
        get { return _sb.ToString(); }
    }

    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
            return base.Visit(null);

        _sb.AppendLine(new string(' ', _indent * 2) + node.Kind.ToString());
        _indent++;

        var node2 = base.Visit(node);
        _indent--;
        return node2;
    }
}";

            _engine = new ScriptEngine();
            _engine.AddReference(Assembly.GetEntryAssembly());
            _engine.AddReference("Roslyn.Compilers.CSharp.dll");

            txtOutputVisited.TextArea.TextView.LineTransformers.Add(_highlightErrorLine = new HighlightErrorLine());
            UpdateEverything();
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                UpdateEverything();
        }

        private void UpdateEverything()
        {
            infoBlock.Text = "Updating...";
            var code = string.Empty;

            try
            {
                var tree = SyntaxTree.ParseText(txtCode.Text);
                var visitor = new Visitor();
                visitor.Visit(tree.GetRoot());
                txtOutput.Text = visitor.Tree;

                // inception
                var session = _engine.CreateSession(this);

                // first add usings
                code =
@"using System.Text;
using Roslyn.Compilers.CSharp;";

                // then add original C# code text
                code += "var code = @\"" + txtCode.Text + "\";";

                // now add visitor code
                code += txtVisitorCode.Text;

                // finally, the logic itself
                code +=
@"

var tree = SyntaxTree.ParseText(code);
var visitor = new Visitor();
visitor.Visit(tree.GetRoot());
Result(visitor.Tree);
";
                session.Execute(code);
                infoBlock.Text = "Done";
            }
            catch (CompilationErrorException ex)
            {
                infoBlock.Background = _salmonBrush;
                infoBlock.Text = ex.Diagnostics.ToString() + Environment.NewLine;
                txtOutputVisited.Text = code;
                _highlightErrorLine.Errors = ex.Diagnostics;
                txtOutputVisited.ShowLineNumbers = true;
                txtOutputVisited.ScrollToLine(ex.Diagnostics[0].Location.GetLineSpan(true).StartLinePosition.Line);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception caught: '{0}'", ex));
            }
        }

        public void Result(string result)
        {
            infoBlock.Background = _whiteBrush;
            infoBlock.Text = string.Empty;
            txtOutputVisited.Text = result;
            txtOutputVisited.ShowLineNumbers = false;
        }

        internal class Visitor : SyntaxRewriter
        {
            private int _indent;
            private readonly StringBuilder _sb = new StringBuilder();

            public string Tree
            {
                get { return _sb.ToString(); }
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null)
                    return base.Visit(null);

                _sb.AppendLine(new string(' ', _indent * 2) + node.Kind.ToString());
                _indent++;

                var node2 = base.Visit(node);
                _indent--;
                return node2;
            }
        }
    }
}
