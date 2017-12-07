using System;

namespace Beazley.ServiceProxy.Net45.Exceptions
{
    public class RestProxyException : Exception
    {
        public RestProxyException(string message) : base(message) { }
        public RestProxyException(string message, Exception innerException): base(message, innerException) { }
    }
}
