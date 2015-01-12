using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;

namespace Audiotica.Data.Service.RunTime
{
    public class GracenoteService
    {
        private string _gnUserId
            ;

        private List<GnRadio> _gnRadios;
        private const string BaseApi = "https://c557056.web.cddbp.net/webapi/json/1.0";
        private const string ClientId = "557056-09A0EA0404EE2FD9B25B2C41BBB4E912";

        private const string UserRegisterPath = BaseApi + "/register?client={0}";
        private const string RadioBasePath = BaseApi + "/radio/";
        private const string RadioCreatePath = RadioBasePath + "create";
        private const string RadioRecommendPath = RadioBasePath + "recommend";
        private const string RadioEventPath = RadioBasePath + "event";

        public string UserId
        {
            get { return _gnUserId ?? (_gnUserId = AppSettingsHelper.Read("GnUserId")); }
            private set
            {
                _gnUserId = value;
                AppSettingsHelper.Write("GnUserId", _gnUserId);
            }
        }

        public List<GnRadio> Radios
        {
            get { return _gnRadios ?? (_gnRadios = AppSettingsHelper.ReadJsonAs<List<GnRadio>>("GnRadios")); }
        }

        private void SaveRadioStations()
        {
            AppSettingsHelper.WriteAsJson("GnRadios", _gnRadios);
        }
    }

    public class GnRadio
    {
        public string ArtistName { get; set; }
        public string GnId { get; set; }
    }
}
