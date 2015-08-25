using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Audiotica.Windows.Controls
{
    public sealed partial class LibraryHeader
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof (string), typeof (LibraryHeader), null);

        public static readonly DependencyProperty SortItemsProperty =
            DependencyProperty.Register("SortItems", typeof (IList<ListBoxItem>), typeof (LibraryHeader),
                new PropertyMetadata(null, SortItemsPropertyChangedCallback));

        public LibraryHeader()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= OnLoaded;
            if (ListBox.Items?.Count > 0)
                ListBox.SelectedIndex = 0;
        }

        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }

        public IList<ListBoxItem> SortItems
        {
            get { return GetValue(SortItemsProperty) as IList<ListBoxItem>; }
            set { SetValue(SortItemsProperty, value); }
        }

        public event EventHandler<ListBoxItem> CurrentSortChanged;

        private static void SortItemsPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var header = (LibraryHeader) o;
            var sortItems = e.NewValue as IList<ListBoxItem>;

            header.SortHyperlinkButton.IsEnabled = sortItems?.Count > 1;

            if (header.ListBox.Items?.Count > 0)
                header.ListBox.SelectedIndex = 0;
        }

        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
                CurrentSortChanged?.Invoke(this, e.AddedItems[0] as ListBoxItem);
        }
    }
}