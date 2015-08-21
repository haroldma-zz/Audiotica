using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Windows.Extensions;

namespace Audiotica.Windows.Common
{
    /// <summary>
    ///     A helper class to extract prominent colors from an image.
    /// </summary>
    public class Palette
    {
        private const int CalculateBitmapMinDimension = 100;
        private const int DefaultCalculateNumberColors = 16;
        private const float TargetDarkLuma = 0.26f;
        private const float MaxDarkLuma = 0.45f;
        private const float MinLightLuma = 0.55f;
        private const float TargetLightLuma = 0.74f;
        private const float MinNormalLuma = 0.3f;
        private const float TargetNormalLuma = 0.5f;
        private const float MaxNormalLuma = 0.7f;
        private const float TargetMutedSaturation = 0.3f;
        private const float MaxMutedSaturation = 0.4f;
        private const float TargetVibrantSaturation = 1f;
        private const float MinVibrantSaturation = 0.35f;
        private const float WeightSaturation = 3f;
        private const float WeightLuma = 6f;
        private const float WeightPopulation = 1f;
        private int _highestPopulation;

        public Palette(WriteableBitmap bitmap) : this(bitmap, DefaultCalculateNumberColors)
        {
        }

        public Palette(WriteableBitmap bitmap, int numColors)
        {
            if (numColors < 1) throw new ArgumentException("must be 1 of greater", nameof(numColors));

            // First we'll scale down the bitmap so it's shortest dimension is 100px
            var scaledBitmap = ScaleBitmapDown(bitmap);

            // Now generate a quantizer from the Bitmap
            var quantizer = ColorCutQuantizer.FromBitmap(scaledBitmap, numColors);

            // Now return a ColorExtractor instance
            Initialize(quantizer.QuantizedColors);
        }

        public Swatch DarkMutedSwatch { get; private set; }
        public Swatch DarkVibrantSwatch { get; private set; }
        public Swatch LightMutedColor { get; private set; }
        public Swatch LightVibrantSwatch { get; private set; }
        public Swatch MutedSwatch { get; private set; }
        public List<Swatch> Swatches { get; private set; }
        public Swatch VibrantSwatch { get; private set; }

        public static async Task<Palette> CreateAsync(byte[] bytes, int numColors = DefaultCalculateNumberColors)
        {
            var rnd = new InMemoryRandomAccessStream();
            await rnd.WriteAsync(bytes.AsBuffer());
            return await CreateAsync(rnd, numColors);
        }

        public static Task<Palette> CreateAsync(Stream stream, int numColors = DefaultCalculateNumberColors)
        {
            return CreateAsync(stream.AsRandomAccessStream(), numColors);
        }

        public static async Task<Palette> CreateAsync(IRandomAccessStream randomAccessStream,
            int numColors = DefaultCalculateNumberColors)
        {
            var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

            // Get the first frame from the decoder
            var frame = await decoder.GetFrameAsync(0);

            var wid = (int) frame.PixelWidth;
            var hgt = (int) frame.PixelHeight;

            var target = new WriteableBitmap(wid, hgt);

            randomAccessStream.Seek(0);
            await target.SetSourceAsync(randomAccessStream);

            return new Palette(target, numColors);
        }

        private void Initialize(List<Swatch> swatches)
        {
            Swatches = swatches;
            ValidSwatches = swatches.Where(color => !ShouldIgnoreColor(color.Color.AsInt())).ToList();

            _highestPopulation = FindMaxPopulation();

            VibrantSwatch = FindColor(TargetNormalLuma, MinNormalLuma, MaxNormalLuma,
                TargetVibrantSaturation, MinVibrantSaturation, 1f);

            LightVibrantSwatch = FindColor(TargetLightLuma, MinLightLuma, 1f,
                TargetVibrantSaturation, MinVibrantSaturation, 1f);

            DarkVibrantSwatch = FindColor(TargetDarkLuma, 0f, MaxDarkLuma,
                TargetVibrantSaturation, MinVibrantSaturation, 1f);

            MutedSwatch = FindColor(TargetNormalLuma, MinNormalLuma, MaxNormalLuma,
                TargetMutedSaturation, 0f, MaxMutedSaturation);

            LightMutedColor = FindColor(TargetLightLuma, MinLightLuma, 1f,
                TargetMutedSaturation, 0f, MaxMutedSaturation);

            DarkMutedSwatch = FindColor(TargetDarkLuma, 0f, MaxDarkLuma,
                TargetMutedSaturation, 0f, MaxMutedSaturation);

            // Now try and generate any missing colors
            GenerateEmptySwatches();
        }

        public List<Swatch> ValidSwatches { get; set; }

        private static WriteableBitmap ScaleBitmapDown(WriteableBitmap bitmap)
        {
            var minDimension = Math.Min(bitmap.PixelWidth, bitmap.PixelHeight);

            if (minDimension <= CalculateBitmapMinDimension)
            {
                // If the bitmap is small enough already, just return it
                return bitmap;
            }

            var scaleRatio = CalculateBitmapMinDimension/(float) minDimension;
            return bitmap.Resize((int) Math.Round(bitmap.PixelWidth*scaleRatio),
                (int) Math.Round(bitmap.PixelHeight*scaleRatio),
                WriteableBitmapExtensions.Interpolation.Bilinear);
        }

        private int FindMaxPopulation()
        {
            return ValidSwatches.Select(swatch => swatch.Population).Concat(new[] {0}).Max();
        }

        private Swatch FindColor(float targetLuma, float minLuma, float maxLuma,
            float targetSaturation, float minSaturation, float maxSaturation)
        {
            Swatch max = null;
            double maxValue = 0f;

            foreach (var swatch in ValidSwatches)
            {
                var sat = swatch.HslColor.S;
                var luma = swatch.HslColor.L;

                if (sat >= minSaturation && sat <= maxSaturation &&
                    luma >= minLuma && luma <= maxLuma &&
                    !IsAlreadySelected(swatch))
                {
                    var thisValue = CreateComparisonValue(sat, targetSaturation, luma, targetLuma,
                        swatch.Population, _highestPopulation);
                    if (max == null || thisValue > maxValue)
                    {
                        max = swatch;
                        maxValue = thisValue;
                    }
                }
            }

            return max;
        }

        private bool IsAlreadySelected(Swatch swatch)
        {
            return VibrantSwatch == swatch || DarkVibrantSwatch == swatch ||
                   LightVibrantSwatch == swatch || MutedSwatch == swatch ||
                   DarkMutedSwatch == swatch || LightMutedColor == swatch;
        }

        private static double CreateComparisonValue(double saturation, double targetSaturation,
            double luma, double targetLuma,
            int population, int highestPopulation)
        {
            return WeightedMean(
                InvertDiff(saturation, targetSaturation), WeightSaturation,
                InvertDiff(luma, targetLuma), WeightLuma,
                population/(double) highestPopulation, WeightPopulation
                );
        }

        private static double InvertDiff(double value, double targetValue)
        {
            return 1f - Math.Abs(value - targetValue);
        }

        private static double WeightedMean(params double[] values)
        {
            double sum = 0f;
            double sumWeight = 0f;

            for (var i = 0; i < values.Length; i += 2)
            {
                var value = values[i];
                var weight = values[i + 1];

                sum += (value*weight);
                sumWeight += weight;
            }

            return sum/sumWeight;
        }

        private void GenerateEmptySwatches()
        {
            if (VibrantSwatch == null)
            {
                // If we do not have a vibrant color...
                if (DarkVibrantSwatch != null)
                {
                    // ...but we do have a dark vibrant, generate the value by modifying the luma
                    var newHsl = DarkVibrantSwatch.HslColor;
                    newHsl.L = TargetNormalLuma;
                    VibrantSwatch = new Swatch(ColorExtensions.FromHsl(newHsl), 0);
                }
            }

            if (DarkVibrantSwatch == null)
            {
                // If we do not have a dark vibrant color...
                if (VibrantSwatch != null)
                {
                    // ...but we do have a vibrant, generate the value by modifying the luma
                    var newHsl = VibrantSwatch.HslColor;
                    newHsl.L = TargetDarkLuma;
                    DarkVibrantSwatch = new Swatch(ColorExtensions.FromHsl(newHsl), 0);
                }
            }
        }

        public bool ShouldIgnoreColor(int color)
        {
            var tempHsl = color.ToColor().ToHsl();
            return ShouldIgnoreColor(tempHsl);
        }

        private static bool ShouldIgnoreColor(Swatch color)
        {
            return ShouldIgnoreColor(color.HslColor);
        }

        private static bool ShouldIgnoreColor(HslColor hslColor)
        {
            return IsWhite(hslColor) || IsBlack(hslColor) ||
                IsNearRedILine(hslColor);
        }


        private const float BlackMaxLightness = 0.05f;
        private const float WhiteMinLightness = 0.95f;

        /**
         * @return true if the color represents a color which is close to black.
         */

        private static bool IsBlack(HslColor hslColor)
        {
            return hslColor.L <= BlackMaxLightness;
        }

        /**
         * @return true if the color represents a color which is close to white.
         */

        private static bool IsWhite(HslColor hslColor)
        {
            return hslColor.L >= WhiteMinLightness;
        }

        /**
         * @return true if the color lies close to the red side of the I line.
         */

        private static bool IsNearRedILine(HslColor hslColor)
        {
            return hslColor.H >= 10f && hslColor.H <= 37f && hslColor.S <= 0.82f;
        }
    }

    public class Swatch
    {
        private const float MinContrastTitleText = 3.0f;
        private const float MinContrastBodyText = 4.5f;

        public Swatch(Color color, int population)
        {
            Color = color;
            HslColor = color.ToHsl();
            HexCode = color.ToString();
            Population = population;

            TitleTextColor = ColorExtensions.GetTextColorForBackground(color.AsInt(), MinContrastTitleText).ToColor();
            BodyTextColor = ColorExtensions.GetTextColorForBackground(color.AsInt(), MinContrastBodyText).ToColor();
        }

        public Swatch(int red, int green, int blue, int totalPopulation)
            : this(ColorExtensions.FromRgb(red, green, blue), totalPopulation)
        {
        }

        public Color TitleTextColor { get; }
        public Color BodyTextColor { get; }
        public Color Color { get; }
        public HslColor HslColor { get; }
        public int Population { get; }
        public string HexCode { get; }
    }
}