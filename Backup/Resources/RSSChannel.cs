using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace STNServices2.Resources
{
    [XmlRoot("channel")]
    public class RSSChannel
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

        [XmlElement(DataType = "string",
        ElementName = "language")]
        public String language { get; set; }

        [XmlElement(DataType = "string",
        ElementName = "webMaster")]
        public String webMaster { get; set; }

        [XmlElement(DataType = "string",
        ElementName = "lastBuildDate")]
        public String lastBuildDate { get; set; }
    }


}