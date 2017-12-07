using Beazley.ServiceModels.Net45.DNO;
using Beazley.ServiceProxy.Net45.Attributes;

namespace Beazley.Consumer.Net45.ServiceInterfaces
{
    public interface IRealService
    {
        [RestApiService(Method = RestSharp.Method.POST, Resource = "premium_rating", Route = "api/REST/doprod_v1/")]
        PremiumResponse GetPremiumQuote(PremiumRequest input);
    }
}
