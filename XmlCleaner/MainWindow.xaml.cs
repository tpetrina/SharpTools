using System;
using System.IO;
using System.Windows.Input;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
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

            if (e.Key == Key.F5)
            {
                Cursor = Cursors.Wait;
                await _vm.CompileAndRun(txtCode.Text);
                txtCode.TextArea.TextView.Redraw();
                Cursor = Cursors.Arrow;
            }
        }
    }
}
