using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Audiotica.Controls.Chart;
using Audiotica.Data.Model.AudioticaCloud;
using GalaSoft.MvvmLight;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace Audiotica.Controls
{
    public sealed partial class ChartSongViewer
    {
        public ChartSongViewer()
        {
            InitializeComponent();

            if (!ViewModelBase.IsInDesignModeStatic) return;
            var data =
                new[] { 0, 1, 5, -3, -1, 0, 2.5, 2.4, 2.3 }.Select((p, i) => new Point(i, p)).ToList();

            var serie = new Serie("Signals") { ShiftSize = 100};
            serie.SetData(data);

            Graph.AutoRedraw = true;
            Graph.AddSerie(serie);
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Graph.ClearSeries();

            var item = DataContext as ChartSong;
            if (item == null) return;

            var color = Colors.White;

            switch (item.ChangeDirection)
            {
                case ChartSong.Direction.Up:
                    color = Colors.Green;
                    break;
                case ChartSong.Direction.Down:
                    color = Colors.Red;
                    break;
            }

            ChangePercentBlock.Foreground = new SolidColorBrush(color);

            var data = item.Signals.Select((p, i) => new Point(i, p)).ToList();

            var serie = new Serie("Signals") {ShiftSize = 100};
            serie.SetData(data);

            Graph.AutoRedraw = true;
            Graph.AddSerie(serie);
        }
    }
}