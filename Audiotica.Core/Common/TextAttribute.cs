using System;

namespace Audiotica.Windows.Enums
{
    public class TextAttribute : Attribute
    {
        public TextAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}