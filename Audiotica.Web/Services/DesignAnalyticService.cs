using System;
using System.Collections.Generic;

namespace Audiotica.Web.Services
{
    public class DesignAnalyticService : IAnalyticService
    {
        public void AnonymousMode()
        {
            throw new NotImplementedException();
        }

        public void Identify(string userId, string username, string email)
        {
            throw new NotImplementedException();
        }

        public void TrackEvent(string name, IDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public void TrackPageView(string name, IDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public StopwatchEvent TrackTimeEvent(string name, IDictionary<string, object> properties = null)
        {
            throw new NotImplementedException();
        }
    }
}