using System.Collections.Generic;
using Audiotica.Windows.Services.RunTime;
using Microsoft.ApplicationInsights;

namespace Audiotica.Windows.Services.Interfaces
{
    internal interface IInsightsService
    {
        TelemetryClient Client { get; }
        InsightsService.InsightsStopwatchEvent TrackTimeEvent(string name, IDictionary<string, string> properties = null);
        void TrackPageView(string name, string parameter);
    }
}