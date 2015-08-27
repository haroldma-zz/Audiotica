using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    public class ScrollListView : ListView
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof (double), typeof (ScrollListView),
                new PropertyMetadata(0, VerticalOffsetPropertyChanged));

        public static readonly DependencyProperty BetterSelectedIndexProperty =
            DependencyProperty.RegisterAttached("BetterSelectedIndex", typeof (int), typeof (ScrollListView),
                new PropertyMetadata(0, BetterSelectedIndexPropertyChanged));

        private ScrollViewer _scroll;

        public ScrollListView()
        {
            Loaded += (s, e) =>
            {
                ScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                SelectionChanged -= OnSelectionChanged;
                SelectionChanged += OnSelectionChanged;
            };

            Unloaded += (s, e) =>
            {
                ScrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
                SelectionChanged -= OnSelectionChanged;
            };
        }

        public ScrollViewer ScrollViewer => _scroll ?? (_scroll = this.GetScrollViewer());

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }

            set { SetValue(VerticalOffsetProperty, value); }
        }

        public int BetterSelectedIndex
        {
            get { return (int) GetValue(BetterSelectedIndexProperty); }

            set { SetValue(BetterSelectedIndexProperty, value); }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            BetterSelectedIndex = SelectedIndex;
        }

        private static void BetterSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (ScrollListView) d;

            RoutedEventHandler handler = null;
            Action action = () =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (handler != null)
                    list.Loaded -= handler;
                // ReSharper restore AccessToModifiedClosure

                list.SelectedIndex = (int) e.NewValue;
            };
            handler = (s, ee) => action();

            if (list.ScrollViewer == null)
                list.Loaded += handler;
            else
                action();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            VerticalOffset = ScrollViewer.VerticalOffset;
        }

        private static void VerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (ScrollListView) d;

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