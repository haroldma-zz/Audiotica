namespace Audiotica.Core.Utilities.Interfaces
{
    public enum SettingsStrategy { Local, Roam, Temp }

    /// <summary>
    ///     Helper class to facilitate access to the application's settings
    /// </summary>
    public interface ISettingsUtility
    {
        bool Exists(string key, SettingsStrategy strategy = SettingsStrategy.Local);
        void Remove(string key, SettingsStrategy strategy = SettingsStrategy.Local);
        void Write<T>(string key, T value, SettingsStrategy strategy = SettingsStrategy.Local);
        T Read<T>(string key, T otherwise, SettingsStrategy strategy = SettingsStrategy.Local);
    }
}