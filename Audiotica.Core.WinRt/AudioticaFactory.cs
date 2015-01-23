#region

using System;
using System.Collections.Generic;
using System.IO;
using Windows.Graphics.Display;
using Windows.Storage;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using SQLite;

#endregion

namespace Audiotica.Core.WinRt
{
    public class AudioticaFactory
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IBitmapFactory _bitmapFactory;
        private readonly IDispatcherHelper _dispatcher;

        public AudioticaFactory(IDispatcherHelper dispatcher, IAppSettingsHelper appSettingsHelper,
            IBitmapFactory bitmapFactory)
        {
            _dispatcher = dispatcher;
            _appSettingsHelper = appSettingsHelper;
            _bitmapFactory = bitmapFactory;
        }

        public ISqlService CreateCollectionSqlService(double version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
            {
                typeof (Artist),
                typeof (Album),
                typeof (Song),
                typeof (Playlist),
                typeof (PlaylistSong)
            };
            var config = new SqlServiceConfig()
            {
                Tables = dbTypes,
                CurrentVersion = version,
                Path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "collection.sqldb"),
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }

        public ISqlService CreatePlayerSqlService(double version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
            {
                typeof (QueueSong)
            };
            var config = new SqlServiceConfig()
            {
                Tables = dbTypes,
                CurrentVersion = version,
                Path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "player.sqldb"),
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }

        public ICollectionService CreateCollectionService(ISqlService collectionSqlService, ISqlService playerSqlService)
        {
            return new CollectionService(collectionSqlService, playerSqlService, _dispatcher, _appSettingsHelper,
                _bitmapFactory, CollectionConstant.MissingArtworkImage, CollectionConstant.LocalStorageAppPath, CollectionConstant.ArtworkPath, CollectionConstant.ArtistsArtworkPath);
        }
    }
}