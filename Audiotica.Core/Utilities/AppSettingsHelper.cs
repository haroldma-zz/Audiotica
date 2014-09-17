#region License

// Copyright (c) 2014 Harry Martinez <harry@zumicts.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System.Diagnostics;
using Windows.Storage;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Core.Utilities
{
    /// <summary>
    ///     Provides a simplify way to read and write strings to the settings ApplicationDataContainer.
    ///     Plus availability to serialize and deserialize complex POCO.
    /// </summary>
    public static class AppSettingsHelper
    {
        #region Private Fields

        private static readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets and deserializes the json value from the settings container pertaining to the key.
        /// </summary>
        public static T ReadJsonAs<T>(string key)
        {
            var value = Read(key);
            var obj = default (T);

            //No string found, return the default
            if (string.IsNullOrEmpty(value)) return default (T);

            try
            {
                obj = JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                Debug.WriteLine("Failed to deserialize JSON from the key '{0}'.", key);
            }

            return obj;
        }

        /// <summary>
        ///     Gets the value from the settings container pertaining to the key as string.
        /// </summary>
        public static string Read(string key)
        {
            return Read<string>(key);
        }

        /// <summary>
        ///     Gets the value from the settings container pertaining to the key.
        /// </summary>
        public static T Read<T>(string key)
        {
            object obj;

            //Try to get the settings value
            if (SettingsContainer.Values.TryGetValue(key, out obj))
            {
                Debug.WriteLine("Found the key '{0}'.", key);

                try
                {
                    //Try casting it
                    return (T)obj;
                }
                catch
                {
                }
            }
            Debug.WriteLine("Key '{0}' not found.", key);
            return default(T);
        }

        /// <summary>
        ///     Writes the object as a json string, using the key, to the container.
        /// </summary>
        public static void WriteAsJson(string key, object value)
        {
            string json = null;

            try
            {
                json = JsonConvert.SerializeObject(value);
            }
            catch
            {
                Debug.WriteLine("Failed to serialize object for the key '{0}'", key);
            }

            Write(key, json);
        }

        /// <summary>
        ///     Writes the value, using the key, to the container.
        /// </summary>
        public static void Write(string key, object value)
        {
            if (SettingsContainer.Values.ContainsKey(key))
                SettingsContainer.Values[key] = value;

            else
                SettingsContainer.Values.Add(key, value);

            Debug.WriteLine("Wroted the value '{0}' with the key '{1}' to settings container.", value, key);
        }

        #endregion
    }
}