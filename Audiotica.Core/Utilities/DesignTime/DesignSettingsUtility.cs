using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Utilities.DesignTime
{
    public class DesignSettingsUtility : ISettingsUtility
    {
        public bool Exists(string key, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            return false;
        }

        public void Remove(string key, SettingsStrategy strategy = SettingsStrategy.Local)
        {
        }

        public void Write<T>(string key, T value, SettingsStrategy strategy = SettingsStrategy.Local)
        {
        }

        public T Read<T>(string key, T otherwise, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            return default(T);
        }
    }
}