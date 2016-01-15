using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Audiotica.Windows.Engine.Mvvm;

namespace Audiotica.Windows.Controls
{
    [ContentProperty(Name = nameof(Content))]
    public class HamburgerButtonInfo : DependencyBindableBase
    {
        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register(nameof(Visibility),
                typeof (Visibility),
                typeof (HamburgerButtonInfo),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        ///     Sets and gets the ClearHistory property.
        ///     If true, navigation stack is cleared when navigating to this page
        /// </summary>
        private bool _clearHistory;

        private UIElement _content;

        private bool _isChecked;

        /// <summary>
        ///     Sets and gets the IsEnabled property.
        ///     Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        private bool _isEnabled = true;

        private double _maxWidth = 9999;

        /// <summary>
        ///     Sets and gets the PageParameter property.
        /// </summary>
        private object _pageParameter;

        /// <summary>
        ///     Sets and gets the PageType property.
        /// </summary>
        private Type _pageType;

        public event RoutedEventHandler Checked;

        public event HoldingEventHandler Holding;

        public event RightTappedEventHandler RightTapped;

        public event RoutedEventHandler Selected;

        public event RoutedEventHandler Tapped;

        public event RoutedEventHandler Unchecked;

        public event RoutedEventHandler Unselected;

        public enum ButtonTypes
        {
            Toggle,
            Command
        }

        public ButtonTypes ButtonType { get; set; } = ButtonTypes.Toggle;

        public bool ClearHistory
        {
            get
            {
                return _clearHistory;
            }
            set
            {
                Set(ref _clearHistory, value);
            }
        }

        public UIElement Content
        {
            get
            {
                return _content;
            }
            set
            {
                Set(ref _content, value);
            }
        }

        public bool? IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                Set(ref _isChecked, value ?? false);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                Set(ref _isEnabled, value);
            }
        }

        public double MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                Set(ref _maxWidth, value);
            }
        }

        public object PageParameter
        {
            get
            {
                return _pageParameter;
            }
            set
            {
                Set(ref _pageParameter, value);
            }
        }

        public Type PageType
        {
            get
            {
                return _pageType;
            }
            set
            {
                Set(ref _pageType, value);
            }
        }

        /// <summary>
        ///     Sets and gets the Visibility property.
        ///     Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return (Visibility)GetValue(VisibilityProperty);
            }
            set
            {
                SetValue(VisibilityProperty, value);
            }
        }

        public void Dispose()
        {
        }

        public override string ToString()
            => string.Format("{0}({1})", PageType?.ToString() ?? "null", PageParameter?.ToString() ?? "null");

        internal void RaiseChecked(RoutedEventArgs args)
        {
            if (ButtonType == ButtonTypes.Toggle)
            {
                Checked?.Invoke(this, args);
            }
        }

        internal void RaiseHolding(HoldingRoutedEventArgs args)
        {
            Holding?.Invoke(this, args);
        }

        internal void RaiseRightTapped(RightTappedRoutedEventArgs args)
        {
            RightTapped?.Invoke(this, args);
        }

        internal void RaiseSelected()
        {
            Selected?.Invoke(this, new RoutedEventArgs());
        }

        internal void RaiseTapped(RoutedEventArgs args)
        {
            Tapped?.Invoke(this, args);
        }

        internal void RaiseUnchecked(RoutedEventArgs args)
        {
            if (ButtonType == ButtonTypes.Toggle)
            {
                Unchecked?.Invoke(this, args);
            }
        }

        internal void RaiseUnselected()
        {
            Unselected?.Invoke(this, new RoutedEventArgs());
        }
    }
}