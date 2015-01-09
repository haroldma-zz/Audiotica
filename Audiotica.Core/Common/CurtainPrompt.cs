#region License

// Copyright (c) 2013 Harold Martinez-Molina <hanthonym@outlook.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using ColorHelper = Audiotica.Core.Utilities.ColorHelper;

#endregion

namespace Audiotica.Core.Common
{
    public class CurtainPrompt
    {
        private readonly Color _color;
        private const int PaddingPopup = 150;
        private int _millisecondsToHide = 1500;
        private static CurtainPrompt _current;
        private Popup _popup;
        private DispatcherTimer _timer;

        public CurtainPrompt(Color color, string msg, bool isError = false)
        {
            _color = color;
            CreatePopup(msg, isError);
            ShowPopup();
        }

        public void Dismiss()
        {
            try
            {
                if (_popup != null)
                    _popup.IsOpen = false;
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                _popup = null;
                _current = null;
            }
            catch
            {
            }
        }

        public static CurtainPrompt Show(string msg)
        {
            return Show(msg, null);
        }

        public static CurtainPrompt Show(string msg, params object[] args)
        {
            return Show(1500, msg, args);
        }

        public static CurtainPrompt Show(int milliToHide, string msg, params object[] args)
        {
            if (args != null)
            {
                msg = string.Format(msg, args);
            }

            if (_current != null)
                _current.Dismiss();

            var curtain = new CurtainPrompt(Colors.DarkGreen ,msg) {_millisecondsToHide = milliToHide};
            _current = curtain;
            return curtain;
        }

        public static CurtainPrompt ShowError(string msg)
        {
            return ShowError(msg, null);
        }

        public static CurtainPrompt ShowError(string msg, params object[] args)
        {
            return ShowError(2500, msg, args);
        }

        public static CurtainPrompt ShowError(int milliToHide, string msg, params object[] args)
        {
            if (args != null)
            {
                msg = string.Format(msg, args);
            }

            if (_current != null)
                _current.Dismiss();

            var curtain = new CurtainPrompt(Colors.DarkRed, msg, true) {_millisecondsToHide = milliToHide};
            _current = curtain;
            return curtain;
        }


        private void CreatePopup(string msg, bool isError)
        {
            _popup = new Popup {VerticalAlignment = VerticalAlignment.Top};

            #region grid

            var grid = new Grid
            {
                Background = new SolidColorBrush(_color),
                Width = Window.Current.Bounds.Width,
                IsHoldingEnabled = true,
                ManipulationMode = ManipulationModes.TranslateY,
                VerticalAlignment = VerticalAlignment.Top
            };

            grid.ManipulationStarted += grid_ManipulationStarted;
            grid.ManipulationDelta += grid_ManipulationDelta;
            grid.ManipulationCompleted += grid_ManipulationCompleted;

            #endregion

            #region stackpanel

            var panel = new Grid
            {
                Margin = new Thickness(30, PaddingPopup, 20, 20),
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            panel.ColumnDefinitions.Add(new ColumnDefinition{Width = GridLength.Auto});
            panel.ColumnDefinitions.Add(new ColumnDefinition());

            #endregion

            #region text blocks

            var title = new TextBlock
            {
                Text = isError ? "" : "",
                FontWeight = FontWeights.Bold,
                FontSize = 22,
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Symbol")
            };
            var subMsg = new TextBlock
            {
                Text = msg,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(subMsg, 1);

            #endregion

            panel.Children.Add(title);
            panel.Children.Add(subMsg);
            grid.Children.Add(panel);

            _popup.Child = grid;
            _popup.IsOpen = true;

            //Make the framework (re)calculate the size of the element
            grid.Measure(new Size(Double.MaxValue, Double.MaxValue));

            _popup.VerticalOffset = -grid.DesiredSize.Height;
        }

        private void ShowPopup()
        {
            //Animate transition
            var slideDownAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = _popup.VerticalOffset,
                To = -(PaddingPopup - 40),
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };

            var sb = new Storyboard();
            sb.Children.Add(slideDownAnimation);
            Storyboard.SetTarget(slideDownAnimation, _popup);
            Storyboard.SetTargetProperty(slideDownAnimation, "VerticalOffset");

            sb.Completed += slideDownAnimation_Completed;

            sb.Begin();
        }

        private void slideDownAnimation_Completed(object sender, object e)
        {
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(_millisecondsToHide)};
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

        private void timer_Tick(object sender, object e)
        {
            if (_timer == null) return;

            _timer.Stop();
            _timer = null;
            StartCurtainAnimation();
        }

        private void StartCurtainAnimation()
        {
            if (_popup == null)
                return;
            //Animate transition
            var verticalExtendAnimation = new DoubleAnimation
            {
                From = _popup.VerticalOffset,
                To = _popup.VerticalOffset + 25,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };

            var sbExtend = new Storyboard();
            sbExtend.Children.Add(verticalExtendAnimation);
            Storyboard.SetTarget(verticalExtendAnimation, _popup);
            Storyboard.SetTargetProperty(verticalExtendAnimation, "VerticalOffset");
            sbExtend.Completed += (s, e) => CompleteCurtainAnimation();
            sbExtend.Begin();
        }

        private void CompleteCurtainAnimation()
        {
            if (_popup == null) return;
            //Animate transition
            var verticalHideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = _popup.VerticalOffset,
                To = -(_popup.Child as Grid).ActualHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };

            var sbHide = new Storyboard();
            sbHide.Children.Add(verticalHideAnimation);
            Storyboard.SetTarget(verticalHideAnimation, _popup);
            Storyboard.SetTargetProperty(verticalHideAnimation, "VerticalOffset");
            sbHide.Completed += (s, h) =>
            {
                try
                {
                    Dismiss();
                }
                catch
                {
                }
            };
            sbHide.Begin();
        }


        private void grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            //Stop the timer
            try
            {
                _timer.Stop();
            }
            catch
            {
            }
        }

        private void grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _popup.VerticalOffset += e.Delta.Translation.Y;

            if (_popup.VerticalOffset >= 0)
                _popup.VerticalOffset = 0;
        }

        private void grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompleteCurtainAnimation();
        }
    }
}