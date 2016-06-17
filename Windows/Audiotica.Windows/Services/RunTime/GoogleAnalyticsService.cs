using System;
using Audiotica.Web.Services;
using GoogleAnalytics;
using GoogleAnalytics.Core;

namespace Audiotica.Windows.Services.RunTime
{
    internal class GoogleAnalyticsService : IGoogleAnalyticsService
    {
        public void SendEvent(string category, string action, string label, long value)
        {
            EasyTracker.GetTracker().SendEvent(category, action, label, value);
        }

        public void SendException(string description, bool isFatal)
        {
            EasyTracker.GetTracker().SendException(description, isFatal);
        }

        public void SendSocial(string network, string action, string target)
        {
            EasyTracker.GetTracker().SendSocial(network, action, target);
        }

        public void SendTiming(TimeSpan time, string category, string variable, string label)
        {
            EasyTracker.GetTracker().SendTiming(time, category, variable, label);
        }

        public void SendTransactionItem(string sku, string name, long priceInMicros, long quantity)
        {
            EasyTracker.GetTracker().SendTransactionItem(new TransactionItem(sku, name, priceInMicros, quantity));
        }

        public void SendView(string screenName)
        {
            EasyTracker.GetTracker().SendView(screenName);
        }

        public void SetCustomDimension(int index, string value)
        {
            EasyTracker.GetTracker().SetCustomDimension(index, value);
        }

        public void SetCustomMetric(int index, long value)
        {
            EasyTracker.GetTracker().SetCustomMetric(index, value);
        }
    }
}