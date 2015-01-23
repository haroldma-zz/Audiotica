using System;
using System.IO;

namespace Audiotica.Core.Common
{
    public interface IBitmapImage
    {
        object Image { get; }
        void SetUri(Uri uri);
        void SetStream(Stream stream);
        void SetDecodedPixel(int size);
    }
}