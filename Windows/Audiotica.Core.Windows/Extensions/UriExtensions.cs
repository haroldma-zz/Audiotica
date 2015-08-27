using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Core.Windows.Extensions
{
    public static class UriExtensions
    {
        public static async Task<Stream> GetStreamAsync(this Uri uri)
        {
            if (uri.Scheme.StartsWith("http"))
                using (var response = await uri.GetAsync())
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsStreamAsync();
                    else
                        return null;
        }
    }
}
