using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Audiotica.Web.Extensions
{
    /// <summary>
    ///     Extensions needed for Restless.
    /// </summary>
    public static class RestlessExtensions
    {
        #region ThrowIf... extensions

        /// <summary>
        ///     Throws an ArgumentException if the given predicate returns true for the given object.
        /// </summary>
        /// <typeparam name="T">The type of the given object. Must not be set explicit.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="predicate">the predicate that is called with the given object as argument.</param>
        /// <param name="msg">The message added to the exception.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIf<T>(this T obj, Func<T, bool> predicate, string msg,
            [CallerMemberName] string memberName = "")
        {
            if (predicate(obj))
                throw new ArgumentException(memberName + " - " + (msg ?? ""));
        }

        /// <summary>
        ///     Throws an ArgumentException if the given predicate returns false for the given object.
        /// </summary>
        /// <typeparam name="T">The type of the given object. Must not be set explicit.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="predicate">the predicate that is called with the given object as argument.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNot<T>(this T obj, Func<T, bool> predicate, string msg,
            [CallerMemberName] string memberName = "")
        {
            if (!predicate(obj))
                throw new ArgumentException(memberName + "-" + (msg ?? ""));
        }

        /// <summary>
        ///     Throws an ArgumentNullException when the given IEnumerable is null, or an ArgumentException if it is empty.
        ///     Can be used for arrays too.
        /// </summary>
        /// <typeparam name="T">The type of the given objects inside the IEnumerable. Must not be set explicit.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> enumerable, string msg = "",
            [CallerMemberName] string memberName = "")
        {
            if (enumerable == null)
                throw new ArgumentNullException(memberName + "-" + (msg ?? ""));
            if (!enumerable.Any())
                throw new ArgumentException("IEnumerable is empty.", memberName + "-" + (msg ?? ""));
        }

        /// <summary>
        ///     Throws an ArgumentNullException when the given string is null, or an ArgumentException if it is empty.
        /// </summary>
        /// <param name="obj">The string.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrEmpty(this string obj, string msg = "",
            [CallerMemberName] string memberName = "")
        {
            if (obj == null)
                throw new ArgumentNullException(memberName + "-" + (msg ?? ""));

            if (obj.Length == 0)
                throw new ArgumentException("String is empty", memberName + "-" + (msg ?? ""));
        }

        /// <summary>
        ///     Throws an exception if the given object is null.
        /// </summary>
        /// <typeparam name="T">The type of the object. Must not be set explicit.</typeparam>
        /// <param name="obj">The given object.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNull<T>(this T obj, string msg = "", [CallerMemberName] string memberName = "")
        {
            if (obj == null)
            {
                if (string.IsNullOrEmpty(msg))
                    msg = obj.GetType().Name;
                throw new ArgumentNullException(memberName + "-" + (msg ?? ""));
            }
        }

        /// <summary>
        ///     Throws an exception if the given object is null or if the obj.ToString() is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the object. Must not be set explicit.</typeparam>
        /// <param name="obj">The given object.</param>
        /// <param name="msg">The message that is added to the exception if it is thrown.</param>
        /// <param name="memberName">The name of the method that called ThrowIf (CallerMemberName).</param>
        public static void ThrowIfNullOrToStrEmpty<T>(this T obj, string msg = "",
            [CallerMemberName] string memberName = "")
        {
            if (string.IsNullOrEmpty(msg))
                msg = obj.GetType().Name;

            if (obj == null)
                throw new ArgumentNullException(memberName + "-" + (msg));

            obj.ToString().ThrowIfNullOrEmpty(msg + " ToString()", memberName);
        }

        #endregion

        #region Parameter and url (Dictionary<string, object>) extensions

        /// <summary>
        ///     Make a parameter string.
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string CreateParamStr(this Dictionary<string, object> paramList)
        {
            var index = 0;
            var result = paramList.Aggregate("",
                (s, p) =>
                    s + WebUtility.UrlEncode(p.Key) + "=" + WebUtility.UrlEncode(p.Value.ToString()) +
                    (index++ < paramList.Count - 1 ? "&" : ""));
            return result;
        }

        private static string AndStr(string str)
        {
            return str == "" ? "" : "&";
        }

        /// <summary>
        ///     Make a parameter string.
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string CreateParamStr(this Dictionary<string, List<object>> paramList)
        {
            //var query = from value in 
            return paramList.Aggregate("", (s, curr) => s + AndStr(s) +
                                                        (from value in curr.Value
                                                            select curr.Key + "=" + value).
                                                            Aggregate("", (seed, c) => seed + AndStr(seed) + c)
                );
        }

        /// <summary>
        /// </summary>
        /// <param name="urlParams"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string FormatUrlWithParams(this Dictionary<string, object> urlParams, string url)
        {
            var builder = new StringBuilder(url);
            if (urlParams != null)
            {
                foreach (var element in urlParams)
                {
                    var pattern = "{" + element.Key + "}";
                    if (url.Contains(pattern))
                        builder.Replace(pattern, WebUtility.UrlEncode(element.Value.ToString()));
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryParams"></param>
        /// <param name="param"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string CreateRequestUri(this string url,
            Dictionary<string, object> queryParams,
            Dictionary<string, object> param,
            string method)
        {
            url = queryParams.FormatUrlWithParams(url);

            // Add query parameter to url
            var query = queryParams.CreateParamStr();

            // if method is GET treat all added parameters as query parameter
            if (method == "GET")
            {
                // Add parameter that are added with Param(..) too, because this is a GET method.
                var pQuery = param.CreateParamStr();
                // set query to post param. query string if query is still emtpy (because no QParam(..) were added)
                // only Param(..) was used even this is a GET method.
                query = (string.IsNullOrEmpty(query) ? pQuery : query + "&" + pQuery);
            }

            if (!string.IsNullOrEmpty(query))
                url += "?" + query;
            return url;
        }

        public static string CreateRequestUri(this string url,
            Dictionary<string, object> queryParams,
            Dictionary<string, List<object>> param,
            string method)
        {
            url = queryParams.FormatUrlWithParams(url);

            // Add query parameter to url
            var query = queryParams.CreateParamStr();

            // if method is GET treat all added parameters as query parameter
            if (method == "GET")
            {
                // Add parameter that are added with Param(..) too, because this is a GET method.
                var pQuery = param.CreateParamStr();
                // set query to post param. query string if query is still emtpy (because no QParam(..) were added)
                // only Param(..) was used even this is a GET method.
                query = (string.IsNullOrEmpty(pQuery) ? query : query + "&" + pQuery);
            }

            if (!string.IsNullOrEmpty(query))
                url += "?" + query;
            return url;
        }

        #endregion
    }
}