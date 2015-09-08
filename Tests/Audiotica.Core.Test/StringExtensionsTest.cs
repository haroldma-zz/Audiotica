using Audiotica.Core.Extensions;
using NUnit.Framework;

namespace Audiotica.Core.Test
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void ToAudioticaSlug()
        {
            const string input = "Skrillex and Diplo - Where Are Ü Now (with Justin Bieber)";
            const string expected = "skrillex diplo where are u now with justin bieber";
            var actual = input.ToAudioticaSlug();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToHtmlStrippedText()
        {
            const string input = "<a href='test'>Hello</a>";
            const string expected = "Hello";
            var actual = input.ToHtmlStrippedText();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToSanitizedFileName()
        {
            const string input = "File* Name?";
            const string expected = "File Name";
            var actual = input.ToSanitizedFileName();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToUnaccentedText()
        {
            const string input = "Jason Derülo";
            const string expected = "Jason Derulo";
            var actual = input.ToUnaccentedText();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToValidFileNameEnding()
        {
            const string input = "File Name:;";
            const string expected = "File Name";
            var actual = input.ToValidFileNameEnding();
            Assert.AreEqual(expected, actual);
        }
    }
}