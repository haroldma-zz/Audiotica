#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.Data.Service.DesignTime
{
    public class DesignXboxMusicService : IXboxMusicService
    {
        #region Helper Methods

        private XboxTrack CreateTrack(string name, XboxArtist artist, string imageUrl)
        {
            return new XboxTrack
            {
                Id = "music.zumicts",
                Name = name,
                Artists = new List<Contributor> {new Contributor("Main", artist)},
                ImageUrl = imageUrl,
                Album = new XboxAlbum {Id = "music.lol", Name = "Haha"},
                Genres = new GenreList {"Pop"}
            };
        }

        private XboxArtist CreateArtist(string name, string imageUrl)
        {
            return new XboxArtist {Id = "music.zumicts", Name = name, ImageUrl = imageUrl, Genres = new GenreList{"Pop"}};
        }

        private XboxAlbum CreateAlbum(string name, string imageUrl, XboxArtist artist = null,
            List<XboxTrack> tracks = null)
        {
            return new XboxAlbum
            {
                Id = "music.zumicts",
                Name = name,
                ImageUrl = imageUrl,
                Artists = new List<Contributor> {new Contributor("Main", artist)},
                Tracks = new XboxPaginatedList<XboxTrack> {Items = tracks},
                Genres = new GenreList {"Pop"}
            };
        }

        #endregion

        public Task<XboxPaginatedList<XboxArtist>> GetFeaturedArtist(int count)
        {
            var results = new XboxPaginatedList<XboxArtist>
            {
                Items = new List<XboxArtist>
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
                        "http://musicimage.xboxlive.com/content/music.9C080600-0200-11DB-89CA-0019B92A3933/image?locale=en-US"),
                }
            };
            return Task.FromResult(results);
        }

        public Task<XboxPaginatedList<XboxAlbum>> GetNewAlbums(int count)
        {
            return GetFeaturedAlbums(5);
        }

        public Task<XboxPaginatedList<XboxAlbum>> GetFeaturedAlbums(int count)
        {
            var results = new XboxPaginatedList<XboxAlbum>
            {
                Items = new List<XboxAlbum>
                {
                    CreateAlbum("Fancy",
                        "http://musicimage.xboxlive.com/content/music.DA353208-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Iggy Azalea", "")),
                    CreateAlbum("Problem",
                        "http://musicimage.xboxlive.com/content/music.55004A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Ariana Grande", "")),
                    CreateAlbum("Wiggle (feat. Snoop Dogg)",
                        "http://musicimage.xboxlive.com/content/music.D2EB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Snoop Dog", "")),
                    CreateAlbum("Dark Horse",
                        "http://musicimage.xboxlive.com/content/music.0B61F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Katy Perry", "")),
                    CreateAlbum("Rude",
                        "http://musicimage.xboxlive.com/content/music.4388EF07-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Magic!", "")),
                    CreateAlbum("Turn Down For What",
                        "http://musicimage.xboxlive.com/content/music.E1810B08-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("DJ Snake", "")),
                    CreateAlbum("All of Me",
                        "http://musicimage.xboxlive.com/content/music.75DADB07-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Someone", "")),
                    CreateAlbum("Happy",
                        "http://musicimage.xboxlive.com/content/music.3A3E0A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Someone", "")),
                    CreateAlbum("Let it Go",
                        "http://musicimage.xboxlive.com/content/music.D0C4F807-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Someone", "")),
                    CreateAlbum("Talk Dirty",
                        "http://musicimage.xboxlive.com/content/music.CFEB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Someone", "")),
                    CreateAlbum("Roar",
                        "http://musicimage.xboxlive.com/content/music.0661F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US",
                        CreateArtist("Someone", ""))
                }
            };

            return Task.FromResult(results);
        }

        public async Task<XboxAlbum> GetAlbumDetails(string id)
        {
            var album = (await GetFeaturedAlbums(1)).Items[1];
            album.Tracks = DesignTracks();
            return album;
        }

        public async Task<List<XboxArtist>> GetRelatedArtists(string id)
        {
            return (await GetFeaturedArtist(1)).Items;
        }

        public Task<List<XboxItem>> GetSpotlight()
        {
            return Task.FromResult(new List<XboxItem>
            {
                new XboxItem
                {
                    ImageUrl = "http://images-eds.xboxlive.com/image?url=JRt.ExKDjXyTfZSxQETAdwl0KqkVHTsd6XuqA6h55kC0t6YrLrpbeeB3eWZbBsksHSBQZx_lX0izzyo_vH6nGRKuyhunCFVW5YFbefkOxQqeyNh5gx3KspAYHdlcwVdtvclGmUCu5pU0NAmp5gzswF7AVCqJZQIi6EySEbP5HvM-",
                    Title = "Maroon 5",
                    Text = "V: Their new album"
                }
            });
        }

        public async Task<XboxArtist> GetArtistDetails(string id)
        {
            var artist = CreateArtist("Iggy Azalea",
                "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US");

            artist.TopTracks = DesignTracks();

            artist.Albums = await GetFeaturedAlbums(1);

            return artist;
        }

        public Task<ContentResponse> Search(string query, int count = 25)
        {
            return Task.FromResult(new ContentResponse()
            {
                Tracks = DesignTracks()
            });
        }

        private XboxPaginatedList<XboxTrack> DesignTracks()
        {
            return new XboxPaginatedList<XboxTrack>
            {
                Items = new List<XboxTrack>
                {
                    CreateTrack("Fancy", CreateArtist("Iggy Azalea", ""),
                        "http://musicimage.xboxlive.com/content/music.DA353208-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Problem", CreateArtist("Ariana Grande", ""),
                        "http://musicimage.xboxlive.com/content/music.55004A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Wiggle (feat. Snoop Dogg)", CreateArtist("Snoop Dog", ""),
                        "http://musicimage.xboxlive.com/content/music.D2EB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Dark Horse", CreateArtist("Katy Perry", ""),
                        "http://musicimage.xboxlive.com/content/music.0B61F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Rude", CreateArtist("Magic!", ""),
                        "http://musicimage.xboxlive.com/content/music.4388EF07-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Turn Down For What", CreateArtist("Someone", "DJ Snake"),
                        "http://musicimage.xboxlive.com/content/music.E1810B08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("All of Me", CreateArtist("Someone", ""),
                        "http://musicimage.xboxlive.com/content/music.75DADB07-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Happy", CreateArtist("Someone", ""),
                        "http://musicimage.xboxlive.com/content/music.3A3E0A08-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Let it Go", CreateArtist("Someone", ""),
                        "http://musicimage.xboxlive.com/content/music.D0C4F807-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Talk Dirty", CreateArtist("Jason Derulo", ""),
                        "http://musicimage.xboxlive.com/content/music.CFEB4008-0100-11DB-89CA-0019B92A3933/image?locale=en-US"),
                    CreateTrack("Roar", CreateArtist("Someone", ""),
                        "http://musicimage.xboxlive.com/content/music.0661F107-0100-11DB-89CA-0019B92A3933/image?locale=en-US")
                }
            };
        }
    }
}