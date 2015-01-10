using System.Collections.Generic;

namespace Audiotica.Data.Model.Deezer
{
    public class DeezerBaseResponse
    {
        public DeezerError error { get; set; }
    }

    public class DeezerResponse<T>
    {
        
    }

    public class DeezerPageResponse<T> : DeezerBaseResponse
    {
        public List<T> data { get; set; }
        public int total { get; set; }
        public string next { get; set; }
    }

    public class DeezerError
    {
        public string type { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
}