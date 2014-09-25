using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlideView.Library.Primitives;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
#elif NETFX_CORE
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Input;
#endif


namespace SlideView.Library
{
    [TemplatePart(Name = SlidingTransformName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = ItemsListName, Type = typeof(ItemsPresenter))]
    public class SlideView : ItemsControl
    {
        private const string SlidingTransformName = "SlidingTransform";
        private const string ItemsListName = "ItemsList";
#if WINDOWS_PHONE
        private const double FlickVelocity = 1000.0;
#elif NETFX_CORE
        private const double FlickVelocity = 1.0;
#endif

        #region SelectedIndex (DependencyProperty)

        /// <summary>
        /// Index for the selected children
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SlideView),
            new PropertyMetadata(0, new PropertyChangedCallback(OnSelectedIndexChanged)));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SlideView)d).OnSelectedIndexChanged(e);
        }

        protected virtual void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e)
        {
            GoToPanel(SelectedIndex);
        }

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
            DependencyProperty.Register("IsSlideEnabled", typeof(bool), typeof(SlideView),
              new PropertyMetadata(true));

        #endregion

        internal SlideViewPanel Panel { get; set; }

        public event EventHandler SelectionChanged;

        internal double ViewportWidth { get; private set; }
        internal double ViewportHeight { get; private set; }

        private ItemsPresenter _itemsList;
        private TranslateTransform _translate;
        private bool _suppressAnimations;

        private Storyboard _sidePanelStoryboard;
        private DoubleAnimation _sidePanelAnimation;

        public SlideView()
        {
            DefaultStyleKey = typeof(SlideView);

            _suppressAnimations = true;

#if NETFX_CORE
            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateRailsX | ManipulationModes.TranslateInertia;
            this.ManipulationStarted += SlideView_ManipulationStarted;
#endif

            this.ManipulationCompleted += SlideView_ManipulationCompleted;
            this.ManipulationDelta += SlideView_ManipulationDelta;
        }

#if WINDOWS_PHONE
        public
#elif NETFX_CORE
        protected
#endif
 override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _suppressAnimations = false;

            _itemsList = GetTemplateChild(ItemsListName) as ItemsPresenter;
#if WINDOWS_PHONE
            _itemsList.CacheMode = new BitmapCache();
#endif
            _translate = GetTemplateChild(SlidingTransformName) as TranslateTransform;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
#if WINDOWS_PHONE
            var appSize = new Size(Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
#elif NETFX_CORE
            var appSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
#endif

            if (appSize.Width > 0)
            {
                ViewportWidth = !double.IsInfinity(availableSize.Width) ? availableSize.Width : appSize.Width;
                ViewportHeight = !double.IsInfinity(availableSize.Height) ? availableSize.Height : appSize.Height;
            }
            else
            {
                ViewportWidth = Math.Min(availableSize.Width, 480);
                ViewportHeight = Math.Min(availableSize.Height, 800);
            }

            base.MeasureOverride(new Size(double.PositiveInfinity, ViewportHeight));

            return new Size(ViewportWidth, ViewportHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize.Width = DesiredSize.Width;
            base.ArrangeOverride(finalSize);

            if (SelectedIndex > 0)
                _translate.X = Panel.GetOffset(SelectedIndex);

            return finalSize;
        }

#if NETFX_CORE
        private void SlideView_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs manipulationStartedEventArgs)
        {
            if (_itemsList.CacheMode == null)
                _itemsList.CacheMode = new BitmapCache();
        }
#endif

#if WINDOWS_PHONE
        void SlideView_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
#elif NETFX_CORE
        void SlideView_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
#endif
        {
            if (!IsSlideEnabled)
            {
                e.Complete();
                return;
            }

#if WINDOWS_PHONE
            var deltaManipulation = e.DeltaManipulation;
#elif NETFX_CORE
            var deltaManipulation = e.Delta;
#endif

            var rightOffset = Panel.GetOffset(SelectedIndex + 1);
            var leftOffset = Panel.GetOffset(SelectedIndex - 1);

            double offset = Math.Max(rightOffset, Math.Min(leftOffset, _translate.X + deltaManipulation.Translation.X));
            _translate.X = offset;

#if NETFX_CORE
            if (e.IsInertial)
            {
                e.Complete();
            }
#endif
        }

#if WINDOWS_PHONE
        void SlideView_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (e.FinalVelocities != null)
            {
                var finalVelocities = e.FinalVelocities.LinearVelocity;
                CompleteManipulation(finalVelocities);
            }
        }
#elif NETFX_CORE
        void SlideView_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!IsSlideEnabled)return;

            var finalVelocities = e.Velocities.Linear;
            CompleteManipulation(finalVelocities);
        }
#endif

        private void CompleteManipulation(Point finalVelocities)
        {
            if (Math.Abs(finalVelocities.X) > FlickVelocity)
            {
                if (finalVelocities.X < 0)
                {
                    GoToPanel(SelectedIndex + 1);
                }
                else
                {
                    GoToPanel(SelectedIndex - 1);
                }

                return;
            }

            var currentOffset = Panel.GetOffset(SelectedIndex) - _translate.X;
            var leftThreshold = (Panel.GetOffset(SelectedIndex) - Panel.GetOffset(SelectedIndex - 1)) / 2;
            var rightThreshold = (Panel.GetOffset(SelectedIndex) - Panel.GetOffset(SelectedIndex + 1)) / 2;

            if (currentOffset > 0 && currentOffset > rightThreshold)
            {
                GoToPanel(SelectedIndex + 1);
            }
            else if (currentOffset < 0 && currentOffset < leftThreshold)
            {
                GoToPanel(SelectedIndex - 1);
            }
            else
            {
                GoToPanel(SelectedIndex);
            }
        }

        private void GoToPanel(int index)
        {
            if (_suppressAnimations)
                return;

            _suppressAnimations = true;

            index = Math.Max(0, Math.Min(Panel.Children.Count - 1, index));
            var offset = Panel.GetOffset(index);

            SelectedIndex = index;

            if (offset != _translate.X)
                AnimateSidePanel(offset);
            else
                GoToPanelCompleted();
        }

        public void AnimateSidePanel(double offset)
        {
            if (_sidePanelStoryboard == null)
            {
                _sidePanelAnimation = new DoubleAnimation
                {
                    EasingFunction = new QuadraticEase(),
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                Storyboard.SetTarget(_sidePanelAnimation, _translate);
#if WINDOWS_PHONE
                Storyboard.SetTargetProperty(_sidePanelAnimation, new PropertyPath(TranslateTransform.XProperty));
#elif NETFX_CORE
                Storyboard.SetTargetProperty(_sidePanelAnimation, "X");
#endif

                _sidePanelStoryboard = new Storyboard();
                _sidePanelStoryboard.Duration = TimeSpan.FromMilliseconds(200);

                _sidePanelStoryboard.Completed += SidePanelStoryboard_Completed;

                _sidePanelStoryboard.Children.Add(_sidePanelAnimation);
            }

#if NETFX_CORE
            if (_itemsList.CacheMode == null)
                _itemsList.CacheMode = new BitmapCache();
#endif

            _sidePanelAnimation.From = _translate.X;
            _sidePanelAnimation.To = offset;
            _sidePanelStoryboard.Begin();
        }

#if WINDOWS_PHONE
        void SidePanelStoryboard_Completed(object sender, EventArgs e)
#elif NETFX_CORE
        void SidePanelStoryboard_Completed(object sender, object e)
#endif
        {
            GoToPanelCompleted();
        }

        private void GoToPanelCompleted()
        {
            _suppressAnimations = false;

#if NETFX_CORE
            _itemsList.CacheMode = null;
#endif

            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }
    }
}
