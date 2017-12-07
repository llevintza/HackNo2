using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Beazley.ServiceProxy.Client
{
    public class HttpRestRequest
    {
        private Encoding _encoding;
        private readonly string _resource;

        public HttpRestRequest()
        {
            FormDataContent = new MultipartFormDataContent();
        }

        public HttpRestRequest(string resource) : this()
        {
            this._resource = resource;
        }

        public HttpRestRequest(string resource, Method method) : this(resource)
        {
            this.Method = method;
        }

        public ICredentials Credentials { get; set; }

        public bool PreAuthenticate { get; set; }

        public IWebProxy Proxy { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public bool UseProxy { get; set; }

        public Method Method { get; set; }

        public Encoding Encoding
        {
            get { return _encoding ?? (_encoding = Encoding.UTF8); }
            set { _encoding = value; }
        }

        public string RequestUrl => this._resource;

        public MultipartFormDataContent FormDataContent { get; private set; }

       public void AddMultipartFormData(string name, ByteArrayContent content)
        {
            this.FormDataContent.Add(content, $"\"{name}\"");
        }
    }
}
