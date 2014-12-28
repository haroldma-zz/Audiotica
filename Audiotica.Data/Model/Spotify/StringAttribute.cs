using System;

namespace Audiotica.Data.Model.Spotify
{
    public class StringAttribute : Attribute
    {
        public String Text {get;set;}
        public StringAttribute(String text)
        {
            this.Text = text;
        }
    }
}
