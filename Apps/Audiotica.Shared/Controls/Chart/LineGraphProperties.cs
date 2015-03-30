using Windows.UI.Xaml.Media;

namespace Audiotica.Controls.Chart
{
    public sealed partial class LineGraph
    {
        public Brush GuideLineColor { get; set; }
        public Brush GuideTextColor { get; set; }
        public double GuideTextSize { get; set; }
        public int DrawAreaVerticalOffset { get; set; }
        public int DrawAreaHorizontalOffset { get; set; }

        public bool ShowText { get; set; }

        /// <summary>
        ///     Set thickness of the guidelines
        /// </summary>
        public double GuideLineThickness { get; set; }

        /// <summary>
        ///     Draw X guidelines
        /// </summary>
        public bool DrawXGuideLines { get; set; }

        /// <summary>
        ///     Draw Y guideLines
        /// </summary>
        public bool DrawYGuideLines { get; set; }

        /// <summary>
        ///     Draw X and Y guideLines
        /// </summary>
        public bool DrawGuideLines
        {
            get { return DrawXGuideLines && DrawYGuideLines; }
            set
            {
                DrawXGuideLines = value;
                DrawYGuideLines = value;
            }
        }

        /// <summary>
        ///     Fill the line with the Serie color
        /// </summary>
        public bool Fill { get; set; }

        /// <summary>
        ///     Automatically redraw the graph after a data change
        /// </summary>
        public bool AutoRedraw { get; set; }

        /// <summary>
        ///     Distance between x information data
        /// </summary>
        public int XGuideLineResolution { get; set; }

        /// <summary>
        ///     Distance between y information data
        /// </summary>
        public int YGuideLineResolution { get; set; }
    }
}