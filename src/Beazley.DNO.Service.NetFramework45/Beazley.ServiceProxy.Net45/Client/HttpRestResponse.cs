using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beazley.ServiceProxy.Net45.Client
{
    public class HttpRestResponse
    {
        private readonly HttpRestRequest _request;
        private readonly HttpContent _content;

        public HttpRestResponse(HttpRestRequest request, HttpResponseMessage response)
        {
            this._request = request;
            this._content = response.Content;
            this.ContentBytes = this._content.ReadAsByteArrayAsync().Result;
            this.StatusCode = response.StatusCode;
        }

        public byte[] ContentBytes { get; private set; }

        public Stream ContentStream => new MemoryStream(this.ContentBytes);

        public string ContentString => _request.Encoding.GetString(this.ContentBytes);

        public HttpContent Content => this._content;

        public Method Method { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Url => _request.RequestUrl;

    }
}
