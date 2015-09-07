using Audiotica.Web.Extensions;
using Audiotica.Web.Models;
using NUnit.Framework;

namespace Audiotica.Web.Test
{
    [TestFixture]
    public class MatchSongExtensionsTest
    {
        [Test]
        public void SetNameAndArtistFromTitle()
        {
            var match = new MatchSong();
            match.SetNameAndArtistFromTitle("Childish Gambino - 3005", true);
            Assert.AreEqual(match.Title, "3005");
            Assert.AreEqual(match.Artist, "Childish Gambino");
        }
    }
}