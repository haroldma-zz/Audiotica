namespace Audiotica.Core.Interfaces.Utilities
{
    public enum SettingsStrategy
    {
        Local,
        Roaming
    }

    /// <summary>
    ///     Helper class to facilitate access to the application's settings
    /// </summary>
    public interface ISettingsUtility
    {
        string Read(string key);
        T Read<T>(string key);
        T Read<T>(string key, T defaulValue);
        T Read<T>(string key, SettingsStrategy strategy);
        T Read<T>(string key, T defaultValue, SettingsStrategy strategy);
        T ReadJsonAs<T>(string key);
        void Write(string key, object value);
        void Write(string key, object value, SettingsStrategy strategy);
        void WriteAsJson(string key, object value);
    }
}