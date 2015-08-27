using System;
using System.Collections.Generic;
using System.Windows.Input;
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

        public static readonly DependencyProperty DefaultSortIndexProperty =
            DependencyProperty.Register("DefaultSortIndex", typeof (int), typeof (LibraryHeader),
                new PropertyMetadata(0));

        public static readonly DependencyProperty CurrentSortChangedCommandProperty =
            DependencyProperty.Register("CurrentSortChangedCommand", typeof (ICommand), typeof (LibraryHeader),
                new PropertyMetadata(0));

        public LibraryHeader()
        {
            InitializeComponent();
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

        public int DefaultSortIndex
        {
            get { return (int) GetValue(DefaultSortIndexProperty); }
            set { SetValue(DefaultSortIndexProperty, value); }
        }

        public ICommand CurrentSortChangedCommand
        {
            get { return (ICommand) GetValue(CurrentSortChangedCommandProperty); }
            set { SetValue(CurrentSortChangedCommandProperty, value); }
        }

        public event EventHandler<ListBoxItem> CurrentSortChanged;

        private static void SortItemsPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var header = (LibraryHeader) o;
            var sortItems = e.NewValue as IList<ListBoxItem>;

            header.SortHyperlinkButton.IsEnabled = sortItems?.Count > 1;
        }

        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                var item = e.AddedItems[0] as ListBoxItem;
                CurrentSortChanged?.Invoke(this, item);
                CurrentSortChangedCommand?.Execute(item);
            }
        }
    }
}