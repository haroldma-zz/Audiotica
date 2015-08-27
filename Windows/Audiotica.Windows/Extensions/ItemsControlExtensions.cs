using System;
using System.Collections;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Audiotica.Windows.Extensions
{
    /// <summary>
    ///     Handy extension for items controls (listview, gridview, etc).
    /// </summary>
    public static class ItemsControlExtensions
    {
        public static ScrollViewer GetScrollViewer(this ItemsControl itemsControl)
        {
            return itemsControl.GetFirstDescendantOfType<ScrollViewer>();
        }

        public static int GetFirstVisibleIndex(this ItemsControl itemsControl)
        {
            // First checking if no items source or an empty one is used
            if (itemsControl.ItemsSource == null)
            {
                return -1;
            }

            var enumItemsSource = itemsControl.ItemsSource as IEnumerable;

            if (enumItemsSource != null && !enumItemsSource.GetEnumerator().MoveNext())
            {
                return -1;
            }

            // Check if a modern panel is used as an items panel
            var sourcePanel = itemsControl.ItemsPanelRoot;

            if (sourcePanel == null)
            {
                throw new InvalidOperationException(
                    "Can't get first visible index from an ItemsControl with no ItemsPanel.");
            }

            var isp = sourcePanel as ItemsStackPanel;

            if (isp != null)
            {
                return isp.FirstVisibleIndex;
            }

            var iwg = sourcePanel as ItemsWrapGrid;

            if (iwg != null)
            {
                return iwg.FirstVisibleIndex;
            }

            // Check containers for first one in view
            if (sourcePanel.Children.Count == 0)
            {
                return -1;
            }

            if (itemsControl.ActualWidth == 0)
            {
                throw new InvalidOperationException(
                    "Can't get first visible index from an ItemsControl that is not loaded or has zero size.");
            }

            foreach (var container in from FrameworkElement container in sourcePanel.Children
                let bounds = container.TransformToVisual(itemsControl)
                    .TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight))
                where bounds.Left < itemsControl.ActualWidth &&
                      bounds.Top < itemsControl.ActualHeight &&
                      bounds.Right > 0 &&
                      bounds.Bottom > 0
                select container)
            {
                return itemsControl.IndexFromContainer(container);
            }

            throw new InvalidOperationException();
        }
    }
}