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

namespace STNServices2.Resources
{
    public class SensorViews
    {
        [XmlElement(typeof(List<BAROMETRIC_VIEW>),
        ElementName = "Baro_View")]
        public List<BAROMETRIC_VIEW> Baro_View { get; set; }

        [XmlElement(typeof(List<METEOROLOGICAL_VIEW>),
        ElementName = "Met_View")]
        public List<METEOROLOGICAL_VIEW> Met_View { get; set; }

        [XmlElement(typeof(List<RAPID_DEPLOYMENT_VIEW>),
        ElementName = "RDG_View")]
        public List<RAPID_DEPLOYMENT_VIEW> RDG_View { get; set; }

        [XmlElement(typeof(List<STORM_TIDE_VIEW>),
        ElementName = "StormTide_View")]
        public List<STORM_TIDE_VIEW> StormTide_View { get; set; }

        [XmlElement(typeof(List<WAVE_HEIGHT_VIEW>),
        ElementName = "WaveHeight_View")]
        public List<WAVE_HEIGHT_VIEW> WaveHeight_View { get; set; }

    }
    //[XmlRoot("Results")]
    //public class InstrumentSerialNumberList
    //{
    //    [XmlArray("INSTRUMENTS")]
    //    [XmlArrayItem(typeof(BaseInstrument),
    //    ElementName = "INSTRUMENT")]
    //    public List<BaseInstrument> Instruments { get; set; }

    //}


    //public class SimpleInstrument:BaseInstrument
    //{
    //    #region Properties

    //    [XmlElement(typeof(SimpleSensorType),
    //    ElementName = "TYPE")]
    //    public SimpleSensorType SensorType { get; set; }

    //    [XmlElement(DataType = "string",
    //    ElementName = "DEPLOYMENT_TYPE")]
    //    public SimpleDeploymentType DeploymentType { get; set; }

    //    [XmlElement(DataType = "string",
    //    ElementName = "DEPLOYMENT_NOTES")]
    //    public String DeploymentNotes { get; set; }

    //    [XmlElement(DataType = "string",
    //    ElementName = "HOUSING_SERIAL_NUMBER")]
    //    public String HousingSerialNumber { get; set; }
    //    #endregion

    //    #region Constructors

    //    public SimpleInstrument()
    //    {
    //        ID = -1;
    //        SensorType = new SimpleSensorType();
    //        DeploymentType = new SimpleDeploymentType();
    //        DeploymentNotes = string.Empty;
    //        SerialNumber = string.Empty;
    //        HousingSerialNumber = string.Empty;

    //    }//end SimpleInstrument
    //    #endregion

    //}//end SimpleInstrument


    //public class BaseInstrument
    //{ 
    //   #region Properties
    //    [XmlElement(DataType = "int",
    //    ElementName = "ID")]
    //    public Int32 ID { get; set; }

    //    [XmlElement(DataType = "string",
    //    ElementName = "SERIAL_NUMBER")]
    //    public String SerialNumber { get; set; }

    //    #endregion

    //    #region Constructors

    //    public BaseInstrument()
    //    {
    //        ID = -1;
    //        SerialNumber = string.Empty;

    //    }//end SimpleInstrument
    //    #endregion
    //}//end BaseInstrument
}//end namespace