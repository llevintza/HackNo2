using System;
using System.Collections.Generic;

namespace Beazley.ServiceModels
{
    public class SchemeKeys
    {
        public string ProductName { get; set; }

        public DateTime EffectiveDateTime { get; set; }
        
        public List<SchemeKey> Keys { get; set; }
    }
}
