namespace Audiotica.Core.Utilities.Interfaces
{
    public interface IAppSettingsUtility
    {
        string DownloadsPath { get; set; }
        string TempDownloadsPath { get; }
        int Theme { get; set; }
        bool Ads { get; set; }
    }
}