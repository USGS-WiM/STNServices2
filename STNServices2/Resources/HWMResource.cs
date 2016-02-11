//------------------------------------------------------------------------------
//----- HWMResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   HWM resources through the HTTP uniform interface.
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
    [XmlRoot("ArrayOfHWM")]
    public class HWMList
    {
        [DataMember]
        [XmlElement(typeof(SimpleHWM),
        ElementName = "HWM")]
        public List<SimpleHWM> HWMs { get; set; }
    }//end class HWMList

    [DataContract]
    public class SimpleHWM : HypermediaEntity
    {
        [DataMember]
        [XmlElement(DataType = "int",
        ElementName = "HWM_ID")]
        public Int32 HWM_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SITE_NO")]
        public string SITE_NO { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "LATITUDE")]
        public Decimal LATITUDE_DD { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "LONGITUDE")]
        public Decimal LONGITUDE_DD { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_LOCATIONDESCRIPTION")]
        public string HWM_LOCATIONDESCRIPTION { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "ELEV_FT")]
        public Decimal ELEV_FT { get; set; }

        [DataMember]
        [XmlElement(typeof(DateTime?),
        ElementName = "FLAG_DATE")]
        public DateTime? FLAG_DATE { get; set; }

        [DataMember]
        [XmlElement(typeof(DateTime?),
        ElementName = "SURVEY_DATE")]
        public DateTime? SURVEY_DATE { get; set; }


        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("HWMs/{0}", this.HWM_ID);
                    break;

                case refType.POST:
                    uriString = "HWMs";
                    break;

            }
            return uriString;
        }
        #endregion

    }//end class SimpleHWM

    [DataContract]
    public class HWMDownloadable
    {
        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "HWM_ID")]
        public decimal HWM_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "WATERBODY")]
        public string WATERBODY { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SITE_ID")]
        public decimal SITE_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "EVENT")]
        public string EVENT { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_TYPE")]
        public string HWM_TYPE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_QUALITY")]
        public string HWM_QUALITY { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_LOCATION_DESCRIPTION")]
        public string HWM_LOCATION_DESCRIPTION { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "LATITUDE")]
        public Decimal LATITUDE { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "LONGITUDE")]
        public Decimal LONGITUDE { get; set; }

        [DataMember]
        [XmlElement(typeof(Decimal),
        ElementName = "ELEV_FT")]
        public Decimal ELEV_FT { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "VERTICAL_DATUM")]
        public string VERTICAL_DATUM { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "VERTICAL_COLLECT_METHOD")]
        public string VERTICAL_COLLECT_METHOD { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HORIZONTAL_COLLECT_METHOD")]
        public string HORIZONTAL_COLLECT_METHOD { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "BANK")]
        public string BANK { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "APPROVAL_MEMBER")]
        public string APPROVAL_MEMBER { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "MARKER_NAME")]
        public string MARKER_NAME { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "HEIGHT_ABV_GND")]
        public decimal HEIGHT_ABV_GND { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_NOTES")]
        public string HWM_NOTES { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HWM_ENVIRONMENT")]
        public string HWM_ENVIRONMENT { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "FLAG_DATE")]
        public string FLAG_DATE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SURVEY_DATE")]
        public string SURVEY_DATE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "STILLWATER")]
        public string STILLWATER { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "FLAG_MEMBER_NAME")]
        public string FLAG_MEMBER_NAME { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SURVEY_MEMBER_NAME")]
        public string SURVEY_MEMBER_NAME { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SITE_NO")]
        public String SITE_NO { get; set; }

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
        ElementName = "PERM_HOUSING_INSTALLED")]
        public String PERM_HOUSING_INSTALLED { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SITE_NOTES")]
        public String SITE_NOTES { get; set; }
    }//end class HWMDownloadable


    public class HWMLayer : FeatureBase
    {
        #region "Fields"
        private string _waterBody;
        private DateTime? _surveyDate;
        private string _foundOn;
        private string _locationDescription;
        private string _bank;
        private string _arrivalTime;
        private string _departureTime;
        private string _Notes;

        #endregion
        #region "Properties"

        public Int32 HWM_ID { get; set; }
        public string WATERBODY
        {
            get { return _waterBody; }
            set { _waterBody = LimitLength(value, 100); }
        }
        public Int32? SITE_ID { get; set; }
        public Int32? EVENT_ID { get; set; }
        public Int32? HWM_TYPE_ID { get; set; }
        public string HWM_FOUNDON
        {
            get { return _foundOn; }
            set { _foundOn = this.LimitLength(value, 50); }
        }
        public Int32? HWM_QUALITY_ID { get; set; }
        public string HWM_LOCATIONDESCRIPTION
        {
            get { return _locationDescription; }
            set { _locationDescription = this.LimitLength(value, 1000); }
        }
        public DateTime? SURVEY_DATE { get; set; }

        public double? ELEV_FT { get; set; }
        public Int32? VDATUM_ID { get; set; }
        public Int32? FLAG_MEMBER_ID { get; set; }
        public Int32? SURVEY_MEMBER_ID { get; set; }
        public Int32? VCOLLECT_METHOD_ID { get; set; }
        public string BANK
        {
            get { return _bank; }
            set { _bank = this.LimitLength(value, 1); }
        }
        public Int32? APPROVAL_ID { get; set; }
        public string ARRIVAL_TIME
        {
            get { return _arrivalTime; }
            set { _arrivalTime = this.LimitLength(value, 5); }
        }
        public string DEPARTURE_TIME
        {
            get { return _departureTime; }
            set { _departureTime = this.LimitLength(value, 5); }
        }
        public Int32? MARKER_ID { get; set; }
        public double? HEIGHT_ABOVE_GND { get; set; }
        public Int32? HCOLLECT_METHOD_ID { get; set; }
        public Int32? PEAK_SUMMARY_ID { get; set; }
        public string HWM_NOTES
        {
            get { return _Notes; }
            set { _Notes = this.LimitLength(value, 1000); }
        }
        public DateTime? FLAG_DATE { get; set; }

        #endregion
        #region "Constructor"
        public HWMLayer(HWM hwm)
            : base()
        {

            this.HWM_ID = Convert.ToInt32(hwm.HWM_ID);
            this.WATERBODY = !string.IsNullOrEmpty(hwm.WATERBODY) ? hwm.WATERBODY : string.Empty;
            this.SITE_ID = Convert.ToInt32(hwm.SITE_ID);
            this.EVENT_ID = Convert.ToInt32(hwm.EVENT_ID);
            this.HWM_TYPE_ID = Convert.ToInt32(hwm.HWM_TYPE_ID);
            this.HWM_QUALITY_ID = Convert.ToInt32(hwm.HWM_QUALITY_ID);
            this.HWM_LOCATIONDESCRIPTION = !string.IsNullOrEmpty(hwm.HWM_LOCATIONDESCRIPTION) ? hwm.HWM_LOCATIONDESCRIPTION : string.Empty;
            this.LATITUDE_DD = Convert.ToDouble(hwm.LATITUDE_DD);
            this.LONGITUDE_DD = Convert.ToDouble(hwm.LONGITUDE_DD);
            this.SURVEY_DATE = hwm.SURVEY_DATE;
            this.ELEV_FT = Convert.ToDouble(hwm.ELEV_FT);
            this.VDATUM_ID = Convert.ToInt32(hwm.VDATUM_ID);
            this.FLAG_MEMBER_ID = Convert.ToInt32(hwm.FLAG_MEMBER_ID);
            this.SURVEY_MEMBER_ID = Convert.ToInt32(hwm.SURVEY_MEMBER_ID);
            this.VCOLLECT_METHOD_ID = Convert.ToInt32(hwm.VCOLLECT_METHOD_ID);
            this.BANK = !string.IsNullOrEmpty(hwm.BANK) ? hwm.BANK : string.Empty;
            this.APPROVAL_ID = Convert.ToInt32(hwm.APPROVAL_ID);
            this.MARKER_ID = Convert.ToInt32(hwm.MARKER_ID);
            this.HEIGHT_ABOVE_GND = Convert.ToDouble(hwm.HEIGHT_ABOVE_GND);
            this.HCOLLECT_METHOD_ID = Convert.ToInt32(hwm.HCOLLECT_METHOD_ID);
            this.PEAK_SUMMARY_ID = Convert.ToInt32(hwm.PEAK_SUMMARY_ID);
            this.HWM_NOTES = !string.IsNullOrEmpty(hwm.HWM_NOTES) ? hwm.HWM_NOTES : String.Empty;
            this.FLAG_DATE = hwm.FLAG_DATE;


        }//end HWMLayerAttributes
        #endregion
        #region "Methods"
        #endregion
        #region "Helper Methods"
        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        //public long UnixTicks(DateTime dt)
        // {
        //     DateTime d1 = new DateTime(1970, 1, 1);
        //     DateTime d2 = dt.ToUniversalTime();
        //     TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
        //     return (long)ts.TotalMilliseconds;
        // }

        #endregion

    }//end class HWMLayer

}//end namespace
