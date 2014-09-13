using System.Collections.Generic;

namespace Audiotica.Data.Model
{
    public class VKSong
    {
        public int aid { get; set; }
        public int owner_id { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public string url { get; set; }
        public string lyrics_id { get; set; }
        public int genre { get; set; }
    }

    public class VKRoot
    {
        public VKError error { get; set; }
        public List<object> response { get; set; }
    }

    public class VKError
    {
        public string error_msg { get; set; }
        public string captcha_sid { get; set; }
        public string captcha_img { get; set; }
    }

    public class VKCaptcha
    {
        public VKCaptcha(string id, string key)
        {
            captcha_sid = id;
            captcha_key = key;
        }
        public string captcha_sid { get; set; }
        public string captcha_key { get; set; }
    }
}
