using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using Audiotica.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Audiotica
{
    public class AudioticaPluginHelper
    {
        private readonly Dictionary<string, PluginTask<object>> _pluginTasks =
            new Dictionary<string, PluginTask<object>>();

        private WebView _sandboxWebView;
        public bool IsSandboxLoaded { get; set; }
        public event EventHandler SandboxLoaded;

        private void _sandboxWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _sandboxWebView.NavigationCompleted -= _sandboxWebView_NavigationCompleted;
            IsSandboxLoaded = true;
            OnSandboxLoaded();
        }

        public void Load(WebView sandboxWebView)
        {
            _sandboxWebView = sandboxWebView;
            _sandboxWebView.NavigationCompleted += _sandboxWebView_NavigationCompleted;
            _sandboxWebView.ScriptNotify += SandboxWebViewOnScriptNotify;
            _sandboxWebView.NavigateToLocalStreamUri(_sandboxWebView.BuildLocalStreamUri("autc", "/sandbox.html"),
                new LocalUriResolver());
        }

        private void SandboxWebViewOnScriptNotify(object sender, NotifyEventArgs notifyEventArgs)
        {
            var result = notifyEventArgs.Value;
            var id = result.Substring(0, result.IndexOf(";"));
            result = result.Replace(id + ";", "");
            id = id.Substring(id.IndexOf(":") + 1);

            if (result.StartsWith("autc-http:"))
                HandleHttpRequest(id, result);
            else
                HandlePluginResult(id, result.Trim());
        }

        private async void HandleHttpRequest(string id, string result)
        {
            var json = result.Substring(result.IndexOf(":") + 1);
            var request = await json.DeserializeAsync<AutcHttpRequest>();

            if (request == null) return;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Audiotica-Plugin-Bot/1.0");

                FormUrlEncodedContent content = null;
                if (request.Data != null)
                    content = new FormUrlEncodedContent(request.Data);
                bool success;
                string responseText;
                string statusCode;

                switch (request.Method.ToUpper())
                {
                    case "POST":
                        using (var resp = await client.PostAsync(request.Url, content))
                        {
                            success = resp.IsSuccessStatusCode;
                            statusCode = ((int)resp.StatusCode).ToString();
                            responseText = await resp.Content.ReadAsStringAsync();
                        }
                        break;
                    case "PUT":
                        using (var resp = await client.PutAsync(request.Url, content))
                        {
                            success = resp.IsSuccessStatusCode;
                            statusCode = ((int)resp.StatusCode).ToString();
                            responseText = await resp.Content.ReadAsStringAsync();
                        }
                        break;
                    default:
                        var method = HttpMethod.Get;
                        switch (request.Method.ToUpper())
                        {
                            case "HEAD":
                                method = HttpMethod.Head;
                                break;
                            case "DELETE":
                                method = HttpMethod.Delete;
                                break;
                        }

                        using (
                            var resp =
                                await client.SendAsync(new HttpRequestMessage(method, request.Url)))
                        {
                            success = resp.IsSuccessStatusCode;
                            statusCode = ((int)resp.StatusCode).ToString();
                            responseText = await resp.Content.ReadAsStringAsync();
                        }
                        break;
                }

                if (content != null)
                    content.Dispose();

               await  _sandboxWebView.InvokeScriptAsync("autcHttpResult", new[] { id, success.ToString().ToLower(), responseText, statusCode });
            }
        }

        private async void HandlePluginResult(string id, string result)
        {
            var pluginTask = _pluginTasks[id];
            _pluginTasks.Remove(id);

            switch (result)
            {
                // all plugins should return a value, else the plugin executer returns autc-nope
                // otherwise we have a big problem
                case null:
                    pluginTask.Task.SetException(new PluginException("Unknown plugin error."));
                    return;
                case "autc-nope":
                    pluginTask.Task.SetResult(pluginTask.Default);
                    return;
            }

            if (result.StartsWith("autc-ex:"))
            {
                pluginTask.Task.SetException(new PluginException(result.Substring(result.IndexOf(":") + 1)));
                return;
            }


            if (pluginTask.Type == typeof (int))
                pluginTask.Task.SetResult(int.Parse(result));
            else if (pluginTask.Type == typeof (double))
                pluginTask.Task.SetResult(double.Parse(result));
            else if (pluginTask.Type == typeof (bool))
                pluginTask.Task.SetResult(result == "autc-true");
            else if (pluginTask.Type == typeof (string))
                pluginTask.Task.SetResult(result);
            else
                pluginTask.Task.SetResult(await result.DeserializeAsync<JToken>());
        }

        public async Task<object> ExecuteAsync<T>(string js, Dictionary<string, string> parameters)
        {
            var sessionId = Guid.NewGuid().ToString("n");
            var task = new TaskCompletionSource<object>();

            _pluginTasks.Add(sessionId, new PluginTask<object>
            {
                Default = default(T),
                Task = task,
                Type = typeof (T)
            });
            await
                _sandboxWebView.InvokeScriptAsync("execute",
                    new[] {sessionId, js, JsonConvert.SerializeObject(parameters)});

            return await task.Task;
        }

        protected virtual void OnSandboxLoaded()
        {
            var handler = SandboxLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        internal class AutcHttpRequest
        {
            public string Method { get; set; }
            public string Url { get; set; }
            public Dictionary<string, string> Data { get; set; }
        }
    }

    public class PluginTask<T>
    {
        public TaskCompletionSource<T> Task { get; set; }
        public T Default { get; set; }
        public Type Type { get; set; }
    }

    public class PluginException : Exception
    {
        public PluginException(string message) : base(message)
        {
        }
    }

    internal class LocalUriResolver : IUriToStreamResolver
    {
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            var path = uri.AbsolutePath;

            // Because of the signature of the this method, it can't use await, so we 
            // call into a seperate helper method that can use the C# await pattern.
            return GetContent(path).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string path)
        {
            // We use a package folder as the source, but the same principle should apply
            // when supplying content from other locations
            try
            {
                var localUri = new Uri("ms-appx:///Assets/Html" + path);
                var f = await StorageFile.GetFileFromApplicationUriAsync(localUri);
                var stream = await f.OpenAsync(FileAccessMode.Read);
                return stream;
            }
            catch (Exception)
            {
                throw new Exception("Invalid path");
            }
        }
    }
}