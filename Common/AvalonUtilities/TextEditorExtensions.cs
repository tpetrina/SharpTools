using System.Windows;
using ICSharpCode.AvalonEdit;

namespace Common.AvalonUtilities
{
    public class TextEditorExtensions
    {
        #region Text attached property

        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text",
            typeof(string),
            typeof(TextEditorExtensions),
            new PropertyMetadata(string.Empty, TextProperty_Changed));

        private static void TextProperty_Changed(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ((TextEditor)o).Text = (string)args.NewValue;
        }

        #endregion
    }
}
