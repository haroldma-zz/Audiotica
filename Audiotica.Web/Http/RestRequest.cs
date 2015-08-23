using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Web.Deserializer;
using Audiotica.Web.Extensions;
using Void = Audiotica.Web.Models.Void;

namespace Audiotica.Web.Http
{
    /// <summary>
    ///     Parameter type enum. Query, FormUrlEncoded or Url.
    /// </summary>
    public enum ParameterType
    {
        NotSpecified,

        /// <summary>
        ///     Parameter is added to the URL as query parameter (?name=value).
        /// </summary>
        Query,

        /// <summary>
        ///     Parameter is added to a POST request with FormUrlEncoded Http content.
        /// </summary>
        FormUrlEncoded,

        /// <summary>
        ///     Parameter is used to format the URL string (replaces a {name}).
        /// </summary>
        Url
    }

    /// <summary>
    ///     RestRequest class.
    /// </summary>
    /// <remarks>
    ///     Currently the RestRequest does not verify that the underlying HttpRequestMessage.Content
    ///     is set correctly. The developer is responsible for setting a correct HttpContent.
    ///     For example a POST request should use FormUrlEncoded content when parameters are needed.
    ///     By default (ctor) every RestRequest got his own underlying HttpRequestMessage and HttpClient
    ///     to send the constructed request.
    /// </remarks>
    public class RestRequest : IDisposable
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public RestRequest()
        {
            registerDefaultHandlers();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="defaultRequest">The initial request message, or null if not used.</param>
        /// <param name="httpClient">The initial http client, or null if not used.</param>
        public RestRequest(HttpRequestMessage defaultRequest, HttpClient httpClient = null)
        {
            if (defaultRequest != null)
                Request = defaultRequest;
            if (httpClient != null)
                Client = httpClient;
            registerDefaultHandlers();
        }

        public bool DeserializeOnError { get; set; }

        /// <summary>
        ///     Dispose the request.
        /// </summary>
        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
                Client = null;
            }
            if (Request != null)
            {
                Request.Dispose();
                Request = null;
            }
            GC.SuppressFinalize(this);
        }

        #region Variables 

        /// <summary>
        ///     Content (de)serialization handler.
        /// </summary>
        internal Dictionary<string, IDeserializer> ContentHandler = new Dictionary<string, IDeserializer>();

        /// <summary>
        ///     Url query parameters: ?name=value
        /// </summary>
        internal Dictionary<string, object> QueryParams = new Dictionary<string, object>();

        /// <summary>
        ///     When method is GET then added as query parameters too.
        ///     Otherwise added as FormUrlEncoded parameters: name=value
        /// </summary>
        internal Dictionary<string, List<object>> Param = new Dictionary<string, List<object>>();

        /// <summary>
        ///     Url parameters ../{name}.
        /// </summary>
        internal Dictionary<string, object> UrlParams = new Dictionary<string, object>();

        /// <summary>
        ///     The url string. Can contain {name} and/or format strings {0}.
        /// </summary>
        internal string Url = "";

        /// <summary>
        ///     Last url format {} set with UrlFormat.
        /// </summary>
        internal object[] UrlFormatParams = null;

        internal CancellationToken ExternalToken;

        /// <summary>
        ///     HttpClient used to send the request message.
        /// </summary>
        internal HttpClient Client = new HttpClient();

        /// <summary>
        ///     Internal request message.
        /// </summary>
        internal HttpRequestMessage Request = new HttpRequestMessage();

        #endregion

        #region CancellationToken, HttpClient and HttpRequestMessage propertys

        /// <summary>
        ///     HttpClient property.
        /// </summary>
        internal HttpClient HttpClient
        {
            get { return Client; }
            set { Client = value; }
        }

        /// <summary>
        ///     HttpRequestMessage property.
        /// </summary>
        internal HttpRequestMessage HttpRequest
        {
            get { return Request; }
            set { Request = value; }
        }

        #endregion

        #region Helper functions

        /// <summary>
        ///     A helper function that is doing all the "hard" work setting up the request and sending it.
        ///     1) The Url is formated using String.Format if UrlParam´s where added.
        ///     2) The query parameter are added to the URL with RestlessExtensions.CreateRequestUri
        ///     3) The request is send.
        ///     4) The RestResponse is set.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized data. Set to IVoid if no deserialization is wanted.</typeparam>
        /// <param name="successAction">Action that is called on success. (No exceptions and HttpStatus code is ok).</param>
        /// <param name="errorAction">Action that is called when an error occures. (Exceptions or HttpStatus code not ok).</param>
        /// <returns>A taks containing the RestResponse with the deserialized data if T is not IVoid and no error occured.</returns>
        internal async Task<RestResponse<T>> BuildAndSendRequest<T>(
            Action<RestResponse<T>> successAction = null,
            Action<RestResponse<T>> errorAction = null)
        {
            // RestResponse<T> result = new RestResponse<T>();
            // TODO: Good or bad to have a reference from the response to the request?!
            // TODO: the result.Response.RequestMessage already points to this.Request?! (underlying Http request).
            var result = new RestResponse<T>(this);
            try
            {
                // First format the Url
                if (UrlFormatParams != null && UrlFormatParams.Length > 0)
                    Url = string.Format(Url, UrlFormatParams);

                if (UrlParams != null && UrlParams.Count > 0)
                {
                    foreach (var item in UrlParams)
                        Url = Url.Replace("{" + item.Key + "}", WebUtility.UrlEncode(item.Value.ToString()));
                }

                Request.RequestUri =
                    new Uri(Url.CreateRequestUri(QueryParams, Param, Request.Method.Method).ToUnaccentedText());

                result.HttpResponse = await Client.SendAsync(
                    Request, ExternalToken).DontMarshall();

                if (result.HttpResponse.IsSuccessStatusCode || DeserializeOnError)
                    result.Data = await TryDeserialization<T>(result.HttpResponse).DontMarshall();
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            // call success or error action if necessary
            if (result.IsException || !result.HttpResponse.IsSuccessStatusCode)
                ActionIfNotNull(result, errorAction);
            else
                ActionIfNotNull(result, successAction);

            return result;
        }

        private void ActionIfNotNull<T>(RestResponse<T> response, Action<RestResponse<T>> action)
        {
            action?.Invoke(response);
        }

        /// <summary>
        ///     Check if param contains a value for the given name already
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>True if already containing value for given name, false otherwise.</returns>
        protected bool ContainsParam(string name)
        {
            return Param.ContainsKey(name);
        }

        #region Serialization

        private void registerDefaultHandlers()
        {
            // TODO: Why not reusing the deserializer?
            // register default handlers
            ContentHandler.Add("application/json", new JsonDeserializer());
            ContentHandler.Add("application/xml", new DotNetXmlDeserializer());
            ContentHandler.Add("text/json", new JsonDeserializer());
            ContentHandler.Add("text/x-json", new JsonDeserializer());
            ContentHandler.Add("text/javascript", new JsonDeserializer());
            ContentHandler.Add("text/xml", new DotNetXmlDeserializer());
            ContentHandler.Add("text/html", new HtmlDeserializer());
            ContentHandler.Add("*", new JsonDeserializer());
        }

        private async Task<T> TryDeserialization<T>(HttpResponseMessage response)
        {
            var result = default(T);
            if (!(typeof (T).GetTypeInfo().IsAssignableFrom(typeof (Void).GetTypeInfo())))
            {
                // TODO: Check media type for json and xml?
                var deserializer = GetHandler(response.Content.Headers.ContentType.MediaType);
                result = deserializer.Deserialize<T>(await response.Content.ReadAsStringAsync().DontMarshall());
            }
            return result;
        }

        /// <summary>
        ///     Retrieve the handler for the specified MIME content type
        /// </summary>
        /// <param name="contentType">MIME content type to retrieve</param>
        /// <returns>IDeserializer instance</returns>
        protected IDeserializer GetHandler(string contentType)
        {
            if (string.IsNullOrEmpty(contentType) && ContentHandler.ContainsKey("*"))
            {
                return ContentHandler["*"];
            }

            var semicolonIndex = contentType.IndexOf(';');
            if (semicolonIndex > -1) contentType = contentType.Substring(0, semicolonIndex);
            IDeserializer handler = null;
            if (ContentHandler.ContainsKey(contentType))
            {
                handler = ContentHandler[contentType];
            }
            else if (ContentHandler.ContainsKey("*"))
            {
                handler = ContentHandler["*"];
            }

            return handler;
        }

        #endregion

        #endregion
    }
}