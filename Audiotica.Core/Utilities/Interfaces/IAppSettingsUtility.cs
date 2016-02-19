using System.ComponentModel;

namespace Audiotica.Core.Utilities.Interfaces
{
    public interface IAppSettingsUtility : INotifyPropertyChanged
    {
        string DownloadsPath { get; set; }
        string TempDownloadsPath { get; }
        int Theme { get; set; }
        bool Ads { get; set; }
    }
}