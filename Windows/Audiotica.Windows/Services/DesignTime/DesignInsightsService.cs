using System;
using System.Collections.Generic;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.RunTime;
using Microsoft.ApplicationInsights;

namespace Audiotica.Windows.Services.DesignTime
{
    internal class DesignInsightsService : IInsightsService
    {
        public TelemetryClient Client { get; }

        public InsightsService.InsightsStopwatchEvent TrackTimeEvent(string name,
            IDictionary<string, string> properties = null)
        {
            throw new NotImplementedException();
        }

        public void TrackPageView(string name, string parameter)
        {
            throw new NotImplementedException();
        }
    }
}