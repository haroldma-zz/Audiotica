#region

using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.Data.Spotify.Models;
using Audiotica.View.Setting;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class HomePage
    {
        private readonly HubSection _spotlightSection;

        public HomePage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            //_spotlightSection = SpotlightSection;
            //MainHub.Sections.Remove(_spotlightSection);
            Messenger.Default.Register<bool>(this, "spotlight", SpotlightLoaded);
        }

        private void SpotlightLoaded(bool loaded)
        {
            if (loaded)
            {
                //MainHub.Sections.Insert(1, _spotlightSection);
            }
        }

        private void HandleAction(SpotlightFeature feature)
        {
            var action = feature.Action.Substring(feature.Action.IndexOf(":", StringComparison.Ordinal) + 1);
            bool supported = true;

            if (feature.Action.StartsWith("web:"))
            {
                Launcher.LaunchUriAsync(new Uri(action));
            }
            else if (feature.Action.StartsWith("artist:"))
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + action);
            }
            else if (feature.Action.StartsWith("page:"))
            {
                switch (action)
                {
                    case "cloud":
                        App.Navigator.GoTo<CloudPage, ZoomOutTransition>(null);
                        break;
                    case "lastfm":
                        App.Navigator.GoTo<LastFmPage, ZoomOutTransition>(null);
                        break;
                    default:
                        if (action.StartsWith("search:"))
                        {
                            action = action.Substring(feature.Action.IndexOf(":", StringComparison.Ordinal) + 1);
                            App.Navigator.GoTo<SearchPage, ZoomInTransition>(action);
                        }
                        else
                        {
                            supported = false;
                        }
                        break;
                }
            }
            else
            {
                supported = false;
            }

            if (!supported)
            {

                CurtainPrompt.ShowError("Audiotica can't open this type of link.  Try updating in the store.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SearchPage, ZoomInTransition>(null);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SettingsPage, ZoomInTransition>(null);
        }
    }
}