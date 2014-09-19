#region

using Audiotica.Data.Collection;
using Audiotica.Data.Collection.DesignTime;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Service.DesignTime;
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

            SimpleIoc.Default.Register<ISqlService, SqlService>();

#if WINDOWS_PHONE_APP
            SimpleIoc.Default.Register<AudioPlayerManager>();
#endif

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IXboxMusicService, DesignXboxMusicService>();
                SimpleIoc.Default.Register<ICollectionService, DesignCollectionService>();
            }
            else
            {
                SimpleIoc.Default.Register<IXboxMusicService, XboxMusicService>();
                SimpleIoc.Default.Register<ICollectionService, CollectionService>();
                SimpleIoc.Default.Register<IQueueService, QueueService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<CollectionViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public AlbumViewModel Album
        {
            get { return ServiceLocator.Current.GetInstance<AlbumViewModel>(); }
        }

        public ArtistViewModel Artist
        {
            get { return ServiceLocator.Current.GetInstance<ArtistViewModel>(); }
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