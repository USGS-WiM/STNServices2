//------------------------------------------------------------------------------
//----- File Resource ----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   File resources through the HTTP uniform interface.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 12.16.13 - JKN - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace STNServices2.Resources
{
    public class CITIZEN_FILE : HypermediaEntity
    {
        [XmlElement(typeof(Decimal),
        ElementName = "FILE_ID")]
        public Decimal FILE_ID { get; set; }

        [XmlElement(typeof(String),
        ElementName = "DESCRIPTION")]
        public string DESCRIPTION { get; set; }

        [XmlElement(typeof(Decimal),
        ElementName = "LATITUDE")]
        public Decimal? LATITUDE_DD { get; set; }
        public bool ShouldSerializeLATITUDE_DD()
        { return LATITUDE_DD.HasValue; }

        [XmlElement(typeof(Decimal),
        ElementName = "LONGITUDE")]
        public Decimal? LONGITUDE_DD { get; set; }
        public bool ShouldSerializeLONGITUDE_DD()
        { return LONGITUDE_DD.HasValue; }

        [XmlElement(typeof(DateTime),
        ElementName = "FILE_DATE")]
        public DateTime? FILE_DATE { get; set; }
        public bool ShouldSerializeFILE_DATE()
        { return FILE_DATE.HasValue; }

        [XmlElement(typeof(Decimal),
        ElementName = "VALIDATED")]
        public Decimal? VALIDATED { get; set; }
        public bool ShouldSerializeVALIDATED()
        { return VALIDATED.HasValue; }

        [XmlElement(typeof(String),
        ElementName = "VALIDATOR_USERID")]
        public String VALIDATOR_USERID { get; set; }

        [XmlElement(typeof(DateTime),
        ElementName = "DATE_VALIDATED")]
        public DateTime? DATE_VALIDATED { get; set; }
        public bool ShouldSerializeDATE_VALIDATED()
        { return DATE_VALIDATED.HasValue; }

        [XmlElement(typeof(Decimal),
        ElementName = "LOCATOR_TYPE_ID")]
        public Decimal? LOCATOR_TYPE_ID { get; set; }
        public bool ShouldSerializeLOCATOR_TYPE_ID()
        { return LOCATOR_TYPE_ID.HasValue; }

        [XmlElement(typeof(List<KEYWORD>),
        ElementName = "KEYWORDS")]
        public List<KEYWORD> KEYWORDS { get; set; }

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("FILES/{0}", this.FILE_ID);
                    break;

                case refType.POST:
                    uriString = "FILES";
                    break;

            }
            return uriString;
        }
        #endregion
    }//end 

}//end namespace