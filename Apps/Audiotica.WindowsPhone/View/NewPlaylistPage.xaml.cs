#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
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
            Bar = BottomAppBar;
            BottomAppBar = null;
        }

        private async void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
            var name = PlaylistNameText.Text;

            button.IsEnabled = false;
            PlaylistNameText.IsEnabled = false;

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
                    CurtainPrompt.Show("PlaylistCreateSuccess".FromLanguageResource(), playlist.Name);
                    App.Navigator.GoBack();
                }
            }

            button.IsEnabled = true;
            PlaylistNameText.IsEnabled = true;
        }
    }
}