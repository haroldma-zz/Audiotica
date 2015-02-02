using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data;
using Audiotica.Data.Collection;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Service.RunTime;
using Audiotica.Data.Spotify;
#if __ANDROID__
using Audiotica.Android.Implementations;
#elif __IOS__
using Audiotica.iOS.Implementations;
#endif

using DryIoc;

namespace Audiotica
{
    public class Locator
    {
        public Locator()
        {
            this.Container = new Container();
            this.Container.Register<IBitmapFactory, PclBitmapFactory>(Reuse.Singleton);
            this.Container.Register<IAppSettingsHelper, AppSettingsHelper>(Reuse.Singleton);
            this.Container.Register<IDispatcherHelper, DispatcherHelper>(Reuse.Singleton);
            this.Container.Register<INotificationManager, NotificationManager>(Reuse.Singleton);
            this.Container.Register<ICredentialHelper, CredentialHelper>(Reuse.Singleton);

            var factory = new AudioticaFactory(this.DispatcherHelper, this.AppSettingsHelper, this.BitmapFactory);

            this.Container.Register<IScrobblerService, ScrobblerService>(Reuse.Singleton);
            this.Container.Register<SpotifyWebApi>(Reuse.Singleton);
            this.Container.Register<ISpotifyService, SpotifyService>(Reuse.Singleton);

            this.Container.RegisterDelegate(r => factory.CreateCollectionSqlService(9), Reuse.Singleton);
            this.Container.RegisterDelegate(
                r => factory.CreatePlayerSqlService(4), 
                Reuse.Singleton, 
                named: "BackgroundSql");
            this.Container.RegisterDelegate(
                r => factory.CreateCollectionService(this.SqlService, this.BgSqlService), 
                Reuse.Singleton);

            this.Container.Register<Mp3MatchEngine>(Reuse.Singleton);
        }

        public Container Container { get; set; }

        public ICollectionService CollectionService
        {
            get
            {
                return this.Container.Resolve<ICollectionService>();
            }
        }

        public INotificationManager NotificationManager
        {
            get
            {
                return this.Container.Resolve<INotificationManager>();
            }
        }

        public Mp3MatchEngine Mp3MatchEngine
        {
            get
            {
                return this.Container.Resolve<Mp3MatchEngine>();
            }
        }

        public IBitmapFactory BitmapFactory
        {
            get
            {
                return this.Container.Resolve<IBitmapFactory>();
            }
        }

        public IAppSettingsHelper AppSettingsHelper
        {
            get
            {
                return this.Container.Resolve<IAppSettingsHelper>();
            }
        }

        public IDispatcherHelper DispatcherHelper
        {
            get
            {
                return this.Container.Resolve<IDispatcherHelper>();
            }
        }

        public ISqlService SqlService
        {
            get
            {
                return this.Container.Resolve<ISqlService>();
            }
        }

        public ISqlService BgSqlService
        {
            get
            {
                return this.Container.Resolve<ISqlService>("BackgroundSql");
            }
        }

        public IScrobblerService ScrobblerService
        {
            get
            {
                return this.Container.Resolve<IScrobblerService>();
            }
        }

        public ISpotifyService SpotifyService
        {
            get
            {
                return this.Container.Resolve<ISpotifyService>();
            }
        }
    }
}