#region

using Windows.UI.Xaml;

#region

using Windows.UI.Xaml.Controls;

#endregion

#endregion

namespace Audiotica.Core.Common
{
    public sealed class MaterialCard : ContentControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.RegisterAttached("HeaderText", typeof (string), typeof (MaterialCard),
                null);

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.RegisterAttached("IsLoading", typeof (bool), typeof (MaterialCard),
                new PropertyMetadata(false));

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

        public static void SetIsLoading(DependencyObject element, bool value)
        {
            element.SetValue(IsLoadingProperty, value);
        }

        public static bool GetIsLoading(DependencyObject element)
        {
            return (bool) element.GetValue(IsLoadingProperty);
        }
    }
}