using System;
using System.Linq;
using System.Reflection;
using Windows.Foundation.Collections;
using Windows.Storage;
using Audiotica.Core.Utilities.Interfaces;
using Newtonsoft.Json;

namespace Audiotica.Core.Windows.Utilities
{
    public class SettingsUtility : ISettingsUtility
    {
        const string Parse = "Parse";
        readonly Type[] _primitives = {
            typeof (Enum),
            typeof (string),
            typeof (char),
            typeof (Guid),

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
            typeof (ulong),

            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (TimeSpan),
        };

        private IPropertySet Container(SettingsStrategy strategy)
        {
            return (strategy == SettingsStrategy.Local)
                ? ApplicationData.Current.LocalSettings.Values
                : ApplicationData.Current.RoamingSettings.Values;
        }

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
            if (IsPrimitive(typeof(T)))
            {
                try { settings[key] = value.ToString(); }
                catch { settings[key] = string.Empty; }
            }
            else
            {
                var json = Serialize(value);
                try { settings[key] = json; }
                catch { settings[key] = string.Empty; }
            }
        }

        public T Read<T>(string key, T otherwise, SettingsStrategy strategy = SettingsStrategy.Local)
        {
            var settings = Container(strategy);
            if (!settings.ContainsKey(key))
                return otherwise;
            try
            {
                if (IsPrimitive(typeof(T)))
                {
                    var value = settings[key].ToString();
                    var parse = typeof(T).GetRuntimeMethod(Parse, new[] { typeof(string) });
                    return (T)parse.Invoke(null, new object[] { value });
                }
                else
                {
                    var json = settings[key].ToString();
                    return Deserialize<T>(json);
                }
            }
            catch { return otherwise; }
        }

        private string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item);
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        private bool IsPrimitive(Type type)
        {
            var nulls = from t in _primitives
                        where t.GetTypeInfo().IsValueType
                        select typeof(Nullable<>).MakeGenericType(t);
            var all = _primitives.Concat(nulls);
            if (all.Any(x => x.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())))
                return true;
            var nullable = Nullable.GetUnderlyingType(type);
            return nullable != null && nullable.GetTypeInfo().IsEnum;
        }
    }
}