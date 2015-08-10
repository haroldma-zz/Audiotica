using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer.Models
{

    public class DataResponse<T>
    {
        public List<T> Data { get; set; }
    }
}