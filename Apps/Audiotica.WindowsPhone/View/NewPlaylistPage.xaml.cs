#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;

#endregion


namespace Audiotica.View
{
    public sealed partial class NewPlaylistPage
    {
        public NewPlaylistPage()
        {
            InitializeComponent();
        }

        private List<Song> _songs;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                _songs = e.Parameter as List<Song>;
            }
        }

        private async void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var name = PlaylistNameText.Text;

            button.IsEnabled = false;
            PlaylistNameText.IsEnabled = false;
            //TODO [Harry,20141219] ui blocker

            if (string.IsNullOrEmpty(name))
            {
                CurtainPrompt.ShowError("PlaylistCreateNameForgot".FromLanguageResource());
            }
            else
            {
                if (App.Locator.CollectionService.
                    Playlists.FirstOrDefault(p => 
                        String.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    CurtainPrompt.ShowError("PlaylistCreateNameTaken".FromLanguageResource());
                }
                else
                {
                    
                    var playlist = await App.Locator.CollectionService.CreatePlaylistAsync(name);
                    foreach (var song in _songs)
                    {
                        await App.Locator.CollectionService.AddToPlaylistAsync(playlist, song);
                    }
                    CurtainPrompt.Show("PlaylistCreateSuccess".FromLanguageResource(), playlist.Name);
                    Frame.GoBack();
                }
            }

            button.IsEnabled = true;
            PlaylistNameText.IsEnabled = true;
        }
    }
}