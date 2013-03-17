using System;
using System.IO;
using System.Windows.Input;
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
        }

        private async void TxtCode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Cursor = Cursors.Wait;
                await _vm.CompileAndRun(txtCode.Text);
                Cursor = Cursors.Arrow;
            }
        }
    }
}
