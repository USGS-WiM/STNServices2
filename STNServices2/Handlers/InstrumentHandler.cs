//------------------------------------------------------------------------------
//----- InstrumentHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Instrument resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 02.07.13 - JKN - Added query to get Instruments by eventId and siteID
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.03.12 - JKN -Added Role Authorization 
// 06.08.12 - JB - Fixed Instrument Serial Number URI
// 06.04.12 - jkn -Created

#endregion

using STNServices2.Resources;
using STNServices2.Authentication;

using OpenRasta.Web;
using OpenRasta.Security;
using OpenRasta.Diagnostics;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;


namespace STNServices2.Handlers
{

    public class InstrumentHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "INSTRUMENTS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllInstruments")]
        public OperationResult GetAll()
        {
            List<INSTRUMENT> Insts = new List<INSTRUMENT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    Insts = aSTNE.INSTRUMENTs.OrderBy(inst => inst.INSTRUMENT_ID).ToList();

                    if (Insts != null)
                        Insts.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = Insts };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            INSTRUMENT anInstrument;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anInstrument = aSTNE.INSTRUMENTs.SingleOrDefault(inst => inst.INSTRUMENT_ID == entityId);

                    if (anInstrument != null)
                        anInstrument.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using            

                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileInstrument")]
        public OperationResult GetFileInstrument(Int32 fileId)
        {
            INSTRUMENT anInstrument;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anInstrument = aSTNE.FILES.FirstOrDefault(f => f.FILE_ID == fileId).INSTRUMENT;

                    if (anInstrument != null)
                        anInstrument.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentStatusInstrument")]
        public OperationResult GetInstrumentStatusInstrument(Int32 instrumentStatusId)
        {
            INSTRUMENT anInstrument;

            //Return BadRequest if there is no ID
            if (instrumentStatusId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anInstrument = aSTNE.INSTRUMENT_STATUS.FirstOrDefault(i => i.INSTRUMENT_STATUS_ID == instrumentStatusId).INSTRUMENT;

                    if (anInstrument != null)
                        anInstrument.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileInstrument")]
        public OperationResult GetDataFileInstrument(Int32 dataFileId)
        {
            INSTRUMENT anInstrument;

            //Return BadRequest if there is no ID
            if (dataFileId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anInstrument = aSTNE.DATA_FILE.FirstOrDefault(f => f.DATA_FILE_ID == dataFileId).INSTRUMENT;

                    if (anInstrument != null)
                        anInstrument.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteInstruments")]
        public OperationResult GetSiteInstruments(Int32 siteId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.INSTRUMENTs.AsEnumerable()
                                .Where(instr => instr.SITE_ID == siteId)
                                .OrderBy(instr => instr.INSTRUMENT_ID)
                                .ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SensorTypeInstruments")]
        public OperationResult SensorTypeInstruments(Int32 sensorTypeId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (sensorTypeId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.SENSOR_TYPE.FirstOrDefault(s => s.SENSOR_TYPE_ID == sensorTypeId).INSTRUMENTs.ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SensorBrandInstruments")]
        public OperationResult SensorBrandInstruments(Int32 sensorBrandId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (sensorBrandId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.SENSOR_BRAND.FirstOrDefault(s => s.SENSOR_BRAND_ID == sensorBrandId).INSTRUMENTs.ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDeploymentTypeInstruments")]
        public OperationResult GetDeploymentTypeInstruments(Int32 deploymentTypeId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (deploymentTypeId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.DEPLOYMENT_TYPE.FirstOrDefault(s => s.DEPLOYMENT_TYPE_ID == deploymentTypeId).INSTRUMENTs.ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventInstruments")]
        public OperationResult GetEventInstruments(Int32 eventId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (eventId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.EVENTS.FirstOrDefault(e => e.EVENT_ID == eventId).INSTRUMENTs.ToList();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSensorViews")]
        public OperationResult GetSensorViews([Optional] Int32 eventId)
        {
            SensorViews SensorViews = new SensorViews();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    var baro = aSTNE.BAROMETRIC_VIEW.ToList();
                    var met = aSTNE.METEOROLOGICAL_VIEW.ToList();
                    var RDG = aSTNE.RAPID_DEPLOYMENT_VIEW.ToList();
                    var storm = aSTNE.STORM_TIDE_VIEW.ToList();
                    var wave = aSTNE.WAVE_HEIGHT_VIEW.ToList();

                    //Return All Sensor Views data if there is no ID
                    if (eventId > 0)
                    {
                        SensorViews.Baro_View = baro.Where(b => b.EVENT_ID == eventId).ToList();
                        SensorViews.Met_View = met.Where(b => b.EVENT_ID == eventId).ToList();
                        SensorViews.RDG_View = RDG.Where(b => b.EVENT_ID == eventId).ToList();
                        SensorViews.StormTide_View = storm.Where(b => b.EVENT_ID == eventId).ToList();
                        SensorViews.WaveHeight_View = wave.Where(b => b.EVENT_ID == eventId).ToList();
                    }
                    else
                    {
                        SensorViews.Baro_View = baro;
                        SensorViews.Met_View = met;
                        SensorViews.RDG_View = RDG;
                        SensorViews.StormTide_View = storm;
                        SensorViews.WaveHeight_View = wave;
                    }
                }//end using

                return new OperationResult.OK { ResponseResource = SensorViews };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //I DONT SEE THIS BEING USED ANYWHERE
        [HttpOperation(ForUriName = "GetReportInstruments")]
        public OperationResult GetReportInstruments(string aDate, Int32 eventId, string stateAbbrev)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (eventId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.EVENTS.FirstOrDefault(e => e.EVENT_ID == eventId).INSTRUMENTs.ToList();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventInstruments")]
        public OperationResult Get(Int32 siteId, Int32 eventId)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            //Return BadRequest if there is no ID
            if (siteId <= 0 && eventId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentList = aSTNE.INSTRUMENTs.AsEnumerable()
                                .Where(instr => instr.SITE_ID == siteId && instr.EVENT_ID == eventId)
                                .OrderBy(instr => instr.INSTRUMENT_ID)
                                .ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetFilteredInstruments")]
        public OperationResult GetFilteredInstruments([Optional] string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID,
                                                      [Optional] string states, [Optional] string counties, [Optional] string statusIDs,
                                                      [Optional] string collectionConditionIDs, [Optional] string deploymentTypeIDs)
        {
            List<InstrumentDownloadable> instrumentList = new List<InstrumentDownloadable>();
            try
            {
                char[] delimiterChars = { ';', ',', ' ' };
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                List<decimal> statusIdList = !string.IsNullOrEmpty(statusIDs) ? statusIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> collectionConditionIdList = !string.IsNullOrEmpty(collectionConditionIDs) ? collectionConditionIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> deploymentTypeIdList = !string.IsNullOrEmpty(deploymentTypeIDs) ? deploymentTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<INSTRUMENT> query;
                    query = aSTNE.INSTRUMENTs.Where(s => s.INSTRUMENT_ID > 0);

                    if (eventIdList != null && eventIdList.Count > 0)
                        query = query.Where(i => i.EVENT_ID.HasValue && eventIdList.Contains(i.EVENT_ID.Value));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(i => i.EVENT.EVENT_TYPE_ID.HasValue && eventTypeList.Contains(i.EVENT.EVENT_TYPE_ID.Value));

                    if (eventStatusID > 0)
                        query = query.Where(i => i.EVENT.EVENT_STATUS_ID.HasValue && i.EVENT.EVENT_STATUS_ID.Value == eventStatusID);

                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(i => stateList.Contains(i.SITE.STATE));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(i => countyList.Contains(i.SITE.COUNTY));

                    if (collectionConditionIdList != null && collectionConditionIdList.Count > 0)
                        query = query.Where(i => i.INST_COLLECTION_ID.HasValue && collectionConditionIdList.Contains(i.INST_COLLECTION_ID.Value));

                    if (deploymentTypeIdList != null && deploymentTypeIdList.Count > 0)
                        query = query.Where(i => i.DEPLOYMENT_TYPE_ID.HasValue && deploymentTypeIdList.Contains(i.DEPLOYMENT_TYPE_ID.Value));

                    if (statusIdList != null && statusIdList.Count > 0)
                        query = query.AsEnumerable().Where(i => (i.INSTRUMENT_STATUS == null || i.INSTRUMENT_STATUS.Count <= 0) ? false :
                                                                    statusIdList.Contains(i.INSTRUMENT_STATUS.OrderByDescending(insStat => insStat.TIME_STAMP)
                                                                    .Where(stat => stat != null).FirstOrDefault().STATUS_TYPE_ID.Value)).AsQueryable();

                    instrumentList = query.AsEnumerable().Select(
                        inst => new InstrumentDownloadable
                        {
                            INSTRUMENT_ID = inst.INSTRUMENT_ID,
                            SENSOR_TYPE = inst.SENSOR_TYPE.SENSOR,
                            SENSOR_TYPE_ID = inst.SENSOR_TYPE.SENSOR_TYPE_ID,
                            DEPLOYMENT_TYPE = inst.DEPLOYMENT_TYPE_ID.HasValue ? GetDeploymentType(aSTNE, inst.DEPLOYMENT_TYPE_ID.Value) : "",
                            DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID.HasValue ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
                            SERIAL_NUMBER = inst.SERIAL_NUMBER,
                            HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
                            INTERVAL_IN_SEC = inst.INTERVAL,
                            SITE_ID = inst.SITE_ID,
                            EVENT = inst.EVENT_ID.HasValue ? GetEvent(aSTNE, inst.EVENT_ID.Value) : "",
                            LOCATION_DESCRIPTION = !string.IsNullOrEmpty(inst.LOCATION_DESCRIPTION) ? GetLocationDesc(inst.LOCATION_DESCRIPTION) : "",
                            COLLECTION_CONDITION = inst.INST_COLLECTION_ID.HasValue ? GetCollectConditions(aSTNE, inst.INST_COLLECTION_ID.Value) : "",
                            HOUSING_TYPE = inst.HOUSING_TYPE_ID.HasValue ? GetHouseType(aSTNE, inst.HOUSING_TYPE_ID.Value) : "",
                            VENTED = (inst.VENTED == null || inst.VENTED == "No") ? "No" : "Yes",
                            SENSOR_BRAND = inst.SENSOR_BRAND_ID.HasValue ? GetSensorBrand(aSTNE, inst.SENSOR_BRAND_ID.Value) : "",
                            STATUS = GetSensorStatus(aSTNE, inst.INSTRUMENT_ID),
                            TIMESTAMP = GetSensorStatDate(aSTNE, inst.INSTRUMENT_ID),
                            SITE_NO = inst.SITE.SITE_NO,
                            LATITUDE = inst.SITE.LATITUDE_DD,
                            LONGITUDE = inst.SITE.LONGITUDE_DD,
                            DESCRIPTION = inst.SITE.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(inst.SITE.SITE_DESCRIPTION) : "",
                            NETWORK = inst.SITE_ID.HasValue ? SiteHandler.GetSiteNetwork(aSTNE, inst.SITE_ID.Value) : "",
                            STATE = inst.SITE.STATE,
                            COUNTY = inst.SITE.COUNTY,
                            WATERBODY = inst.SITE.WATERBODY,
                            HORIZONTAL_DATUM = inst.SITE.HDATUM_ID > 0 ? SiteHandler.GetHDatum(aSTNE, inst.SITE.HDATUM_ID) : "",
                            PRIORITY = inst.SITE.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, inst.SITE.PRIORITY_ID.Value) : "",
                            ZONE = inst.SITE.ZONE,
                            HORIZONTAL_COLLECT_METHOD = inst.SITE.HCOLLECT_METHOD_ID.HasValue ? SiteHandler.GetHCollMethod(aSTNE, inst.SITE.HCOLLECT_METHOD_ID.Value) : "",
                            PERM_HOUSING_INSTALLED = inst.SITE.IS_PERMANENT_HOUSING_INSTALLED == null || inst.SITE.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
                            SITE_NOTES = !string.IsNullOrEmpty(inst.SITE.SITE_NOTES) ? SiteHandler.GetSiteNotes(inst.SITE.SITE_NOTES) : ""

                        }).ToList();



                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }




        [HttpOperation(ForUriName = "GetSiteInstrumentsByInternalId")]
        public OperationResult GetSiteInstrumentsByInternalId(String siteNo)
        {
            List<INSTRUMENT> instrumentList = new List<INSTRUMENT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    var aSite = aSTNE.SITES.Where(site => site.SITE_NO == siteNo).Select(s => s.SITE_ID).FirstOrDefault();

                    instrumentList = aSTNE.INSTRUMENTs.AsEnumerable()
                                .Where(inst => inst.SITE_ID == aSite)
                                .OrderBy(inst => inst.INSTRUMENT_ID)
                                .ToList<INSTRUMENT>();

                    if (instrumentList != null)
                        instrumentList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SerialNumbers")]
        public OperationResult GetInstrumentSerialNumbers()
        {
            InstrumentSerialNumberList instList = new InstrumentSerialNumberList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instList.Instruments = aSTNE.INSTRUMENTs.AsEnumerable().Select(
                        inst => new BaseInstrument
                        {
                            ID = Convert.ToInt32(inst.INSTRUMENT_ID),
                            SerialNumber = inst.SERIAL_NUMBER
                        }
                    ).ToList<BaseInstrument>();

                }//end using

                return new OperationResult.OK { ResponseResource = instList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFullInstruments")]
        public OperationResult GetFullInstruments(Int32 instrumentId)
        {
            //get the instrument and all instrument_stats together for an instrument
            FullInstrument fullInstrument;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<INSTRUMENT> instrument = aSTNE.INSTRUMENTs.Where(inst => inst.INSTRUMENT_ID == instrumentId);

                    fullInstrument = instrument.AsEnumerable().Select(
                        inst => new FullInstrument
                        {
                            Instrument = new Instrument
                            {
                                INSTRUMENT_ID = inst.INSTRUMENT_ID,
                                SENSOR_TYPE_ID = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE_ID.Value : 0,
                                Sensor_Type = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE.SENSOR : "",
                                DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
                                Deployment_Type = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE.METHOD : "",
                                SERIAL_NUMBER = inst.SERIAL_NUMBER,
                                HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
                                INTERVAL = inst.INTERVAL != null ? inst.INTERVAL.Value : 0,
                                SITE_ID = inst.SITE_ID != null ? inst.SITE_ID.Value : 0,
                                EVENT_ID = inst.EVENT_ID != null ? inst.EVENT_ID.Value : 0,
                                LOCATION_DESCRIPTION = inst.LOCATION_DESCRIPTION,
                                INST_COLLECTION_ID = inst.INST_COLLECTION_ID != null ? inst.INST_COLLECTION_ID.Value : 0,
                                Inst_Collection = inst.INST_COLLECTION_ID != null ? inst.INSTR_COLLECTION_CONDITIONS.CONDITION : "",
                                HOUSING_TYPE_ID = inst.HOUSING_TYPE_ID != null ? inst.HOUSING_TYPE_ID.Value : 0,
                                Housing_Type = inst.HOUSING_TYPE_ID != null ? inst.HOUSING_TYPE.TYPE_NAME : "",
                                VENTED = inst.VENTED,
                                SENSOR_BRAND_ID = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND_ID.Value : 0,
                                Sensor_Brand = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND.BRAND_NAME : ""
                            },
                            InstrumentStats = getInstStats(inst.INSTRUMENT_ID)
                        }).FirstOrDefault();
                }//end using            

                return new OperationResult.OK { ResponseResource = fullInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end Get        

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFullInstrumentList")]
        public OperationResult GetFullInstrumentList(Int32 siteId)
        {
            //get list of instrument and all instrument_stats together for a site
            List<FullInstrument> fullInstrumentList;

            //Return BadRequest if there is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<INSTRUMENT> instrumentList = aSTNE.INSTRUMENTs.Where(instr => instr.SITE_ID == siteId);

                    fullInstrumentList = instrumentList.AsEnumerable().Select(
                        inst => new FullInstrument
                        {
                            Instrument = new Instrument
                            {
                                INSTRUMENT_ID = inst.INSTRUMENT_ID,
                                SENSOR_TYPE_ID = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE_ID.Value : 0,
                                Sensor_Type = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE.SENSOR : "",
                                DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
                                Deployment_Type = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE.METHOD : "",
                                SERIAL_NUMBER = inst.SERIAL_NUMBER,
                                HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
                                INTERVAL = inst.INTERVAL != null ? inst.INTERVAL.Value : 0,
                                SITE_ID = inst.SITE_ID != null ? inst.SITE_ID.Value : 0,
                                EVENT_ID = inst.EVENT_ID != null ? inst.EVENT_ID.Value : 0,
                                LOCATION_DESCRIPTION = inst.LOCATION_DESCRIPTION,
                                INST_COLLECTION_ID = inst.INST_COLLECTION_ID != null && inst.INST_COLLECTION_ID > 0 ? inst.INST_COLLECTION_ID.Value : 0,
                                Inst_Collection = inst.INST_COLLECTION_ID != null && inst.INST_COLLECTION_ID > 0 ? inst.INSTR_COLLECTION_CONDITIONS.CONDITION : "",
                                HOUSING_TYPE_ID = inst.HOUSING_TYPE_ID != null && inst.HOUSING_TYPE_ID > 0 ? inst.HOUSING_TYPE_ID.Value : 0,
                                Housing_Type = inst.HOUSING_TYPE_ID != null && inst.HOUSING_TYPE_ID > 0 ? inst.HOUSING_TYPE.TYPE_NAME : "",
                                VENTED = inst.VENTED,
                                SENSOR_BRAND_ID = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND_ID.Value : 0,
                                Sensor_Brand = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND.BRAND_NAME : ""
                            },
                            InstrumentStats = getInstStats(inst.INSTRUMENT_ID)
                        }).ToList();
                }//end using            

                return new OperationResult.OK { ResponseResource = fullInstrumentList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end Get        
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "PostInstrument")]
        public OperationResult Post(INSTRUMENT anInstrument)
        {
            //Return BadRequest if missing required fields
            if ((anInstrument.DEPLOYMENT_TYPE_ID <= 0))
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //if this is a proposed sensor..no need to check if exists (if it doesn't have a serial number --required by form)
                        if (anInstrument.SERIAL_NUMBER != null)
                        {
                            if (!Exists(aSTNE.INSTRUMENTs, ref anInstrument))
                            {
                                aSTNE.INSTRUMENTs.AddObject(anInstrument);
                                aSTNE.SaveChanges();
                            }//end if
                        }
                        else
                        {
                            aSTNE.INSTRUMENTs.AddObject(anInstrument);
                            aSTNE.SaveChanges();
                        }//end if
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key
         * 
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, INSTRUMENT anInstrument)
        {
            INSTRUMENT instrumentToUpdate = null;
            //Return BadRequest if missing required fields
            if ((anInstrument.SENSOR_TYPE_ID <= 0 || entityId <= 0))
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        //Grab the instrument row to update
                        instrumentToUpdate = aSTNE.INSTRUMENTs.SingleOrDefault(instrument => instrument.INSTRUMENT_ID == entityId);
                        //Update fields
                        instrumentToUpdate.SENSOR_TYPE_ID = anInstrument.SENSOR_TYPE_ID;
                        instrumentToUpdate.DEPLOYMENT_TYPE_ID = anInstrument.DEPLOYMENT_TYPE_ID;
                        instrumentToUpdate.LOCATION_DESCRIPTION = anInstrument.LOCATION_DESCRIPTION;
                        instrumentToUpdate.SERIAL_NUMBER = anInstrument.SERIAL_NUMBER;
                        instrumentToUpdate.HOUSING_SERIAL_NUMBER = anInstrument.HOUSING_SERIAL_NUMBER;
                        instrumentToUpdate.INTERVAL = anInstrument.INTERVAL;
                        instrumentToUpdate.SITE_ID = anInstrument.SITE_ID;
                        instrumentToUpdate.EVENT_ID = anInstrument.EVENT_ID;
                        instrumentToUpdate.INST_COLLECTION_ID = anInstrument.INST_COLLECTION_ID;
                        instrumentToUpdate.HOUSING_TYPE_ID = anInstrument.HOUSING_TYPE_ID;
                        instrumentToUpdate.SENSOR_BRAND_ID = anInstrument.SENSOR_BRAND_ID;
                        instrumentToUpdate.VENTED = anInstrument.VENTED;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anInstrument };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.PUT

        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteInstrument")]
        public OperationResult Delete(Int32 instrumentId)
        {
            //Return BadRequest if missing required fields
            if (instrumentId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //first delete the INSTRUMENT_STATUSes for this INSTRUMENT, then delete the INSTRUMENT
                        List<INSTRUMENT_STATUS> stats = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instrumentId).ToList();

                        stats.ForEach(s => aSTNE.INSTRUMENT_STATUS.DeleteObject(s));
                        aSTNE.SaveChanges();

                        //now delete the instrument
                        INSTRUMENT ObjectToBeDeleted = aSTNE.INSTRUMENTs.SingleOrDefault(instr => instr.INSTRUMENT_ID == instrumentId);
                        //delete it
                        aSTNE.INSTRUMENTs.DeleteObject(ObjectToBeDeleted);

                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HTTP.DELETE
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<INSTRUMENT> entityRDS, ref INSTRUMENT anEntity)
        {
            INSTRUMENT existingEntity;
            INSTRUMENT thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.SENSOR_TYPE_ID == thisEntity.SENSOR_TYPE_ID &&
                                                              (e.DEPLOYMENT_TYPE_ID.Value == thisEntity.DEPLOYMENT_TYPE_ID.Value || thisEntity.DEPLOYMENT_TYPE_ID.Value <= 0 || !thisEntity.DEPLOYMENT_TYPE_ID.HasValue) &&
                                                              (e.SENSOR_BRAND_ID.Value == thisEntity.SENSOR_BRAND_ID.Value || thisEntity.SENSOR_BRAND_ID.Value <= 0 || !thisEntity.SENSOR_BRAND_ID.HasValue) &&
                                                              (e.INTERVAL == thisEntity.INTERVAL || thisEntity.INTERVAL <= 0 || thisEntity.INTERVAL == null) &&
                                                              (e.EVENT_ID == thisEntity.EVENT_ID || thisEntity.EVENT_ID <= 0 || thisEntity.EVENT_ID == null) &&
                                                              (string.Equals(e.LOCATION_DESCRIPTION.ToUpper(), thisEntity.LOCATION_DESCRIPTION.ToUpper()) || string.IsNullOrEmpty(thisEntity.LOCATION_DESCRIPTION)) &&
                                                              (string.Equals(e.SERIAL_NUMBER.ToUpper(), thisEntity.SERIAL_NUMBER.ToUpper()) || string.IsNullOrEmpty(thisEntity.SERIAL_NUMBER)) &&
                                                              (string.Equals(e.VENTED.ToUpper(), thisEntity.VENTED.ToUpper()) || string.IsNullOrEmpty(thisEntity.VENTED)) &&
                                                              (string.Equals(e.HOUSING_SERIAL_NUMBER.ToUpper(), thisEntity.HOUSING_SERIAL_NUMBER.ToUpper()) || string.IsNullOrEmpty(thisEntity.HOUSING_SERIAL_NUMBER)));


                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        #region methods for instrumentDownloadable

        private decimal getValue(System.Data.Objects.DataClasses.EntityCollection<INSTRUMENT_STATUS> entityCollection)
        {
            var x = entityCollection.AsEnumerable().OrderByDescending(insStat => insStat.TIME_STAMP).ToList();
            var y = x.FirstOrDefault().STATUS_TYPE_ID.Value;
            return y;
        }//end HttpMethod.GET

        private List<Instrument_Status> getInstStats(decimal instrumentId)
        {
            List<Instrument_Status> instrumentStatusList;

            using (STNEntities2 aSTNE = GetRDS())
            {
                instrumentStatusList = aSTNE.INSTRUMENT_STATUS.AsEnumerable()
                                     .Where(instStat => instStat.INSTRUMENT_ID == instrumentId)
                                     .OrderByDescending(instStat => instStat.TIME_STAMP)
                                     .Select(i => new Instrument_Status
                                     {
                                         INSTRUMENT_STATUS_ID = i.INSTRUMENT_STATUS_ID,
                                         STATUS_TYPE_ID = i.STATUS_TYPE_ID != null ? i.STATUS_TYPE_ID.Value : 0,
                                         Status = i.STATUS_TYPE_ID != null ? i.STATUS_TYPE.STATUS : "",
                                         INSTRUMENT_ID = i.INSTRUMENT_ID != null ? i.INSTRUMENT_ID.Value : 0,
                                         TIME_STAMP = i.TIME_STAMP.Value,
                                         TIME_ZONE = i.TIME_ZONE,
                                         NOTES = i.NOTES//,
                                         // MEMBER_ID = i.MEMBER_ID != null ? i.MEMBER_ID.Value : 0
                                     }).ToList();
            }
            return instrumentStatusList;
        }

        private string GetDeploymentType(STNEntities2 aSTNE, decimal depTypeId)
        {
            string depName = string.Empty;
            if (depTypeId > 0)
            {
                DEPLOYMENT_TYPE thisDp = aSTNE.DEPLOYMENT_TYPE.Where(x => x.DEPLOYMENT_TYPE_ID == depTypeId).FirstOrDefault();
                if (thisDp != null)
                    depName = thisDp.METHOD;


            }
            return depName;
        }
        private string GetEvent(STNEntities2 aSTNE, decimal eventId)
        {
            string eventName = string.Empty;
            if (eventId > 0)
            {
                EVENT ev = aSTNE.EVENTS.Where(x => x.EVENT_ID == eventId).FirstOrDefault();
                if (ev != null)
                    eventName = ev.EVENT_NAME;
            }
            return eventName;
        }
        private string GetLocationDesc(string desc)
        {
            string locDesc = string.Empty;

            if (desc.Length > 100)
            {
                locDesc = desc.Substring(0, 100);
            }
            else
            {
                locDesc = desc;
            }

            return locDesc;
        }
        private string GetCollectConditions(STNEntities2 aSTNE, decimal cc)
        {
            string collCond = string.Empty;
            INSTR_COLLECTION_CONDITIONS icc = aSTNE.INSTR_COLLECTION_CONDITIONS.Where(x => x.ID == cc).FirstOrDefault();
            if (icc != null)
                collCond = icc.CONDITION;

            return collCond;
        }
        private string GetHouseType(STNEntities2 aSTNE, decimal houseType)
        {
            string htName = string.Empty;
            HOUSING_TYPE ht = aSTNE.HOUSING_TYPE.Where(x => x.HOUSING_TYPE_ID == houseType).FirstOrDefault();
            if (ht != null)
                htName = ht.TYPE_NAME;

            return htName;
        }
        private string GetSensorBrand(STNEntities2 aSTNE, decimal sb)
        {
            string sensName = string.Empty;
            SENSOR_BRAND hb = aSTNE.SENSOR_BRAND.Where(x => x.SENSOR_BRAND_ID == sb).FirstOrDefault();
            if (hb != null)
                sensName = hb.BRAND_NAME;

            return sensName;
        }
        private string GetSensorStatus(STNEntities2 aSTNE, decimal instId)
        {
            string stat = string.Empty;
            INSTRUMENT_STATUS instrStat = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instId).OrderByDescending(y => y.TIME_STAMP).FirstOrDefault();
            if (instrStat != null)
            {
                switch (Convert.ToInt32(instrStat.STATUS_TYPE_ID.Value))
                {
                    case 1:
                        stat = "Deployed";
                        break;
                    case 2:
                        stat = "Retrieved";
                        break;
                    case 3:
                        stat = "Lost";
                        break;
                    default:
                        stat = "Proposed";
                        break;
                }
            }
            return stat;
        }
        private string GetSensorStatDate(STNEntities2 aSTNE, decimal instId)
        {
            string dt = string.Empty;
            INSTRUMENT_STATUS instrStat = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instId).OrderByDescending(y => y.TIME_STAMP).FirstOrDefault();
            if (instrStat != null)
            {
                if (instrStat.TIME_STAMP != null)
                {
                    dt = ((DateTime)(instrStat.TIME_STAMP)).ToString() + " " + instrStat.TIME_ZONE;
                }
            }
            return dt;
        }
        #endregion

        #endregion
    }//end class InstrumentHandler
}//end namespace