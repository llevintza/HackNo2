using System;
using System.Collections.Generic;

namespace Beazley.ServiceModels.Net45.DNO
{
    public class SchemeKeys
    {
        public string ProductName { get; set; }

        public DateTime EffectiveDateTime { get; set; }
        
        public List<SchemeKey> Keys { get; set; }
    }
}
