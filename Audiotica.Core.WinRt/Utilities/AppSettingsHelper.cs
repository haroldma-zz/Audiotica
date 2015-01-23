#region

using Windows.Storage;
using Audiotica.Core.Utils.Interfaces;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Core.WinRt.Utilities
{
    public class AppSettingsHelper : IAppSettingsHelper
    {
        public string Read(string key)
        {
            return Read<string>(key);
        }

        public T Read<T>(string key)
        {
            return Read(key, default(T));
        }

        public T Read<T>(string key, T defaulValue)
        {
            return Read(key, defaulValue, SettingsStrategy.Local);
        }

        public T Read<T>(string key, SettingsStrategy strategy)
        {
            return Read(key, default(T), strategy);
        }

        public T Read<T>(string key, T defaultValue, SettingsStrategy strategy)
        {
            object obj;

            //Try to get the settings value
            if (GetContainerFromStrategy(strategy).Values.TryGetValue(key, out obj))
            {
                try
                {
                    //Try casting it
                    return (T) obj;
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        public T ReadJsonAs<T>(string key)
        {
            var value = Read(key);
            var obj = default(T);

            //No string found, return the default
            if (string.IsNullOrEmpty(value)) return obj;

            try
            {
                obj = JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
            }

            return obj;
        }

        public void Write(string key, object value)
        {
            Write(key, value, SettingsStrategy.Local);
        }

        public void Write(string key, object value, SettingsStrategy strategy)
        {
            if (GetContainerFromStrategy(strategy).Values.ContainsKey(key))
                GetContainerFromStrategy(strategy).Values[key] = value;

            else
                GetContainerFromStrategy(strategy).Values.Add(key, value);
        }

        public void WriteAsJson(string key, object value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value);
                Write(key, json);
            }
            catch
            {
            }
        }

        private static ApplicationDataContainer GetContainerFromStrategy(SettingsStrategy location)
        {
            switch (location)
            {
                case SettingsStrategy.Roaming:
                    return ApplicationData.Current.RoamingSettings;
                default:
                    return ApplicationData.Current.LocalSettings;
            }
        }
    }
}