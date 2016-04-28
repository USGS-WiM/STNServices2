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
        public contact_type contactType { get; set; }
    }
}



////------------------------------------------------------------------------------
////----- SiteResource -----------------------------------------------------------
////------------------------------------------------------------------------------

////-------1---------2---------3---------4---------5---------6---------7---------8
////       01234567890123456789012345678901234567890123456789012345678901234567890
////-------+---------+---------+---------+---------+---------+---------+---------+

//// copyright:   2015 WiM - USGS

////    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
////              Jeremy K. Newson USGS Wisconsin Internet Mapping
////              
////  
////   purpose:   Report resources.
////              Equivalent to the model in MVC.
////
////discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
////              SiteResource contains additional rederers of the derived EF POCO classes. 
////              https://github.com/openrasta/openrasta/wiki/Resources
////
////     

//#region Comments
//// 08.18.15 - tr - Created for bring Reports and their Contacts together
//#endregion
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Xml.Serialization;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Json;

//namespace STNServices2.Resources
//{
//    [DataContract]
//    public class ReportResource
//    {
//        [DataMember]
//        [XmlElement(typeof(REPORTING_METRICS),
//        ElementName = "Report")]
//        public REPORTING_METRICS Report { get; set; }

//        [DataMember]
//        [XmlElement(typeof(List<ReportContactModel>),
//        ElementName = "ReportContacts")]
//        public List<ReportContactModel> ReportContacts { get; set; }

//    }

//    [DataContract]
//    public class ReportContactModel
//    {
//        [DataMember]
//        [XmlElement(typeof(decimal),
//        ElementName = "ContactId")]
//        public decimal ContactId { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "FNAME")]
//        public string FNAME { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "LNAME")]
//        public string LNAME { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "PHONE")]
//        public string PHONE { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "ALT_PHONE")]
//        public string ALT_PHONE { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "EMAIL")]
//        public string EMAIL { get; set; }

//        [DataMember]
//        [XmlElement(typeof(string),
//        ElementName = "TYPE")]
//        public string TYPE { get; set; }

//    }

//}//end namespace
