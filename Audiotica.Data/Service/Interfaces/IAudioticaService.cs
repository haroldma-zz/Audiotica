using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;

namespace Audiotica.Data.Service.Interfaces
{
    public interface IAudioticaService
    {
        AudioticaUser CurrentUser { get;}
        bool IsAuthenticated { get; }
        string AuthenticationToken { get; }
        HttpClient CreateHttpClient();
        Task<BaseAudioticaResponse> LoginAsync(string username, string password);
        Task<BaseAudioticaResponse> RegisterAsync(string username, string password, string email);
        void Logout();
        Task<BaseAudioticaResponse<AudioticaUser>> GetProfileAsync();
        Task<BaseAudioticaResponse> SubscribeAsync(SubscriptionType plan, SubcriptionTimeFrame timeFrame, AudioticaStripeCard card, string coupon = null);
        Task<BaseAudioticaResponse<List<WebSong>>> GetMatchesAsync(string title, string artist, int limit = 1);

        Task<AudioticaSpotlight> GetSpotlightAsync();
    }
}
