using System;
using System.Linq;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using XmlCleaner.ViewModels;

namespace XmlCleaner.AvalonExtensions
{
    public class HighlightErrorLine : DocumentColorizingTransformer
    {
        private MainViewModel _vm;

        public HighlightErrorLine(MainViewModel vm)
        {
            _vm = vm;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (_vm.BuildErrors == null)
                return;

            var text = CurrentContext.Document.GetText(line);
            var start = line.Offset;
            var end = line.Offset + text.Length;

            foreach (var error in _vm.BuildErrors)
            {
                var span = error.Location.SourceSpan;
                if (span.Start >= start && span.Start <= end)
                    ChangeLinePart(span.Start, Math.Min(span.End, end), HighlightError);
            }
        }

        private void HighlightError(VisualLineElement visualLineElement)
        {
            visualLineElement.TextRunProperties.SetBackgroundBrush(Brushes.PaleVioletRed);
        }
    }
}
