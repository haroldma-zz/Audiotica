using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.EchoNest
{
    class EchoBiographyRoot : EchoListResponse
    {
        public List<EchoBiography> biographies { get; set; }
    }

    public class EchoBiography
    {
        public string text { get; set; }
        public string site { get; set; }
        public string url { get; set; }
    }
}
