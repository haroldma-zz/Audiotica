using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Audiotica.Windows.Extensions
{
    /// <summary>
    ///     Extension methods and attached properties for the ListView class.
    /// </summary>
    public static class ListViewExtensions
    {
        /// <summary>
        ///     Scrolls a vertical ListView to the bottom.
        /// </summary>
        /// <param name="listView"></param>
        public static void ScrollToBottom(this ListView listView)
        {
            var scrollViewer = listView.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
        }

        #region BindableSelection

        /// <summary>
        ///     BindableSelection Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty BindableSelectionProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelection",
                typeof (ObservableCollection<object>),
                typeof (ListViewExtensions),
                new PropertyMetadata(null, OnBindableSelectionChanged));
        
        /// <summary>
        ///     Gets the BindableSelection property. This dependency property
        ///     indicates the list of selected items that is synchronized
        ///     with the items selected in the ListView.
        /// </summary>
        public static ObservableCollection<object> GetBindableSelection(DependencyObject d)
        {
            return (ObservableCollection<object>) d.GetValue(BindableSelectionProperty);
        }

        /// <summary>
        ///     Sets the BindableSelection property. This dependency property
        ///     indicates the list of selected items that is synchronized
        ///     with the items selected in the ListView.
        /// </summary>
        public static void SetBindableSelection(
            DependencyObject d,
            ObservableCollection<object> value)
        {
            d.SetValue(BindableSelectionProperty, value);
        }

        /// <summary>
        ///     Handles changes to the BindableSelection property.
        /// </summary>
        /// <param name="d">
        ///     The <see cref="DependencyObject" /> on which
        ///     the property has changed value.
        /// </param>
        /// <param name="e">
        ///     Event data that is issued by any event that
        ///     tracks changes to the effective value of this property.
        /// </param>
        private static void OnBindableSelectionChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var oldBindableSelection = e.OldValue;
            var newBindableSelection = GetBindableSelection(d);

            if (oldBindableSelection != null)
            {
                var handler = GetBindableSelectionHandler(d);
                SetBindableSelectionHandler(d, null);
                handler.Detach();
            }

            if (newBindableSelection != null)
            {
                var handler = new ListViewBindableSelectionHandler(
                    (ListViewBase) d, newBindableSelection);
                SetBindableSelectionHandler(d, handler);
            }
        }

        #endregion

        #region BindableSelectionHandler

        /// <summary>
        ///     BindableSelectionHandler Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty BindableSelectionHandlerProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelectionHandler",
                typeof (ListViewBindableSelectionHandler),
                typeof (ListViewExtensions),
                new PropertyMetadata(null));

        /// <summary>
        ///     Gets the BindableSelectionHandler property. This dependency property
        ///     indicates BindableSelectionHandler for a ListView - used
        ///     to manage synchronization of BindableSelection and SelectedItems.
        /// </summary>
        public static ListViewBindableSelectionHandler GetBindableSelectionHandler(
            DependencyObject d)
        {
            return
                (ListViewBindableSelectionHandler)
                    d.GetValue(BindableSelectionHandlerProperty);
        }

        /// <summary>
        ///     Sets the BindableSelectionHandler property. This dependency property
        ///     indicates BindableSelectionHandler for a ListView - used to manage synchronization of BindableSelection and
        ///     SelectedItems.
        /// </summary>
        public static void SetBindableSelectionHandler(
            DependencyObject d,
            ListViewBindableSelectionHandler value)
        {
            d.SetValue(BindableSelectionHandlerProperty, value);
        }

        #endregion

        #region ItemToBringIntoView

        /// <summary>
        ///     ItemToBringIntoView Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemToBringIntoViewProperty =
            DependencyProperty.RegisterAttached(
                "ItemToBringIntoView",
                typeof (object),
                typeof (ListViewExtensions),
                new PropertyMetadata(null, OnItemToBringIntoViewChanged));

        /// <summary>
        ///     Gets the ItemToBringIntoView property. This dependency property
        ///     indicates the item that should be brought into view.
        /// </summary>
        public static object GetItemToBringIntoView(DependencyObject d)
        {
            return d.GetValue(ItemToBringIntoViewProperty);
        }

        /// <summary>
        ///     Sets the ItemToBringIntoView property. This dependency property
        ///     indicates the item that should be brought into view when first set.
        /// </summary>
        public static void SetItemToBringIntoView(DependencyObject d, object value)
        {
            d.SetValue(ItemToBringIntoViewProperty, value);
        }

        /// <summary>
        ///     Handles changes to the ItemToBringIntoView property.
        /// </summary>
        /// <param name="d">
        ///     The <see cref="DependencyObject" /> on which
        ///     the property has changed value.
        /// </param>
        /// <param name="e">
        ///     Event data that is issued by any event that
        ///     tracks changes to the effective value of this property.
        /// </param>
        private static void OnItemToBringIntoViewChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newItemToBringIntoView =
                d.GetValue(ItemToBringIntoViewProperty);

            if (newItemToBringIntoView != null)
            {
                var listView = (ListView) d;
                listView.ScrollIntoView(newItemToBringIntoView);
            }
        }

        #endregion
    }
}