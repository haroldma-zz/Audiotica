using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Audiotica.Web.Http;
using Audiotica.Web.Serializer;
using static System.String;
using Void = Audiotica.Web.Models.Void;

namespace Audiotica.Web.Extensions
{
    public static class RestRequestExtensions
    {
        #region Upload file via multipart form and stream content. Possible parameters are added via FormUrlEncoded content.

        /// <summary>
        ///     Uploads a binary (file) stream using a MultipartFormDataContent and a (sub) StreamContent.
        ///     AddFormUrl() is called before the StreamContent is added to the MultipartFormDataContent.
        ///     AddFormUrl() will add all parameter to the request that are added with Param(..).
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="localPath">The "path" of the (file) stream that will be uploaded.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>
        ///     A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.
        /// </returns>
        public static async Task<RestResponse<T>> UploadFileFormData<T>(
            this RestRequest request,
            Stream streamContent,
            string contentType,
            string localPath,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            // Only check for null or empty, not for existing
            // here its only used for content-disposition
            // the file is should be loaded allready, see fileStream
            localPath.ThrowIfNullOrEmpty("localPath");

            // TODO: create and add (random?) boundary
            request.AddMultipartForm();
            if (request.Param.Count > 0)
                request.AddFormUrl(); // Add form url encoded parameter to request if needed    

            request.AddStream(
                streamContent,
                contentType,
                1024,
                Path.GetFileNameWithoutExtension(localPath),
                Path.GetFileName(localPath));

            return await request.BuildAndSendRequest(successAction, errorAction);
        }

        #endregion

        #region Set request methods GET, HEAD, POST, PUT ...

        /// <summary>
        ///     Sets the HttpMethod given by string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="method">The HttpMethod string. For example "GET".</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Method<T>(this T request, string method)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod(method);
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to GET.
        /// </summary>
        /// <returns>this.</returns>
        public static T Get<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = HttpMethod.Get;
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to HEAD.
        /// </summary>
        /// <returns>this.</returns>
        public static T Head<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("HEAD");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to POST.
        /// </summary>
        /// <returns>this.</returns>
        public static T Post<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("POST");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to PUT.
        /// </summary>
        /// <returns>this.</returns>
        public static T Put<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("PUT");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to DELETE.
        /// </summary>
        /// <returns>this.</returns>
        public static T Delete<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("DELETE");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to TRACE.
        /// </summary>
        /// <returns>this.</returns>
        public static T Trace<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("TRACE");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to CONNECT.
        /// </summary>
        /// <returns>this.</returns>
        public static T Connect<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("CONNECT");
            return request;
        }

        /// <summary>
        ///     Set the HttpMethod to PATCH.
        /// </summary>
        /// <returns>this.</returns>
        public static T Patch<T>(this T request)
            where T : RestRequest
        {
            request.HttpRequest.Method = new HttpMethod("PATCH");
            return request;
        }

        #endregion

        #region Set and add HttpContent, Byte, form url encoded, multipart, multipart form, stream and string content.

        /// <summary>
        ///     Adds a HttpContent to the Request.
        ///     Multiple contents can be set.
        ///     For example first a MultipartContent can be added with AddMultipart(..).
        ///     Then a StreamContent can be added to this MultipartContent with AddStream(..).
        ///     If the underlying request.Content is a MultipartContent or MultipartFormDataContent
        ///     -&gt; the content is added to this MultipartContent.
        ///     Otherwise the request.Content is simply set to the given content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="content">The HttpContent.</param>
        /// <param name="name">A name can be needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name can be needed when content is a MultipartFormDataContent already.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddContent<T>(this T request, HttpContent content, string name = "", string fileName = "")
            where T : RestRequest
        {
            content.ThrowIfNull(nameof(content));

            // If content is a multipart already then add it as sub content to the multipart.
            var dataContent = request.Request.Content as MultipartFormDataContent;
            if (dataContent != null)
            {
                // For MultipartFormDataContent name and fileName must be set, so chech them first.
                name.ThrowIfNullOrEmpty("name");
                fileName.ThrowIfNullOrEmpty("fileName");
                dataContent.Add(content, name, fileName);
            }
            else
            {
                var multipartContent = request.Request.Content as MultipartContent;
                if (multipartContent != null)
                    multipartContent.Add(content);
                else
                    request.Request.Content = content;
            }
            return request;
        }

        /// <summary>
        ///     Sets the underlying HttpContent to null.
        /// </summary>
        /// <returns>this.</returns>
        public static T ClearContent<T>(this T request)
            where T : RestRequest
        {
            request.Request.Content = null;
            return request;
        }

        /// <summary>
        ///     Adds a ByteArrayContent to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="buffer">The buffer containing data.</param>
        /// <param name="name">
        ///     A name is needed if underlying HttpContent is MultipartFormDataContent. (for example multiple file
        ///     uploads)
        /// </param>
        /// <param name="fileName">A file name is needed if underlying HttpContent is MultipartFormDataContent.</param>
        /// <returns>
        ///     this
        /// </returns>
        public static T AddByteArray<T>(this T request, byte[] buffer, string name = "", string fileName = "")
            where T : RestRequest
        {
            buffer.ThrowIfNullOrEmpty(nameof(buffer));
            return request.AddContent(new ByteArrayContent(buffer), name, fileName);
        }

        /// <summary>
        ///     Adds a FormUrlEncodedContent to the request.
        ///     If kvPairs are given and kvPairs.Length % 2 is even and length is not zero
        ///     the kvPairs array is treated as a key value pair list.
        ///     These key-value pairs are added to the FormUrlEncodedContent on construction.
        ///     If no kvPairs are given all parameters added with Param(..) are added to the new
        ///     FromUrlEncodedContent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="kvPairs">The list of key-value pairs. Must contain an even number of string objects if used.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddFormUrl<T>(this T request, params string[] kvPairs)
            where T : RestRequest
        {
            var keyValues = new List<KeyValuePair<string, string>>();

            if (kvPairs == null || kvPairs.Length == 0)
            {
                keyValues.AddRange(from element in request.Param
                    from value in element.Value
                    select new KeyValuePair<string, string>(element.Key, value.ToString()));
            }
            else
            {
                kvPairs.ThrowIf(pairs => pairs.Length%2 != 0, "kvPairs. No value for every name given.");

                for (var i = 0; i < kvPairs.Length; i += 2)
                    keyValues.Add(new KeyValuePair<string, string>(kvPairs[i], kvPairs[i + 1]));
            }
            return request.AddContent(new FormUrlEncodedContent(keyValues));
        }

        /// <summary>
        ///     Adds a MultipartContent to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="subtype">The sub type if needed.</param>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddMultipart<T>(this T request, string subtype = "", string boundary = "")
            where T : RestRequest
        {
            HttpContent content;

            if (IsNullOrEmpty(subtype))
                content = new MultipartContent();
            else if (IsNullOrEmpty(boundary))
                content = new MultipartContent(subtype);
            else
                content = new MultipartContent(subtype, boundary);
            return request.AddContent(content);
        }

        /// <summary>
        ///     Adds a MultipartFormDataContent to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="boundary">The boundary if needed.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddMultipartForm<T>(this T request, string boundary = "")
            where T : RestRequest
        {
            return
                request.AddContent(IsNullOrEmpty(boundary)
                    ? new MultipartFormDataContent()
                    : new MultipartFormDataContent(boundary));
        }

        /// <summary>
        ///     Adds a StreamContent to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="stream">The stream to be added.</param>
        /// <param name="mediaType">The media type of the stream.</param>
        /// <param name="buffersize">The buffer size used to process the stream. Default is 1024.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddStream<T>(this T request, Stream stream, string mediaType, int buffersize = 1024,
            string name = "", string fileName = "")
            where T : RestRequest
        {
            stream.ThrowIfNull("stream");
            mediaType.ThrowIfNullOrEmpty("mediaType");
            buffersize.ThrowIf(b => b <= 0, "bufferSize");

            var content = new StreamContent(stream, buffersize);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            return request.AddContent(content, name, fileName);
        }

        /// <summary>
        ///     Adds a StringContent to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="content">The string content.</param>
        /// <param name="encoding">The content encoding.</param>
        /// <param name="mediaType">The content media type.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T AddString<T>(this T request, string content, Encoding encoding, string mediaType,
            string name = "", string fileName = "")
            where T : RestRequest
        {
            content.ThrowIfNullOrEmpty(nameof(content));
            encoding.ThrowIfNull(nameof(encoding));
            mediaType.ThrowIfNullOrEmpty(nameof(mediaType));
            return request.AddContent(new StringContent(content, encoding, mediaType), name, fileName);
        }

        /// <summary>
        ///     Adds an object as serialized json string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="obj">The object that will be serialized and added as json string content.</param>
        /// <param name="clrPropertyNameToLower">if set to <c>true</c> [color property name to lower].</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>
        ///     this.
        /// </returns>
        /// <remarks>
        ///     Throws exception if the given object is null, or if the
        ///     serialized json string is null or empty.
        /// </remarks>
        public static T AddJson<T>(this T request, object obj, bool clrPropertyNameToLower = false, string name = "",
            string fileName = "")
            where T : RestRequest
        {
            obj.ThrowIfNull("BaseRestRequest");
            var serializer = new JsonSerializer();
            var jsonContent = serializer.Serialize(obj, clrPropertyNameToLower);
            jsonContent.ThrowIfNullOrEmpty("BaseRestRequest", "jsonStr");
            // .net default encoding is UTF-8
            if (!IsNullOrEmpty(jsonContent))
            {
                request.AddContent(new StringContent(jsonContent, Encoding.UTF8, serializer.ContentType), name, fileName);
            }
            return request;
        }

        /// <summary>
        ///     Adds an object as serialized xml string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="obj">The object that will be serialized and added as xml string content.</param>
        /// <param name="name">A name needed when content is a MultipartFormDataContent already.</param>
        /// <param name="fileName">A file name needed when content is a MultipartFormDataContent already.</param>
        /// <returns>
        ///     this.
        /// </returns>
        /// <remarks>
        ///     Throws exception if the given object is null, or if the
        ///     serialized xml string is null or empty.
        /// </remarks>
        public static T AddXml<T>(this T request, object obj, string name = "", string fileName = "")
            where T : RestRequest
        {
            obj.ThrowIfNull(nameof(obj));
            var serializer = new DotNetXmlSerializer();
            var xmlContent = serializer.Serialize(obj);
            xmlContent.ThrowIfNullOrEmpty("BaseRestRequest", "xmlContent");
            // .net default encoding is UTF-8
            if (!IsNullOrEmpty(xmlContent))
            {
                request.AddContent(new StringContent(xmlContent, Encoding.UTF8, serializer.ContentType), name, fileName);
            }
            return request;
        }

        #endregion

        #region Url, CancellationToken, parameters and headers

        /// <summary>
        ///     Set the CancellationToken for this request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T CancelToken<T>(this T request, CancellationToken token)
            where T : RestRequest
        {
            token.ThrowIfNull(nameof(token));
            request.ExternalToken = token;
            return request;
        }

        /// <summary>
        ///     Sets the URL string for this request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="url">The URL string.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Url<T>(this T request, string url)
            where T : RestRequest
        {
            url.ThrowIfNull(nameof(url));
            request.Url = url;
            return request;
        }

        /// <summary>
        ///     Sets the URL format parameter for this request.
        ///     A test String.Format is done to verify the input objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="objects">The format parameter objects.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T UrlFormat<T>(this T request, params object[] objects)
            where T : RestRequest
        {
            objects.ThrowIfNullOrEmpty(nameof(objects));
            request.UrlFormatParams = objects;
            return request;
        }

        /// <summary>
        ///     Map an action over the underlying HttpRequestMessage.
        ///     Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        ///     Usage: request.RequestAction(r =&gt; r.Content = ...);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="action">An action that takes a HttpRequestMessage as argument.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T RequestAction<T>(this T request, Action<HttpRequestMessage> action)
            where T : RestRequest
        {
            action.ThrowIfNull(nameof(action));
            action(request.Request);
            return request;
        }

        /// <summary>
        ///     Map an action over the underlying HttpClient.
        ///     Can be used to set "exotic" things, that are not exposed by the BaseRestRequest.
        ///     Usage: request.ClientAction(c =&gt; c.Timeout = ...);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="action">An action that takes a HttpClient as argument.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T ClientAction<T>(this T request, Action<HttpClient> action)
            where T : RestRequest
        {
            action.ThrowIfNull(nameof(action));
            action(request.Client);
            return request;
        }

        public static T Basic<T>(this T request, string authentication)
            where T : RestRequest
        {
            authentication.ThrowIfNullOrEmpty(nameof(authentication));

            var base64AuthStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(authentication));
            request.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthStr);

            return request;
        }

        /// <summary>
        ///     Adds a Http Basic authorization header to the request.
        ///     The result string is Base64 encoded internally.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The user password.</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Basic<T>(this T request, string username, string password)
            where T : RestRequest
        {
            username.ThrowIfNullOrEmpty(nameof(username));
            password.ThrowIfNullOrEmpty(nameof(password));

            var base64AuthStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
            request.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthStr);

            return request;
        }

        /// <summary>
        ///     Adds a Http Bearer authorization header to the request.
        ///     The given token string is Base64 encoded internally.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="token">The token string.</param>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="toBase64">if set to <c>true</c> [to base64].</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Bearer<T>(this T request, string token, string tokenType = "Bearer", bool toBase64 = true)
            where T : RestRequest
        {
            token.ThrowIfNullOrEmpty(nameof(token));
            if (toBase64)
            {
                //string base64AccessToken = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(token));
                token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            }
            request.Request.Headers.Authorization = new AuthenticationHeaderValue(tokenType, token);
            return request;
        }

        public static T ParamIfNotEmpty<T>(this T request, string name, object value,
            ParameterType type = ParameterType.FormUrlEncoded)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            if (!IsNullOrEmpty(value.ToString()))
                request = request.Param(name, value);

            return request;
        }

        /// <summary>
        ///     Adds a parameter to the request. Can be a Query, FormUrlEncoded or Url parameter.
        ///     If a value for the given name is already set, the old parameter value is overwritten silently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <param name="type">The ParameterType.</param>
        /// <returns>
        ///     this.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     BaseRestRequest - ParameterType.Url - Url does not contain a parameter :  +
        ///     name
        /// </exception>
        public static T Param<T>(this T request, string name, object value, ParameterType type)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            value.ThrowIfNullOrToStrEmpty(nameof(value));

            switch (type)
            {
                case ParameterType.FormUrlEncoded:
                case ParameterType.NotSpecified:
                    request.Param(name, value);
                    break;
                case ParameterType.Query:
                    request.QueryParams[name] = value;
                    break;
                default:
                    if (request.Url.Contains("{" + name + "}"))
                        request.UrlParams[name] = value;
                    else
                        throw new ArgumentException(
                            "BaseRestRequest - ParameterType.Url - Url does not contain a parameter : " + name);
                    break;
            }

            return request;
        }

        /// <summary>
        ///     Adds an url parameter to the request.
        ///     Url parameters are part of the set url string of the form {name}.
        ///     The {name} is replaced by the given value before the request is sent.
        ///     If an url parameter value for the given name already exists the
        ///     old value is overwritten silently.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <returns>
        ///     this.
        /// </returns>
        /// <exception cref="System.ArgumentException">BaseRestRequest - UrlParam - Url does not contain a parameter :  + name</exception>
        public static T UrlParam<T>(this T request, string name, object value)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            value.ThrowIfNullOrToStrEmpty(nameof(value));
            request.Url.ThrowIfNullOrEmpty("url - cannot set UrlParameter. Url is null or empty.");

            if (request.Url.Contains("{" + name + "}"))
                request.UrlParams[name] = value;
            else
                throw new ArgumentException("BaseRestRequest - UrlParam - Url does not contain a parameter : " + name);

            return request;
        }

        /// <summary>
        ///     Parameters the specified name.
        /// </summary>
        /// <remarks>
        ///     Should be used with POST/PUT.
        ///     If added multiple times the content will contain
        ///     ?name=value1 & name=value2 & name=value3...
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="addAsMultiple">if set to <c>true</c> [add as multiple].</param>
        /// <returns></returns>
        public static T Param<T>(this T request, string name, object value, bool addAsMultiple = false)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            value.ThrowIfNullOrToStrEmpty(nameof(value));

            List<object> paramValues;
            if (request.Param.TryGetValue(name, out paramValues))
            {
                if (addAsMultiple)
                    paramValues.Add(value);
                else
                    paramValues[0] = value; // overwrite the value if not addAsMultiple
            }
            else
            {
                // First time this parameter with given name is added.
                paramValues = new List<object> {value};
                request.Param[name] = paramValues;
            }
            //param[name] = value;
            return request;
        }

        /// <summary>
        ///     Adds a query parameter (?name=value) to the request.
        ///     The parameter-value pair is added to the URL before sending the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value (should be convertible to string).</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T QParam<T>(this T request, string name, object value)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            value.ThrowIfNullOrToStrEmpty(nameof(value));

            request.QueryParams[name] = value;
            return request;
        }

        /// <summary>
        ///     Adds a header with a single value to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value (should be convertible to string).</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Header<T>(this T request, string name, string value)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            value.ThrowIfNullOrEmpty(nameof(value));

            request.Request.Headers.Add(name, value);
            return request;
        }

        public static T Ajax<T>(this T request)
            where T : RestRequest
        {
            return request.Header("X-Requested-With", "XMLHttpRequest");
        }

        public static T Referer<T>(this T request, string referer)
            where T : RestRequest
        {
            return request.Header("Referer", referer);
        }

        /// <summary>
        ///     Adds a header with multiple values to the request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="name">The header name.</param>
        /// <param name="values">The header values (should be convertible to string).</param>
        /// <returns>
        ///     this.
        /// </returns>
        public static T Header<T>(this T request, string name, IEnumerable<string> values)
            where T : RestRequest
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            var enumerable = values as string[] ?? values.ToArray();

            enumerable.ThrowIfNullOrEmpty(nameof(values));
            request.Request.Headers.Add(name, enumerable);
            return request;
        }

        #endregion

        #region Get HttpWebResponse or RestResponse async

        /// <summary>
        ///     Sends the request and return the raw HttpResponseMessage.
        /// </summary>
        /// <returns>Task containing the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> GetResponseAsync<T>(this T request)
            where T : RestRequest
        {
            if (request.Request.Method.Method != "GET" && request.Request.Content == null && request.Param.Count > 0)
                request.AddFormUrl(); // Add form url encoded parameter to request if needed

            request.Request.RequestUri = new Uri(
                request.Url.CreateRequestUri(
                    request.QueryParams,
                    request.Param,
                    request.Request.Method.Method));
            return await request.Client.SendAsync(request.Request);
        }

        /// <summary>
        ///     Sends the request and returns a RestResponse with generic type IVoid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>
        ///     A Task containing the RestRespone. There will be no deserialized data, but the RestResponse.Response
        ///     (HttpResponseMessage) will be set.
        /// </returns>
        public static async Task<RestResponse<Void>> GetRestResponseAsync<T>(
            this T request,
            Action<RestResponse<Void>> successAction = null,
            Action<RestResponse<Void>> errorAction = null)
            where T : RestRequest
        {
            if (request.Request.Method.Method != "GET" && request.Request.Content == null && request.Param.Count > 0)
                request.AddFormUrl(); // Add form url encoded parameter to request if needed

            return await request.BuildAndSendRequest(successAction, errorAction);
        }

        #endregion

        #region Fetch RestResponse and deserialize directly

        /// <summary>
        ///     Sends the request and returns the RestResponse containing deserialized data
        ///     from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>
        ///     A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.
        /// </returns>
        public static async Task<RestResponse<Void>> Fetch(
            this RestRequest request,
            Action<RestResponse<Void>> successAction = null,
            Action<RestResponse<Void>> errorAction = null)
        {
            if (request.Request.Method.Method != "GET" && request.Request.Content == null && request.Param.Count > 0)
                request.AddFormUrl(); // Add form url encoded parameter to request if needed

            return await request.BuildAndSendRequest(successAction, errorAction);
        }

        /// <summary>
        ///     Sends the request and returns the RestResponse containing deserialized data
        ///     from the HttpResponseMessage.Content if T is not IVoid.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>
        ///     A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.
        /// </returns>
        public static async Task<RestResponse<T>> Fetch<T>(
            this RestRequest request,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            if (request.Request.Method.Method != "GET" && request.Request.Content == null && request.Param.Count > 0)
                request.AddFormUrl(); // Add form url encoded parameter to request if needed

            return await request.BuildAndSendRequest(successAction, errorAction);
        }

        #endregion

        #region Upload file binary with StreamContent

        public static async Task<RestResponse<Void>> UploadFileBinary(
            this RestRequest request,
            Stream streamContent,
            string contentType,
            Action<RestResponse<Void>> successAction = null,
            Action<RestResponse<Void>> errorAction = null)
        {
            return await request.UploadFileBinary<Void>(streamContent, contentType, successAction, errorAction);
        }

        /// <summary>
        ///     Uploads a binary (file) stream using StreamContent.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="streamContent">The (file) stream that will be uploaded.</param>
        /// <param name="contentType">The file content type.</param>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>
        ///     A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.
        /// </returns>
        public static async Task<RestResponse<T>> UploadFileBinary<T>(
            this RestRequest request,
            Stream streamContent,
            string contentType,
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            streamContent.ThrowIfNull("fileStream");
            contentType.ThrowIfNullOrEmpty("contentType");

            request.AddStream(streamContent, contentType);

            return await request.BuildAndSendRequest(successAction, errorAction);
        }

        #endregion
    }
}