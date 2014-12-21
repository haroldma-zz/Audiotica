#region

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Newtonsoft.Json.Linq;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class VkProvider : IMp3Provider
    {
        private const string SearchUrl = "https://api.vk.com/method/audio.search.json?&q={0}&access_token={1}&auto_complete=1&v=3.0&test_mode=0&count={2}";

        //TODO [Harry,20140913] make this work, needs login and captcha handling
        //taken from Audiotica 6-alpha1-project1
        public async Task<string> GetMatch(string title, string artist, string album = null)
        {
            var url = string.Format(SearchUrl, title + " " + artist, "TODO:NEED TOKEN", 5);

//            if (captcha != null)
//                url += "&captcha_sid=" + captcha.captcha_sid + "&captcha_key=" + captcha.captcha_key;

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(new Uri(url));

                var json = await resp.Content.ReadAsStringAsync();

                //ThrowIfError(resp);

                var parseResp = await json.DeserializeAsync<VKRoot>();

//                if (parseResp.error != null)
//                {
//                    if (parseResp.error.error_msg.Contains("Captcha"))
//                        throw new CaptachaException(parseResp.error.captcha_sid, parseResp.error.captcha_img);
//                    if (parseResp.error.error_msg.Contains("authorization"))
//                        throw new UnAuthException();
//                    throw new Exception();
//                }

                var linq = parseResp.response.Where(p => p is JObject).ToList();

                return linq.Select(t => t as JObject).Select(o => o.ToObject(typeof(VKSong)) as VKSong).FirstOrDefault().url;
            }
        }
    }
}