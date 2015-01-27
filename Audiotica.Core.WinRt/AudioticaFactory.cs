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
        private readonly IAppSettingsHelper appSettingsHelper;

        private readonly IBitmapFactory bitmapFactory;

        private readonly IDispatcherHelper dispatcher;

        private readonly string folderPath;

        public AudioticaFactory(
            IDispatcherHelper dispatcher, 
            IAppSettingsHelper appSettingsHelper, 
            IBitmapFactory bitmapFactory)
        {
            this.dispatcher = dispatcher;
            this.appSettingsHelper = appSettingsHelper;
            this.bitmapFactory = bitmapFactory;

#if __ANDROID__
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#elif WINRT
            this.folderPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#endif
        }

        public ICollectionService CreateCollectionService(
            ISqlService collectionSqlService, 
            ISqlService playerSqlService)
        {
            return new CollectionService(
                collectionSqlService, 
                playerSqlService, 
                this.dispatcher, 
                this.appSettingsHelper, 
                this.bitmapFactory, 
                AppConstant.MissingArtworkImage, 
#if __ANDROID__
                "file://" + _folderPath
#elif WINRT
                AppConstant.LocalStorageAppPath
#endif
                , 
                AppConstant.ArtworkPath, 
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
                                 Path = Path.Combine(this.folderPath, "collection.sqldb"), 
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
                                 Path = Path.Combine(this.folderPath, "player.sqldb"), 
                                 OnUpdate = onUpdate
                             };
            return new SqlService(config);
        }
    }
}