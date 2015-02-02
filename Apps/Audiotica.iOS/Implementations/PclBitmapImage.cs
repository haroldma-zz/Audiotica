using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Audiotica.Core.Common;
using Audiotica.Core.Utils.Interfaces;

using Foundation;

using UIKit;

namespace Audiotica.iOS.Implementations
{
    internal class PclBitmapFactory : IBitmapFactory
    {
        public IBitmapImage CreateImage(Uri uri)
        {
            return new PclBitmapImage(uri);
        }
    }

    internal class PclBitmapImage : IBitmapImage, INotifyPropertyChanged
    {
        private static readonly Dictionary<Uri, UIImage> BitmapCache = new Dictionary<Uri, UIImage>();

        private Uri _currentUri;

        private object _image;

        public PclBitmapImage()
        {
        }

        public PclBitmapImage(Uri uri)
        {
            this.SetUri(uri);
        }

        public object Image
        {
            get
            {
                return this._image;
            }

            private set
            {
                this._image = value;
                this.OnPropertyChanged();
            }
        }

        public async void SetUri(Uri uri)
        {
            if (uri == null || this._currentUri == uri)
            {
                return;
            }

            this._currentUri = uri;

            UIImage value;
            if (BitmapCache.TryGetValue(uri, out value))
            {
                this.Image = value;
                return;
            }

            if (uri.Scheme.StartsWith("http"))
            {
                var stream = await this.GetAsync(uri);
                if (stream != null)
                {
                    this.SetStream(stream);
                }
            }
            else if (uri.Scheme == "file")
            {
                try
                {
                    var stream =
                        await
                        Task.FromResult(
                            File.Open(uri.AbsoluteUri.Replace("file://", string.Empty), FileMode.Open, FileAccess.Read));
                    this.SetStream(stream);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void SetBundle(string name)
        {
            this.Image = UIImage.FromFile(name);
        }

        public async void SetStream(Stream stream)
        {
            try
            {
                this.Image = await Task.FromResult(UIImage.LoadFromData(NSData.FromStream(stream)));
                stream.Dispose();

                if (this._currentUri != null)
                {
                    BitmapCache.Add(this._currentUri, (UIImage)this.Image);
                }
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
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
    }
}