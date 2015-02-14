#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

#endregion

namespace Audiotica
{
    internal class ZoomOutTransition : PageTransition
    {
        public override void Play(Action completed)
        {
            var storyboard = new Storyboard();
            var fromPage = FromPage;
            var scaleTransform = new ScaleTransform
            {
                CenterX = FromPage.ActualWidth/2,
                CenterY = FromPage.ActualHeight/2,
                ScaleX = 1,
                ScaleY = 1
            };
            fromPage.RenderTransform = scaleTransform;
            FromPage.Opacity = 1;
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            var doubleAnimationUsingKeyFrame = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame1 = doubleAnimationUsingKeyFrame;
            var keyFrames = doubleAnimationUsingKeyFrame1.KeyFrames;
            var easingDoubleKeyFrame = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            keyFrames.Add(easingDoubleKeyFrame);
            var doubleKeyFrameCollection = doubleAnimationUsingKeyFrame1.KeyFrames;
            var easingDoubleKeyFrame1 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 0.3,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection.Add(easingDoubleKeyFrame1);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame1, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame1,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame1);
            var doubleAnimationUsingKeyFrame2 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame3 = doubleAnimationUsingKeyFrame2;
            var keyFrames1 = doubleAnimationUsingKeyFrame3.KeyFrames;
            var easingDoubleKeyFrame2 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            keyFrames1.Add(easingDoubleKeyFrame2);
            var doubleKeyFrameCollection1 = doubleAnimationUsingKeyFrame3.KeyFrames;
            var easingDoubleKeyFrame3 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 0.3,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection1.Add(easingDoubleKeyFrame3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame3, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame3,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame3);
            var doubleAnimationUsingKeyFrame4 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame5 = doubleAnimationUsingKeyFrame4;
            var keyFrames2 = doubleAnimationUsingKeyFrame5.KeyFrames;
            var easingDoubleKeyFrame4 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            keyFrames2.Add(easingDoubleKeyFrame4);
            var doubleKeyFrameCollection2 = doubleAnimationUsingKeyFrame5.KeyFrames;
            var easingDoubleKeyFrame5 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 0,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection2.Add(easingDoubleKeyFrame5);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame5, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame5, "(UIElement.Opacity)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame5);
            var toPage = ToPage;
            var scaleTransform1 = new ScaleTransform
            {
                CenterX = ToPage.ActualWidth/2,
                CenterY = ToPage.ActualHeight/2,
                ScaleX = 0,
                ScaleY = 0
            };
            toPage.RenderTransform = scaleTransform1;
            ToPage.Opacity = 1;
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            var doubleAnimationUsingKeyFrame6 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame7 = doubleAnimationUsingKeyFrame6;
            var keyFrames3 = doubleAnimationUsingKeyFrame7.KeyFrames;
            var easingDoubleKeyFrame6 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1.4,
                EasingFunction = new CubicEase()
            };
            keyFrames3.Add(easingDoubleKeyFrame6);
            var doubleKeyFrameCollection3 = doubleAnimationUsingKeyFrame7.KeyFrames;
            var easingDoubleKeyFrame7 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection3.Add(easingDoubleKeyFrame7);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame7, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame7,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame7);
            var doubleAnimationUsingKeyFrame8 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame9 = doubleAnimationUsingKeyFrame8;
            var keyFrames4 = doubleAnimationUsingKeyFrame9.KeyFrames;
            var easingDoubleKeyFrame8 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1.4,
                EasingFunction = new CubicEase()
            };
            keyFrames4.Add(easingDoubleKeyFrame8);
            var doubleKeyFrameCollection4 = doubleAnimationUsingKeyFrame9.KeyFrames;
            var easingDoubleKeyFrame9 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection4.Add(easingDoubleKeyFrame9);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame9, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame9,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame9);
            var doubleAnimationUsingKeyFrame10 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame11 = doubleAnimationUsingKeyFrame10;
            var keyFrames5 = doubleAnimationUsingKeyFrame11.KeyFrames;
            var easingDoubleKeyFrame10 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 0.1,
                EasingFunction = new CubicEase()
            };
            keyFrames5.Add(easingDoubleKeyFrame10);
            var doubleKeyFrameCollection5 = doubleAnimationUsingKeyFrame11.KeyFrames;
            var easingDoubleKeyFrame11 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection5.Add(easingDoubleKeyFrame11);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame11, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame11, "(UIElement.Opacity)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame11);

            try
            {
                storyboard.Begin();
                if (completed != null)
                {
                    completed.Invoke();
                }
            }
            catch
            {
                base.Play(completed);
            }
        }

        public override void PlayReverse(Action completed)
        {
            var storyboard = new Storyboard();
            var toPage = ToPage;
            var scaleTransform = new ScaleTransform
            {
                CenterX = ToPage.ActualWidth/2,
                CenterY = ToPage.ActualHeight/2,
                ScaleX = 1,
                ScaleY = 1
            };
            toPage.RenderTransform = scaleTransform;
            ToPage.Opacity = 1;
            storyboard.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            var doubleAnimationUsingKeyFrame = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame1 = doubleAnimationUsingKeyFrame;
            var keyFrames = doubleAnimationUsingKeyFrame1.KeyFrames;
            var easingDoubleKeyFrame = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            keyFrames.Add(easingDoubleKeyFrame);
            var doubleKeyFrameCollection = doubleAnimationUsingKeyFrame1.KeyFrames;
            var easingDoubleKeyFrame1 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1.4,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection.Add(easingDoubleKeyFrame1);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame1, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame1,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame1);
            var doubleAnimationUsingKeyFrame2 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame3 = doubleAnimationUsingKeyFrame2;
            var keyFrames1 = doubleAnimationUsingKeyFrame3.KeyFrames;
            var easingDoubleKeyFrame2 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            keyFrames1.Add(easingDoubleKeyFrame2);
            var doubleKeyFrameCollection1 = doubleAnimationUsingKeyFrame3.KeyFrames;
            var easingDoubleKeyFrame3 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1.4,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection1.Add(easingDoubleKeyFrame3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame3, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame3,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame3);
            var doubleAnimationUsingKeyFrame4 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame5 = doubleAnimationUsingKeyFrame4;
            var keyFrames2 = doubleAnimationUsingKeyFrame5.KeyFrames;
            var easingDoubleKeyFrame4 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 1,
                EasingFunction = new CubicEase()
            };
            keyFrames2.Add(easingDoubleKeyFrame4);
            var doubleKeyFrameCollection2 = doubleAnimationUsingKeyFrame5.KeyFrames;
            var easingDoubleKeyFrame5 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 0,
                EasingFunction = new CubicEase()
            };
            doubleKeyFrameCollection2.Add(easingDoubleKeyFrame5);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame5, ToPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame5, "(UIElement.Opacity)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame5);
            var fromPage = FromPage;
            var scaleTransform1 = new ScaleTransform
            {
                CenterX = FromPage.ActualWidth/2,
                CenterY = FromPage.ActualHeight/2,
                ScaleX = 1,
                ScaleY = 1
            };
            fromPage.RenderTransform = scaleTransform1;
            FromPage.Opacity = 1;
            var doubleAnimationUsingKeyFrame6 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame7 = doubleAnimationUsingKeyFrame6;
            var keyFrames3 = doubleAnimationUsingKeyFrame7.KeyFrames;
            var easingDoubleKeyFrame6 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 0.3,
                EasingFunction = new SineEase()
            };
            keyFrames3.Add(easingDoubleKeyFrame6);
            var doubleKeyFrameCollection3 = doubleAnimationUsingKeyFrame7.KeyFrames;
            var easingDoubleKeyFrame7 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection3.Add(easingDoubleKeyFrame7);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame7, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame7,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame7);
            var doubleAnimationUsingKeyFrame8 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame9 = doubleAnimationUsingKeyFrame8;
            var keyFrames4 = doubleAnimationUsingKeyFrame9.KeyFrames;
            var easingDoubleKeyFrame8 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 0.3,
                EasingFunction = new SineEase()
            };
            keyFrames4.Add(easingDoubleKeyFrame8);
            var doubleKeyFrameCollection4 = doubleAnimationUsingKeyFrame9.KeyFrames;
            var easingDoubleKeyFrame9 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection4.Add(easingDoubleKeyFrame9);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame9, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame9,
                "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame9);
            var doubleAnimationUsingKeyFrame10 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(1)),
                FillBehavior = FillBehavior.HoldEnd
            };
            var doubleAnimationUsingKeyFrame11 = doubleAnimationUsingKeyFrame10;
            var keyFrames5 = doubleAnimationUsingKeyFrame11.KeyFrames;
            var easingDoubleKeyFrame10 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = 0,
                EasingFunction = new SineEase()
            };
            keyFrames5.Add(easingDoubleKeyFrame10);
            var doubleKeyFrameCollection5 = doubleAnimationUsingKeyFrame11.KeyFrames;
            var easingDoubleKeyFrame11 = new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15)),
                Value = 1,
                EasingFunction = new SineEase()
            };
            doubleKeyFrameCollection5.Add(easingDoubleKeyFrame11);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrame11, FromPage);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrame11, "(UIElement.Opacity)");
            storyboard.Children.Add(doubleAnimationUsingKeyFrame11);

            try
            {
                storyboard.Begin();
                if (completed != null)
                {
                    completed.Invoke();
                }
            }
            catch
            {
                base.Play(completed);
            }
        }
    }
}