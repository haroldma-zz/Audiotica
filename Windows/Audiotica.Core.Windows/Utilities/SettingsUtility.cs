using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.Foundation.Collections;
using Windows.Storage;
using Audiotica.Core.Extensions;
using Audiotica.Core.Helpers;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;

namespace Audiotica.Core.Windows.Utilities
{
    public class SettingsUtility : ISettingsUtility
    {
        private const string FileFallback = "SettingsUtility-Fallback/{0}.txt";

        private readonly Type[] _primitives =
        {
            typeof (string),
            typeof (char),
            typeof (bool),
            typeof (byte),
            typeof (short),
            typeof (int),
            typeof (long),
            typeof (float),
            typeof (double),
            typeof (decimal),
            typeof (sbyte),
            typeof (ushort),
            typeof (uint),
            typeof (ulong)
        };

        public bool Exists(string key, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            var settings = Container(strategy);
            return settings.ContainsKey(key);
        }

        public void Remove(string key, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            var settings = Container(strategy);
            if (settings.ContainsKey(key))
                settings.Remove(key);
        }

        public void Write<T>(string key, T value, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            var settings = Container(strategy);
            if (IsPrimitive(typeof (T)))
            {
                try
                {
                    settings[key] = value;
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                var json = Serialize(value);
                try
                {
                    settings[key] = json;
                }
                catch
                {
                    // too big, fallback to file
                    var file =
                        AsyncHelper.RunSync(
                            () =>
                                StorageHelper.CreateFileAsync(string.Format(FileFallback, key),
                                    option: CreationCollisionOption.ReplaceExisting));
                    using (var stream = AsyncHelper.RunSync(() => file.OpenStreamForWriteAsync()))
                    {
                        var bytes = Encoding.UTF8.GetBytes(json);
                        stream.Write(bytes, 0, bytes.Length);
                        settings[key] = string.Format(FileFallback, key);
                    }
                }
            }
        }

        public T Read<T>(string key, T otherwise, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            var settings = Container(strategy);
            if (!settings.ContainsKey(key))
                return otherwise;
            try
            {
                var o = settings[key];
                if (IsPrimitive(typeof (T)))
                {
                    return (T) o;
                }

                var json = o.ToString();

                var fallback = string.Format(FileFallback, key);
                if (json == fallback)
                {
                    var file = AsyncHelper.RunSync(() => StorageHelper.GetFileAsync(fallback));
                    using (var stream = AsyncHelper.RunSync(() => file.OpenStreamForReadAsync()))
                    {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    }
                    AsyncHelper.RunSync(() => file.DeleteAsync().AsTask());
                }

                return Deserialize<T>(json);
            }
            catch
            {
                return otherwise;
            }
        }

        private IPropertySet Container(SettingsStrategy strategy)
        {
            return (strategy == SettingsStrategy.Local)
                ? ApplicationData.Current.LocalSettings.Values
                : ApplicationData.Current.RoamingSettings.Values;
        }

        private string Serialize<T>(T item)
        {
            return item.SerializeToJson();
        }

        private T Deserialize<T>(string json)
        {
            return json.TryDeserializeJson<T>();
        }

        private bool IsPrimitive(Type type)
        {
            var nulls = from t in _primitives
                where t.GetTypeInfo().IsValueType
                select typeof (Nullable<>).MakeGenericType(t);
            var all = _primitives.Concat(nulls);
            if (all.Any(x => x.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())))
                return true;
            var nullable = Nullable.GetUnderlyingType(type);
            return nullable != null && nullable.GetTypeInfo().IsEnum;
        }
    }
}