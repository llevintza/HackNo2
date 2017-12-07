using System;
using System.Net.Http;
using System.Text;


namespace Beazley.ServiceProxy.Client
{
    public class HttpRestClient
    {
        public string BaseUrl { get; private set; }
        private HttpClient _httpClient;
        public Encoding Encoding;


        public HttpRestClient(string baseUrl)
        {
            this.BaseUrl = baseUrl;
        }

        public HttpRestClient() : this(null) { }

        public void ExecuteAsync(HttpRestRequest request, Action<HttpRestResponse> callback)
        {
            using (var httpClientHandler = new HttpClientHandler()
            {
                Credentials = request.Credentials,
                PreAuthenticate = request.PreAuthenticate,
                UseProxy = request.UseProxy,
                Proxy = request.Proxy,
                UseDefaultCredentials = request.UseDefaultCredentials
            })
            {
                using (this._httpClient = new HttpClient(httpClientHandler))
                {
                    switch (request.Method)
                    {
                        case Method.GET:
                            this.ExecuteGetAsync(request, callback);
                            break;
                        case Method.POST:
                            this.ExecutePostAsync(request, callback);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void ExecuteGetAsync(HttpRestRequest request, Action<HttpRestResponse> callback)
        {
            using (var response = this._httpClient.GetAsync(this.ComposeEndpointUrl(request)).Result)
            {
                ProcessResposnse(request, response, callback);
            }
        }

        private void ExecutePostAsync(HttpRestRequest request, Action<HttpRestResponse> callback)
        {
            using (var response = this._httpClient.PostAsync(this.ComposeEndpointUrl(request), request.FormDataContent).Result)
            {
                ProcessResposnse(request, response, callback);
            }
        }

        private void ProcessResposnse(HttpRestRequest request, HttpResponseMessage response, Action<HttpRestResponse> callback)
        {
            var responseData = new HttpRestResponse(request, response);
            callback(responseData);
        }

        private string ComposeEndpointUrl(HttpRestRequest request)
        {
            return $"{this.BaseUrl}/{request.RequestUrl}";
        }
    }
}
