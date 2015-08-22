using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    public static class ColorUtils
    {
        public static int Darken(int color, float fraction)
        {
            return BlendColors(unchecked ((int) ColorExtensions.Black), color, fraction);
        }

        public static int Lighten(int color, float fraction)
        {
            return BlendColors(unchecked((int) ColorExtensions.White), color, fraction);
        }

        /**
         * @return luma value according to to YIQ color space.
         */

        public static int CalculateYiqLuma(int color)
        {
            return
                (int)
                    Math.Round((299*ColorExtensions.Red(color) + 587*ColorExtensions.Green(color) +
                                114*ColorExtensions.Blue(color))/1000f);
        }

        /**
         * Blend {@code color1} and {@code color2} using the given ratio.
         *
         * @param ratio of which to blend. 1.0 will return {@code color1}, 0.5 will give an even blend,
         *              0.0 will return {@code color2}.
         */

        public static int BlendColors(int color1, int color2, float ratio)
        {
            var inverseRatio = 1f - ratio;
            var r = (ColorExtensions.Red(color1)*ratio) + (ColorExtensions.Red(color2)*inverseRatio);
            var g = (ColorExtensions.Green(color1)*ratio) + (ColorExtensions.Green(color2)*inverseRatio);
            var b = (ColorExtensions.Blue(color1)*ratio) + (ColorExtensions.Blue(color2)*inverseRatio);
            return ColorExtensions.Rgb((int) r, (int) g, (int) b);
        }

        public static int ChangeBrightness(int color, float fraction)
        {
            return CalculateYiqLuma(color) >= 128
                ? Darken(color, fraction)
                : Lighten(color, fraction);
        }

        public static int CalculateContrast(MedianCutQuantizer.ColorNode color1,
            MedianCutQuantizer.ColorNode color2)
        {
            return Math.Abs(CalculateYiqLuma(color1.GetRgb())
                            - CalculateYiqLuma(color2.GetRgb()));
        }

        public static double CalculateColorfulness(MedianCutQuantizer.ColorNode node)
        {
            var hsv = node.GetRgb().ToColor().ToHsv();
            var hsl = node.GetRgb().ToColor().ToHsl();
            if (IsWhite(hsl) || IsBlack(hsl))
                return 0;
            return hsv.S*hsv.V;
        }

        private const float BlackMaxLightness = 0.05f;
        private const float WhiteMinLightness = 0.95f;

        /**
         * @return true if the color represents a color which is close to black.
         */

        public static bool IsBlack(HslColor hslColor)
        {
            return hslColor.L <= BlackMaxLightness;
        }

        /**
         * @return true if the color represents a color which is close to white.
         */

        public static bool IsWhite(HslColor hslColor)
        {
            return hslColor.L >= WhiteMinLightness;
        }
    }

    public class FloatUtils
    {
        public static float WeightedAverage(params float[] values)
        {
            float sum = 0;
            float sumWeight = 0;

            for (var i = 0; i < values.Length; i += 2)
            {
                var value = values[i];
                var weight = values[i + 1];

                sum += (value*weight);
                sumWeight += weight;
            }

            return sum/sumWeight;
        }
    }

    public class ColorScheme
    {
        public int BackgroundColor { get; set; }
        public int ForegroundColor { get; set; }
        public int PrimaryAccent;
        public int PrimaryText;
        public int SecondaryAccent;
        public int SecondaryText;
        public int TertiaryAccent;

        public ColorScheme(int backgroundColor, int foregroundColor, int primaryAccent, int secondaryAccent, int tertiaryAccent,
            int primaryText, int secondaryText)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
            PrimaryAccent = primaryAccent;
            SecondaryAccent = secondaryAccent;
            TertiaryAccent = tertiaryAccent;
            PrimaryText = primaryText;
            SecondaryText = secondaryText;
        }
    }

    public class DominantColorCalculator
    {
        private const int NumColors = 10;
        private const int PrimaryTextMinContrast = 135;
        private const int SecondaryMinDiffHuePrimary = 120;
        private const int TertiaryMinContrastPrimary = 20;
        private const int TertiaryMinContrastSecondary = 90;
        private readonly MedianCutQuantizer.ColorNode[] _mPalette;
        private readonly MedianCutQuantizer.ColorNode[] _mWeightedPalette;
        private ColorScheme _mColorScheme;

        public DominantColorCalculator(WriteableBitmap bitmap)
        {
            using (var context = new BitmapContext(bitmap))
            {
                var mcq = new MedianCutQuantizer(context.Pixels, NumColors);

                _mPalette = mcq.GetQuantizedColors();
                _mWeightedPalette = Weight(_mPalette);

                FindColors();
            }
        }

        public static async Task<DominantColorCalculator> CreateAsync(byte[] bytes)
        {
            var rnd = new InMemoryRandomAccessStream();
            await rnd.WriteAsync(bytes.AsBuffer());
            return await CreateAsync(rnd);
        }

        public static Task<DominantColorCalculator> CreateAsync(Stream stream)
        {
            return CreateAsync(stream.AsRandomAccessStream());
        }

        public static async Task<DominantColorCalculator> CreateAsync(IRandomAccessStream randomAccessStream)
        {
            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

            // Get the first frame from the decoder
            var frame = await decoder.GetFrameAsync(0);

            var wid = (int) frame.PixelWidth;
            var hgt = (int) frame.PixelHeight;

            var target = new WriteableBitmap(wid, hgt);

            randomAccessStream.Seek(0);
            await target.SetSourceAsync(randomAccessStream);

            return new DominantColorCalculator(target);
        }

        public ColorScheme GetColorScheme()
        {
            return _mColorScheme;
        }

        private void FindColors()
        {
            var backgroundColor = _mPalette[0];
            var foregroundColor = FindPrimaryTextColor(backgroundColor);

            var primaryAccentColor = FindPrimaryAccentColor();
            var secondaryAccentColor = FindSecondaryAccentColor(primaryAccentColor);

            var tertiaryAccentColor = FindTertiaryAccentColor(
                primaryAccentColor, secondaryAccentColor);

            var primaryTextColor = FindPrimaryTextColor(primaryAccentColor);
            var secondaryTextColor = FindSecondaryTextColor(primaryAccentColor);

            _mColorScheme = new ColorScheme(backgroundColor.GetRgb(),
                foregroundColor,
                primaryAccentColor.GetRgb(),
                secondaryAccentColor.GetRgb(),
                tertiaryAccentColor,
                primaryTextColor,
                secondaryTextColor);
        }

        /**
         * @return the first color from our weighted palette.
         */

        private MedianCutQuantizer.ColorNode FindPrimaryAccentColor()
        {
            return _mWeightedPalette[0];
        }

        /**
         * @return the next color in the weighted palette which ideally has enough difference in hue.
         */

        private MedianCutQuantizer.ColorNode FindSecondaryAccentColor(MedianCutQuantizer.ColorNode primary)
        {
            var primaryHue = primary.GetRgb().ToColor().ToHsv().H;

            // Find the first color which has sufficient difference in hue from the primary
            foreach (var candidate in _mWeightedPalette)
            {
                var candidateHue = candidate.GetRgb().ToColor().ToHsv().H;

                // Calculate the difference in hue, if it's over the threshold return it
                if (Math.Abs(primaryHue - candidateHue) >= SecondaryMinDiffHuePrimary)
                {
                    return candidate;
                }
            }

            // If we get here, just return the second weighted color
            return _mWeightedPalette[1];
        }

        /**
         * @return the first color from our weighted palette which has sufficient contrast from the
         *         primary and secondary colors.
         */

        private int FindTertiaryAccentColor(MedianCutQuantizer.ColorNode primary, MedianCutQuantizer.ColorNode secondary)
        {
            // Find the first color which has sufficient contrast from both the primary & secondary
            foreach (var color in _mWeightedPalette.Where(color => ColorUtils.CalculateContrast(color, primary) >= TertiaryMinContrastPrimary
                                                                   && ColorUtils.CalculateContrast(color, secondary) >= TertiaryMinContrastSecondary))
            {
                return color.GetRgb();
            }

            // We couldn't find a colour. In that case use the primary colour, modifying it's brightness
            // by 45%
            return ColorUtils.ChangeBrightness(secondary.GetRgb(), 0.45f);
        }

        /**
         * @return the first color which has sufficient contrast from the primary colors.
         */

        private int FindPrimaryTextColor(MedianCutQuantizer.ColorNode primary)
        {
            // Try and find a colour with sufficient contrast from the primary colour
            foreach (var color in _mPalette.Where(color => ColorUtils.CalculateContrast(color, primary) >= PrimaryTextMinContrast))
            {
                return color.GetRgb();
            }

            // We haven't found a colour, so return black/white depending on the primary colour's
            // brightness
            return
                unchecked(
                    (int)
                        (ColorUtils.CalculateYiqLuma(primary.GetRgb()) >= 128
                            ? ColorExtensions.Black
                            : ColorExtensions.White));
        }

        /**
         * @return return black/white depending on the primary colour's brightness
         */

        private static int FindSecondaryTextColor(MedianCutQuantizer.ColorNode primary)
        {
            return
                unchecked(
                    (int)
                        (ColorUtils.CalculateYiqLuma(primary.GetRgb()) >= 128
                            ? ColorExtensions.Black
                            : ColorExtensions.White));
        }

        private static MedianCutQuantizer.ColorNode[] Weight(MedianCutQuantizer.ColorNode[] palette)
        {
            var copy = new MedianCutQuantizer.ColorNode[palette.Length];
            Array.Copy(palette, copy, palette.Length);
            var maxCount = palette[0].GetCount();

            Array.Sort(copy, new NodeComparer(maxCount));
            
            return copy;
        }

        private static float CalculateWeight(MedianCutQuantizer.ColorNode node, int maxCount)
        {
            return FloatUtils.WeightedAverage((float) ColorUtils.CalculateColorfulness(node), 3f,
                node.GetCount()/(float) maxCount, 1f);
        }

        internal class NodeComparer : IComparer<MedianCutQuantizer.ColorNode>
        {
            private readonly int _maxCount;

            public NodeComparer(int maxCount)
            {
                _maxCount = maxCount;
            }

            public int Compare(MedianCutQuantizer.ColorNode x, MedianCutQuantizer.ColorNode y)
            {
                var xWeight = CalculateWeight(x, _maxCount);
                var yWeight = CalculateWeight(y, _maxCount);

                if (xWeight < yWeight)
                {
                    return 1;
                }
                if (xWeight > yWeight)
                {
                    return -1;
                }
                return 0;
            }
        }
    }

    public class MedianCutQuantizer
    {
        private readonly ColorNode[] _quantColors; // quantized colors
        private ColorNode[] _imageColors; // original (unique) image colors

        public MedianCutQuantizer(IReadOnlyList<int> pixels, int kmax)
        {
            _quantColors = FindRepresentativeColors(pixels, kmax).OrderByDescending(p => p.Cnt).ToArray();
        }

        public int CountQuantizedColors()
        {
            return _quantColors.Length;
        }

        public ColorNode[] GetQuantizedColors()
        {
            return _quantColors;
        }

        private ColorNode[] FindRepresentativeColors(IReadOnlyList<int> pixels, int kmax)
        {
            var colorHist = new ColorHistogram(pixels);
            var colorCount = colorHist.GetNumberOfColors();
            ColorNode[] rCols;

            _imageColors = new ColorNode[colorCount];
            for (var i = 0; i < colorCount; i++)
            {
                var rgb = colorHist.GetColor(i);
                var cnt = colorHist.GetCount(i);
                _imageColors[i] = new ColorNode(rgb, cnt);
            }

            if (colorCount <= kmax)
            {
                // image has fewer colors than Kmax
                rCols = _imageColors;
            }
            else
            {
                var initialBox = new ColorBox(0, colorCount - 1, 0, _imageColors);
                var colorSet = new List<ColorBox> {initialBox};
                var k = 1;
                var done = false;
                while (k < kmax && !done)
                {
                    var nextBox = FindBoxToSplit(colorSet);
                    if (nextBox != null)
                    {
                        var newBox = nextBox.SplitBox();
                        colorSet.Add(newBox);
                        k = k + 1;
                    }
                    else
                    {
                        done = true;
                    }
                }
                rCols = AverageColors(colorSet);
            }
            return rCols;
        }

        public void QuantizeImage(int[] pixels)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                var color = FindClosestColor(pixels[i]);
                pixels[i] = ColorExtensions.Rgb(color.Red, color.Grn, color.Blu);
            }
        }

        private ColorNode FindClosestColor(int rgb)
        {
            var idx = FindClosestColorIndex(rgb);
            return _quantColors[idx];
        }

        private int FindClosestColorIndex(int rgb)
        {
            var red = ColorExtensions.Red(rgb);
            var grn = ColorExtensions.Green(rgb);
            var blu = ColorExtensions.Blue(rgb);
            var minIdx = 0;
            var minDistance = int.MaxValue;
            for (var i = 0; i < _quantColors.Length; i++)
            {
                var color = _quantColors[i];
                var d2 = color.Distance2(red, grn, blu);
                if (d2 < minDistance)
                {
                    minDistance = d2;
                    minIdx = i;
                }
            }
            return minIdx;
        }

        private static ColorBox FindBoxToSplit(IEnumerable<ColorBox> colorBoxes)
        {
            ColorBox boxToSplit = null;
            // from the set of splitable color boxes
            // select the one with the minimum level
            var minLevel = int.MaxValue;
            foreach (var box in colorBoxes)
            {
                if (box.ColorCount() >= 2)
                {
                    // box can be split
                    if (box.Level < minLevel)
                    {
                        boxToSplit = box;
                        minLevel = box.Level;
                    }
                }
            }
            return boxToSplit;
        }

        private static ColorNode[] AverageColors(IReadOnlyCollection<ColorBox> colorBoxes)
        {
            var n = colorBoxes.Count;
            var avgColors = new ColorNode[n];
            var i = 0;
            foreach (var box in colorBoxes)
            {
                avgColors[i] = box.GetAverageColor();
                i = i + 1;
            }
            return avgColors;
        }

        // -------------- class ColorNode -------------------------------------------

        public class ColorNode
        {
            internal ColorNode(int rgb, int cnt)
            {
                Red = ColorExtensions.Red(rgb);
                Grn = ColorExtensions.Green(rgb);
                Blu = ColorExtensions.Blue(rgb);
                Cnt = cnt;
            }

            internal ColorNode(int red, int grn, int blu, int cnt)
            {
                Red = red;
                Grn = grn;
                Blu = blu;
                Cnt = cnt;
            }

            public int Cnt { get; }
            public int Red { get; }
            public int Grn { get; }
            public int Blu { get; }

            public int GetRgb()
            {
                return ColorExtensions.Rgb(Red, Grn, Blu);
            }

            public int GetCount()
            {
                return Cnt;
            }

            public int Distance2(int red, int grn, int blu)
            {
                // returns the squared distance between (red, grn, blu)
                // and this this color
                var dr = Red - red;
                var dg = Grn - grn;
                var db = Blu - blu;
                return dr*dr + dg*dg + db*db;
            }
        }

        // -------------- class ColorBox -------------------------------------------

        private class ColorBox
        {
            private readonly Dictionary<ColorDimension, IComparer<ColorNode>> _comparers =
                new Dictionary<ColorDimension, IComparer<ColorNode>>
                {
                    {ColorDimension.Red, new RedComparer()},
                    {ColorDimension.Blue, new BlueComparer()},
                    {ColorDimension.Green, new GreenComparer()}
                };

            private readonly ColorNode[] _imageColors;
            private readonly int _lower; // lower index into 'imageColors'
            private int _bmin, _bmax; // range of contained colors in blue dimension
            private int _count; // number of pixels represented by thos color box
            private int _gmin, _gmax; // range of contained colors in green dimension
            private int _rmin, _rmax; // range of contained colors in red dimension
            private int _upper; // upper index into 'imageColors'

            internal ColorBox(int lower, int upper, int level, ColorNode[] imageColors)
            {
                _lower = lower;
                _upper = upper;
                Level = level;
                _imageColors = imageColors;
                Trim();
            }

            public int Level { get; private set; }

            public int ColorCount()
            {
                return _upper - _lower;
            }

            private void Trim()
            {
                // recompute the boundaries of this color box
                _rmin = 255;
                _rmax = 0;
                _gmin = 255;
                _gmax = 0;
                _bmin = 255;
                _bmax = 0;
                _count = 0;
                for (var i = _lower; i <= _upper; i++)
                {
                    var color = _imageColors[i];
                    _count = _count + color.Cnt;
                    var r = color.Red;
                    var g = color.Grn;
                    var b = color.Blu;
                    if (r > _rmax)
                    {
                        _rmax = r;
                    }
                    if (r < _rmin)
                    {
                        _rmin = r;
                    }
                    if (g > _gmax)
                    {
                        _gmax = g;
                    }
                    if (g < _gmin)
                    {
                        _gmin = g;
                    }
                    if (b > _bmax)
                    {
                        _bmax = b;
                    }
                    if (b < _bmin)
                    {
                        _bmin = b;
                    }
                }
            }

            // Split this color box at the median point along its
            // longest color dimension
            public ColorBox SplitBox()
            {
                if (ColorCount() < 2) // this box cannot be split
                {
                    return null;
                }
                // find longest dimension of this box:
                var dim = GetLongestColorDimension();

                // find median along dim
                var med = FindMedian(dim);

                // now split this box at the median return the resulting new
                // box.
                var nextLevel = Level + 1;
                var newBox = new ColorBox(med + 1, _upper, nextLevel, _imageColors);
                _upper = med;
                Level = nextLevel;
                Trim();
                return newBox;
            }

            // Find longest dimension of this color box (RED, GREEN, or BLUE)
            private ColorDimension GetLongestColorDimension()
            {
                var rLength = _rmax - _rmin;
                var gLength = _gmax - _gmin;
                var bLength = _bmax - _bmin;
                if (bLength >= rLength && bLength >= gLength)
                {
                    return ColorDimension.Blue;
                }
                if (gLength >= rLength && gLength >= bLength)
                {
                    return ColorDimension.Green;
                }
                return ColorDimension.Red;
            }

            // Find the position of the median in RGB space along
            // the red, green or blue dimension, respectively.
            private int FindMedian(ColorDimension dim)
            {
                // sort color in this box along dimension dim:
                Array.Sort(_imageColors, _lower, _upper - _lower, _comparers[dim]);
                // find the median point:
                var half = _count/2;
                int nPixels, median;
                for (median = _lower, nPixels = 0; median < _upper; median++)
                {
                    nPixels = nPixels + _imageColors[median].Cnt;
                    if (nPixels >= half)
                    {
                        break;
                    }
                }
                return median;
            }

            public ColorNode GetAverageColor()
            {
                var rSum = 0;
                var gSum = 0;
                var bSum = 0;
                var n = 0;
                for (var i = _lower; i <= _upper; i++)
                {
                    var ci = _imageColors[i];
                    var cnt = ci.Cnt;
                    rSum = rSum + cnt*ci.Red;
                    gSum = gSum + cnt*ci.Grn;
                    bSum = bSum + cnt*ci.Blu;
                    n = n + cnt;
                }
                double nd = n;
                var avgRed = (int) (0.5 + rSum/nd);
                var avgGrn = (int) (0.5 + gSum/nd);
                var avgBlu = (int) (0.5 + bSum/nd);
                return new ColorNode(avgRed, avgGrn, avgBlu, n);
            }

            private class RedComparer : IComparer<ColorNode>
            {
                public int Compare(ColorNode x, ColorNode y)
                {
                    return x.Red - y.Red;
                }
            }

            private class BlueComparer : IComparer<ColorNode>
            {
                public int Compare(ColorNode x, ColorNode y)
                {
                    return x.Blu - y.Blu;
                }
            }

            private class GreenComparer : IComparer<ColorNode>
            {
                public int Compare(ColorNode x, ColorNode y)
                {
                    return x.Grn - y.Grn;
                }
            }
        }

        private class ColorHistogram
        {
            private readonly int[] _colorArray;
            private readonly int[] _countArray;

            internal ColorHistogram(IEnumerable<int> pixelsOrig)
            {
                var pixelsCpy = pixelsOrig.Where(p =>
                {
                    var color = p.ToColor();
                    return color.A >= 125 && !(color.R > 250 && color.B > 250);
                }).Select(p => 0xFFFFFF & p).ToArray();
                
                Array.Sort(pixelsCpy);

                // count unique colors:
                var k = -1; // current color index
                var curColor = -1;
                foreach (var t in pixelsCpy)
                {
                    if (t != curColor)
                    {
                        k++;
                        curColor = t;
                    }
                }
                var nColors = k + 1;

                // tabulate and count unique colors:
                _colorArray = new int[nColors];
                _countArray = new int[nColors];
                k = -1; // current color index
                curColor = -1;
                foreach (var t in pixelsCpy)
                {
                    if (t != curColor)
                    {
                        // new color
                        k++;
                        curColor = t;
                        _colorArray[k] = curColor;
                        _countArray[k] = 1;
                    }
                    else
                    {
                        _countArray[k]++;
                    }
                }
            }

            public int GetNumberOfColors()
            {
                if (_colorArray == null)
                {
                    return 0;
                }
                return _colorArray.Length;
            }

            public int GetColor(int index)
            {
                return _colorArray[index];
            }

            public int GetCount(int index)
            {
                return _countArray[index];
            }
        }

        // The main purpose of this enumeration class is associate
        // the color dimensions with the corresponding comparators.
        private enum ColorDimension
        {
            Red,
            Green,
            Blue
        }
    }
}