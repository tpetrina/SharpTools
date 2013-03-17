using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace XmlCleaner.AvalonUtilities
{
    public class TextEditorTextBindingBehavior : Behavior<TextEditor>
    {
        #region Text dependency property

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
            typeof(string),
            typeof(TextEditorTextBindingBehavior),
            new PropertyMetadata(string.Empty));

        #endregion

        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            base.OnDetaching();
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            Text = AssociatedObject.Text;
        }
    }
}
