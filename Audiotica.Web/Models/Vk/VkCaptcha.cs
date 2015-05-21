using Newtonsoft.Json;

namespace Audiotica.Web.Models.Vk
{
    public class VkCaptcha
    {
        public VkCaptcha(string id, string key)
        {
            CaptchaSid = id;
            CaptchaKey = key;
        }

        [JsonProperty("captcha_sid")]
        public string CaptchaSid { get; set; }

        [JsonProperty("captcha_key")]
        public string CaptchaKey { get; set; }
    }
}