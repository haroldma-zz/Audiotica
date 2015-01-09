#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class NowPlayingSheet : IModalSheetPage
    {
        public NowPlayingSheet()
        {
            InitializeComponent();
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
        }

        public void OnClosed()
        {
            Popup = null;
        }
    }
}