using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.Data.Service.RunTime;

using GalaSoft.MvvmLight;

namespace Audiotica.ViewModel
{
    public class ManualMatchViewModel : ViewModelBase
    {
        private readonly IDispatcherHelper dispatcherHelper;

        private readonly Mp3SearchService searchService;

        public Song CurrentSong { get; set; }

        private ObservableCollection<WebSong> meile;

        private ObservableCollection<WebSong> mp3Clan;

        private ObservableCollection<WebSong> mp3Skull;

        private ObservableCollection<WebSong> mp3Truck;

        private ObservableCollection<WebSong> netease;

        private bool _isNeteaseLoading;

        private bool _isMp3TruckLoading;

        private bool _isMp3ClanLoading;

        private bool _isMeileLoading;

        private bool _isMp3SkullLoading;

        public ManualMatchViewModel(IAppSettingsHelper appSettingsHelper, IDispatcherHelper dispatcherHelper)
        {
            this.dispatcherHelper = dispatcherHelper;
            searchService = new Mp3SearchService(appSettingsHelper);
            MessengerInstance.Register<Song>(this, "manual-match", ReceiveSong);
        }

        public bool IsNeteaseLoading
        {
            get
            {
                return _isNeteaseLoading;
            }
            set
            {
                Set(ref _isNeteaseLoading, value);
            }
        }

        public bool IsMp3TruckLoading
        {
            get
            {
                return _isMp3TruckLoading;
            }
            set
            {
                Set(ref _isMp3TruckLoading, value);
            }
        }

        public bool IsMp3ClanLoading
        {
            get
            {
                return _isMp3ClanLoading;
            }
            set
            {
                Set(ref _isMp3ClanLoading, value);
            }
        }

        public bool IsMeileLoading
        {
            get
            {
                return _isMeileLoading;
            }
            set
            {
                Set(ref _isMeileLoading, value);
            }
        }

        public bool IsMp3SkullLoading
        {
            get
            {
                return _isMp3SkullLoading;
            }
            set
            {
                Set(ref _isMp3SkullLoading, value);
            }
        }

        public ObservableCollection<WebSong> Mp3Truck
        {
            get
            {
                return mp3Truck;
            }

            set
            {
                Set(ref mp3Truck, value);
            }
        }

        public ObservableCollection<WebSong> Mp3Clan
        {
            get
            {
                return mp3Clan;
            }

            set
            {
                Set(ref mp3Clan, value);
            }
        }

        public ObservableCollection<WebSong> Netease
        {
            get
            {
                return netease;
            }

            set
            {
                Set(ref netease, value);
            }
        }

        public ObservableCollection<WebSong> Meile
        {
            get
            {
                return meile;
            }

            set
            {
                Set(ref meile, value);
            }
        }

        public ObservableCollection<WebSong> Mp3Skull
        {
            get
            {
                return mp3Skull;
            }

            set
            {
                Set(ref mp3Skull, value);
            }
        }

        private async void ReceiveSong(Song song)
        {
            CurrentSong = song;
            Mp3Truck = null;
            Mp3Clan = null;
            Meile = null;
            Netease = null;
            Mp3Skull = null;

            var tasks = new List<Task>
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        await dispatcherHelper.RunAsync(() => IsMp3TruckLoading = true);
                        var results =
                            await
                            searchService.SearchMp3Truck(CurrentSong.Name, CurrentSong.Artist.Name, checkAllLinks: true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                IsMp3TruckLoading = false;

                                if (results == null)
                                {
                                    return;
                                }

                                Mp3Truck = new ObservableCollection<WebSong>(results);
                            });
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        await dispatcherHelper.RunAsync(() => IsMp3ClanLoading = true);

                        var results =
                            await
                            searchService.SearchMp3Clan(
                                CurrentSong.Name, 
                                CurrentSong.Artist.Name, 
                                limit: 25, checkAllLinks: true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                IsMp3ClanLoading = false;

                                if (results == null)
                                {
                                    return;
                                }

                                Mp3Clan = new ObservableCollection<WebSong>(results);
                            });
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        await dispatcherHelper.RunAsync(() => IsNeteaseLoading = true);

                        var results =
                            await
                            searchService.SearchNetease(
                                CurrentSong.Name, 
                                CurrentSong.Artist.Name, 
                                limit: 25, checkAllLinks: true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                IsNeteaseLoading = false;

                                if (results == null)
                                {
                                    return;
                                }

                                Netease = new ObservableCollection<WebSong>(results);
                            });
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {

                        await dispatcherHelper.RunAsync(() => IsMeileLoading = true);
                        var results =
                            await
                            searchService.SearchMeile(
                                CurrentSong.Name, 
                                CurrentSong.Artist.Name, 
                                limit: 25, checkAllLinks: true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                IsMeileLoading = false;

                                if (results == null)
                                {
                                    return;
                                }

                                Meile = new ObservableCollection<WebSong>(results);
                            });
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        await dispatcherHelper.RunAsync(() => IsMp3SkullLoading = true);

                        var results =
                            await
                            searchService.SearchMp3Skull(CurrentSong.Name, CurrentSong.Artist.Name, checkAllLinks: true);

                        await dispatcherHelper.RunAsync(
                            () =>
                            {
                                IsMp3SkullLoading = false;

                                if (results == null)
                                {
                                    return;
                                }

                                Mp3Skull = new ObservableCollection<WebSong>(results);
                            });
                    })
            };
            await Task.WhenAll(tasks);
        }
    }
}