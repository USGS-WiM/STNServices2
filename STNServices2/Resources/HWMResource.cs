using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Xml.Serialization;
using STNDB;

namespace STNServices2.Resources
{   

    [XmlInclude(typeof(hwm))]
    public class HWMDownloadable : hwm
    {
        public string eventName { get; set; }
        public string hwmTypeName { get; set; }
        public string hwm_label { get; set; }
        public string hwmQualityName { get; set; }
        public string verticalDatumName { get; set; }
        public string verticalMethodName { get; set; }
        public string approvalMember { get; set; }
        public string markerName { get; set; }
        public string horizontalMethodName { get; set; }
        public string horizontalDatumName { get; set; }
        public string flagMemberName { get; set; }
        public string surveyMemberName { get; set; }
        public string site_no { get; set; }
        public string siteDescription { get; set; }
        public string sitePriorityName { get; set; }
        public string networkNames { get; set; }
        public string stateName { get; set; }
        public string countyName { get; set; }
        public string siteZone { get; set; }
        public string sitePermHousing { get; set; }
        public string siteNotes { get; set; }
        public double site_latitude { get; set; }
        public double site_longitude { get; set; }
    }//end class HWMDownloadable


    //public class HWMLayer : FeatureBase
    //{
    //    #region "Fields"
    //    private string _waterBody;
    //    private DateTime? _surveyDate;
    //    private string _foundOn;
    //    private string _locationDescription;
    //    private string _bank;
    //    private string _arrivalTime;
    //    private string _departureTime;
    //    private string _Notes;

    //    #endregion
    //    #region "Properties"

    //    public Int32 HWM_ID { get; set; }
    //    public string WATERBODY
    //    {
    //        get { return _waterBody; }
    //        set { _waterBody = LimitLength(value, 100); }
    //    }
    //    public Int32? SITE_ID { get; set; }
    //    public Int32? EVENT_ID { get; set; }
    //    public Int32? HWM_TYPE_ID { get; set; }
    //    public string HWM_FOUNDON
    //    {
    //        get { return _foundOn; }
    //        set { _foundOn = this.LimitLength(value, 50); }
    //    }
    //    public Int32? HWM_QUALITY_ID { get; set; }
    //    public string HWM_LOCATIONDESCRIPTION
    //    {
    //        get { return _locationDescription; }
    //        set { _locationDescription = this.LimitLength(value, 1000); }
    //    }
    //    public DateTime? SURVEY_DATE { get; set; }

    //    public double? ELEV_FT { get; set; }
    //    public Int32? VDATUM_ID { get; set; }
    //    public Int32? FLAG_MEMBER_ID { get; set; }
    //    public Int32? SURVEY_MEMBER_ID { get; set; }
    //    public Int32? VCOLLECT_METHOD_ID { get; set; }
    //    public string BANK
    //    {
    //        get { return _bank; }
    //        set { _bank = this.LimitLength(value, 1); }
    //    }
    //    public Int32? APPROVAL_ID { get; set; }
    //    public string ARRIVAL_TIME
    //    {
    //        get { return _arrivalTime; }
    //        set { _arrivalTime = this.LimitLength(value, 5); }
    //    }
    //    public string DEPARTURE_TIME
    //    {
    //        get { return _departureTime; }
    //        set { _departureTime = this.LimitLength(value, 5); }
    //    }
    //    public Int32? MARKER_ID { get; set; }
    //    public double? HEIGHT_ABOVE_GND { get; set; }
    //    public Int32? HCOLLECT_METHOD_ID { get; set; }
    //    public Int32? PEAK_SUMMARY_ID { get; set; }
    //    public string HWM_NOTES
    //    {
    //        get { return _Notes; }
    //        set { _Notes = this.LimitLength(value, 1000); }
    //    }
    //    public DateTime? FLAG_DATE { get; set; }

    //    #endregion
    //    #region "Constructor"
    //    public HWMLayer(HWM hwm)
    //        : base()
    //    {

    //        this.HWM_ID = Convert.ToInt32(hwm.HWM_ID);
    //        this.WATERBODY = !string.IsNullOrEmpty(hwm.WATERBODY) ? hwm.WATERBODY : string.Empty;
    //        this.SITE_ID = Convert.ToInt32(hwm.SITE_ID);
    //        this.EVENT_ID = Convert.ToInt32(hwm.EVENT_ID);
    //        this.HWM_TYPE_ID = Convert.ToInt32(hwm.HWM_TYPE_ID);
    //        this.HWM_QUALITY_ID = Convert.ToInt32(hwm.HWM_QUALITY_ID);
    //        this.HWM_LOCATIONDESCRIPTION = !string.IsNullOrEmpty(hwm.HWM_LOCATIONDESCRIPTION) ? hwm.HWM_LOCATIONDESCRIPTION : string.Empty;
    //        this.LATITUDE_DD = Convert.ToDouble(hwm.LATITUDE_DD);
    //        this.LONGITUDE_DD = Convert.ToDouble(hwm.LONGITUDE_DD);
    //        this.SURVEY_DATE = hwm.SURVEY_DATE;
    //        this.ELEV_FT = Convert.ToDouble(hwm.ELEV_FT);
    //        this.VDATUM_ID = Convert.ToInt32(hwm.VDATUM_ID);
    //        this.FLAG_MEMBER_ID = Convert.ToInt32(hwm.FLAG_MEMBER_ID);
    //        this.SURVEY_MEMBER_ID = Convert.ToInt32(hwm.SURVEY_MEMBER_ID);
    //        this.VCOLLECT_METHOD_ID = Convert.ToInt32(hwm.VCOLLECT_METHOD_ID);
    //        this.BANK = !string.IsNullOrEmpty(hwm.BANK) ? hwm.BANK : string.Empty;
    //        this.APPROVAL_ID = Convert.ToInt32(hwm.APPROVAL_ID);
    //        this.MARKER_ID = Convert.ToInt32(hwm.MARKER_ID);
    //        this.HEIGHT_ABOVE_GND = Convert.ToDouble(hwm.HEIGHT_ABOVE_GND);
    //        this.HCOLLECT_METHOD_ID = Convert.ToInt32(hwm.HCOLLECT_METHOD_ID);
    //        this.PEAK_SUMMARY_ID = Convert.ToInt32(hwm.PEAK_SUMMARY_ID);
    //        this.HWM_NOTES = !string.IsNullOrEmpty(hwm.HWM_NOTES) ? hwm.HWM_NOTES : String.Empty;
    //        this.FLAG_DATE = hwm.FLAG_DATE;


    //    }//end HWMLayerAttributes
    //    #endregion
    //    #region "Methods"
    //    #endregion
    //    #region "Helper Methods"
    //    // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
    //    //public long UnixTicks(DateTime dt)
    //    // {
    //    //     DateTime d1 = new DateTime(1970, 1, 1);
    //    //     DateTime d2 = dt.ToUniversalTime();
    //    //     TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
    //    //     return (long)ts.TotalMilliseconds;
    //    // }

    //    #endregion

    //}//end class HWMLayer

}//end namespace
