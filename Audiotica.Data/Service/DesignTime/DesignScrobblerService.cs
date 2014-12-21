#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Service.Interfaces;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.Data.Service.DesignTime
{
    public class DesignScrobblerService : IScrobblerService
    {
        public Task<LastAlbum> GetDetailAlbum(string name, string artist)
        {
            var album = CreateAlbum("Problem", "Ariana Grande",
                "http://musicimage.xboxlive.com/content/music.55004A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US");
            return Task.FromResult(album);
        }

        public Task<LastAlbum> GetDetailAlbumByMbid(string mbid)
        {
            throw new NotImplementedException();
        }

        public Task<LastTrack> GetDetailTrack(string name, string artist)
        {
            throw new NotImplementedException();
        }

        public Task<LastTrack> GetDetailTrackByMbid(string mbid)
        {
            throw new NotImplementedException();
        }

        public Task<LastArtist> GetDetailArtist(string name)
        {
            return Task.FromResult(CreateArtist("Iggy Azalea",
                "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US"));
        }

        public Task<LastArtist> GetDetailArtistByMbid(string mbid)
        {
            throw new NotImplementedException();
        }

        public Task<PageResponse<LastTrack>> GetArtistTopTracks(string name)
        {
            var pageResp = PageResponse<LastTrack>.CreateSuccessResponse(CreateDesignTracks());
            return Task.FromResult(pageResp);
        }

        public Task<PageResponse<LastAlbum>> GetArtistTopAlbums(string name)
        {
            var pageResp = PageResponse<LastAlbum>.CreateSuccessResponse(CreateDesignAlbums());
            return Task.FromResult(pageResp);
        }

        public Task<PageResponse<LastTrack>> SearchTracksAsync(string query, int page = 1, int limit = 30)
        {
            var pageResp = PageResponse<LastTrack>.CreateSuccessResponse(CreateDesignTracks());
            return Task.FromResult(pageResp);
        }

        public Task<PageResponse<LastArtist>> SearchArtistAsync(string query, int page = 1, int limit = 30)
        {
            var pageResp = PageResponse<LastArtist>.CreateSuccessResponse(CreateDesignArtists());
            return Task.FromResult(pageResp);
        }

        public Task<PageResponse<LastAlbum>> SearchAlbumsAsync(string query, int page = 1, int limit = 30)
        {
            throw new NotImplementedException();
        }

        public Task<PageResponse<LastTrack>> GetTopTracksAsync(int page = 1, int limit = 30)
        {
            var pageResp = PageResponse<LastTrack>.CreateSuccessResponse(CreateDesignTracks());
            return Task.FromResult(pageResp);
        }

        public Task<PageResponse<LastArtist>> GetTopArtistsAsync(int page = 1, int limit = 30)
        {
            var pageResp = PageResponse<LastArtist>.CreateSuccessResponse(CreateDesignArtists());
            return Task.FromResult(pageResp);
        }

        public Task<List<LastArtist>> GetSimilarArtistsAsync(string name, int limit = 30)
        {
            return Task.FromResult(CreateDesignArtists());
        }

        public Task<List<LastTrack>> GetSimilarTracksAsync(string name, string artistName, int limit = 30)
        {
            return Task.FromResult(CreateDesignTracks());
        }

        private LastTrack CreateTrack(string title, string artist, string artwork)
        {
            return new LastTrack
            {
                Name = title,
                ArtistName = artist,
                Images =
                    new LastImageSet
                    {
                        ExtraLarge =
                            new Uri(artwork)
                    }
            };
        }

        private List<LastTrack> CreateDesignTracks()
        {
            return new List<LastTrack>
            {
                CreateTrack("Fancy", "Iggy Azalea",
                    "http://musicimage.xboxlive.com/content/music.DA353208-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateTrack("Problem", "Ariana Grande",
                    "http://musicimage.xboxlive.com/content/music.55004A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateTrack("Wiggle", "Jason Derulo",
                    "http://musicimage.xboxlive.com/content/music.D2EB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateTrack("Dark Horse", "Katy Perry",
                    "http://musicimage.xboxlive.com/content/music.0B61F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateTrack("Rude", "Magic!",
                    "http://musicimage.xboxlive.com/content/music.4388EF07-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateTrack("Turn Down for What", "DJ Snake",
                    "http://musicimage.xboxlive.com/content/music.E1810B08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
            };
        }

        private List<LastAlbum> CreateDesignAlbums()
        {
            return new List<LastAlbum>
            {
                CreateAlbum("Fancy", "Iggy Azalea",
                    "http://musicimage.xboxlive.com/content/music.DA353208-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateAlbum("Problem", "Ariana Grande",
                    "http://musicimage.xboxlive.com/content/music.55004A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateAlbum("Wiggle", "Jason Derulo",
                    "http://musicimage.xboxlive.com/content/music.D2EB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateAlbum("Dark Horse", "Katy Perry",
                    "http://musicimage.xboxlive.com/content/music.0B61F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateAlbum("Rude", "Magic!",
                    "http://musicimage.xboxlive.com/content/music.4388EF07-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateAlbum("Turn Down for What", "DJ Snake",
                    "http://musicimage.xboxlive.com/content/music.E1810B08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
            };
        }

        private List<LastArtist> CreateDesignArtists()
        {
            return new List<LastArtist>
            {
                CreateArtist("Iggy Azalea",
                    "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateArtist("Ariana Grande",
                    "http://musicimage.xboxlive.com/content/music.89C84300-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateArtist("Jason Derulo",
                    "http://musicimage.xboxlive.com/content/music.9C080600-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateArtist("Iggy Azalea",
                    "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateArtist("Ariana Grande",
                    "http://musicimage.xboxlive.com/content/music.89C84300-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                CreateArtist("Jason Derulo",
                    "http://musicimage.xboxlive.com/content/music.9C080600-0200-11DB-89CA-0019B92A3933/image?locale=en-US")
            };
        }

        private LastArtist CreateArtist(string artist, string artwork, bool withSimilar = true)
        {
            var a = new LastArtist
            {
                Name = artist,
                MainImage = new LastImageSet {ExtraLarge = new Uri(artwork)},
                Bio = new LastWiki
                {
                    YearFormed = 2011,
                    Content = "In the begining it was called musicDownload... then it was reborn as something else..."
                }
            };

            if (withSimilar)
            {
                a.Similar = new List<LastArtist>
                {
                    CreateArtist("Iggy Azalea",
                        "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US", false),
                    CreateArtist("Ariana Grande",
                        "http://musicimage.xboxlive.com/content/music.89C84300-0200-11DB-89CA-0019B92A3933/image?locale=en-US", false),
                    CreateArtist("Jason Derulo",
                        "http://musicimage.xboxlive.com/content/music.9C080600-0200-11DB-89CA-0019B92A3933/image?locale=en-US", false)
                };
            }

            return a;
        }

        private LastAlbum CreateAlbum(string name, string artist, string artwork)
        {
            return new LastAlbum
            {
                Name = name,
                ArtistName = artist,
                Tracks = CreateDesignTracks(),
                TopTags = new List<LastTag> {new LastTag {Name = "pop"}},
                Images = new LastImageSet {ExtraLarge = new Uri(artwork)}
            };
        }
    }
}