using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;

namespace Audiotica.Data.Service.DesignTime
{
    public class DesignAudioticaService : IAudioticaService
    {
        public AudioticaUser CurrentUser
        {
            get
            {
                return new AudioticaUser { Email = "hello@audiotica.fm", Username = "ILoveAudiotica6" };
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }

        public string AuthenticationToken
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public HttpClient CreateHttpClient()
        {
            throw new NotImplementedException();
        }

        public Task<BaseAudioticaResponse> LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<BaseAudioticaResponse> RegisterAsync(string username, string password, string email)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public Task<BaseAudioticaResponse<AudioticaUser>> GetProfileAsync()
        {
            throw new NotImplementedException();
        }

        public Task<BaseAudioticaResponse> SubscribeAsync(
            SubscriptionType plan, 
            SubcriptionTimeFrame timeFrame, 
            AudioticaStripeCard card, 
            string coupon = null)
        {
            throw new NotImplementedException();
        }

        public Task<BaseAudioticaResponse<List<WebSong>>> GetMatchesAsync(string title, string artist, int limit = 1)
        {
            throw new NotImplementedException();
        }

        public Task<AudioticaSpotlight> GetSpotlightAsync()
        {
            return
                Task.FromResult(
                    new AudioticaSpotlight
                    {
                        LargeFeatures =
                            new List<SpotlightFeature>
                            {
                                new SpotlightFeature
                                {
                                    Title = "Cloud sync and more!", 
                                    Text = "Checkout the Audiotica Cloud and get your first month free!", 
                                    ImageUri = "ms-appx:///Assets/ListeningToMusic.jpg"
                                }
                            }, 
                        MediumFeatures =
                            new List<SpotlightFeature>
                            {
                                new SpotlightFeature
                                {
                                    Title = "Cloud sync and more!", 
                                    Text = "Checkout the Audiotica Cloud and get your first month free!", 
                                    ImageUri = "ms-appx:///Assets/ListeningToMusic.jpg"
                                }
                            }
                    });
        }
    }
}