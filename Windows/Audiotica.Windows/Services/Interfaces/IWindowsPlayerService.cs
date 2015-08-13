using Audiotica.Database.Models;
using Audiotica.Web.Models;

namespace Audiotica.Windows.Services
{
    public interface IWindowsPlayerService
    {
        void Play(WebSong song);
        void Play(Track track);
    }
}