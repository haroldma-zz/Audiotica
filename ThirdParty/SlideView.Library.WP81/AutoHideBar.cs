using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using System.Windows.Controls.Primitives;
#elif NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace SlideView.Library
{
    [TemplateVisualState(GroupName = CommonGroupStateName, Name = ShowStateName)]
    [TemplateVisualState(GroupName = CommonGroupStateName, Name = HideStateName)]
    public class AutoHideBar : ContentControl
    {
        internal const string CommonGroupStateName = "CommonStates";
        internal const string ShowStateName = "Show";
        internal const string HideStateName = "Hide";

        #region ScrollControl (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public FrameworkElement ScrollControl
        {
            get { return (FrameworkElement)GetValue(ScrollControlProperty); }
            set { SetValue(ScrollControlProperty, value); }
        }
        public static readonly DependencyProperty ScrollControlProperty =
            DependencyProperty.Register("ScrollControl", typeof(FrameworkElement), typeof(AutoHideBar),
            new PropertyMetadata(null, new PropertyChangedCallback(OnScrollControlChanged)));

        private static void OnScrollControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoHideBar)d).OnScrollControlChanged(e);
        }

        protected virtual void OnScrollControlChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_scroller != null)
            {
                DetachScroller(_scroller);
            }

            if (e.NewValue != null)
            {
                var el = e.NewValue as UIElement;
                FindAndAttachScrollViewer(el);
            }
        }

        #endregion

        #region ShowOnTop (DependencyProperty)

        /// <summary>
        /// Shows the navigation bar when on top of the list
        /// </summary>
        public bool ShowOnTop
        {
            get { return (bool)GetValue(ShowOnTopProperty); }
            set { SetValue(ShowOnTopProperty, value); }
        }
        public static readonly DependencyProperty ShowOnTopProperty =
            DependencyProperty.Register("ShowOnTop", typeof(bool), typeof(AutoHideBar),
              new PropertyMetadata(true));

        #endregion

        private const double TopListOffsetDisplay = 10;
        private const double MinimumOffsetScrollingUp = 50;
        private const double MinimumOffsetScrollingDown = 10;

        private UIElement _scroller;
        private DoubleAnimation _hideAnimation;

        private double _firstOffsetValue;
        private double _lastOffsetValue;

        public AutoHideBar()
        {
            DefaultStyleKey = typeof(AutoHideBar);
        }

#if WINDOWS_PHONE
        public
#elif NETFX_CORE
        protected 
#endif
            override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var hideState = (base.GetTemplateChild(HideStateName) as VisualState);
            if (hideState != null)
                _hideAnimation = hideState.Storyboard.Children[0] as DoubleAnimation;
        }

        private void FindAndAttachScrollViewer(UIElement start)
        {
            _scroller = FindScroller(start);
            AttachScroller(_scroller);
        }

        private UIElement FindScroller(UIElement start)
        {
            UIElement target = null;

            if (IsScroller(start))
            {
                target = start;
            }
            else
            {
                int childCount = VisualTreeHelper.GetChildrenCount(start);

                for (int i = 0; i < childCount; i++)
                {
                    var el = VisualTreeHelper.GetChild(start, i) as UIElement;

                    if (IsScroller(start))
                    {
                        target = el;
                    }
                    else
                    {
                        target = FindScroller(el);
                    }

                    if (target != null)
                        break;
                }
            }

            return target as UIElement;
        }

        private bool IsScroller(UIElement el)
        {
            return ((el is ScrollBar && ((ScrollBar) el).Orientation == Orientation.Vertical)
#if WP8
                    || el is ViewportControl
#endif
                );
        }

        private void AttachScroller(UIElement scroller)
        {
            if (scroller == null)
                return;

            if (scroller is ScrollBar)
            {
                ((ScrollBar)scroller).ValueChanged += scrollbar_ValueChanged;
            }
#if WP8
            else if (scroller is ViewportControl)
            {
                ((ViewportControl)scroller).ViewportChanged += AutoHideBar_ViewportChanged;
            }
#endif
        }

        private void DetachScroller(UIElement scroller)
        {
            if (scroller == null)
                return;

            if (scroller is ScrollBar)
            {
                ((ScrollBar)scroller).ValueChanged -= scrollbar_ValueChanged;
            }
#if WP8
            else if (scroller is ViewportControl)
            {
                ((ViewportControl)scroller).ViewportChanged -= AutoHideBar_ViewportChanged;
            }
#endif
        }

#if WINDOWS_PHONE
        void scrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
#elif NETFX_CORE
        void scrollbar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
#endif
        {
            UpdateVisualState(e.NewValue);
        }

#if WP8
        void AutoHideBar_ViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            var viewport = (ViewportControl)_scroller;
            UpdateVisualState(viewport.Viewport.Y);
        }
#endif

        private void UpdateVisualState(double value)
        {
            Debug.WriteLine("Scrolling : " + value);

            if (ShowOnTop && value <= TopListOffsetDisplay)
            {
                _firstOffsetValue = value;
                Show();
            }
            else if (_firstOffsetValue - value < -MinimumOffsetScrollingDown) // scrolling down
            {
                _firstOffsetValue = value;
                Hide();
            }
            else // scrolling up
            {
                if (_firstOffsetValue - value > MinimumOffsetScrollingUp)
                {
                    _firstOffsetValue = value;
                    Show();
                }
            }
            
            _lastOffsetValue = value;
        }

        private void Show()
        {
            VisualStateManager.GoToState(this, ShowStateName, true);
        }

        private void Hide()
        {
            if (_hideAnimation != null)
                _hideAnimation.To = -ActualHeight;

            VisualStateManager.GoToState(this, HideStateName, true);
        }
    }
}
