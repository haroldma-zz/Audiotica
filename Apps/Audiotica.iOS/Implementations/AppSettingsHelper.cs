using Audiotica.Core.Utils.Interfaces;

using Foundation;

using Newtonsoft.Json;

namespace Audiotica.iOS.Implementations
{
    internal class AppSettingsHelper : IAppSettingsHelper
    {
        private readonly NSUserDefaults _defaults = NSUserDefaults.StandardUserDefaults;

        public string Read(string key)
        {
            return this.Read<string>(key);
        }

        public T Read<T>(string key)
        {
            return this.Read(key, default(T));
        }

        public T Read<T>(string key, T defaulValue)
        {
            return this.Read(key, defaulValue, SettingsStrategy.Local);
        }

        public T Read<T>(string key, SettingsStrategy strategy)
        {
            return this.Read(key, default(T), strategy);
        }

        public T Read<T>(string key, T defaultValue, SettingsStrategy strategy)
        {
            // strategy is ignored, ios doesn't have a roaming settings option
            object returnValue = this._defaults.ValueForKey(new NSString(key));

            if (returnValue == null)
            {
                return defaultValue;
            }

            return (T)returnValue;
        }

        public T ReadJsonAs<T>(string key)
        {
            var value = this.Read(key);
            var obj = default(T);

            // No string found, return the default
            if (string.IsNullOrEmpty(value))
            {
                return obj;
            }

            try
            {
                obj = JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                // ignored
            }

            return obj;
        }

        public void Write(string key, object value)
        {
            this.Write(key, value, SettingsStrategy.Local);
        }

        public void Write(string key, object value, SettingsStrategy strategy)
        {
            if (value is int)
            {
                this._defaults.SetInt((int)value, key);
            }
            else if (value is long || value is float || value is double)
            {
                this._defaults.SetFloat((long)value, key);
            }
            else if (value is bool)
            {
                this._defaults.SetBool((bool)value, key);
            }
            else
            {
                this._defaults.SetString((string)value, key);
            }
        }

        public void WriteAsJson(string key, object value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value);
                this.Write(key, json);
            }
            catch
            {
                // ignored
            }
        }
    }
}