#region

using System;
using Windows.UI.Xaml;

#region

using Windows.UI.Xaml.Controls;

#endregion

#endregion

namespace Audiotica.Core.Common
{
    public sealed class MaterialCard : ContentControl
    {
        public event RoutedEventHandler ActionButtonClick;

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.RegisterAttached("HeaderText", typeof (string), typeof (MaterialCard),
                null);

        public static readonly DependencyProperty ActionButtonTextProperty =
            DependencyProperty.RegisterAttached("ActionButtonText", typeof(string), typeof(MaterialCard),
                new PropertyMetadata("See More"));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.RegisterAttached("IsLoading", typeof (bool), typeof (MaterialCard),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsActionButtonEnabledProperty =
            DependencyProperty.RegisterAttached("IsActionButtonEnabled", typeof(bool), typeof(MaterialCard),
                new PropertyMetadata(true));

        public static readonly DependencyProperty ActionButtonVisibilityProperty =
            DependencyProperty.RegisterAttached("ActionButtonVisibility", typeof(Visibility), typeof(MaterialCard),
                new PropertyMetadata(Visibility.Collapsed));

        public MaterialCard()
        {
            DefaultStyleKey = typeof (MaterialCard);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var button = GetTemplateChild("PART_ACTION_BUTTON") as Button;
            if (button != null)
                button.Click += (sender, args) =>
                {
                    if (ActionButtonClick != null)
                        ActionButtonClick(sender, args);
                };
        }

        public static void SetHeaderText(DependencyObject element, string value)
        {
            element.SetValue(HeaderTextProperty, value);
        }

        public static string GetHeaderText(DependencyObject element)
        {
            return (string) element.GetValue(HeaderTextProperty);
        }

        public static void SetActionButtonText(DependencyObject element, string value)
        {
            element.SetValue(ActionButtonTextProperty, value);
        }

        public static string GetActionButtonText(DependencyObject element)
        {
            return (string)element.GetValue(ActionButtonTextProperty);
        }

        public static void SetIsLoading(DependencyObject element, bool value)
        {
            element.SetValue(IsLoadingProperty, value);
        }

        public static bool GetIsLoading(DependencyObject element)
        {
            return (bool) element.GetValue(IsLoadingProperty);
        }

        public static void SetIsActionButtonEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsActionButtonEnabledProperty, value);
        }

        public static bool GetIsActionButtonEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsActionButtonEnabledProperty);
        }

        public static void SetActionButtonVisibility(DependencyObject element, Visibility value)
        {
            element.SetValue(ActionButtonVisibilityProperty, value);
        }

        public static Visibility GetActionButtonVisibility(DependencyObject element)
        {
            return (Visibility)element.GetValue(ActionButtonVisibilityProperty);
        }
    }
}