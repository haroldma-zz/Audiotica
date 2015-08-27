using Newtonsoft.Json;

namespace Audiotica.Web.Models.Vk
{
    public class VkError
    {
        [JsonProperty("error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("captcha_sid")]
        public string CaptchaSid { get; set; }

        [JsonProperty("captcha_img")]
        public string CaptchaImg { get; set; }
    }
}