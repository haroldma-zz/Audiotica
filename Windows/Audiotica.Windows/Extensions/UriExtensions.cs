using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Audiotica.Web.Extensions;

namespace Audiotica.Windows.Extensions
{
    public static class UriExtensions
    {
        public static async Task<Stream> GetStreamAsync(this Uri uri)
        {
            if (uri.Scheme.StartsWith("http"))
                using (var response = await uri.GetAsync())
                    return response.IsSuccessStatusCode
                        ? new MemoryStream(await response.Content.ReadAsByteArrayAsync())
                        : null;

            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await file.OpenStreamForReadAsync();
        }
    }
}