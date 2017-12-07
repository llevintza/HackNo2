using System;
using RestSharp;

namespace Beazley.ServiceProxy.Net45.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RestApiService : Attribute
    {
        private string _routePrefix;

        private string _urlParameters;

        private string _resourceName;

        public string Route
        {
            get { return !string.IsNullOrWhiteSpace(this._routePrefix) ? this._routePrefix : string.Empty; }
            set { this._routePrefix = value; }
        }

        public string Params
        {
            get { return !string.IsNullOrWhiteSpace(this._urlParameters) ? this._urlParameters : string.Empty; }
            set { this._urlParameters = value; }
        }

        public string Resource {
            get { return !string.IsNullOrWhiteSpace(this._resourceName) ? this._resourceName : string.Empty; }
            set { this._resourceName = value; }
        }
        
        public Method Method { get; set; }

        public RestApiService()
        {
            Method = Method.POST;
        }

        public RestApiService(Method method)
        {
            Method = method;
            this._resourceName = null;
            this._routePrefix = null;
            this._urlParameters = null;
        }
    }
}
