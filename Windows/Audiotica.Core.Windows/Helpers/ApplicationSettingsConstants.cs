namespace Audiotica.Core.Windows.Helpers
{
    public static class ApplicationSettingsConstants
    {
        // Data keys
        public const string QueueId = "queueid";
        public const string Position = "position";
        public const string BackgroundTaskState = "backgroundtaskstate"; // Started, Running, Cancelled
        public const string AppState = "appstate"; // Suspended, Resumed
        public const string AppSuspendedTimestamp = "appsuspendedtimestamp";
        public const string AppResumedTimestamp = "appresumedtimestamp";
        public const string IsArtistAdaptiveColorEnabled = "IsArtistColorArtworkEnabled";
        public const string SongSort = "SongSort";
        public const string AlbumSort = "AlbumSort";
    }
}