#region

using Windows.UI.Xaml;
using Audiotica.Core.Utilities;
using Audiotica.Core.Utils.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.AdMediator.WindowsPhone81;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class AdMediatorBar
    {
        public AdMediatorBar()
        {
            InitializeComponent();
            SimpleIoc.Default.Register(() => this);
            Loaded += (sender, args) =>
            {
                var ads = App.Locator.AppSettingsHelper.Read("Ads", true, SettingsStrategy.Roaming);
                var owns = App.Locator.Settings.IsAdsEnabled;

                if (!owns && !ads)
                {
                    App.Locator.AppSettingsHelper.Write("Ads", true, SettingsStrategy.Roaming);
                    ads = true;
                }

                if (ads)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            };
        }

        private AdMediatorControl _adMediator793A03;

        public void Enable()
        {
            Visibility = Visibility.Visible;
            if (_adMediator793A03 == null)
            {
                _adMediator793A03 = new AdMediatorControl
                {
                    Id = "AdMediator-Id-AAEBB1F0-7803-406A-83F8-88A32158097A",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                RootGrid.Children.Add(_adMediator793A03);
            }
            else
                _adMediator793A03.Resume();
        }

        public void Disable()
        {
            Visibility = Visibility.Collapsed;

            if (_adMediator793A03 == null) return;

            _adMediator793A03.Disable();
            RootGrid.Children.Remove(_adMediator793A03);
            _adMediator793A03 = null;
        }
    }
}