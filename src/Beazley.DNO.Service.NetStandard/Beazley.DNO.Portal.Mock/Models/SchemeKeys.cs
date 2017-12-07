using System;
using System.Collections.Generic;

namespace Beazley.DNO.Portal.Mock.Models
{
    public class SchemeKeys
    {
        public string ProductName { get; set; }

        public DateTime EffectiveDateTime { get; set; }

        /// <remarks/>
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool EffectiveDateTimeSpecified
        //{
        //    get
        //    {
        //        return this.effectiveDateTimeFieldSpecified;
        //    }
        //    set
        //    {
        //        this.effectiveDateTimeFieldSpecified = value;
        //    }
        //}

        public bool EffectiveDateTimeSpecified { get; set; }


        public List<SchemeKey> Keys { get; set; }
    }
}
