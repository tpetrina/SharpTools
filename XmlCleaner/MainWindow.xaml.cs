using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.AvalonExtensions;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
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
            _vm.PropertyChanged += _vm_PropertyChanged;

            // make sure we can start coding immediately
            txtCode.Focus();

            // error checking will be done later on
            _vm.InputText = string.Join(Environment.NewLine, File.ReadAllLines("PBZPodrunicaZagreb.kml"));

            //txtCode.TextArea.TextView.LineTransformers.Add(new ColorizeAvalonEdit());
            txtCode.TextArea.TextView.LineTransformers.Add(_highlightErrorLine = new HighlightErrorLine());

            txtCode.TextArea.TextEntered += TextArea_TextEntered;
            txtCode.TextArea.TextEntering += TextArea_TextEntering;
        }

        void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BuildErrors")
                _highlightErrorLine.Errors = _vm.BuildErrors;
        }

        CompletionWindow _completionWindow;
        readonly List<MyCompletionData> _completionTypes = new List<MyCompletionData>();
        private readonly HighlightErrorLine _highlightErrorLine;

        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // find out which item is on the left
                var tree = SyntaxTree.ParseText(txtCode.Text);
            }
            else if (e.Text == " " && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!_completionTypes.Any())
                {
                    if (_vm.Types != null)
                        _completionTypes.AddRange(_vm.Types.Select(t => new MyCompletionData(t.Name)));
                    else
                        return;
                }

                _completionWindow = new CompletionWindow(txtCode.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                foreach (var item in _completionTypes)
                    data.Add(item);

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
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
            //else if (e.Key == Key.Space &&
            //         (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            //{
            //    completionWindow = new CompletionWindow(txtCode.TextArea);
            //    var data = completionWindow.CompletionList.CompletionData;
            //    data.Add(new MyCompletionData("Item1"));
            //    data.Add(new MyCompletionData("Item2"));
            //    data.Add(new MyCompletionData("Item3"));
            //    completionWindow.Show();
            //    completionWindow.Closed += delegate
            //    {
            //        completionWindow = null;
            //    };
            //}
        }

        private async void BuildSyntaxTree(string code)
        {
            TxtSyntax.Text = String.Empty;
            TxtSyntax.Text = await Task.Run(async () =>
                {
                    try
                    {
                        SyntaxTree compiled = SyntaxTree.ParseText(code);
                        var visitor = new Visitor();
                        visitor.Visit(await compiled.GetRootAsync());
                        return visitor.Text;
                    }
                    catch
                    { }
                    return string.Empty;
                });
        }
    }

    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            this.Text = text;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return "Description for " + this.Text; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public double Priority
        {
            get { return 1; }
        }
    }

    public class Visitor : SyntaxRewriter
    {
        readonly StringBuilder _sb = new StringBuilder();
        private int _indent;

        public string Text
        {
            get
            { return _sb.ToString(); }
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            _indent++;
            if (node != null)
                _sb.AppendLine(new string(' ', _indent * 2) + node.Kind.ToString());

            var newNode = base.Visit(node);
            _indent--;
            return newNode;
        }
    }
}
