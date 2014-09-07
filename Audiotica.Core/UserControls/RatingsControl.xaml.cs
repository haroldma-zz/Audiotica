#region

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#endregion

namespace Audiotica.Core.UserControls
{
    public sealed partial class RatingsControl : UserControl
    {
        /// <summary>
        ///     BackgroundColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (SolidColorBrush),
                typeof (RatingsControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnBackgroundColorChanged));

        /// <summary>
        ///     StarForegroundColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty StarForegroundColorProperty =
            DependencyProperty.Register("StarForegroundColor", typeof (SolidColorBrush),
                typeof (RatingsControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnStarForegroundColorChanged));

        /// <summary>
        ///     StarOutlineColor Dependency Property
        /// </summary>
        public static readonly DependencyProperty StarOutlineColorProperty =
            DependencyProperty.Register("StarOutlineColor", typeof (SolidColorBrush),
                typeof (RatingsControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnStarOutlineColorChanged));

        /// <summary>
        ///     Value Dependency Property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (double),
                typeof (RatingsControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent),
                    OnValueChanged));

        /// <summary>
        ///     NumberOfStars Dependency Property
        /// </summary>
        public static readonly DependencyProperty NumberOfStarsProperty =
            DependencyProperty.Register("NumberOfStars", typeof (Int32), typeof (RatingsControl),
                new PropertyMetadata(5,
                    OnNumberOfStarsChanged));

        public RatingsControl()
        {
            InitializeComponent();
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
            get
            {
                try
                {
                    return (double) GetValue(ValueProperty);
                }
                catch
                {
                    return 0.0;
                }
            }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the NumberOfStars property.
        /// </summary>
        public Int32 NumberOfStars
        {
            get { return (Int32) GetValue(NumberOfStarsProperty); }
            set { SetValue(NumberOfStarsProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the BackgroundColor property.
        /// </summary>
        private static void OnBackgroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (RatingsControl) d;
            foreach (Star star in control.spStars.Children)
                star.BackgroundColor = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the StarForegroundColor property.
        /// </summary>
        private static void OnStarForegroundColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (RatingsControl) d;
            foreach (Star star in control.spStars.Children)
                star.StarForegroundColor = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the StarOutlineColor property.
        /// </summary>
        private static void OnStarOutlineColorChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (RatingsControl) d;
            foreach (Star star in control.spStars.Children)
                star.StarOutlineColor = (SolidColorBrush) e.NewValue;
        }

        /// <summary>
        ///     Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var ratingsControl = (RatingsControl) d;
            SetupStars(ratingsControl);
        }

        /// <summary>
        ///     Handles changes to the NumberOfStars property.
        /// </summary>
        private static void OnNumberOfStarsChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var ratingsControl = (RatingsControl) d;
            SetupStars(ratingsControl);
        }

        /// <summary>
        ///     Sets up stars when Value or NumberOfStars properties change
        ///     Will only show up to the number of stars requested (up to Maximum)
        ///     so if Value > NumberOfStars * 1, then Value is clipped to maximum
        ///     number of full stars
        /// </summary>
        /// <param name="ratingsControl"></param>
        private static void SetupStars(RatingsControl ratingsControl)
        {
            var localValue = ratingsControl.Value;

            ratingsControl.spStars.Children.Clear();
            for (var i = 0; i < ratingsControl.NumberOfStars; i++)
            {
                var star = new Star
                {
                    BackgroundColor = ratingsControl.BackgroundColor,
                    StarForegroundColor = ratingsControl.StarForegroundColor,
                    StarOutlineColor = ratingsControl.StarOutlineColor
                };
                if (localValue > 1)
                    star.Value = 1.0;
                else if (localValue > 0)
                {
                    star.Value = localValue;
                }
                else
                {
                    star.Value = 0.0;
                }

                localValue -= 1.0;
                ratingsControl.spStars.Children.Insert(i, star);
            }
        }
    }
}