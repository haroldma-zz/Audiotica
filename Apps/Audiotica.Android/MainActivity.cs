using System;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Audiotica.Android.Implementations;
using Audiotica.Android.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Android
{
    [Activity(Label = "Audiotica", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            var listView = FindViewById<ListView>(Resource.Id.songListView);
            listView.Adapter = new CustomArrayAdapter<Song>(this, Resource.Layout.songs_list_view_layout,
                App.Current.Locator.CollectionService.Songs, GetView);

            listView.ItemClick += ListViewOnItemClick;
        }

        private async void ListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var song = App.Current.Locator.CollectionService.Songs[itemClickEventArgs.Position];
            await CollectionHelper.PlaySongsAsync(song, App.Current.Locator.CollectionService.Songs.ToList());
        }

        private View GetView(int position, View convertView, ViewGroup parent)
        {
            var song = App.Current.Locator.CollectionService.Songs[position];

            var v = convertView;

            if (v == null)
            {
                var vi = LayoutInflater.FromContext(ApplicationContext);
                v = vi.Inflate(Resource.Layout.songs_list_view_layout, null);
            }

            if (song != null)
            {
                var name = v.FindViewById<TextView>(Resource.Id.songNameListView);
                var artistName = v.FindViewById<TextView>(Resource.Id.artistNameSongListView);
                var albumImage = v.FindViewById<ImageView>(Resource.Id.songsListAlbumThumbnail);

                if (name != null)
                {
                    name.Text = song.Name;
                }
                if (artistName != null)
                {
                    artistName.Text = song.ArtistName;
                }
                if (albumImage != null)
                {
                    if (song.Album.Artwork != null)
                    {
                        BindAlbumBitmap(albumImage, song.Album);
                    }
                    song.Album.PropertyChanged += (sender, args) => { BindAlbumBitmap(albumImage, song.Album); };
                }
            }

            return v;
        }

        private void BindAlbumBitmap(ImageView image, Album album)
        {
            ((PclBitmapImage) album.Artwork).PropertyChanged +=
                (sender, args) => { image.SetImageBitmap(album.Artwork.Image as Bitmap); };
            image.SetImageBitmap(album.Artwork.Image as Bitmap);
        }

        private async void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            var songs = await App.Current.Locator.SpotifyService.SearchTracksAsync("the vamps somebody to you");
            var track = songs.Items.First();
            var album = await App.Current.Locator.SpotifyService.GetAlbumAsync(track.Album.Id);

            App.Current.Locator.NotificationManager.Show("Finding audio for '{0}'.", track.Name);
            try
            {
                var preparedSong = track.ToSong();

                var url = await App.Current.Locator.Mp3MatchEngine.FindMp3For(track.Name, track.Artist.Name);

                if (string.IsNullOrEmpty(url))
                {
                    App.Current.Locator.NotificationManager.ShowError("No mp3 '{0}'.", track.Name);
                    return;
                }

                preparedSong.ArtistName = string.Join(", ", track.Artists.Select(p => p.Name));
                preparedSong.Album = album.ToAlbum();
                preparedSong.Artist = album.Artist.ToArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;
                preparedSong.AudioUrl = url;


                await App.Current.Locator.CollectionService.AddSongAsync(preparedSong,
                    album.Images[0].Url).ConfigureAwait(false);

                App.Current.Locator.NotificationManager.Show("Song '{0}' saved!", track.Name);
            }
            catch (Exception e)
            {
                App.Current.Locator.NotificationManager.ShowError("Problem saving '{0}'.", track.Name);
            }
        }
    }
}