using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Audiotica.Controls.Chart
{
    public sealed class Serie
    {
        private IList<Point> _data;

        public Serie(string title)
        {
            _data = new List<Point>();

            MinY = Double.NaN;
            MaxY = Double.NaN;

            MinX = Double.NaN;
            MaxX = Double.NaN;

            Title = title;

            ShiftSize = 100;
        }

        public string Title { private set; get; }
        public double MinY { private set; get; }
        public double MaxY { private set; get; }
        public double MinX { private set; get; }
        public double MaxX { private set; get; }
        public Path UiElement { get; set; }
        public Brush Color { get; set; }
        public int ShiftSize { get; set; }

        public IList<Point> Data
        {
            set { SetData(value); }
            get { return _data; }
        }

        public event RoutedEventHandler DataUpdated;

        public void SetData(IList<Point> data)
        {
            this._data = data;
            CalculateMinMax();

            OnDataUpdated(new RoutedEventArgs());
        }

        private void OnDataUpdated(RoutedEventArgs e)
        {
            if (DataUpdated != null)
            {
                DataUpdated(this, e);
            }
        }

        public void AddPoint(Point p, bool shift)
        {
            if (Data.Count == 0)
            {
                Data.Add(p);
                CalculateMinMax();
                OnDataUpdated(new RoutedEventArgs());
            }
            else if (p.X > Data.Last().X)
            {
                if (shift && Data.Count >= ShiftSize)
                {
                    var loops = Data.Count - ShiftSize;

                    for (var i = 0; i < loops; i++)
                    {
                        Debug.WriteLine("MIN X : {0}", Data[i]);
                        Data.RemoveAt(i);
                    }
                }

                Data.Add(p);
                CalculateMinMax();
                OnDataUpdated(new RoutedEventArgs());
            }

            Debug.WriteLine("MAX X : {0}", p);
        }

        private void CalculateMinMax()
        {
            MinY = Double.NaN;
            MaxY = Double.NaN;
            MinX = Double.NaN;
            MaxX = Double.NaN;

            foreach (var p in _data)
            {
                if (Double.IsNaN(MinY) || p.Y < MinY)
                {
                    MinY = p.Y;
                }
                if (Double.IsNaN(MaxY) || p.Y > MaxY)
                {
                    MaxY = p.Y;
                }
                if (Double.IsNaN(MinX) || p.X < MinX)
                {
                    MinX = p.X;
                }
                if (Double.IsNaN(MaxX) || p.X > MaxX)
                {
                    MaxX = p.X;
                }
            }
        }
    }
}