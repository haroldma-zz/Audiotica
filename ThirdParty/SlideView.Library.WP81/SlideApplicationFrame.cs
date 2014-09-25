using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows.Media;
#elif NETFX_CORE
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
#endif

namespace SlideView.Library
{
    [TemplatePart(Name = SlideViewName, Type = typeof(SlideView))]
    public class SlideApplicationFrame 
#if WINDOWS_PHONE
        : PhoneApplicationFrame
#elif NETFX_CORE
        : Frame
#endif
    {
        internal const string SlideViewName = "MainPanel";
        internal const string PageCacheName = "PreviousPageCache";
        internal const string PageTransitionForwardName = "PageTransitionForward";
        internal const string PageTransitionBackwardName = "PageTransitionBackward";
        internal const string HeaderName = "Header";

        public static int IndexLeftPanel = 0;
        public static int IndexCenterPanel = 1;
        public static int IndexRightPanel = 2;

        public event EventHandler SelectedPanelIndexChanged;

        #region LeftContent (DependencyProperty)

        public object LeftContent
        {
            get { return (object)GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }
        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register("LeftContent", typeof(object), typeof(SlideApplicationFrame),
              new PropertyMetadata(null));

        #endregion

        #region RightContent (DependencyProperty)

        public object RightContent
        {
            get { return (object)GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }
        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register("RightContent", typeof(object), typeof(SlideApplicationFrame),
              new PropertyMetadata(null));

        #endregion

        #region Header (DependencyProperty)

        /// <summary>
        /// Content for the header
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(SlideApplicationFrame),
              new PropertyMetadata(null));

        #endregion


        #region HeaderTemplate (DependencyProperty)

        /// <summary>
        /// Data template for the header
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(SlideApplicationFrame),
              new PropertyMetadata(null));

        #endregion


        #region IsSlideEnabled (DependencyProperty)

        /// <summary>
        /// If true, enables the slide interaction between panels, otherwise disables it
        /// </summary>
        public bool IsSlideEnabled
        {
            get { return (bool)GetValue(IsSlideEnabledProperty); }
            set { SetValue(IsSlideEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsSlideEnabledProperty =
            DependencyProperty.Register("IsSlideEnabled", typeof(bool), typeof(SlideApplicationFrame),
            new PropertyMetadata(true, OnIsSlideEnabledChanged));

        private static void OnIsSlideEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideApplicationFrame)d).OnIsSlideEnabledChanged();
        }

        protected virtual void OnIsSlideEnabledChanged()
        {
            if (_mainPanel != null)
                _mainPanel.IsSlideEnabled = IsSlideEnabled;
        }

        #endregion


        #region SidePanelWidth (DependencyProperty)

        /// <summary>
        /// Side panels width
        /// </summary>
        public double SidePanelWidth
        {
            get { return (double)GetValue(SidePanelWidthProperty); }
            set { SetValue(SidePanelWidthProperty, value); }
        }
        public static readonly DependencyProperty SidePanelWidthProperty =
            DependencyProperty.Register("SidePanelWidth", typeof(double), typeof(SlideApplicationFrame),
              new PropertyMetadata(400d));

        #endregion


        #region HideHeader (Attached DependencyProperty)

        public static readonly DependencyProperty HideHeaderProperty =
            DependencyProperty.RegisterAttached("HideHeader", typeof(bool), typeof(SlideApplicationFrame), new PropertyMetadata(false, OnHideHeaderChanged));

        public static void SetHideHeader(DependencyObject o, bool value)
        {
            o.SetValue(HideHeaderProperty, value);
        }

        public static bool GetHideHeader(DependencyObject o)
        {
            return (bool)o.GetValue(HideHeaderProperty);
        }

        private static void OnHideHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO
        }

        #endregion

        #region AutoHideAppBar (DependencyProperty)

        /// <summary>
        /// If true, hides the application bar when side panels are displayed, only appears on the central panel
        /// </summary>
        public bool AutoHideAppBar
        {
            get { return (bool)GetValue(AutoHideAppBarProperty); }
            set { SetValue(AutoHideAppBarProperty, value); }
        }
        public static readonly DependencyProperty AutoHideAppBarProperty =
            DependencyProperty.Register("AutoHideAppBar", typeof(bool), typeof(SlideApplicationFrame),
              new PropertyMetadata(true));

        #endregion

        #region IsPageTransitionEnabled (DependencyProperty)

        /// <summary>
        /// If true, enables page transitions, otherwise disables them
        /// </summary>
        public bool IsPageTransitionEnabled
        {
            get { return (bool)GetValue(IsPageTransitionEnabledProperty); }
            set { SetValue(IsPageTransitionEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsPageTransitionEnabledProperty =
            DependencyProperty.Register("IsPageTransitionEnabled", typeof(bool), typeof(SlideApplicationFrame),
              new PropertyMetadata(true));

        #endregion

        #region SelectedPanelIndex (DependencyProperty)

        /// <summary>
        /// Index for the selected panel
        /// </summary>
        public int SelectedPanelIndex
        {
            get { return (int)GetValue(SelectedPanelIndexProperty); }
            set { SetValue(SelectedPanelIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedPanelIndexProperty =
            DependencyProperty.Register("SelectedPanelIndex", typeof(int), typeof(SlideApplicationFrame),
            new PropertyMetadata(IndexCenterPanel, new PropertyChangedCallback(OnSelectedPanelIndexChanged)));

        private static void OnSelectedPanelIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideApplicationFrame)d).OnSelectedPanelIndexChanged(e);
        }

        protected virtual void OnSelectedPanelIndexChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_mainPanel == null)
                return;

            if (_mainPanel.SelectedIndex != SelectedPanelIndex)
                _mainPanel.SelectedIndex = SelectedPanelIndex;

#if WINDOWS_PHONE
            if (AutoHideAppBar && Content is PhoneApplicationPage)
            {
                var page = (PhoneApplicationPage)Content;
                if (page.ApplicationBar != null)
                {
                    if (SelectedPanelIndex != IndexCenterPanel)
                    {
                        _appBarVisibility = page.ApplicationBar.IsVisible;
                        page.ApplicationBar.IsVisible = false;
                    }
                    else if (_appBarVisibility.HasValue)
                    {
                        page.ApplicationBar.IsVisible = _appBarVisibility.Value;
                    }
                }
            }
#elif NETFX_CORE
            var page = Content as Page;
            if (AutoHideAppBar && page != null)
            {
                if (page.BottomAppBar != null)
                {
                    if (SelectedPanelIndex != IndexCenterPanel)
                    {
                        _appBarVisibility = page.BottomAppBar.Visibility == Visibility.Visible;
                        page.BottomAppBar.Visibility = Visibility.Collapsed;
                    }
                    else if (_appBarVisibility.HasValue)
                    {
                        page.BottomAppBar.Visibility = _appBarVisibility.Value ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
#endif
        }

        #endregion

        private SlideView _mainPanel;
        private Panel _header;
        private Image _previousPageCache;
        private Storyboard _pageTransitionForward;
        private Storyboard _pageTransitionBackward;
        private bool? _appBarVisibility;

        public SlideApplicationFrame()
        {
            DefaultStyleKey = typeof(SlideApplicationFrame);

            Navigating += SlideApplicationFrame_Navigating;
            Navigated += SlideApplicationFrame_Navigated;

#if WINDOWS_PHONE
            BackKeyPress += SlideApplicationFrame_BackKeyPress;
#elif NETFX_CORE
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }

#if WINDOWS_PHONE
        void SlideApplicationFrame_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Pressing back displays the center panel
            if (SelectedPanelIndex != IndexCenterPanel)
            {
                e.Cancel = true;
                SelectedPanelIndex = IndexCenterPanel;
            }
        }
#elif NETFX_CORE
        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            // Pressing back displays the center panel
            if (SelectedPanelIndex != IndexCenterPanel)
            {
                e.Handled = true;
                SelectedPanelIndex = IndexCenterPanel;
            }
        }
#endif

        void SlideApplicationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            _appBarVisibility = null;

            if (_header != null)
            {
                var content = e.Content as DependencyObject;
                if (content != null)
                {
                    var hide = GetHideHeader(content);
                    ToggleHeaderVisibility(hide);
                }
            }
        }

        void SlideApplicationFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward
                || e.NavigationMode == NavigationMode.Back
                || e.NavigationMode == NavigationMode.New)
                BeginTransition(e.NavigationMode != NavigationMode.Back);

            // Every time we navigate, we animate to the center panel
            if (_mainPanel != null)
                SelectedPanelIndex = IndexCenterPanel;
        }

#if WINDOWS_PHONE
        public
#elif NETFX_CORE
        protected 
#endif
            override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _header = GetTemplateChild(HeaderName) as Panel;

#if NETFX_CORE
            SidePanelWidth = Window.Current.Bounds.Width * 0.85;
#endif

            _mainPanel = GetTemplateChild(SlideViewName) as SlideView;

            #region allowing to use only one slide panel

            //NOTE [Harry,20140925] this is more of a hack. Maybe fix it to work better?

            var rightContentGrid = GetTemplateChild("RightContetGrid") as Grid;
            var leftContentGrid = GetTemplateChild("LeftContentGrid") as Grid;

            if (RightContent == null)
                rightContentGrid.Width = 0;
            if (LeftContent == null)
            {
                leftContentGrid.Width = 0;
                //set the index to 0
                IndexCenterPanel = 0;
                SelectedPanelIndex = IndexCenterPanel;
            }

            #endregion

            _mainPanel.SelectionChanged += MainPanel_SelectionChanged;

            _previousPageCache = GetTemplateChild(PageCacheName) as Image;

            var root = VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
            if (root != null)
            {
                _pageTransitionForward = root.Resources[PageTransitionForwardName] as Storyboard;
                if (_pageTransitionForward != null)
                    _pageTransitionForward.Completed += PageTransition_Completed;
                else
                    IsPageTransitionEnabled = false;

                _pageTransitionBackward = root.Resources[PageTransitionBackwardName] as Storyboard;
                if (_pageTransitionBackward != null)
                    _pageTransitionBackward.Completed += PageTransition_Completed;
                else
                    IsPageTransitionEnabled = false;
            }

            OnIsSlideEnabledChanged();
        }

        void MainPanel_SelectionChanged(object sender, EventArgs e)
        {
            SelectedPanelIndex = _mainPanel.SelectedIndex;

            if (SelectedPanelIndexChanged != null)
                SelectedPanelIndexChanged(this, EventArgs.Empty);
        }

#if WINDOWS_PHONE
        void PageTransition_Completed(object sender, EventArgs e)
#elif NETFX_CORE
        void PageTransition_Completed(object sender, object e)
#endif
        {
            _pageTransitionForward.Stop();
            _pageTransitionBackward.Stop();

            if (_previousPageCache != null)
                _previousPageCache.Source = null;
        }

        private void BeginTransition(bool isForward)
        {
            if (!IsPageTransitionEnabled)
                return;

            if (_previousPageCache != null)
            {
                //// Take a screenshot of the previous page to animate the transition
                //var bitmap = new WriteableBitmap(Content as UIElement, null);
                //_previousPageCache.Source = bitmap;
            }

            if (isForward && _pageTransitionForward != null)
            {
                _pageTransitionForward.Begin();
            }

            if (!isForward && _pageTransitionBackward != null)
            {
                _pageTransitionBackward.Begin();
            }
        }

        /// <summary>
        /// Toggle the header visibility
        /// </summary>
        /// <param name="show">True to show the header, false otherwise</param>
        public void ToggleHeaderVisibility(bool hide)
        {
            if (_header != null)
            {
                _header.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
