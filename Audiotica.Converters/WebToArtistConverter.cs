using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Converters
{
    public class WebToArtistConverter : IConverter<WebArtist, Artist>
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IBasicMetadataProvider> _providers;

        public WebToArtistConverter(IEnumerable<IMetadataProvider> providers, ILibraryService libraryService)
        {
            _libraryService = libraryService;
            _providers = providers.FilterAndSort<IBasicMetadataProvider>();
        }

        public async Task<WebArtist> FillPartialAsync(WebArtist other)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);

            if (other.IsPartial)
            {
                var web = await provider.GetArtistAsync(other.Token);
                other.SetFrom(web);
            }
            return other;
        }

        public async Task<List<WebArtist>> FillPartialAsync(IEnumerable<WebArtist> others)
        {
            var tasks = others.Select(FillPartialAsync).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<Artist> ConvertAsync(WebArtist other, bool ignoreLibrary = false)
        {
            await FillPartialAsync(other);

            var artist = new Artist
            {
                Name = other.Name,
                ArtworkUri = other.Artwork.ToString()
            };

            var libraryArtist = _libraryService.Artists.FirstOrDefault(p => p.Name.EqualsIgnoreCase(artist.Name));
            other.PreviousConversion = libraryArtist ?? artist;

            return ignoreLibrary ? artist : libraryArtist ?? artist;
        }

        public async Task<List<Artist>> ConvertAsync(IEnumerable<WebArtist> others, bool ignoreLibrary = false)
        {
            var tasks = others.Select(p => ConvertAsync(p, ignoreLibrary)).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}