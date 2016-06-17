using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Audiotica.Web.Services
{
    public class AnalyticService : IAnalyticService
    {
        private readonly IGoogleAnalyticsService _googleAnalyticsService;
        private string _userId;

        public AnalyticService(IGoogleAnalyticsService googleAnalyticsService)
        {
            _googleAnalyticsService = googleAnalyticsService;
#if DEBUG
            IsTrackingEnabled = true;
#else
            IsTrackingEnabled = false;
#endif
        }

        public bool IsTrackingEnabled { get; }

        public void AnonymousMode()
        {
            _userId = null;
        }

        public void Identify(string userId, string username, string email)
        {
        }

        public void TrackEvent(string name, IDictionary<string, object> properties = null)
        {
            if (!IsTrackingEnabled)
                return;
            
            _googleAnalyticsService.SendEvent(name, null, null, 0);
        }

        public void TrackPageView(string name, IDictionary<string, object> properties = null)
        {
            if (!IsTrackingEnabled)
                return;

            _googleAnalyticsService.SendView(name);
        }

        public StopwatchEvent TrackTimeEvent(string name, IDictionary<string, object> properties = null)
        {
            return new StopwatchEvent(this, name, properties, IsTrackingEnabled);
        }
    }

    public class StopwatchEvent : IDisposable
    {
        private readonly bool _isTrackingEnabled;
        private readonly IAnalyticService _service;
        private IDictionary<string, object> _properties;

        internal StopwatchEvent(
            IAnalyticService service,
            string eventName,
            IDictionary<string, object> properties,
            bool isTrackingEnabled)
        {
            _service = service;
            _properties = properties;
            _isTrackingEnabled = isTrackingEnabled;
            EventName = eventName;
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public string EventName { get; }

        public Stopwatch Stopwatch { get; }

        public void AddProperty(string name, string value)
        {
            if (_properties == null)
                _properties = new Dictionary<string, object>();
            _properties.Add(name, value);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            Stopwatch.Stop();

            if (_isTrackingEnabled)
            {
                _properties.Add("processing time", Stopwatch.Elapsed.TotalMilliseconds);
                _service.TrackEvent(EventName, _properties);
            }

            Debug.WriteLine($"Stopwatch Event: {EventName} [{Stopwatch.Elapsed}]");
        }
    }
}