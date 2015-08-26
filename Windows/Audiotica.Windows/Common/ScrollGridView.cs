using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    public class ScrollGridView : GridView
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof (double), typeof (ScrollGridView),
                new PropertyMetadata(0, VerticalOffsetPropertyChanged));

        private ScrollViewer _scroll;

        public ScrollGridView()
        {
            Loaded += (s, e) =>
            {
                ScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            };
        }

        public ScrollViewer ScrollViewer => _scroll ?? (_scroll = this.GetScrollViewer());

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }

            set { SetValue(VerticalOffsetProperty, value); }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            VerticalOffset = ScrollViewer.VerticalOffset;
        }

        private static void VerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (ScrollGridView) d;

            RoutedEventHandler handler = null;
            Action action = () =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (handler != null)
                    list.Loaded -= handler;
                // ReSharper restore AccessToModifiedClosure

                if (list.VerticalOffset != list.ScrollViewer.VerticalOffset)
                    list.ScrollViewer.ChangeView(null, (double) e.NewValue, null, true);
            };
            handler = (s, ee) => action();

            if (list.ScrollViewer == null)
                list.Loaded += handler;
            else
                action();
        }
    }
}