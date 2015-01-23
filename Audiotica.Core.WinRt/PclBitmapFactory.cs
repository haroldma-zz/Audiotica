#region

using System;
using Audiotica.Core.Common;
using Audiotica.Core.Utils.Interfaces;

#endregion

namespace Audiotica.Core.WinRt
{
    public class PclBitmapFactory : IBitmapFactory
    {
        public IBitmapImage CreateImage(Uri uri)
        {
            return new PclBitmapImage(uri);
        }
    }
}