namespace Audiotica.Core.Utils.Interfaces
{
    public interface IAppSettingsHelper
    {
        string Read(string key);
        T Read<T>(string key);
        T Read<T>(string key, SettingsStrategy strategy);
        T Read<T>(string key, T defaulValue);
        T Read<T>(string key, T defaultValue, SettingsStrategy strategy);
        T ReadJsonAs<T>(string key);
        void Write(string key, object value);
        void Write(string key, object value, SettingsStrategy strategy);
        void WriteAsJson(string key, object value);
    }

    public enum SettingsStrategy
    {
        /// <summary>Local, isolated folder</summary>
        Local,

        /// <summary>Cloud, isolated folder. 100k cumulative limit.</summary>
        Roaming
    }
}