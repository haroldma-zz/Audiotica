#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.Data
{
    public enum Mp3Provider
    {
        Mp3Truck, 

        SoundCloud, 

        Netease, 

        Mp3Clan, 

        Meile, 

        YouTube, 

        Mp3Skull
    }

    public class Mp3MatchEngine
    {
        private readonly IAudioticaService audioticaService;
        private readonly INotificationManager notificationManager;

        private readonly IDispatcherHelper _dispatcherHelper;

        private readonly Mp3Provider[] providers =
        {
             Mp3Provider.Mp3Skull, Mp3Provider.Netease, Mp3Provider.Mp3Truck, Mp3Provider.Mp3Clan, 
             Mp3Provider.Meile, Mp3Provider.SoundCloud
        };

        private readonly Mp3SearchService _service;

        public Mp3MatchEngine(IAppSettingsHelper settingsHelper, IAudioticaService audioticaService, INotificationManager notificationManager, IDispatcherHelper dispatcherHelper)
        {
            this.audioticaService = audioticaService;
            this.notificationManager = notificationManager;
            _dispatcherHelper = dispatcherHelper;
            this.service = new Mp3SearchService(settingsHelper);
        }

        public async Task<string> FindMp3For(string title, string artist)
        {
            title = title.ToLower().Replace("feat.", "ft.") // better alternatives for matching
                .Replace("- bonus track", string.Empty)
                .Replace("bonus track", string.Empty)
                .Replace("- live", "(live)")
                .Replace("- remix", "(remix)")
                .Replace("a cappella", "acappella")
                .Replace("- acoustic version", "(acoustic)")
                .Replace("- acoustic", "(acoustic)")
                .Replace("- cover", "(cover)")
                .Replace("- stereo", string.Empty)
                .Replace("- mono", string.Empty)
                .Replace("- intro", string.Empty)
                .Replace("- no intro", string.Empty)
                .Replace("- ep version", string.Empty)
                .Replace("- deluxe edition", string.Empty)
                .Trim();

            if (audioticaService.IsAuthenticated && audioticaService.CurrentUser.Subscription != SubscriptionType.None)
            {
                var matchResp = await audioticaService.GetMatchesAsync(title, artist);

                if (matchResp.Success && matchResp.Data != null && matchResp.Data.Count > 0)
                {
                    return matchResp.Data.FirstOrDefault().AudioUrl;
                }

                if (matchResp.StatusCode != HttpStatusCode.Unauthorized)
                {
                    await _dispatcherHelper.RunAsync(
                        () =>
                        {
                            notificationManager.ShowError(
                                "Problem with Audiotica Cloud \"{0}\", finding mp3 locally.",
                                matchResp.Message ?? "Unknown");
                        });
                }
            }


            var currentProvider = 0;
            string url = null;

            while (currentProvider < this._providers.Length)
            {
<<<<<<< HEAD
                var mp3Provider = this._providers[currentProvider];
                url = await this.GetMatch(mp3Provider, title, artist).ConfigureAwait(false);
=======
                var mp3Provider = this.providers[currentProvider];
                try
                {
                    url = await this.GetMatch(mp3Provider, title, artist).ConfigureAwait(false);
                }
                catch { }
>>>>>>> origin/development

                if (url != null)
                {
                    break;
                }

                currentProvider++;
            }

            return url;
        }

        public async Task<string> GetMatch(Mp3Provider provider, string title, string artist, string album = null)
        {
            var webSongs = new List<WebSong>();

            switch (provider)
            {
                case Mp3Provider.Netease:
                    webSongs = await this._service.SearchNetease(title, artist, album).ConfigureAwait(false);
                    break;
<<<<<<< HEAD
                case Mp3Provider.YouTube:
                    webSongs = await this._service.SearchYoutube(title, artist, album).ConfigureAwait(false);
                    break;
=======
>>>>>>> origin/development
                case Mp3Provider.Mp3Clan:
                    webSongs = await this._service.SearchMp3Clan(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Meile:
                    webSongs = await this._service.SearchMeile(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Truck:
                    webSongs = await this._service.SearchMp3Truck(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.SoundCloud:
                    webSongs = await this._service.SearchSoundCloud(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Skull:
                    webSongs = await this._service.SearchMp3Skull(title, artist, album).ConfigureAwait(false);
                    break;
            }

            var bestWebSong = webSongs != null && webSongs.Any() ? webSongs.FirstOrDefault(p => p.IsBestMatch) : null;
            return bestWebSong == null ? null : bestWebSong.AudioUrl;
        }
    }
}