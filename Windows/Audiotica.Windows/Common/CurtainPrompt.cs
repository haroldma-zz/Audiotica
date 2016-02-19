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

namespace Audiotica.Windows.Common
{
    public class CurtainPrompt
    {
        private const int PaddingPopup = 150;
        private static CurtainPrompt _current;
        private readonly Action _action;
        private readonly Color _color;
        private double _millisecondsToHide = 1500;
        private Popup _popup;
        private DispatcherTimer _timer;

        public CurtainPrompt(Color color, string msg, Action action, bool isError = false)
        {
            _color = color;
            _action = action;
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
                // ignored
            }
        }

        public static CurtainPrompt Show(string msg, Action action = null, TimeSpan? duration = null)
        {
            return InternalCreate(msg, action, duration?.TotalMilliseconds ?? 1500, Colors.DarkGreen);
        }

        public static CurtainPrompt ShowError(string msg, Action action = null, TimeSpan? duration = null)
        {
            return InternalCreate(msg, action, duration?.TotalMilliseconds ?? 2500, Colors.DarkRed);
        }

        private static CurtainPrompt InternalCreate(string msg, Action action, double milliseconds, Color color)
        {
            _current?.Dismiss();

            var curtain = new CurtainPrompt(color, msg, action, color == Colors.DarkRed)
            {
                _millisecondsToHide = milliseconds
            };

            _current = curtain;
            return curtain;
        }

        private void CreatePopup(string msg, bool isError)
        {
            _popup = new Popup
            {
                VerticalAlignment = VerticalAlignment.Top,
                RenderTransform = new TranslateTransform()
            };

            #region grid

            var grid = new Grid
            {
                Background = new SolidColorBrush(_color),
                Width = Window.Current.Bounds.Width,
                VerticalAlignment = VerticalAlignment.Top,
                ManipulationMode = ManipulationModes.TranslateY
            };

            grid.ManipulationStarted += grid_ManipulationStarted;
            grid.ManipulationDelta += grid_ManipulationDelta;
            grid.ManipulationCompleted += grid_ManipulationCompleted;
            grid.Tapped += GridOnTapped;

            #endregion

            #region stackpanel

            var panel = new Grid
            {
                Margin = new Thickness(30, PaddingPopup, 20, 20),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            panel.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
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
            grid.Measure(new Size(double.MaxValue, double.MaxValue));
        }

        private void grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _timer?.Stop();
        }

        private void grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var transform = (TranslateTransform) _popup.RenderTransform;
            if (transform == null)
            {
                return;
            }
            transform.Y += e.Delta.Translation.Y;

            if (transform.Y >= 0)
                transform.Y = 0;
        }

        private void grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            CompleteCurtainAnimation();
        }

        private void GridOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            CompleteCurtainAnimation();

            if (_action == null) return;

            try
            {
                _action();
            }
            catch
            {
                // ignored
            }
        }

        private void ShowPopup()
        {
            //Animate transition
            var slideDownAnimation = new DoubleAnimation
            {
                From = -_popup.Child.DesiredSize.Height,
                To = -(PaddingPopup - 40),
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new SineEase()
            };

            var sb = new Storyboard();
            sb.Children.Add(slideDownAnimation);
            Storyboard.SetTarget(slideDownAnimation, _popup);
            Storyboard.SetTargetProperty(slideDownAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

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
            CompleteCurtainAnimation();
        }

        private void CompleteCurtainAnimation()
        {
            if (_popup == null) return;
            //Animate transition
            var slideAnimation = new DoubleAnimation
            {
                From = ((TranslateTransform) _popup.RenderTransform)?.Y,
                To = -((Grid) _popup.Child).ActualHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                EasingFunction = new SineEase()
            };

            var sbHide = new Storyboard();
            sbHide.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, _popup);
            Storyboard.SetTargetProperty(slideAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");
            sbHide.Completed += (s, h) =>
            {
                try
                {
                    Dismiss();
                }
                catch
                {
                    // ignored
                }
            };
            sbHide.Begin();
        }
    }
}