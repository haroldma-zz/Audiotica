#region

using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#endregion

namespace Audiotica.Core.UserControls
{
    public sealed partial class Star : UserControl
    {
        private const Int32 STAR_SIZE = 12;

        /// <summary>
        ///     BackgroundColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor",
                typeof (SolidColorBrush), typeof (Star),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnBackgroundColorChanged));

        /// <summary>
        ///     StarForegroundColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty StarForegroundColorProperty =
            DependencyProperty.Register("StarForegroundColor", typeof (SolidColorBrush),
                typeof (Star),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnStarForegroundColorChanged));

        /// <summary>
        ///     StarOutlineColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty StarOutlineColorProperty =
            DependencyProperty.Register("StarOutlineColor", typeof (SolidColorBrush),
                typeof (Star),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnStarOutlineColorChanged));

        /// <summary>
        ///     Value Dependency Property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (double),
                typeof (Star),
                new PropertyMetadata(0.0,
                    OnValueChanged));

        public Star()
        {
            DataContext = this;
            InitializeComponent();

            gdStar.Width = STAR_SIZE;
            gdStar.Height = STAR_SIZE;
            gdStar.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, STAR_SIZE, STAR_SIZE)
            };

            mask.Width = STAR_SIZE;
            mask.Height = STAR_SIZE;
            SizeChanged += Star_SizeChanged;
        }

        /// <summary>
        ///     Gets or sets the BackgroundColor property.
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the StarForegroundColor property.
        /// </summary>
        public SolidColorBrush StarForegroundColor
        {
            get { return (SolidColorBrush) GetValue(StarForegroundColorProperty); }
            set { SetValue(StarForegroundColorProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the StarOutlineColor property.
        /// </summary>
        public SolidColorBrush StarOutlineColor
        {
            get { return (SolidColorBrush) GetValue(StarOutlineColorProperty); }
            set { SetValue(StarOutlineColorProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the Value property.
        /// </summary>
        public double Value
        {
            get { return (double) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void Star_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        /// <summary>
        ///     Handles changes to the BackgroundColor property.
        /// </summary>
        private static void OnBackgroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Star) d;
            control.gdStar.Background = (SolidColorBrush) e.NewValue;
            control.mask.Fill = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the StarForegroundColor property.
        /// </summary>
        private static void OnStarForegroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Star) d;
            control.starForeground.Fill = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the StarOutlineColor property.
        /// </summary>
        private static void OnStarOutlineColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (Star) d;
            control.starOutline.Stroke = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var starControl = (Star) d;
            starControl.starForeground.Fill = Math.Abs(starControl.Value) <= 0 ? new SolidColorBrush(Colors.Gray) : starControl.StarForegroundColor;

            var marginLeftOffset = (Int32) (starControl.Value*STAR_SIZE);
            starControl.mask.Margin = new Thickness(marginLeftOffset, 0, 0, 0);
            starControl.InvalidateArrange();
            starControl.InvalidateMeasure();
        }
    }
}