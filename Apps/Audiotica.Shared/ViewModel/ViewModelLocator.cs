#region

using Windows.UI.Xaml;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.DesignTime;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Service.RunTime;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
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

#if WINDOWS_PHONE_APP
            SimpleIoc.Default.Register<AudioPlayerHelper>();
#endif
            SimpleIoc.Default.Register(() => Window.Current.Dispatcher);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<ICollectionService, DesignCollectionService>();
                SimpleIoc.Default.Register<IQueueService, QueueService>();
                SimpleIoc.Default.Register<ISqlService, DesignSqlService>();
            }
            else
            {
                SimpleIoc.Default.Register<IScrobblerService, ScrobblerService>();
                SimpleIoc.Default.Register<ICollectionService, CollectionService>();
                SimpleIoc.Default.Register<IQueueService, QueueService>();
                SimpleIoc.Default.Register<ISqlService, SqlService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<CollectionAlbumViewModel>();
            SimpleIoc.Default.Register<CollectionArtistViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<CollectionViewModel>();
            SimpleIoc.Default.Register<PlayerViewModel>();
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

        public CollectionViewModel Collection
        {
            get { return ServiceLocator.Current.GetInstance<CollectionViewModel>(); }
        }

        public ICollectionService CollectionService
        {
            get { return SimpleIoc.Default.GetInstance<ICollectionService>(); }
        }

        public IScrobblerService ScrobblerService
        {
            get { return SimpleIoc.Default.GetInstance<IScrobblerService>(); }
        }

        public IQueueService QueueService
        {
            get { return SimpleIoc.Default.GetInstance<IQueueService>(); }
        }

        public ISqlService SqlService
        {
            get { return SimpleIoc.Default.GetInstance<ISqlService>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}