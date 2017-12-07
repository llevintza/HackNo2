using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beazley.DNO.Portal.Mock.Models
{
    public class PremiumRequest
    {
        public int  NumberOfEmployees { get; set; }

        public long Revenue { get; set; }

        public SchemeKeys SchemeKeys { get; set; }
    }
}
