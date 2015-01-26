using Audiotica.Android.Implementations;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data;
using Audiotica.Data.Collection;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Service.RunTime;
using Audiotica.Data.Spotify;
using DryIoc;

namespace Audiotica.Android
{
    public class Locator
    {
        public Locator()
        {
            Container = new Container();
            Container.Register<IBitmapFactory, PclBitmapFactory>(Reuse.Singleton);
            Container.Register<IAppSettingsHelper, AppSettingsHelper>(Reuse.Singleton);
            Container.Register<IDispatcherHelper, DispatcherHelper>(Reuse.Singleton);
            Container.Register<INotificationManager, NotificationManager>(Reuse.Singleton);
            Container.Register<ICredentialHelper, CredentialHelper>(Reuse.Singleton);

            var factory = new AudioticaFactory(DispatcherHelper, AppSettingsHelper, BitmapFactory);

            Container.Register<IScrobblerService, ScrobblerService>(Reuse.Singleton);
            Container.Register<SpotifyWebApi>(Reuse.Singleton);
            Container.Register<ISpotifyService, SpotifyService>(Reuse.Singleton);

            Container.RegisterDelegate(r => factory.CreateCollectionSqlService(9), Reuse.Singleton);
            Container.RegisterDelegate(r => factory.CreatePlayerSqlService(4), Reuse.Singleton, named: "BackgroundSql");
            Container.RegisterDelegate(r => factory.CreateCollectionService(SqlService, BgSqlService), Reuse.Singleton);

            Container.Register<Mp3MatchEngine>(Reuse.Singleton);
        }

        public Container Container { get; set; }

        public ICollectionService CollectionService
        {
            get { return Container.Resolve<ICollectionService>(); }
        }

        public INotificationManager NotificationManager
        {
            get { return Container.Resolve<INotificationManager>(); }
        }

        public Mp3MatchEngine Mp3MatchEngine
        {
            get { return Container.Resolve<Mp3MatchEngine>(); }
        }

        public IBitmapFactory BitmapFactory
        {
            get { return Container.Resolve<IBitmapFactory>(); }
        }

        public IAppSettingsHelper AppSettingsHelper
        {
            get { return Container.Resolve<IAppSettingsHelper>(); }
        }

        public IDispatcherHelper DispatcherHelper
        {
            get { return Container.Resolve<IDispatcherHelper>(); }
        }

        public ISqlService SqlService
        {
            get { return Container.Resolve<ISqlService>(); }
        }

        public ISqlService BgSqlService
        {
            get { return Container.Resolve<ISqlService>("BackgroundSql"); }
        }

        public IScrobblerService ScrobblerService
        {
            get { return Container.Resolve<IScrobblerService>(); }
        }

        public ISpotifyService SpotifyService
        {
            get { return Container.Resolve<ISpotifyService>(); }
        }
    }
}