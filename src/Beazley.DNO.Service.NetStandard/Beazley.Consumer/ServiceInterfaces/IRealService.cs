using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beazley.ServiceModels;
using Beazley.ServiceProxy.Attributes;

namespace Beazley.Consumer.ServiceInterfaces
{
    public interface IRealService
    {
        [RestApiService(Method = RestSharp.Method.POST, Resource = "premium_rating", Route = "api/REST/doprod_v1/")]
        PremiumResponse GetPremiumQuote(PremiumRequest input);
    }
}
