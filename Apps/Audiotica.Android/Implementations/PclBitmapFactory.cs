using System;
using Audiotica.Core.Common;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Android.Implementations
{
    internal class PclBitmapFactory : IBitmapFactory
    {
        public IBitmapImage CreateImage(Uri uri)
        {
            return new PclBitmapImage(uri);
        }
    }
}