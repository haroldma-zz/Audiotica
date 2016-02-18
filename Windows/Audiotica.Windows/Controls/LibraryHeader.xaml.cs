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
            DependencyProperty.Register("CurrentSortChangedCommand", typeof (ICommand), typeof (LibraryHeader), null);

        public static readonly DependencyProperty ShuffleAllCommandProperty =
            DependencyProperty.Register("ShuffleAllCommand", typeof (ICommand), typeof (LibraryHeader), null);

        public static readonly DependencyProperty IsSelectModeProperty =
           DependencyProperty.Register("IsSelectMode", typeof(bool?), typeof(LibraryHeader), new PropertyMetadata(false));

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

        public ICommand ShuffleAllCommand
        {
            get { return (ICommand) GetValue(ShuffleAllCommandProperty); }
            set { SetValue(ShuffleAllCommandProperty, value); }
        }

        public bool? IsSelectMode
        {
            get { return (bool?)GetValue(IsSelectModeProperty); }
            set { SetValue(IsSelectModeProperty, value); }
        }

        public event EventHandler<ListBoxItem> CurrentSortChanged;
        public event EventHandler ShuffleAll;

        private static void SortItemsPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var header = (LibraryHeader) o;
            var sortItems = e.NewValue as IList<ListBoxItem>;

            header.SortButton.IsEnabled = sortItems?.Count > 1;
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

        private void ShuffleAll_Click(object sender, RoutedEventArgs e)
        {
            ShuffleAll?.Invoke(this, EventArgs.Empty);
            ShuffleAllCommand?.Execute(EventArgs.Empty);
        }
    }
}