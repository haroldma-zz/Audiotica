#region

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Audiotica.Core.Utilities;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Utilities;
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
using IF.Lastfm.Core.Api;
using Microsoft.Practices.ServiceLocation;
using SQLite;
using DispatcherHelper = GalaSoft.MvvmLight.Threading.DispatcherHelper;

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

            SimpleIoc.Default.Register<INotificationManager, NotificationManager>();
            SimpleIoc.Default.Register<ICredentialHelper, PclCredentialHelper>();
            SimpleIoc.Default.Register<IAppSettingsHelper, AppSettingsHelper>();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IAudioticaService, DesignAudioticaService>();
                SimpleIoc.Default.Register<IScrobblerService, DesignScrobblerService>();
                SimpleIoc.Default.Register<ICollectionService, DesignCollectionService>();
                SimpleIoc.Default.Register<ISqlService, DesignSqlService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDispatcherHelper>(() => new PclDispatcherHelper(DispatcherHelper.UIDispatcher));
                SimpleIoc.Default.Register<IBitmapFactory, PclBitmapFactory>();

                var factory = new AudioticaFactory(PclDispatcherHelper, AppSettingsHelper, BitmapFactory);

                SimpleIoc.Default.Register<IScrobblerService, ScrobblerService>();
                SimpleIoc.Default.Register<SpotifyWebApi>();
                SimpleIoc.Default.Register<ISpotifyService, SpotifyService>();

                SimpleIoc.Default.Register(() => factory.CreateCollectionSqlService(9, async (connection, d) =>
                {
                    if (!(d > 0) || !(d < 8)) return;

                    if (App.Locator.CollectionService.IsLibraryLoaded)
                        await CollectionHelper.MigrateAsync();
                    else
                        App.Locator.CollectionService.LibraryLoaded += (sender, args) =>
                            CollectionHelper.MigrateAsync();
                }));
                SimpleIoc.Default.Register(() => factory.CreatePlayerSqlService(4), "BackgroundSql");
                SimpleIoc.Default.Register(() => factory.CreateCollectionService(SqlService, BgSqlService));

                SimpleIoc.Default.Register<ISongDownloadService>(() => new SongDownloadService(CollectionService, SqlService, DispatcherHelper.UIDispatcher));
                SimpleIoc.Default.Register<IAudioticaService, AudioticaService>();
            }

            SimpleIoc.Default.Register<Mp3MatchEngine>();
            SimpleIoc.Default.Register<AppVersionHelper>();
            SimpleIoc.Default.Register<CollectionCommandHelper>();
            SimpleIoc.Default.Register<AudioPlayerHelper>();
            SimpleIoc.Default.Register<CollectionViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PlayerViewModel>();
            SimpleIoc.Default.Register<AudioticaCloudViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<CollectionAlbumViewModel>();
            SimpleIoc.Default.Register<CollectionArtistViewModel>();
            SimpleIoc.Default.Register<CollectionPlaylistViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<SpotifyAlbumViewModel>();
            SimpleIoc.Default.Register<SpotifyArtistViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<CollectionStatisticsViewModel>();
            SimpleIoc.Default.Register<ManualMatchViewModel>();
        }

        public IAudioticaService AudioticaService
        {
            get { return ServiceLocator.Current.GetInstance<IAudioticaService>(); }
        }

        public SpotifyWebApi Spotify
        {
            get { return ServiceLocator.Current.GetInstance<SpotifyWebApi>(); }
        }

        public IAppSettingsHelper AppSettingsHelper
        {
            get { return ServiceLocator.Current.GetInstance<IAppSettingsHelper>(); }
        }
        
        public IDispatcherHelper PclDispatcherHelper
        {
            get { return ServiceLocator.Current.GetInstance<IDispatcherHelper>(); }
        }

        public IBitmapFactory BitmapFactory
        {
            get { return ServiceLocator.Current.GetInstance<IBitmapFactory>(); }
        }

        public ICredentialHelper CredentialHelper
        {
            get { return ServiceLocator.Current.GetInstance<ICredentialHelper>(); }
        }

        public Mp3MatchEngine Mp3MatchEngine
        {
            get { return ServiceLocator.Current.GetInstance<Mp3MatchEngine>(); }
        }

        public AudioticaService AudioticaCloud
        {
            get { return ServiceLocator.Current.GetInstance<AudioticaService>(); }
        }

        public AdMediatorBar Ads
        {
            get { return ServiceLocator.Current.GetInstance<AdMediatorBar>(); }
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }
        
        public AudioticaCloudViewModel Cloud
        {
            get { return ServiceLocator.Current.GetInstance<AudioticaCloudViewModel>(); }
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

        public ManualMatchViewModel Manual
        {
            get { return ServiceLocator.Current.GetInstance<ManualMatchViewModel>(); }
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

        public CollectionStatisticsViewModel Statistics
        {
            get { return ServiceLocator.Current.GetInstance<CollectionStatisticsViewModel>(); }
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

        public AppVersionHelper AppVersionHelper
        {
            get { return SimpleIoc.Default.GetInstance<AppVersionHelper>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}