using System;
using RestSharp;

namespace Beazley.ServiceProxy.Net45.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class RestApiParam : Attribute
    {
        public ParameterType ParameterType { get; set; }
    }
}
