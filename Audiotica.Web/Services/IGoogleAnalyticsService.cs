using System;

namespace Audiotica.Web.Services
{
    public interface IGoogleAnalyticsService
    {
        void SendEvent(string category, string action, string label, long value);

        void SendException(string description, bool isFatal);

        void SendSocial(string network, string action, string target);

        void SendTiming(TimeSpan time, string category, string variable, string label);

        void SendTransactionItem(string sku, string name, long priceInMicros, long quantity);

        void SendView(string screenName);

        void SetCustomDimension(int index, string value);

        void SetCustomMetric(int index, long value);
    }
}