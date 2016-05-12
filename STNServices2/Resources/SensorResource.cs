using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using STNDB;

namespace STNServices2.Resources
{
    public class sensor_view
    {
        public Int32 site_id { get; set; }
        public string site_no { get; set; }
        public string site_name { get; set; }
        public double latitude_dd { get; set; }
        public double longitude_dd { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public Int32 instrument_id { get; set; }
        public Int32 event_id { get; set; }
        public string event_name { get; set; }
        public Nullable<DateTime> event_start_date { get; set; }
        public Nullable<DateTime> event_end_date { get; set; }
        public Nullable<Int32> sensor_type_id { get; set; }
        public string sensor { get; set; }
        public  Nullable<Int32> deployment_type_id { get; set; }
        public string method { get; set; }
        public string status { get; set; }
        public Nullable<DateTime> time_stamp { get; set; }

    }//end class sensor_view

    [XmlInclude(typeof(instrument))]
    public class InstrumentDownloadable : instrument
    {
        public string sensorType { get; set; }
        public string deploymentType { get; set; }
        public string eventName { get; set; }
        public string collectionCondition { get; set; }
        public string housingType { get; set; }
        public string sensorBrand { get; set; }
        public Nullable<Int32> statusId { get; set; }
        public Nullable<DateTime> timeStamp { get; set; }
        public string site_no { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string siteDescription { get; set; }
        public string networkNames { get; set; }
        public string stateName { get; set; }
        public string countyName { get; set; }
        public string siteWaterbody { get; set; }
        public string siteHDatum { get; set; }
        public string sitePriorityName { get; set; }
        public string siteZone { get; set; }
        public string siteHCollectMethod { get; set; }
        public string sitePermHousing { get; set; }
        public string siteNotes { get; set; }
    }//end class InstrumentDownloadable

    [XmlInclude(typeof(instrument))]
    public class FullInstrument : instrument
    {
        public string sensorType { get; set; }
        public string deploymentType { get; set; }
        public string instCollection { get; set; }
        public string housingType { get; set; }
        public string sensorBrand { get; set; }
       
    }
    [XmlInclude(typeof(instrument_status))]
    public class Instrument_Status : instrument_status
    {
        public string status { get; set; }
        public string vdatum { get; set; }

    }

}
