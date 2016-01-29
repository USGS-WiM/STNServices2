//------------------------------------------------------------------------------
//----- Instrument -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Instrument resources through the HTTP uniform interface.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 05.29.12 - jkn - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace STNServices2.Resources
{

    [XmlRoot("Results")]
    public class InstrumentSerialNumberList
    {
        [XmlArray("INSTRUMENTS")]
        [XmlArrayItem(typeof(BaseInstrument),
        ElementName = "INSTRUMENT")]
        public List<BaseInstrument> Instruments { get; set; }

    }

    public class BaseInstrument
    {
        #region Properties
        [XmlElement(DataType = "int",
        ElementName = "ID")]
        public Int32 ID { get; set; }

        [XmlElement(DataType = "string",
        ElementName = "SERIAL_NUMBER")]
        public String SerialNumber { get; set; }

        #endregion

        #region Constructors

        public BaseInstrument()
        {
            ID = -1;
            SerialNumber = string.Empty;

        }//end SimpleInstrument
        #endregion
    }//end BaseInstrument

    public class FullInstrument
    {
        [DataMember]
        [XmlElement(typeof(Instrument),
        ElementName = "Instrument")]
        public Instrument Instrument { get; set; }

        [DataMember]
        [XmlElement(typeof(Instrument_Status),
        ElementName = "InstrumentStats")]
        public List<Instrument_Status> InstrumentStats { get; set; }

    }

    public class Instrument
    {
        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INSTRUMENT_ID")]
        public decimal INSTRUMENT_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SENSOR_TYPE_ID")]
        public decimal? SENSOR_TYPE_ID { get; set; }
        public bool ShouldSerializeSENSOR_TYPE_ID()
        { return SENSOR_TYPE_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Sensor_Type")]
        public string Sensor_Type { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "DEPLOYMENT_TYPE_ID")]
        public decimal? DEPLOYMENT_TYPE_ID { get; set; }
        public bool ShouldSerializeDEPLOYMENT_TYPE_ID()
        { return DEPLOYMENT_TYPE_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Deployment_Type")]
        public string Deployment_Type { get; set; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "SERIAL_NUMBER")]
        public string SERIAL_NUMBER { get; set; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "HOUSING_SERIAL_NUMBER")]
        public string HOUSING_SERIAL_NUMBER { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INTERVAL")]
        public decimal? INTERVAL { get; set; }
        public bool ShouldSerializeINTERVAL()
        { return INTERVAL.HasValue; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SITE_ID")]
        public decimal? SITE_ID { get; set; }
        public bool ShouldSerializeSITE_ID()
        { return SITE_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "EVENT_ID")]
        public decimal? EVENT_ID { get; set; }
        public bool ShouldSerializeEVENT_ID()
        { return EVENT_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "LOCATION_DESCRIPTION")]
        public string LOCATION_DESCRIPTION { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INST_COLLECTION_ID")]
        public decimal? INST_COLLECTION_ID { get; set; }
        public bool ShouldSerializeINST_COLLECTION_ID()
        { return INST_COLLECTION_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Inst_Collection")]
        public string Inst_Collection { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "HOUSING_TYPE_ID")]
        public decimal? HOUSING_TYPE_ID { get; set; }
        public bool ShouldSerializeHOUSING_TYPE_ID()
        { return HOUSING_TYPE_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Housing_Type")]
        public string Housing_Type { get; set; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "VENTED")]
        public string VENTED { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SENSOR_BRAND_ID")]
        public decimal? SENSOR_BRAND_ID { get; set; }
        public bool ShouldSerializeSENSOR_BRAND_ID()
        { return SENSOR_BRAND_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Sensor_Brand")]
        public string Sensor_Brand { get; set; }

    }

    public class Instrument_Status
    {
        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INSTRUMENT_STATUS_ID")]
        public decimal INSTRUMENT_STATUS_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "STATUS_TYPE_ID")]
        public decimal? STATUS_TYPE_ID { get; set; }
        public bool ShouldSerializeSTATUS_TYPE_ID()
        { return STATUS_TYPE_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "Status")]
        public string Status { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INSTRUMENT_ID")]
        public decimal? INSTRUMENT_ID { get; set; }
        public bool ShouldSerializeINSTRUMENT_ID()
        { return INSTRUMENT_ID.HasValue; }

        [DataMember]
        [XmlElement(typeof(DateTime),
        ElementName = "TIME_STAMP")]
        public DateTime? TIME_STAMP { get; set; }
        public bool ShouldSerializeTIME_STAMP()
        { return TIME_STAMP.HasValue; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "TIME_ZONE")]
        public string TIME_ZONE { get; set; }

        [DataMember]
        [XmlElement(typeof(string),
        ElementName = "NOTES")]
        public string NOTES { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "MEMBER_ID")]
        public decimal MEMBER_ID { get; set; }

        //[DataMember]
        //[XmlElement(typeof(MEMBER),
        //ElementName = "COLLECT_MEMBER")]
        //public MEMBER COLLECT_MEMBER { get; set; }
    }

    public class InstrumentDownloadable
    {
        //for Portal Download tool

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "INSTRUMENT_ID")]
        public decimal INSTRUMENT_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SENSOR_TYPE")]
        public string SENSOR_TYPE { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "SENSOR_TYPE_ID")]
        public decimal SENSOR_TYPE_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "DEPLOYMENT_TYPE")]
        public String DEPLOYMENT_TYPE { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal),
        ElementName = "DEPLOYMENT_TYPE_ID")]
        public decimal DEPLOYMENT_TYPE_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SERIAL_NUMBER")]
        public String SERIAL_NUMBER { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HOUSING_SERIAL_NUMBER")]
        public String HOUSING_SERIAL_NUMBER { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal?),
        ElementName = "INTERVAL_IN_SEC")]
        public decimal? INTERVAL_IN_SEC { get; set; }

        [DataMember]
        [XmlElement(typeof(decimal?),
        ElementName = "SITE_ID")]
        public decimal? SITE_ID { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "EVENT")]
        public String EVENT { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "LOCATION_DESCRIPTION")]
        public String LOCATION_DESCRIPTION { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "COLLECTION_CONDITION")]
        public string COLLECTION_CONDITION { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "HOUSING_TYPE")]
        public String HOUSING_TYPE { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "VENTED")]
        public String VENTED { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "SENSOR_BRAND")]
        public String SENSOR_BRAND { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "STATUS")]
        public String STATUS { get; set; }

        [DataMember]
        [XmlElement(typeof(String),
        ElementName = "TIMESTAMP")]
        public String TIMESTAMP { get; set; }

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
    }
}//end namespace

