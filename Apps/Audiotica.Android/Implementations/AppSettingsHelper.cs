using Android.App;
using Android.Content;
using Audiotica.Core.Utils.Interfaces;
using Newtonsoft.Json;

namespace Audiotica.Android.Implementations
{
    public class AppSettingsHelper : IAppSettingsHelper
    {
        public AppSettingsHelper()
        { 
            LocalSettings = Application.Context.GetSharedPreferences("Audiotica.Android", FileCreationMode.Private);
        }

        public ISharedPreferences LocalSettings { get; set; }

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
            //strategy is ignored, Android doesn't have a roaming settings option

            var type = typeof (T);
            var defaultObject = (object) defaultValue;
            object returnValue;

            if (type == typeof (int))
                returnValue = LocalSettings.GetInt(key, (int) defaultObject);
            else if (type == typeof (long))
                returnValue = LocalSettings.GetLong(key, (long) defaultObject);
            else if (type == typeof (float))
                returnValue = LocalSettings.GetFloat(key, (float) defaultObject);
            else if (type == typeof (bool))
                returnValue = LocalSettings.GetBoolean(key, (bool) defaultObject);

            else
                returnValue = LocalSettings.GetString(key, (string) defaultObject);

            return (T) returnValue;
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
                // ignored
            }

            return obj;
        }

        public void Write(string key, object value)
        {
            Write(key, value, SettingsStrategy.Local);
        }

        public void Write(string key, object value, SettingsStrategy strategy)
        {
            var type = value.GetType();
            var editor = LocalSettings.Edit();

            if (type == typeof (int))
                editor.PutInt(key, (int) value);
            else if (type == typeof (long))
                editor.PutLong(key, (long) value);
            else if (type == typeof(float) || type == typeof(double))
                editor.PutFloat(key, (float) value);
            else if (type == typeof (bool))
                editor.PutBoolean(key, (bool) value);
            else
                editor.PutString(key, (string) value);

            editor.Commit();
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
                // ignored
            }
        }
    }
}