#region License

// Copyright (c) 2014 Harry
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System.Threading.Tasks;
using Audiotica.Data.Mp3Providers;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.Data
{
    public static class Mp3MatchEngine
    {
        private static readonly IMp3Provider[] Providers =
        {
            //new VkProvider(), Need to finish this later, VK uses login and sometimes captcha
            //but it is the best for mp3 matching
            new NeteaseProvider(),
            new MeileProvider(),
            new SoundCloudProvider()
        };

        public static async Task<string> FindMp3For(XboxTrack track)
        {
            //match engines get better results using ft instead of feat
            //so rename if it contains that
            var title = track.Name.Replace("feat.", "ft.");
            var artist = track.Artists[0].Artist.Name;


            var currentProvider = 0;
            while (currentProvider < Providers.Length)
            {
                var mp3Provider = Providers[currentProvider];
                var url = await mp3Provider.GetMatch(title, artist);

                if (url != null)
                    return url;

                currentProvider++;
            }

            return null;
        }
    }
}