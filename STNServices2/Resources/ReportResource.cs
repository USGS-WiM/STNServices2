using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using STNDB;

namespace STNServices2.Resources
{
    [XmlInclude(typeof(contact))]
    public class Contact : contact
    {
        public string contactType { get; set; }
    }

    [XmlInclude(typeof(reporting_metrics))]
    public class ReportResource : reporting_metrics
    {
        public List<ReportContactModel> ReportContacts { get; set; }
    }

    [XmlInclude(typeof(contact))]
    public class ReportContactModel : contact
    {      
        public string type { get; set; }
    }
}//end namespace
