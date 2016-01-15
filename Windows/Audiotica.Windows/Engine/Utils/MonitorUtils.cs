using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace Audiotica.Windows.Engine.Utils
{
    public class MonitorUtils
    {
        public event EventHandler Changed;

        public InchesInfo Inches { get; private set; }

        public PixelsInfo Pixels { get; private set; }

        private MonitorUtils(WindowWrapper windowWrapper)
        {
            var di = windowWrapper.DisplayInformation();
            di.OrientationChanged += new WeakReference<MonitorUtils, DisplayInformation, object>(this)
            {
                EventAction = (i, s, e) => Changed?.Invoke(i, EventArgs.Empty),
                DetachAction = (i, w) => di.OrientationChanged -= w.Handler
            }.Handler;

            var av = windowWrapper.ApplicationView();
            av.VisibleBoundsChanged += new WeakReference<MonitorUtils, ApplicationView, object>(this)
            {
                EventAction = (i, s, e) => Changed?.Invoke(i, EventArgs.Empty),
                DetachAction = (i, w) => av.VisibleBoundsChanged -= w.Handler
            }.Handler;

            Inches = new InchesInfo(windowWrapper);
            Pixels = new PixelsInfo(windowWrapper);
        }

        #region singleton

        private static Dictionary<WindowWrapper, MonitorUtils> Cache = new Dictionary<WindowWrapper, MonitorUtils>();

        public static MonitorUtils Current(WindowWrapper windowWrapper = null)
        {
            windowWrapper = windowWrapper ?? WindowWrapper.Current();
            if (!Cache.ContainsKey(windowWrapper))
            {
                var item = new MonitorUtils(windowWrapper);
                Cache.Add(windowWrapper, item);
                windowWrapper.ApplicationView().Consolidated += new WeakReference<MonitorUtils, ApplicationView, object>(item)
                {
                    EventAction = (i, s, e) => Cache.Remove(windowWrapper),
                    DetachAction = (i, w) => windowWrapper.ApplicationView().Consolidated -= w.Handler
                }.Handler;
            }
            return Cache[windowWrapper];
        }

        #endregion singleton

        public class InchesInfo
        {
            private WindowWrapper WindowWrapper;

            public InchesInfo(WindowWrapper windowWrapper)
            {
                WindowWrapper = windowWrapper;
            }

            public double Height
            {
                get
                {
                    var rect = PointerDevice.GetPointerDevices().Last().PhysicalDeviceRect;
                    return rect.Height / 96;
                }
            }

            public double Width
            {
                get
                {
                    var rect = PointerDevice.GetPointerDevices().Last().PhysicalDeviceRect;
                    return rect.Width / 96;
                }
            }

            public double Diagonal
            {
                get
                {
                    return Math.Sqrt(Math.Pow(Height, 2) + Math.Pow(Width, 2));
                }
            }
        }

        public class PixelsInfo
        {
            private WindowWrapper WindowWrapper;

            public PixelsInfo(WindowWrapper windowWrapper)
            {
                WindowWrapper = windowWrapper;
            }

            public int Height
            {
                get
                {
                    var rect = PointerDevice.GetPointerDevices().Last().ScreenRect;
                    var scale = WindowWrapper.DisplayInformation().RawPixelsPerViewPixel;
                    return (int)(rect.Height * scale);
                }
            }

            public int Width
            {
                get
                {
                    var rect = PointerDevice.GetPointerDevices().Last().ScreenRect;
                    var scale = WindowWrapper.DisplayInformation().RawPixelsPerViewPixel;
                    return (int)(rect.Width * scale);
                }
            }

            public double Diagonal
            {
                get
                {
                    return Math.Sqrt(Math.Pow(Height, 2) + Math.Pow(Width, 2));
                }
            }
        }
    }
}