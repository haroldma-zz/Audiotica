using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Audiotica.Core.Utils;
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

        private Song currentSong;

        private ObservableCollection<WebSong> mp3Truck;

        private ObservableCollection<WebSong> mp3Clan;

        private ObservableCollection<WebSong> meile;

        private ObservableCollection<WebSong> netease;

        private ObservableCollection<WebSong> mp3Skull;

        public ManualMatchViewModel(IAppSettingsHelper appSettingsHelper, IDispatcherHelper dispatcherHelper)
        {
            this.dispatcherHelper = dispatcherHelper;
            this.searchService = new Mp3SearchService(appSettingsHelper);
            this.MessengerInstance.Register<Song>(this, "manual-match", this.ReceiveSong);
        }

        public ObservableCollection<WebSong> Mp3Truck
        {
            get
            {
                return this.mp3Truck;
            }

            set
            {
                this.Set(ref this.mp3Truck, value);
            }
        }

        public ObservableCollection<WebSong> Mp3Clan
        {
            get
            {
                return this.mp3Clan;
            }

            set
            {
                this.Set(ref this.mp3Clan, value);
            }
        }

        public ObservableCollection<WebSong> Netease
        {
            get
            {
                return this.netease;
            }

            set
            {
                this.Set(ref this.netease, value);
            }
        }

        public ObservableCollection<WebSong> Meile
        {
            get
            {
                return this.meile;
            }

            set
            {
                this.Set(ref this.meile, value);
            }
        }

        public ObservableCollection<WebSong> Mp3Skull
        {
            get
            {
                return this.mp3Skull;
            }

            set
            {
                this.Set(ref this.mp3Skull, value);
            }
        }

        private async void ReceiveSong(Song song)
        {
            this.currentSong = song;
            this.Mp3Truck = null;
            this.Mp3Clan = null;
            this.Meile = null;
            this.Netease = null;
            this.Mp3Skull = null;

            var tasks = new List<Task>
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        var results =
                            await
                            this.searchService.SearchMp3Truck(
                                this.currentSong.Name, 
                                this.currentSong.Artist.Name, 
                                checkAllLinks: true);

                        if (results == null)
                        {
                            return;
                        }

                        dispatcherHelper.RunAsync(() => this.Mp3Truck = new ObservableCollection<WebSong>(results));
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        var results =
                            await
                            this.searchService.SearchMp3Clan(
                                this.currentSong.Name, 
                                this.currentSong.Artist.Name, 
                                limit: 25, 
                                checkAllLinks: true);

                        if (results == null)
                        {
                            return;
                        }

                        dispatcherHelper.RunAsync(() => this.Mp3Clan = new ObservableCollection<WebSong>(results));
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        var results =
                            await
                            this.searchService.SearchNetease(
                                this.currentSong.Name, 
                                this.currentSong.Artist.Name, 
                                limit: 25, 
                                checkAllLinks: true);

                        if (results == null)
                        {
                            return;
                        }

                        dispatcherHelper.RunAsync(() => this.Netease = new ObservableCollection<WebSong>(results));
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        var results =
                            await
                            this.searchService.SearchMeile(
                                this.currentSong.Name, 
                                this.currentSong.Artist.Name, 
                                limit: 25, 
                                checkAllLinks: true);

                        if (results == null)
                        {
                            return;
                        }

                        dispatcherHelper.RunAsync(() => this.Meile = new ObservableCollection<WebSong>(results));
                    }), 
                Task.Factory.StartNew(
                    async () =>
                    {
                        var results =
                            await
                            this.searchService.SearchMp3Skull(
                                this.currentSong.Name, 
                                this.currentSong.Artist.Name, 
                                checkAllLinks: true);

                        if (results == null)
                        {
                            return;
                        }

                        dispatcherHelper.RunAsync(() => this.Mp3Skull = new ObservableCollection<WebSong>(results));
                    })
            };
            await Task.WhenAll(tasks);
        }
    }
}