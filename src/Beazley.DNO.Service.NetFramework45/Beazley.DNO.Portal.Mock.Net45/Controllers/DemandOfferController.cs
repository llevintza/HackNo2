using System.Collections.Generic;
using System.Web.Http;
using System.Threading.Tasks;
using Beazley.DNO.Portal.Mock.Models;

namespace Beazley.DNO.Portal.Mock.Controllers
{
    [RoutePrefix("api/dno")]
    public class DemandOfferController : ApiController
    {
        //// GET api/values 
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        [HttpPost]
        [Route("premium")]
        public async Task<IHttpActionResult> GetPremium(PremiumRequest input)
        {

            return await Task.FromResult(Ok(new PremiumResponse { GrossPremium = 155.6}));
        }
    }
}
