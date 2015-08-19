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
    public class WebToAlbumConverter : IConverter<WebAlbum, Album>
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IBasicMetadataProvider> _providers;
        private readonly IConverter<WebArtist, Artist> _webArtistConverter;

        public WebToAlbumConverter(IEnumerable<IMetadataProvider> providers, ILibraryService libraryService,
            IConverter<WebArtist, Artist> webArtistConverter)
        {
            _libraryService = libraryService;
            _webArtistConverter = webArtistConverter;
            _providers = providers.FilterAndSort<IBasicMetadataProvider>();
        }

        public async Task<Album> ConvertAsync(WebAlbum other)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);

            if (other.IsPartial)
            {
                var web = await provider.GetAlbumAsync(other.Token);
                other.SetFrom(web);
            }

            var album = new Album
            {
                Title = other.Title,
                ArtworkUri = other.Artwork.ToString(),
                Artist = await _webArtistConverter.ConvertAsync(other.Artist),
                Year = other.ReleasedDate?.Year
            };

            var libraryAlbum = _libraryService.Albums.FirstOrDefault(p => p.Title.EqualsIgnoreCase(album.Title));
            other.PreviousConversion = libraryAlbum ?? album;

            return libraryAlbum ?? album;
        }

        public async Task<List<Album>> ConvertAsync(IEnumerable<WebAlbum> others)
        {
            var tasks = others.Select(ConvertAsync).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}