using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beazley.ServiceModels;
using Beazley.ServiceProxy.Attributes;

namespace Beazley.Consumer.ServiceInterfaces
{
    interface IDnoService
    {
        [RestApiService(Method = RestSharp.Method.POST, Resource = "premium", Route = "dno")]
        PremiumResponse GetPremiumQuote(
            [RestApiParam(ParameterType = RestSharp.ParameterType.RequestBody)]
            PremiumRequest input);
    }
}