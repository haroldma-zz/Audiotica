#region

using System;
using System.Collections.Generic;
using System.IO;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using SQLite;

#if __ANDROID__
using Audiotica.Android;
#elif WINRT
using Audiotica.Core.WinRt;
#endif

#endregion

// ReSharper disable once CheckNamespace
namespace Audiotica
{
    public class AudioticaFactory
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IBitmapFactory _bitmapFactory;
        private readonly IDispatcherHelper _dispatcher;
        private readonly string _folderPath;

        public AudioticaFactory(IDispatcherHelper dispatcher, IAppSettingsHelper appSettingsHelper,
            IBitmapFactory bitmapFactory)
        {
            _dispatcher = dispatcher;
            _appSettingsHelper = appSettingsHelper;
            _bitmapFactory = bitmapFactory;

#if __ANDROID__
            _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif WINRT
            _folderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif
        }

        public ISqlService CreateCollectionSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
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
                Path = Path.Combine(_folderPath, "collection.sqldb"),
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }

        public ISqlService CreatePlayerSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
            {
                typeof (QueueSong)
            };
            var config = new SqlServiceConfig()
            {
                Tables = dbTypes,
                CurrentVersion = version,
                Path = Path.Combine(_folderPath, "player.sqldb"),
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }

        public ICollectionService CreateCollectionService(ISqlService collectionSqlService, ISqlService playerSqlService)
        {
            return new CollectionService(collectionSqlService, playerSqlService, _dispatcher, _appSettingsHelper,
                _bitmapFactory,  AppConstant.MissingArtworkImage, 
#if __ANDROID__
                "file://" + _folderPath
#elif WINRT
                AppConstant.LocalStorageAppPath
#endif
                , AppConstant.ArtworkPath, AppConstant.ArtistsArtworkPath);
        }
    }
}