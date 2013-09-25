using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;

namespace Common.AvalonExtensions
{
    public class HighlightErrorLine : DocumentColorizingTransformer
    {
        public ReadOnlyArray<CommonDiagnostic> Errors { get; set; }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (Errors == null)
                return;

            var text = CurrentContext.Document.GetText(line);
            var start = line.Offset;
            var end = line.Offset + text.Length;

            foreach (var error in Errors)
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
