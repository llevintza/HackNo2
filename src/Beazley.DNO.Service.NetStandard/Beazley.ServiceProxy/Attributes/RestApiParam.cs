using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Beazley.ServiceProxy.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class RestApiParam : Attribute
    {
        public ParameterType ParameterType { get; set; }
    }
}
