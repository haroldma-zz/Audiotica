using System.Threading.Tasks;

namespace Audiotica.Data.Mp3Providers
{
    public interface IMp3Provider
    {
        Task<string> GetMatch(string title, string artist);
    }
}
