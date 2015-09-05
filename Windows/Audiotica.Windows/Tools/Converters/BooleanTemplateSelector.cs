using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Audiotica.Windows.Tools.Converters
{
    public class BooleanTemplateSelector : DataTemplateSelector
    {
        public bool IsTrue { get; set; }
        public DataTemplate TrueTemplate { get; set; }
        public DataTemplate FalseTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return IsTrue ? TrueTemplate : FalseTemplate;
        }
    }
}