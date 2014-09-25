using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Model.LastFm;
using Audiotica.Data.Model.Musicbrainz;

namespace Audiotica.Data.Service.Interfaces
{
    public interface IScrobblerService
    {
        Task<MbRelease> GetMbAlbum(string id);
        Task<MbArtist> GetMbArtist(string id);

        Task<FmDetailAlbum> GetDetailAlbum(string name, string artist);
        Task<FmDetailTrack> GetDetailTrack(string name, string artist);
        Task<FmDetailArtist> GetDetailArtist(string name);

        Task<FmResults> SearchTracksAsync(string query, int page = 1, int limit = 30);
        Task<FmResults> SearchArtistAsync(string query, int page = 1, int limit = 30);
        Task<FmResults> SearchAlbumsAsync(string query, int page = 1, int limit = 30);

        Task<FmTrackResults> GetTopTracksAsync(int page = 1, int limit = 30);
        Task<FmArtistResults> GetTopArtistsAsync(int page = 1, int limit = 30);

        Task<FmArtistResults> GetSimilarArtistsAsync(int page = 1, int limit = 30);
        Task<FmTrackResults> GetSimilarTracksAsync(int page = 1, int limit = 30);
    }
}
