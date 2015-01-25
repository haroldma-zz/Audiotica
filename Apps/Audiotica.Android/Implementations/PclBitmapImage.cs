using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics;
using Audiotica.Core.Common;

namespace Audiotica.Android.Implementations
{
    public class PclBitmapImage : IBitmapImage, INotifyPropertyChanged
    {
        private static readonly Dictionary<Uri, Bitmap> BitmapCache = new Dictionary<Uri, Bitmap>(); 

        private Uri _currentUri;
        private object _image;

        public PclBitmapImage()
        {
        }

        public PclBitmapImage(Uri uri)
        {
            SetUri(uri);
        }

        public object Image
        {
            get { return _image; }
            private set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public async void SetUri(Uri uri)
        {
            if (uri == null || _currentUri == uri)
                return;

            _currentUri = uri;

            if (BitmapCache.ContainsKey(uri))
            {
                Image = BitmapCache[uri];
                return;
            }

            if (uri.Scheme.StartsWith("http"))
            {
                var stream = await GetAsync(uri);
                if (stream != null)
                    SetStream(stream);
            }
            else if (uri.Scheme == "file")
            {
                try
                {
                    var stream =
                        await
                            Task.FromResult(File.Open(uri.AbsoluteUri.Replace("file://", ""), FileMode.Open,
                                FileAccess.Read));
                    SetStream(stream);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public async void SetStream(Stream stream)
        {
            try
            {
                Image = await BitmapFactory.DecodeStreamAsync(stream);
                stream.Dispose();

                if (_currentUri != null)
                    BitmapCache.Add(_currentUri, (Bitmap) Image);
            }
            catch
            {
                // ignored
            }
        }

        public void SetDecodedPixel(int size)
        {
            // ignored
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void SetResource(Resources res, int resId)
        {
            try
            {
                Image = await BitmapFactory.DecodeResourceAsync(res, resId);
            }
            catch
            {
                // ignored
            }
        }

        private async Task<Stream> GetAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(uri);
                return resp.IsSuccessStatusCode ? await resp.Content.ReadAsStreamAsync() : null;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}