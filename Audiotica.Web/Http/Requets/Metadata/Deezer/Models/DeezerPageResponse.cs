using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer.Models
{
    public class DeezerPageResponse<T> : DeezerBaseResponse
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }
        public string Next { get; set; }
    }
}