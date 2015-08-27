using System;
using System.Net.Http;
using Void = Audiotica.Web.Models.Void;

namespace Audiotica.Web.Http
{
    /// <summary>
    ///     A class representing a REST response message.
    ///     It contains the raw HttpResponseMessage returned from the request.
    ///     Further it contains the deserialized data if no exception occured, T != IVoid
    ///     and the response status code matches.
    /// </summary>
    /// <typeparam name="T">The type of the data that will be deserialized.</typeparam>
    public class RestResponse<T> : IDisposable
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public RestResponse()
        {
            Exception = null;
            HttpResponse = null;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="request">Reference to a BaseRestRequest.</param>
        public RestResponse(RestRequest request)
        {
            Request = request;
            Exception = null;
            HttpResponse = null;
        }

        /// <summary>
        ///     That BaseRestRequest this rest response comes from.
        /// </summary>
        public RestRequest Request { get; }

        /// <summary>
        ///     Check if the returned status code matches the wanted status code.
        /// </summary>
        public bool IsSuccessStatusCode => HttpResponse != null && HttpResponse.IsSuccessStatusCode;

        /// <summary>
        ///     Check if the request that was producing this response has encountered an exception.
        /// </summary>
        public bool IsException => Exception != null;

        /// <summary>
        ///     Check if T is IVoid
        /// </summary>
        public bool IsNothing => typeof (T) == typeof (Void);

        /// <summary>
        ///     Check if a deserialized object is available.
        /// </summary>
        public bool HasData => !IsNothing && (Data != null && !Data.Equals(default(T)));

        /// <summary>
        ///     The Exception that could be thrown during the request fetching.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        ///     The "raw" HttpResponseMessage.
        /// </summary>
        public HttpResponseMessage HttpResponse { get; internal set; }

        /// <summary>
        ///     The deserialized data if T is not INothing.
        /// </summary>
        public T Data { get; internal set; }

        /// <summary>
        ///     Dispose the request.
        /// </summary>
        public void Dispose()
        {
            // free managed resources
            Request?.Dispose();
            HttpResponse?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     If an exception occurred during the request throw it again.
        ///     Usage:
        ///     var data = response.ThrowIfException().Data;
        /// </summary>
        /// <returns>this.</returns>
        public RestResponse<T> ThrowIfException()
        {
            if (Exception != null)
                throw Exception;
            return this;
        }
    }
}