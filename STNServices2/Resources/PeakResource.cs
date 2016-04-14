//------------------------------------------------------------------------------
//----- PeakResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2015 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Peak resources through the HTTP uniform interface.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 02.08.13 - JKN - Added HWMLayerAttribute for AddHWM geoprocessing support
// 06.19.12 - JKN - Removed HWMList, Renamed to HWMResource
// 05.29.12 - jkn - Created, Moved from HWMHandler
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace STNServices2.Resources
{

    [DataContract]
    public class PeakDownloadable
    {
        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "PEAK_SUMMARY_ID")]
        public decimal PEAK_SUMMARY_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "MEMBER_NAME")]
        public string MEMBER_NAME { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "PEAK_DATE")]
        public string PEAK_DATE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "IS_PEAK_DATE_ESTIMATED")]
        public string IS_PEAK_DATE_ESTIMATED { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "PEAK_TIME")]
        public string PEAK_TIME { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "TIME_ZONE")]
        public string TIME_ZONE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "IS_PEAK_TIME_ESTIMATED")]
        public string IS_PEAK_TIME_ESTIMATED { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "PEAK_STAGE")]
        public decimal PEAK_STAGE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "IS_PEAK_STAGE_ESTIMATED")]
        public string IS_PEAK_STAGE_ESTIMATED { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "PEAK_DISCHARGE")]
        public decimal PEAK_DISCHARGE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "IS_PEAK_DISCHARGE_ESTIMATED")]
        public string IS_PEAK_DISCHARGE_ESTIMATED { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "HEIGHT_ABOVE_GND")]
        public decimal HEIGHT_ABOVE_GND { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "IS_HAG_ESTIMATED")]
        public string IS_HAG_ESTIMATED { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "AEP")]
        public decimal AEP { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "AEP_LOWCI")]
        public decimal AEP_LOWCI { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "AEP_UPPERCI")]
        public decimal AEP_UPPERCI { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "AEP_RANGE")]
        public decimal AEP_RANGE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "CALC_NOTES")]
        public String CALC_NOTES { get; set; }


        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "VERTICAL_DATUM")]
        public string VERTICAL_DATUM { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SITE_ID")]
        public decimal SITE_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SITE_NO")]
        public String SITE_NO { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "LATITUDE")]
        public decimal LATITUDE { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "LONGITUDE")]
        public decimal LONGITUDE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "DESCRIPTION")]
        public String DESCRIPTION { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "NETWORK")]
        public string NETWORK { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "STATE")]
        public String STATE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "COUNTY")]
        public String COUNTY { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "WATERBODY")]
        public String WATERBODY { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HORIZONTAL_DATUM")]
        public String HORIZONTAL_DATUM { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "PRIORITY")]
        public string PRIORITY { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "ZONE")]
        public String ZONE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HORIZONTAL_COLLECT_METHOD")]
        public String HORIZONTAL_COLLECT_METHOD { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "PERM_HOUSING_INSTALLED")]
        public String PERM_HOUSING_INSTALLED { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SITE_NOTES")]
        public String SITE_NOTES { get; set; }
    }//end class HWMDownloadable

}//end namespace
