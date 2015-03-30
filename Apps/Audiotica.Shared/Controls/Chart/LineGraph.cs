using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Audiotica.Controls.Chart
{
    public sealed partial class LineGraph : Canvas
    {
        private readonly Queue<Brush> _colors;
        private readonly IList<Serie> _series;
        private readonly IList<UIElement> _xDataMarkers;
        private readonly IList<UIElement> _yDataMarkers;
        private double _drawAreaBottom, _drawAreaLeft, _divisionX, _divisionY;
        private bool _loaded;
        private double _seriesMinX, _seriesMaxX, _seriesMinY, _seriesMaxY;
        private UIElement _xGuideLines, _yGuideLines;

        public LineGraph()
        {
            _colors = new Queue<Brush>();
            _colors.Enqueue(new SolidColorBrush(Colors.White));
            _colors.Enqueue(new SolidColorBrush(Colors.Blue));
            _colors.Enqueue(new SolidColorBrush(Colors.Green));
            _colors.Enqueue(new SolidColorBrush(Colors.Yellow));
            _colors.Enqueue(new SolidColorBrush(Colors.Purple));

            _xDataMarkers = new List<UIElement>();
            _yDataMarkers = new List<UIElement>();

            _series = new List<Serie>();

            GuideLineColor = new SolidColorBrush(Colors.LightGray);
            GuideTextColor = new SolidColorBrush(Colors.Black);

            XGuideLineResolution = 100;
            YGuideLineResolution = 100;

            GuideTextSize = 11;

            GuideLineThickness = 1;

            DrawAreaHorizontalOffset = 40;
            DrawAreaVerticalOffset = 40;

            _loaded = false;

            AutoRedraw = true;

            Loaded += delegate
            {
                _loaded = true;
                this.Draw();
            };
        }

        public void AddSerie(Serie s)
        {
            if (s != null)
            {
                s.Color = _colors.Dequeue();
                _series.Add(s);
                s.DataUpdated += SerieDataUpdated;

                if (_loaded)
                {
                    Draw();
                }
            }
        }

        private void SerieDataUpdated(Object sender, RoutedEventArgs e)
        {
            if (AutoRedraw)
            {
                Draw();
            }
        }

        private void Draw()
        {
            var dataIsAvailable = false;

            _seriesMinY = Double.NaN;
            _seriesMaxY = Double.NaN;
            _seriesMinX = Double.NaN;
            _seriesMaxX = Double.NaN;

            // Get and set the MIN and MAX of all series
            foreach (var serie in _series)
            {
                if (serie.Data.Count() > 1)
                {
                    dataIsAvailable = true;
                }

                if (Double.IsNaN(_seriesMinY) || serie.MinY < _seriesMinY)
                {
                    _seriesMinY = serie.MinY;
                }

                if (Double.IsNaN(_seriesMaxY) || serie.MaxY > _seriesMaxY)
                {
                    _seriesMaxY = serie.MaxY;
                }

                if (Double.IsNaN(_seriesMinX) || serie.MinX < _seriesMinX)
                {
                    _seriesMinX = serie.MinX;
                }

                if (Double.IsNaN(_seriesMaxX) || serie.MaxX > _seriesMaxX)
                {
                    _seriesMaxX = serie.MaxX;
                }
            }

            if (dataIsAvailable)
            {
                foreach (var serie in _series)
                {
                    if (serie.Data.Count() > 1)
                    {
                        DrawLine(serie);
                    }
                }

                if (DrawXGuideLines)
                {
                    DrawXLines();
                }

                if (DrawYGuideLines)
                {
                    DrawYLines();
                }
            }
        }

        private void DrawLine(Serie serie)
        {
            //this.InvalidateMeasure(); not sure why anymore...

            RemoveLine(serie);

            var height = ActualHeight - (2*DrawAreaVerticalOffset);
            var width = ActualWidth - (2*DrawAreaHorizontalOffset);

            _drawAreaBottom = 0 + height + DrawAreaVerticalOffset;
                // bottom left of drawing area, add 1 so the offset gets added below
            _drawAreaLeft = 0 + DrawAreaHorizontalOffset; // left of drawing area, add 1 so the offset gets added left

            _divisionX = (width/(_seriesMaxX - _seriesMinX));
            Debug.WriteLine("{0} : {1} / ({2} - {3}) = {4}", Name, width, _seriesMaxX, _seriesMinX, _divisionX);
            _divisionY = (height/(_seriesMaxY - _seriesMinY));

            var myPath = new Path();
            var myPathFigure = new PathFigure();
            var myPathSegmentCollection = new PathSegmentCollection();
            var myPathFigureCollection = new PathFigureCollection();
            var myPathGeometry = new PathGeometry();

            Point p;
            // prepare for drawing by modifying y value
            for (var i = 0; i < serie.Data.Count; i++) // for every datapoint in the serie
            {
                p = serie.Data[i];
                p.X = _drawAreaLeft + (_divisionX*i);
                p.Y = _drawAreaBottom - ((p.Y - _seriesMinY)*_divisionY);
                // data[i] = p; NO - Keep serie data unchanged

                Debug.WriteLine("{0} : ({1}) * {2} = {3}", Name, serie.Data[i], i, p.X);

                if (i == 0)
                {
                    myPathFigure.StartPoint = p;
                }
                else
                {
                    var segment = new LineSegment {Point = p};

                    myPathSegmentCollection.Add(segment);
                }
            }

            if (Fill) // not fully tested
            {
                p.Y = _drawAreaBottom - 0;
                var endsegment = new LineSegment {Point = p};
                myPathSegmentCollection.Add(endsegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;
            myPathFigureCollection.Add(myPathFigure);
            myPathGeometry.Figures = myPathFigureCollection;

            myPath.Stroke = serie.Color;
            myPath.StrokeThickness = 2;
            if (Fill)
            {
                myPath.Fill = serie.Color;
            }

            myPath.Data = myPathGeometry;
            SetZIndex(myPath, 10);

            Children.Add(myPath);
            serie.UiElement = myPath;
        }

        private void DrawXLines()
        {
            RemoveGuidlines(GuideLines.X);

            var width = ActualWidth; // just in case it changes?
            var height = ActualHeight;

            var loops = (int) width/XGuideLineResolution; // amount of x info

            /*
            if ((width / this.data.Count) > this.xResolution) // if there are less data points than the resolution, show the absolute data points
            {
                loops = this.data.Count;
            }
            */

            for (var i = 1; i < loops; i++)
            {
                var x = width/(loops - 1)*i;

                // text
                var length = _seriesMaxX - _seriesMinX;
                var interval = length/(loops - 1);
                //

                var tb = new TextBlock
                {
                    Text = Math.Round(_seriesMinX + (i*interval), 1).ToString(),
                    Foreground = GuideTextColor,
                    FontSize = GuideTextSize
                };

                tb.Margin = new Thickness(x - (tb.Text.Length/2*6), height - 20, 0, 20);

                var p = new Path();
                var geo = new LineGeometry {StartPoint = new Point(x, 20), EndPoint = new Point(x, ActualHeight - 20)};
                p.Stroke = GuideLineColor;
                p.StrokeDashArray = new DoubleCollection {5, 20};
                p.StrokeThickness = GuideLineThickness;
                p.Data = geo;

                if (ShowText)
                    _xDataMarkers.Add(tb);
                _xGuideLines = p;

                if (ShowText)
                    SetZIndex(tb, 15);
                SetZIndex(p, 14);

                if (ShowText)
                    Children.Add(tb);
                Children.Add(p);
            }
        }

        private void DrawYLines()
        {
            RemoveGuidlines(GuideLines.Y);

            var height = ActualHeight;
            var loops = (int) height/YGuideLineResolution; // amount of Y info

            for (var i = 1; i < loops; i++)
            {
                // text
                var length = _seriesMaxY - _seriesMinY;
                var interval = length/(loops - 1);
                //

                var tb = new TextBlock
                {
                    Text = Math.Round(_seriesMinY + (i*interval), 1).ToString(),
                    Foreground = GuideTextColor,
                    FontSize = GuideTextSize,
                    Margin = new Thickness(5, _drawAreaBottom - (i*(ActualHeight/6)) - 5, 0, 0)
                };

                var p = new Path();
                var geo = new LineGeometry
                {
                    StartPoint = new Point(25, _drawAreaBottom - (i*(ActualHeight/6))),
                    EndPoint = new Point(ActualWidth - 25, _drawAreaBottom - (i*(ActualHeight/6)))
                };
                p.Stroke = GuideLineColor;
                p.StrokeDashArray = new DoubleCollection {5, 20};
                p.StrokeThickness = GuideLineThickness;
                p.Data = geo;

                if (ShowText)
                    _yDataMarkers.Add(tb);
                _yGuideLines = p;

                if (ShowText)
                    SetZIndex(tb, 15);
                SetZIndex(p, 14);

                if (ShowText)
                    Children.Add(tb);
                Children.Add(p);
            }
        }

        public void RemoveSerie(Serie s)
        {
            RemoveLine(s);
            _series.Remove(s);
        }

        public void ClearSeries()
        {
            foreach (var series in _series)
            {
                RemoveLine(series);
            }
            _series.Clear();
        }

        private void RemoveLine(Serie s)
        {
            Children.Remove(s.UiElement);
        }

        private void RemoveGuidlines(GuideLines gl)
        {
            switch (gl)
            {
                case GuideLines.X:
                {
                    Children.Remove(_xGuideLines);

                    foreach (var tb in _xDataMarkers)
                    {
                        Children.Remove(tb);
                    }

                    _xDataMarkers.Clear();

                    break;
                }
                case GuideLines.Y:
                {
                    Children.Remove(_yGuideLines);

                    foreach (var tb in _yDataMarkers)
                    {
                        Children.Remove(tb);
                    }

                    _yDataMarkers.Clear();

                    break;
                }
                default:
                {
                    RemoveGuidlines(GuideLines.X);
                    RemoveGuidlines(GuideLines.Y);
                    break;
                }
            }
        }

        public void Redraw()
        {
            Draw();
        }

        private enum GuideLines
        {
            X,
            Y
        }
    }
}