using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Roslyn.Compilers;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace XmlCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ScriptEngine _engine;
        private Session _session;

        public MainWindow()
        {
            InitializeComponent();

            // make sure we can start coding immediately
            txtCode.Focus();

            // error checking will be done later on
            txtInput.Text = string.Join(Environment.NewLine, File.ReadAllLines("PBZPodrunicaZagreb.kml"));

            // this can be cached
            _engine = new ScriptEngine();
            _engine.AddReference(Assembly.GetEntryAssembly());
            _engine.AddReference("System.Core");
            _engine.AddReference("System.Collections");
            _engine.AddReference("System.Linq");
            _engine.AddReference("System.Xml");
            _engine.AddReference("System.Xml.Linq");
            _session = _engine.CreateSession(this);

            //txtCode.TextArea.TextView.LineTransformers.Add(new ColorizeAvalonEdit());

            //txtCode.TextArea.LeftMargins.Add(new FoldingMargin());
        }

        public XDocument InputXml { get; set; }
        public XDocument OutputXml { get; set; }

        public void Print(object o)
        {
            if (o != null)
                MessageBox.Show(o.ToString());
        }

        public void RemoveTags(string tagName)
        {
            foreach (var node in InputXml.Descendants(tagName))
                node.Remove();

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                InputXml.Save(sw);
                txtOutput.Text = sb.ToString();
            }
        }

        public void RemoveTags(XName tagName)
        {
            foreach (var node in InputXml.Descendants(tagName).ToList())
                node.Remove();

            WriteOutput();
        }

        public void WriteOutput()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                InputXml.Save(sw);
                txtOutput.Text = sb.ToString();
            }
        }

        private void TxtCode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                try
                {
                    Cursor = Cursors.Wait;
                    InputXml = XDocument.Parse(txtInput.Text);
                    _session.Execute(txtCode.Text);

                    WriteOutput();
                }
                catch (CompilationErrorException ex)
                {
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Cursor = Cursors.Arrow;
                }
            }
        }
    }
    
    public class ColorizeAvalonEdit : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;
            while ((index = text.IndexOf("var", start)) >= 0)
            {
                base.ChangeLinePart(
                    lineStartOffset + index, // startOffset
                    lineStartOffset + index + 3, // endOffset
                    (VisualLineElement element) =>
                    {
                        // This lambda gets called once for every VisualLineElement
                        // between the specified offsets.
                        Typeface tf = element.TextRunProperties.Typeface;
                        // Replace the typeface with a modified version of
                        // the same typeface
                        element.TextRunProperties.SetTypeface(new Typeface(
                            tf.FontFamily,
                            FontStyles.Italic,
                            FontWeights.Bold,
                            tf.Stretch
                        ));
                    });
                start = index + 1; // search for next occurrence
            }
        }
    }

}
