#region

using Windows.UI.Xaml;

#region

using Windows.UI.Xaml.Controls;

#endregion

#endregion

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Audiotica.Core.Common
{
    public sealed class MaterialCard : ContentControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.RegisterAttached("HeaderText", typeof (string), typeof (MaterialCard),
                null);

        public MaterialCard()
        {
            DefaultStyleKey = typeof (MaterialCard);
        }

        public static void SetHeaderText(DependencyObject element, string value)
        {
            element.SetValue(HeaderTextProperty, value);
        }

        public static string GetHeaderText(DependencyObject element)
        {
            return (string) element.GetValue(HeaderTextProperty);
        }
    }
}