//------------------------------------------------------------------------------
//----- SiteResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Site resources.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 05.29.12 - jkn - Created, Moved from siteHandler
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
    [XmlRoot("ArrayOfSITE")]
    public class SiteList
    {
        [DataMember]
        [XmlElement(typeof(SimpleSite),
        ElementName = "Site"),
        XmlElement(typeof(SitePoint),
        ElementName = "SitePoint")]

        public List<SiteBase> Sites { get; set; }

    }

    [DataContract]
    public class SiteBase : HypermediaEntity
    {
        [DataMember]
        [XmlElement(DataType = "int",
        ElementName = "SITE_ID")]
        public Int32 SITE_ID { get; set; }

        [DataMember]
        [XmlElement(DataType = "string",
        ElementName = "SITE_NO")]
        public String SITE_NO { get; set; }


        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:

                    uriString = string.Format("Sites/{0}", this.SITE_ID);
                    break;

                case refType.POST:
                    uriString = "Sites";
                    break;

            }
            return uriString;
        }
        #endregion

    }

    /* Light version of Site Object, only contains ID and Site_No */
    [DataContract]
    public class SimpleSite : SiteBase
    {
        public SimpleSite()
        {
            SITE_ID = -1;
            SITE_NO = "";
        }
    }

    /* Medium version of Site Object, contains ID, Site_No, and Lat/Long coordinates */
    [DataContract]
    public class SitePoint : SiteBase
    {
        [DataMember]
        [XmlElement(DataType = "decimal",
        ElementName = "Latitude")]
        public decimal? latitude { get; set; }

        [DataMember]
        [XmlElement(DataType = "decimal",
        ElementName = "Longitude")]
        public decimal? longitude { get; set; }

        public SitePoint()
        {
            SITE_ID = -1;
            SITE_NO = "";
        }
    }

    [DataContract]
    public class SiteLocationQuery
    {
        [DataMember]
        [XmlElement(DataType = "decimal",
        ElementName = "SITE_ID")]
        public decimal SITE_ID { get; set; }

        [DataMember]
        [XmlElement(DataType = "string",
        ElementName = "SITE_NO")]
        public string SITE_NO { get; set; }

        [DataMember]
        [XmlElement(DataType = "decimal",
        ElementName = "Latitude")]
        public decimal? latitude { get; set; }

        [DataMember]
        [XmlElement(DataType = "decimal",
        ElementName = "Longitude")]
        public decimal? longitude { get; set; }

        [DataMember]
        [XmlElement(DataType = "string",
        ElementName = "Description")]
        public string Description { get; set; }

        [DataMember]
        [XmlElement(DataType = "string",
        ElementName = "County")]
        public string County { get; set; }

        [DataMember]
        [XmlElement(DataType = "string",
        ElementName = "State")]
        public string State { get; set; }

        [DataMember]
        [XmlElement(DataType = "List<NETWORK_NAME>",
        ElementName = "Networks")]
        public List<NETWORK_NAME> Networks { get; set; }

        [DataMember]
        [XmlElement(DataType = "OBJECTIVE_POINT",
        ElementName = "RecentOP")]
        public OBJECTIVE_POINT RecentOP { get; set; }

        [DataMember]
        [XmlElement(DataType = "List<EVENT>",
        ElementName = "Events")]
        public List<EVENT> Events { get; set; }

    }

    /* full details of Site Object and HWMs */
    [DataContract]
    public class DetailSite : SiteBase
    {
        [DataMember]
        [XmlElement(DataType = "SITE",
        ElementName = "Site")]
        public SITE siteDetail { get; set; }

        [DataMember]
        [XmlElement(typeof(HWM),
        ElementName = "HWM")]
        public List<HWM> hWMDetail { get; set; }

        public DetailSite()
        {
            SITE_ID = -1;
            SITE_NO = "";
        }
    }

    public class SiteLayer : FeatureBase
    {
        #region "Fields"
        private string _siteNo;
        private string _siteName;
        private string _siteDescription;
        private string _address;
        private string _city;
        private string _state;
        private string _zip;
        private string _stationId;
        private string _USGSstationID;
        private string _NOAAstationID;
        private string _county;
        private string _waterBody;
        #endregion
        #region "Properties"
        public Int32 SITE_ID { get; set; }
        public string SITE_NO
        {
            get { return _siteNo; }
            set { _siteNo = LimitLength(value, 50); }
        }
        public string SITE_NAME
        {
            get { return _siteName; }
            set { _siteName = LimitLength(value, 255); }
        }
        public string SITE_DESCRIPTION
        {
            get { return _siteDescription; }
            set { _siteDescription = LimitLength(value, 255); }
        }
        public string ADDRESS
        {
            get { return _address; }
            set { _address = LimitLength(value, 100); }
        }
        public string CITY
        {
            get { return _city; }
            set { _city = LimitLength(value, 100); }
        }
        public string STATE
        {
            get { return _state; }
            set { _state = LimitLength(value, 20); }
        }
        public string ZIP
        {
            get { return _zip; }
            set { _zip = LimitLength(value, 5); }
        }
        public string OTHER_SID
        {
            get { return _stationId; }
            set { _stationId = LimitLength(value, 50); }
        }
        public string USGS_SID
        {
            get { return _USGSstationID; }
            set { _USGSstationID = LimitLength(value, 50); }
        }
        public string NOAA_SID
        {
            get { return _NOAAstationID; }
            set { _NOAAstationID = LimitLength(value, 50); }
        }
        public string COUNTY
        {
            get { return _county; }
            set { _county = LimitLength(value, 50); }
        }
        public string WATERBODY
        {
            get { return _waterBody; }
            set { _waterBody = LimitLength(value, 100); }
        }
        public Int32 HDATUM_ID { get; set; }
        public double? DRAINAGE_AREA_SQMI { get; set; }
        public Int32? LANDOWNERCONTACT_ID { get; set; }
        #endregion
        #region "Constructor"
        public SiteLayer(SITE site)
            : base()
        {
            this.SITE_ID = Convert.ToInt32(site.SITE_ID);
            this.SITE_NO = !string.IsNullOrEmpty(site.SITE_NO) ? site.SITE_NO : string.Empty;
            this.SITE_NAME = !string.IsNullOrEmpty(site.SITE_NAME) ? site.SITE_NAME : string.Empty;
            this.SITE_DESCRIPTION = !string.IsNullOrEmpty(site.SITE_DESCRIPTION) ? site.SITE_DESCRIPTION : string.Empty;
            this.ADDRESS = !string.IsNullOrEmpty(site.ADDRESS) ? site.ADDRESS : string.Empty;
            this.CITY = !string.IsNullOrEmpty(site.CITY) ? site.CITY : string.Empty;
            this.STATE = !string.IsNullOrEmpty(site.STATE) ? site.STATE : string.Empty;
            this.ZIP = !string.IsNullOrEmpty(site.ZIP) ? site.ZIP : string.Empty;
            this.OTHER_SID = !string.IsNullOrEmpty(site.OTHER_SID) ? site.OTHER_SID : string.Empty;
            this.COUNTY = !string.IsNullOrEmpty(site.COUNTY) ? site.COUNTY : string.Empty;
            this.WATERBODY = !string.IsNullOrEmpty(site.WATERBODY) ? site.WATERBODY : string.Empty;
            this.LATITUDE_DD = Convert.ToDouble(site.LATITUDE_DD);
            this.LONGITUDE_DD = Convert.ToDouble(site.LONGITUDE_DD);
            this.HDATUM_ID = Convert.ToInt32(site.HDATUM_ID);
            this.DRAINAGE_AREA_SQMI = Convert.ToDouble(site.DRAINAGE_AREA_SQMI);
            this.LANDOWNERCONTACT_ID = Convert.ToInt32(site.LANDOWNERCONTACT_ID);
            this.USGS_SID = !string.IsNullOrEmpty(site.USGS_SID) ? site.USGS_SID : string.Empty;
            this.NOAA_SID = !string.IsNullOrEmpty(site.NOAA_SID) ? site.NOAA_SID : string.Empty;


        }//end SiteLayer
        #endregion
    }//end class SiteLayer
}//end namespace





