#region

using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Audiotica.View;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class NowPlayingBar
    {
        public NowPlayingBar()
        {
            InitializeComponent();

            var visBinding = new Binding {Source = DataContext, Path = new PropertyPath("CurrentQueue")};
            SetBinding(IsVisibleProperty, visBinding);
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(object), typeof(NowPlayingBar),
                new PropertyMetadata(null, IsVisibleCallback));

        public static void SetIsVisible(DependencyObject element, object value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static object GetIsVisible(DependencyObject element)
        {
            return element.GetValue(IsVisibleProperty);
        }

        private static void IsVisibleCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = e.NewValue != null ? "Visible" : "Collapsed";

            if (App.RootFrame.CurrentSourcePageType == typeof (NowPlayingPage))
                state = "Collapsed";

            VisualStateManager.GoToState(d as NowPlayingBar, state, true);
        }
    }
}