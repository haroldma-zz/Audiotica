using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using NUnit.Framework;

namespace Audiotica.Core.Test
{
    [TestFixture]
    public class ShaTest
    {
        [Test]
        public void Sha1()
        {
            const string input = "sha1 input test";
            const string expected = "135277461872e58c8baa93f25943752bd5e5b85d";
            var actual = SHA.ComputeSHA1(input.ToBytes()).ToHex();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Sha256()
        {
            const string input = "sha256 input test";
            const string expected = "d7ecf2c1caa1c5120a2d90ec7411ba6d26cb83f02fafadcef90bcad201d5b37e";
            var actual = SHA.ComputeSHA256(input.ToBytes()).ToHex();
            Assert.AreEqual(expected, actual);
        }
    }
}