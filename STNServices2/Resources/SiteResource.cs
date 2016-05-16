using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using STNDB;

namespace STNServices2.Resources
{
   [XmlInclude(typeof(site))]
    public class SiteLocationQuery : site
    {
       public List<string> networkNames { get; set; }
       public string RecentOP { get; set; } 
       public List<string> Events { get; set; }
    }
}//end namespace





