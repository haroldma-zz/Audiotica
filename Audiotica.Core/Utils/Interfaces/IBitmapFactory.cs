using System;
using Audiotica.Core.Common;

namespace Audiotica.Core.Utils.Interfaces
{
    public interface IBitmapFactory
    {
        IBitmapImage CreateImage(Uri uri);
    }
}
