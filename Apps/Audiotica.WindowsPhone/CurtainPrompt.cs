#region License

// Copyright (c) 2013 Harry
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
using System.Collections.Generic;
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

namespace Audiotica
{
    public class CurtainPrompt
    {
        private const int Height = 85 + 2;
        private const int PaddingPopup = 150;
        private const int MillisecondsToHide = 3000;
        private static CurtainPrompt current;
        private Popup _popup;
        private DispatcherTimer _timer;

        public CurtainPrompt(string msg, bool isError = false)
        {
            _popup = CreatePopup(msg, isError);
            ShowPopup();
        }

        private int viewStart
        {
            get { return (-2 - PaddingPopup); }
        }

        private int MaxView
        {
            get { return viewStart + PaddingPopup; }
        }

        public void Dismiss()
        {
            try
            {
                _popup.IsOpen = false;
                _popup = null;
                _timer.Stop();
                _timer = null;
                current = null;
            }
            catch
            {
            }
        }

        public static CurtainPrompt Show(string msg)
        {
            if (current != null)
                current.Dismiss();

            var curtain = new CurtainPrompt(msg);
            current = curtain;
            return curtain;
        }

        public static CurtainPrompt ShowError(string msg)
        {
            if (current != null)
                current.Dismiss();

            var curtain = new CurtainPrompt(msg, true);
            current = curtain;
            return curtain;
        }


        private Popup CreatePopup(string msg, bool isError)
        {
            var notification = new Popup();

            #region grid

            var grid = new Grid
            {
                Background = new SolidColorBrush(ColorHelper.GetColorFromHexa("#4B216D")),
                Height = Height + PaddingPopup,
                Width = Window.Current.Bounds.Width,
                IsHoldingEnabled = true,
                ManipulationMode = ManipulationModes.TranslateY
            };

            grid.ManipulationStarted += grid_ManipulationStarted;
            grid.ManipulationDelta += grid_ManipulationDelta;
            grid.ManipulationCompleted += grid_ManipulationCompleted;

            #endregion

            #region stackpanel

            var panel = new StackPanel
            {
                Margin = new Thickness(30, 0, 20, 10),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            #endregion

            #region text blocks

            var title = new TextBlock
            {
                Text = isError ? "" : "",
                FontWeight = FontWeights.Bold,
                FontSize = 30,
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Symbol")
            };
            var subMsg = new TextBlock
            {
                Text = msg,
                FontSize = 22,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White)
            };

            #endregion

            panel.Children.Add(title);
            panel.Children.Add(subMsg);
            grid.Children.Add(panel);

            notification.Child = grid;
            notification.IsOpen = true;
            notification.VerticalOffset = -(Height + PaddingPopup);

            return notification;
        }

        private void ShowPopup()
        {
            //Animate transition
            var slideDownAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = _popup.VerticalOffset,
                To = viewStart,
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
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(MillisecondsToHide)};
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

        private void timer_Tick(object sender, object e)
        {
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
                To = -(Height + PaddingPopup),
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
                    _popup.IsOpen = false;
                    _popup = null;
                    current = null;
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

            if (_popup.VerticalOffset >= MaxView)
                _popup.VerticalOffset = MaxView;
        }

        private void grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.Velocities.Linear.Y <= 0 || _popup.VerticalOffset >= viewStart + 25)
            {
                CompleteCurtainAnimation();
            }
            else
            {
                ShowPopup();
            }
        }
    }
}