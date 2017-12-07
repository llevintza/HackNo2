using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beazley.ServiceModels
{
    public class PremiumRequest
    {
        [JsonProperty(PropertyName = "NoOfEmployees")]
        public int  NumberOfEmployees { get; set; }

        public long Revenue { get; set; }

        public SchemeKeys SchemeKeys { get; set; }
    }
}
