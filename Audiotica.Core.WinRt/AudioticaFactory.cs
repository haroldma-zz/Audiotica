#region

#if __ANDROID__
using Audiotica.Android;
#elif __IOS__
using Audiotica.iOS;
using Foundation;
#elif WINRT
using Audiotica.Core.WinRt;
#endif
using System;
using System.Collections.Generic;
using System.IO;

using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;

using SQLite;

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

        public AudioticaFactory(
            IDispatcherHelper dispatcher, 
            IAppSettingsHelper appSettingsHelper, 
            IBitmapFactory bitmapFactory)
        {
            this._dispatcher = dispatcher;
            this._appSettingsHelper = appSettingsHelper;
            this._bitmapFactory = bitmapFactory;

#if __ANDROID__
            this._folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif __IOS__
            this._folderPath = NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User)[0].AbsoluteString.Replace("file://", "");
#elif WINRT
            this._folderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif
        }

        public ICollectionService CreateCollectionService(
            ISqlService collectionSqlService, 
            ISqlService playerSqlService)
        {
            return new CollectionService(
                collectionSqlService, 
                playerSqlService, 
                this._dispatcher, 
                this._appSettingsHelper, 
                this._bitmapFactory, 
                AppConstant.MissingArtworkImage,
#if __ANDROID__ || __IOS__
                "file://" + this._folderPath
#elif WINRT
                AppConstant.LocalStorageAppPath
#endif
                , AppConstant.ArtworkPath,
                AppConstant.ArtistsArtworkPath);
        }

        public ISqlService CreateCollectionSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type>
            {
                typeof(Artist), 
                typeof(Album), 
                typeof(Song), 
                typeof(Playlist), 
                typeof(PlaylistSong)
            };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes, 
                CurrentVersion = version, 
                Path = Path.Combine(this._folderPath, "collection.sqldb"), 
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }

        public ISqlService CreatePlayerSqlService(int version, Action<SQLiteConnection, double> onUpdate = null)
        {
            var dbTypes = new List<Type> { typeof(QueueSong) };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes, 
                CurrentVersion = version, 
                Path = Path.Combine(this._folderPath, "player.sqldb"), 
                OnUpdate = onUpdate
            };
            return new SqlService(config);
        }
    }
}