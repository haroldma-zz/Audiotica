#region

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Audiotica.Data;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.DesignTime;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Service.DesignTime;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Service.RunTime;
using Audiotica.Data.Spotify;
using Audiotica.PartialView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using IF.Lastfm.Core.Api;
using Microsoft.Practices.ServiceLocation;

#endregion

namespace Audiotica.ViewModel
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var config = GetForegroundConfig();
            var bgConfig = GetBackgroundConfig();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IScrobblerService, DesignScrobblerService>();
                SimpleIoc.Default.Register<ICollectionService, DesignCollectionService>();
                SimpleIoc.Default.Register<ISqlService, DesignSqlService>();
            }
            else
            {
                SimpleIoc.Default.Register<IScrobblerService, ScrobblerService>();
                SimpleIoc.Default.Register<ISpotifyService, SpotifyService>();
                SimpleIoc.Default.Register<ISqlService>(() => new SqlService(config));
                SimpleIoc.Default.Register<ISqlService>(() => new SqlService(bgConfig), "BackgroundSql");
                SimpleIoc.Default.Register<ICollectionService>(() => new CollectionService(SqlService, BgSqlService, DispatcherHelper.UIDispatcher));
                SimpleIoc.Default.Register<ISongDownloadService>(() => new SongDownloadService(CollectionService, SqlService, DispatcherHelper.UIDispatcher));
            }

            SimpleIoc.Default.Register<SpotifyWebApi>();
            SimpleIoc.Default.Register<AudioPlayerHelper>();
            SimpleIoc.Default.Register<CollectionViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register(() => new PlayerViewModel(AudioPlayerHelper, CollectionService, BgSqlService, ScrobblerService));
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<CollectionAlbumViewModel>();
            SimpleIoc.Default.Register<CollectionArtistViewModel>();
            SimpleIoc.Default.Register<CollectionPlaylistViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<SpotifyAlbumViewModel>();
            SimpleIoc.Default.Register<SpotifyArtistViewModel>();
        }

        private SqlServiceConfig GetForegroundConfig()
        {
            var dbTypes = new List<Type>
            {
                typeof (Artist),
                typeof (Album),
                typeof (Song),
                typeof (Playlist),
                typeof (PlaylistSong)
            };
            return new SqlServiceConfig()
            {
                Tables = dbTypes,
                CurrentVersion = 6,
                Path = "collection.sqldb",
                OnUpdate = (connection, d) =>
                {
                    if (d == 5)
                    {
                        using (var statement = connection.Prepare("ALTER TABLE Artist ADD COLUMN HasArtwork INTEGER"))
                        {
                            statement.Step();
                        }
                    }
                }
            };
        }
        private SqlServiceConfig GetBackgroundConfig()
        {
            var dbTypes = new List<Type>
            {
                typeof (HistoryEntry),
                typeof (QueueSong),
            };
            return new SqlServiceConfig()
            {
                Tables = dbTypes,
                CurrentVersion = 1,
                Path = "player.sqldb"
            };
        }

        public SpotifyWebApi Spotify
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyWebApi>(); }
        }

        public AdMediatorBar Ads
        {
            get { return ServiceLocator.Current.GetInstance<AdMediatorBar>(); }
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public AlbumViewModel Album
        {
            get { return ServiceLocator.Current.GetInstance<AlbumViewModel>(); }
        }
        public CollectionAlbumViewModel CollectionAlbum
        {
            get { return ServiceLocator.Current.GetInstance<CollectionAlbumViewModel>(); }
        }

        public ArtistViewModel Artist
        {
            get { return ServiceLocator.Current.GetInstance<ArtistViewModel>(); }
        }

        public CollectionPlaylistViewModel CollectionPlaylist
        {
            get { return ServiceLocator.Current.GetInstance<CollectionPlaylistViewModel>(); }
        }

        public PlayerViewModel Player
        {
            get { return ServiceLocator.Current.GetInstance<PlayerViewModel>(); }
        }

        public AudioPlayerHelper AudioPlayerHelper
        {
            get { return ServiceLocator.Current.GetInstance<AudioPlayerHelper>(); }
        }

        public CollectionArtistViewModel CollectionArtist
        {
            get { return ServiceLocator.Current.GetInstance<CollectionArtistViewModel>(); }
        }

        public SearchViewModel Search
        {
            get { return ServiceLocator.Current.GetInstance<SearchViewModel>(); }
        }

        public SettingsViewModel Settings
        {
            get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); }
        }

        public CollectionViewModel Collection
        {
            get { return ServiceLocator.Current.GetInstance<CollectionViewModel>(); }
        }

        public SpotifyAlbumViewModel SpotifyAlbum
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyAlbumViewModel>(); }
        }

        public SpotifyArtistViewModel SpotifyArtist
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyArtistViewModel>(); }
        }

        public ICollectionService CollectionService
        {
            get { return SimpleIoc.Default.GetInstance<ICollectionService>(); }
        }

        public IScrobblerService ScrobblerService
        {
            get { return SimpleIoc.Default.GetInstance<IScrobblerService>(); }
        }

        public ISongDownloadService Download
        {
            get { return SimpleIoc.Default.GetInstance<ISongDownloadService>(); }
        }

        public ISqlService SqlService
        {
            get { return SimpleIoc.Default.GetInstance<ISqlService>(); }
        }

        public ISqlService BgSqlService
        {
            get { return SimpleIoc.Default.GetInstance<ISqlService>("BackgroundSql"); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}