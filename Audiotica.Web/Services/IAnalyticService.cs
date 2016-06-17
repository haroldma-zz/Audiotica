using System.Collections.Generic;

namespace Audiotica.Web.Services
{
    public interface IAnalyticService
    {
        void AnonymousMode();

        void Identify(string userId, string username, string email);

        void TrackEvent(string name, IDictionary<string, object> properties = null);

        void TrackPageView(string name, IDictionary<string, object> properties = null);

        StopwatchEvent TrackTimeEvent(string name, IDictionary<string, object> properties = null);
    }
}