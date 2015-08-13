using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Helpers;
using Audiotica.Database.Models;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Factory
{
    public class WebToArtistConverter : IConverter<WebArtist, Artist>
    {
        private readonly IMetadataProvider[] _providers;

        public WebToArtistConverter(IMetadataProvider[] providers)
        {
            _providers = providers;
        }

        public async Task<Artist> ConvertAsync(WebArtist other, Action<WebArtist> saveChanges = null)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);

            if (other.IsPartial)
                other = await provider.GetArtistAsync(other.Token);

            var artist = new Artist
            {
                Name = other.Name,
                ArtworkUri = other.Artwork.ToString()
            };

            other.PreviousConversion = artist;
            saveChanges?.Invoke(other);

            return artist;
        }

        public async Task<List<Artist>> ConvertAsync(IEnumerable<WebArtist> others)
        {
            var tasks = others.Select(LibraryConvert).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task<Artist> LibraryConvert(WebArtist webArtist)
        {
            var artist = await ConvertAsync(webArtist);
            var libraryArtist = artist; //TODO: _libraryService
            return libraryArtist ?? artist;
        }
    }
}