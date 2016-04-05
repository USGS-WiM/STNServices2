using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace STNServices2.Resources
{
    public class RSSItem
    {
        [XmlElement(DataType = "string",
        ElementName = "title")]
        public String title { get; set; }

        [XmlElement(DataType = "string",
        ElementName = "description")]
        public String description { get; set; }

        [XmlElement(DataType = "string",
        ElementName = "link")]
        public String link { get; set; }
    }
}