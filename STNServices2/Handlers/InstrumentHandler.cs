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

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using WiM.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;


namespace STNServices2.Handlers
{

    public class InstrumentHandler : STNHandlerBase
    {

        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<instrument> entities = null;
            try{
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            instrument anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument>().SingleOrDefault(inst => inst.instrument_id == entityId);
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileInstrument")]
        public OperationResult GetFileInstrument(Int32 fileId)
        {
            instrument anEntity;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).instrument;
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentStatusInstrument")]
        public OperationResult GetInstrumentStatusInstrument(Int32 instrumentStatusId)
        {
            instrument anEntity;
            try
            {
                if (instrumentStatusId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument_status>().FirstOrDefault(i => i.instrument_status_id == instrumentStatusId).instrument;
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileInstrument")]
        public OperationResult GetDataFileInstrument(Int32 dataFileId)
        {
            instrument anEntity;
            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<data_file>().FirstOrDefault(f => f.data_file_id == dataFileId).instrument;
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteInstruments")]
        public OperationResult GetSiteInstruments(Int32 siteId)
        {
            List<instrument> entities;
            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().AsEnumerable()
                                .Where(instr => instr.site_id == siteId)
                                .OrderBy(instr => instr.instrument_id)
                                .ToList<instrument>();
                    
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SensorTypeInstruments")]
        public OperationResult SensorTypeInstruments(Int32 sensorTypeId)
        {
            List<instrument> entities;
            try
            {
                if (sensorTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<sensor_type>().FirstOrDefault(s => s.sensor_type_id == sensorTypeId).instruments.ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SensorBrandInstruments")]
        public OperationResult SensorBrandInstruments(Int32 sensorBrandId)
        {
            List<instrument> entities;
            try
            {
                if (sensorBrandId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<sensor_brand>().FirstOrDefault(s => s.sensor_brand_id == sensorBrandId).instruments.ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDeploymentTypeInstruments")]
        public OperationResult GetDeploymentTypeInstruments(Int32 deploymentTypeId)
        {
            List<instrument> entities;
            try
            {
                if (deploymentTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<deployment_type>().FirstOrDefault(s => s.deployment_type_id == deploymentTypeId).instruments.ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventInstruments")]
        public OperationResult GetEventInstruments(Int32 eventId)
        {
            List<instrument> entities;
            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<events>().FirstOrDefault(e => e.event_id == eventId).instruments.ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        //[HttpOperation(ForUriName = "GetSensorViews")]
        //public OperationResult GetSensorViews([Optional] Int32 eventId)
        //{
        //    SensorViews SensorViews = new SensorViews();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            var baro = aSTNE.BAROMETRIC_VIEW.ToList();
        //            var met = aSTNE.METEOROLOGICAL_VIEW.ToList();
        //            var RDG = aSTNE.RAPID_DEPLOYMENT_VIEW.ToList();
        //            var storm = aSTNE.STORM_TIDE_VIEW.ToList();
        //            var wave = aSTNE.WAVE_HEIGHT_VIEW.ToList();

        //            //Return All Sensor Views data if there is no ID
        //            if (eventId > 0)
        //            {
        //                SensorViews.Baro_View = baro.Where(b => b.EVENT_ID == eventId).ToList();
        //                SensorViews.Met_View = met.Where(b => b.EVENT_ID == eventId).ToList();
        //                SensorViews.RDG_View = RDG.Where(b => b.EVENT_ID == eventId).ToList();
        //                SensorViews.StormTide_View = storm.Where(b => b.EVENT_ID == eventId).ToList();
        //                SensorViews.WaveHeight_View = wave.Where(b => b.EVENT_ID == eventId).ToList();
        //            }
        //            else
        //            {
        //                SensorViews.Baro_View = baro;
        //                SensorViews.Met_View = met;
        //                SensorViews.RDG_View = RDG;
        //                SensorViews.StormTide_View = storm;
        //                SensorViews.WaveHeight_View = wave;
        //            }
        //        }//end using

        //        return new OperationResult.OK { ResponseResource = SensorViews };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventInstruments")]
        public OperationResult GetSiteEventInstruments(Int32 siteId, Int32 eventId)
        {
            List<instrument> entities;
            try
            {
                if (siteId <= 0 || eventId <=0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().AsEnumerable()
                                .Where(instr => instr.site_id == siteId && instr.event_id == eventId)
                                .OrderBy(instr => instr.instrument_id)
                                .ToList();
                        sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredInstruments")]
        //public OperationResult GetFilteredInstruments([Optional] string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID,
        //                                              [Optional] string states, [Optional] string counties, [Optional] string statusIDs,
        //                                              [Optional] string collectionConditionIDs, [Optional] string deploymentTypeIDs)
        //{
        //    List<InstrumentDownloadable> instrumentList = new List<InstrumentDownloadable>();
        //    try
        //    {
        //        char[] delimiterChars = { ';', ',', ' ' };
        //        //parse the requests
        //        List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
        //        List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
        //        List<decimal> statusIdList = !string.IsNullOrEmpty(statusIDs) ? statusIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> collectionConditionIdList = !string.IsNullOrEmpty(collectionConditionIDs) ? collectionConditionIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> deploymentTypeIdList = !string.IsNullOrEmpty(deploymentTypeIDs) ? deploymentTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<INSTRUMENT> query;
        //            query = aSTNE.INSTRUMENTs.Where(s => s.INSTRUMENT_ID > 0);

        //            if (eventIdList != null && eventIdList.Count > 0)
        //                query = query.Where(i => i.EVENT_ID.HasValue && eventIdList.Contains(i.EVENT_ID.Value));

        //            if (eventTypeList != null && eventTypeList.Count > 0)
        //                query = query.Where(i => i.EVENT.EVENT_TYPE_ID.HasValue && eventTypeList.Contains(i.EVENT.EVENT_TYPE_ID.Value));

        //            if (eventStatusID > 0)
        //                query = query.Where(i => i.EVENT.EVENT_STATUS_ID.HasValue && i.EVENT.EVENT_STATUS_ID.Value == eventStatusID);

        //            if (stateList != null && stateList.Count > 0)
        //                query = query.Where(i => stateList.Contains(i.SITE.STATE));

        //            if (countyList != null && countyList.Count > 0)
        //                query = query.Where(i => countyList.Contains(i.SITE.COUNTY));

        //            if (collectionConditionIdList != null && collectionConditionIdList.Count > 0)
        //                query = query.Where(i => i.INST_COLLECTION_ID.HasValue && collectionConditionIdList.Contains(i.INST_COLLECTION_ID.Value));

        //            if (deploymentTypeIdList != null && deploymentTypeIdList.Count > 0)
        //                query = query.Where(i => i.DEPLOYMENT_TYPE_ID.HasValue && deploymentTypeIdList.Contains(i.DEPLOYMENT_TYPE_ID.Value));

        //            if (statusIdList != null && statusIdList.Count > 0)
        //                query = query.AsEnumerable().Where(i => (i.INSTRUMENT_STATUS == null || i.INSTRUMENT_STATUS.Count <= 0) ? false :
        //                                                            statusIdList.Contains(i.INSTRUMENT_STATUS.OrderByDescending(insStat => insStat.TIME_STAMP)
        //                                                            .Where(stat => stat != null).FirstOrDefault().STATUS_TYPE_ID.Value)).AsQueryable();

        //            instrumentList = query.AsEnumerable().Select(
        //                inst => new InstrumentDownloadable
        //                {
        //                    INSTRUMENT_ID = inst.INSTRUMENT_ID,
        //                    SENSOR_TYPE = inst.SENSOR_TYPE.SENSOR,
        //                    SENSOR_TYPE_ID = inst.SENSOR_TYPE.SENSOR_TYPE_ID,
        //                    DEPLOYMENT_TYPE = inst.DEPLOYMENT_TYPE_ID.HasValue ? GetDeploymentType(aSTNE, inst.DEPLOYMENT_TYPE_ID.Value) : "",
        //                    DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID.HasValue ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
        //                    SERIAL_NUMBER = inst.SERIAL_NUMBER,
        //                    HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
        //                    INTERVAL_IN_SEC = inst.INTERVAL,
        //                    SITE_ID = inst.SITE_ID,
        //                    EVENT = inst.EVENT_ID.HasValue ? GetEvent(aSTNE, inst.EVENT_ID.Value) : "",
        //                    LOCATION_DESCRIPTION = !string.IsNullOrEmpty(inst.LOCATION_DESCRIPTION) ? GetLocationDesc(inst.LOCATION_DESCRIPTION) : "",
        //                    COLLECTION_CONDITION = inst.INST_COLLECTION_ID.HasValue ? GetCollectConditions(aSTNE, inst.INST_COLLECTION_ID.Value) : "",
        //                    HOUSING_TYPE = inst.HOUSING_TYPE_ID.HasValue ? GetHouseType(aSTNE, inst.HOUSING_TYPE_ID.Value) : "",
        //                    VENTED = (inst.VENTED == null || inst.VENTED == "No") ? "No" : "Yes",
        //                    SENSOR_BRAND = inst.SENSOR_BRAND_ID.HasValue ? GetSensorBrand(aSTNE, inst.SENSOR_BRAND_ID.Value) : "",
        //                    STATUS = GetSensorStatus(aSTNE, inst.INSTRUMENT_ID),
        //                    TIMESTAMP = GetSensorStatDate(aSTNE, inst.INSTRUMENT_ID),
        //                    SITE_NO = inst.SITE.SITE_NO,
        //                    LATITUDE = inst.SITE.LATITUDE_DD,
        //                    LONGITUDE = inst.SITE.LONGITUDE_DD,
        //                    DESCRIPTION = inst.SITE.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(inst.SITE.SITE_DESCRIPTION) : "",
        //                    NETWORK = inst.SITE_ID.HasValue ? SiteHandler.GetSiteNetwork(aSTNE, inst.SITE_ID.Value) : "",
        //                    STATE = inst.SITE.STATE,
        //                    COUNTY = inst.SITE.COUNTY,
        //                    WATERBODY = inst.SITE.WATERBODY,
        //                    HORIZONTAL_DATUM = inst.SITE.HDATUM_ID > 0 ? SiteHandler.GetHDatum(aSTNE, inst.SITE.HDATUM_ID) : "",
        //                    PRIORITY = inst.SITE.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, inst.SITE.PRIORITY_ID.Value) : "",
        //                    ZONE = inst.SITE.ZONE,
        //                    HORIZONTAL_COLLECT_METHOD = inst.SITE.HCOLLECT_METHOD_ID.HasValue ? SiteHandler.GetHCollMethod(aSTNE, inst.SITE.HCOLLECT_METHOD_ID.Value) : "",
        //                    PERM_HOUSING_INSTALLED = inst.SITE.IS_PERMANENT_HOUSING_INSTALLED == null || inst.SITE.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
        //                    SITE_NOTES = !string.IsNullOrEmpty(inst.SITE.SITE_NOTES) ? SiteHandler.GetSiteNotes(inst.SITE.SITE_NOTES) : ""

        //                }).ToList();



        //        }//end using

        //        return new OperationResult.OK { ResponseResource = instrumentList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}


        [HttpOperation(ForUriName = "GetSiteInstrumentsByInternalId")]
        public OperationResult GetSiteInstrumentsByInternalId(String siteNo)
        {
            List<instrument> entities;
            try
            {
                if (string.IsNullOrEmpty(siteNo)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().Include(i=>i.site)
                                .Where(inst => inst.site.site_no == siteNo)
                                .OrderBy(inst => inst.instrument_id)
                                .ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        //[HttpOperation(HttpMethod.GET, ForUriName = "SerialNumbers")]
        //public OperationResult GetInstrumentSerialNumbers()
        //{
        //    InstrumentSerialNumberList instList = new InstrumentSerialNumberList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            instList.Instruments = aSTNE.INSTRUMENTs.AsEnumerable().Select(
        //                inst => new BaseInstrument
        //                {
        //                    ID = Convert.ToInt32(inst.INSTRUMENT_ID),
        //                    SerialNumber = inst.SERIAL_NUMBER
        //                }
        //            ).ToList<BaseInstrument>();

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = instList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetFullInstruments")]
        //public OperationResult GetFullInstruments(Int32 instrumentId)
        //{
        //    //get the instrument and all instrument_stats together for an instrument
        //    FullInstrument fullInstrument;

        //    //Return BadRequest if there is no ID
        //    if (instrumentId <= 0)
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<INSTRUMENT> instrument = aSTNE.INSTRUMENTs.Where(inst => inst.INSTRUMENT_ID == instrumentId);

        //            fullInstrument = instrument.AsEnumerable().Select(
        //                inst => new FullInstrument
        //                {
        //                    Instrument = new Instrument
        //                    {
        //                        INSTRUMENT_ID = inst.INSTRUMENT_ID,
        //                        SENSOR_TYPE_ID = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE_ID.Value : 0,
        //                        Sensor_Type = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE.SENSOR : "",
        //                        DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
        //                        Deployment_Type = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE.METHOD : "",
        //                        SERIAL_NUMBER = inst.SERIAL_NUMBER,
        //                        HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
        //                        INTERVAL = inst.INTERVAL != null ? inst.INTERVAL.Value : 0,
        //                        SITE_ID = inst.SITE_ID != null ? inst.SITE_ID.Value : 0,
        //                        EVENT_ID = inst.EVENT_ID != null ? inst.EVENT_ID.Value : 0,
        //                        LOCATION_DESCRIPTION = inst.LOCATION_DESCRIPTION,
        //                        INST_COLLECTION_ID = inst.INST_COLLECTION_ID != null ? inst.INST_COLLECTION_ID.Value : 0,
        //                        Inst_Collection = inst.INST_COLLECTION_ID != null ? inst.INSTR_COLLECTION_CONDITIONS.CONDITION : "",
        //                        HOUSING_TYPE_ID = inst.HOUSING_TYPE_ID != null ? inst.HOUSING_TYPE_ID.Value : 0,
        //                        Housing_Type = inst.HOUSING_TYPE_ID != null ? inst.HOUSING_TYPE.TYPE_NAME : "",
        //                        VENTED = inst.VENTED,
        //                        SENSOR_BRAND_ID = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND_ID.Value : 0,
        //                        Sensor_Brand = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND.BRAND_NAME : ""
        //                    },
        //                    InstrumentStats = getInstStats(inst.INSTRUMENT_ID)
        //                }).FirstOrDefault();
        //        }//end using            

        //        return new OperationResult.OK { ResponseResource = fullInstrument };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end Get        

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetFullInstrumentList")]
        //public OperationResult GetFullInstrumentList(Int32 siteId)
        //{
        //    //get list of instrument and all instrument_stats together for a site
        //    List<FullInstrument> fullInstrumentList;

        //    //Return BadRequest if there is no ID
        //    if (siteId <= 0)
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<INSTRUMENT> instrumentList = aSTNE.INSTRUMENTs.Where(instr => instr.SITE_ID == siteId);

        //            fullInstrumentList = instrumentList.AsEnumerable().Select(
        //                inst => new FullInstrument
        //                {
        //                    Instrument = new Instrument
        //                    {
        //                        INSTRUMENT_ID = inst.INSTRUMENT_ID,
        //                        SENSOR_TYPE_ID = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE_ID.Value : 0,
        //                        Sensor_Type = inst.SENSOR_TYPE_ID != null ? inst.SENSOR_TYPE.SENSOR : "",
        //                        DEPLOYMENT_TYPE_ID = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE_ID.Value : 0,
        //                        Deployment_Type = inst.DEPLOYMENT_TYPE_ID != null && inst.DEPLOYMENT_TYPE_ID != 0 ? inst.DEPLOYMENT_TYPE.METHOD : "",
        //                        SERIAL_NUMBER = inst.SERIAL_NUMBER,
        //                        HOUSING_SERIAL_NUMBER = inst.HOUSING_SERIAL_NUMBER,
        //                        INTERVAL = inst.INTERVAL != null ? inst.INTERVAL.Value : 0,
        //                        SITE_ID = inst.SITE_ID != null ? inst.SITE_ID.Value : 0,
        //                        EVENT_ID = inst.EVENT_ID != null ? inst.EVENT_ID.Value : 0,
        //                        LOCATION_DESCRIPTION = inst.LOCATION_DESCRIPTION,
        //                        INST_COLLECTION_ID = inst.INST_COLLECTION_ID != null && inst.INST_COLLECTION_ID > 0 ? inst.INST_COLLECTION_ID.Value : 0,
        //                        Inst_Collection = inst.INST_COLLECTION_ID != null && inst.INST_COLLECTION_ID > 0 ? inst.INSTR_COLLECTION_CONDITIONS.CONDITION : "",
        //                        HOUSING_TYPE_ID = inst.HOUSING_TYPE_ID != null && inst.HOUSING_TYPE_ID > 0 ? inst.HOUSING_TYPE_ID.Value : 0,
        //                        Housing_Type = inst.HOUSING_TYPE_ID != null && inst.HOUSING_TYPE_ID > 0 ? inst.HOUSING_TYPE.TYPE_NAME : "",
        //                        VENTED = inst.VENTED,
        //                        SENSOR_BRAND_ID = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND_ID.Value : 0,
        //                        Sensor_Brand = inst.SENSOR_BRAND_ID != null ? inst.SENSOR_BRAND.BRAND_NAME : ""
        //                    },
        //                    InstrumentStats = getInstStats(inst.INSTRUMENT_ID)
        //                }).ToList();
        //        }//end using            

        //        return new OperationResult.OK { ResponseResource = fullInstrumentList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end Get       

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "PostInstrument")]
        public OperationResult Post(instrument anEntity)
        {
            try
            {
                if (!anEntity.sensor_type_id.HasValue || anEntity.sensor_type_id <= 0 ||
                    !anEntity.sensor_brand_id.HasValue || anEntity.sensor_brand_id <= 0 ||
                    !anEntity.event_id.HasValue || anEntity.event_id <= 0 ||
                    !anEntity.site_id.HasValue || anEntity.site_id <= 0)
                        throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<instrument>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, instrument anEntity)
        {
            try
            {
                if (!anEntity.sensor_type_id.HasValue || anEntity.sensor_type_id <= 0 ||
                    !anEntity.sensor_brand_id.HasValue || anEntity.sensor_brand_id <= 0 ||
                    !anEntity.event_id.HasValue || anEntity.event_id <= 0 ||
                    !anEntity.site_id.HasValue || anEntity.site_id <= 0)
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<instrument>(entityId, anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Modified { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.PUT

        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        instrument ObjectToBeDeleted = sa.Select<instrument>().SingleOrDefault(c => c.instrument_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<instrument>(ObjectToBeDeleted);
                        #region Cascadedelete?
                        ////delete files associated with this sensor
                        //List<FILES> opFiles = aSTNE.FILES.Where(x => x.INSTRUMENT_ID == instrumentId).ToList();
                        //if (opFiles.Count >= 1)
                        //{
                        //    foreach (FILES f in opFiles)
                        //    {
                        //        //delete data files to this file
                        //        if (f.DATA_FILE_ID.HasValue)
                        //        {
                        //            DATA_FILE df = aSTNE.DATA_FILE.Where(x => x.DATA_FILE_ID == f.DATA_FILE_ID).FirstOrDefault();
                        //            aSTNE.DATA_FILE.DeleteObject(df);
                        //            aSTNE.SaveChanges();
                        //        }
                        //        //delete the file item from s3
                        //        S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                        //        aBucket.DeleteObject(BuildFilePath(f, f.PATH));
                        //        //delete the file
                        //        aSTNE.FILES.DeleteObject(f);
                        //        aSTNE.SaveChanges();
                        //    }
                        //}
                        ////first delete the INSTRUMENT_STATUSes for this INSTRUMENT, then delete the INSTRUMENT
                        //List<INSTRUMENT_STATUS> stats = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instrumentId).ToList();

                        //stats.ForEach(s => aSTNE.INSTRUMENT_STATUS.DeleteObject(s));
                        //aSTNE.SaveChanges();

                        ////now delete the instrument
                        //INSTRUMENT ObjectToBeDeleted = aSTNE.INSTRUMENTs.SingleOrDefault(instr => instr.INSTRUMENT_ID == instrumentId);
                        ////delete it
                        //aSTNE.INSTRUMENTs.DeleteObject(ObjectToBeDeleted);

                        #endregion
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

        #region Helper Methods
      

        //#region methods for instrumentDownloadable

        //private decimal getValue(System.Data.Objects.DataClasses.EntityCollection<INSTRUMENT_STATUS> entityCollection)
        //{
        //    var x = entityCollection.AsEnumerable().OrderByDescending(insStat => insStat.TIME_STAMP).ToList();
        //    var y = x.FirstOrDefault().STATUS_TYPE_ID.Value;
        //    return y;
        //}//end HttpMethod.GET

        //private List<Instrument_Status> getInstStats(decimal instrumentId)
        //{
        //    List<Instrument_Status> instrumentStatusList = new List<Instrument_Status>();

        //    using (STNEntities2 aSTNE = GetRDS())
        //    {
        //        try
        //        {
        //            instrumentStatusList = aSTNE.INSTRUMENT_STATUS.AsEnumerable()
        //                                 .Where(instStat => instStat.INSTRUMENT_ID == instrumentId)
        //                                 .OrderByDescending(instStat => instStat.TIME_STAMP)
        //                                 .Select(i => new Instrument_Status
        //                                 {
        //                                     INSTRUMENT_STATUS_ID = i.INSTRUMENT_STATUS_ID,
        //                                     STATUS_TYPE_ID = i.STATUS_TYPE_ID != null ? i.STATUS_TYPE_ID.Value : 0,
        //                                     Status = i.STATUS_TYPE_ID != null ? i.STATUS_TYPE.STATUS : "",
        //                                     INSTRUMENT_ID = i.INSTRUMENT_ID != null ? i.INSTRUMENT_ID.Value : 0,
        //                                     TIME_STAMP = i.TIME_STAMP.Value,
        //                                     TIME_ZONE = i.TIME_ZONE,
        //                                     NOTES = i.NOTES,
        //                                     MEMBER_ID = i.MEMBER_ID != null ? i.MEMBER_ID.Value : 0,
        //                                     SENSOR_ELEVATION = i.SENSOR_ELEVATION,
        //                                     WS_ELEVATION = i.WS_ELEVATION,
        //                                     GS_ELEVATION = i.GS_ELEVATION,
        //                                     VDATUM_ID = i.VDATUM_ID,
        //                                     VDatum = i.VDATUM_ID.HasValue && i.VDATUM_ID > 0 ? i.VERTICAL_DATUMS.DATUM_ABBREVIATION : ""
        //                                 }).ToList();
        //        }
        //        catch
        //        {
        //            return instrumentStatusList;
        //        }

        //    }
        //    return instrumentStatusList;
        //}

        //private string GetDeploymentType(STNEntities2 aSTNE, decimal depTypeId)
        //{
        //    string depName = string.Empty;
        //    if (depTypeId > 0)
        //    {
        //        DEPLOYMENT_TYPE thisDp = aSTNE.DEPLOYMENT_TYPE.Where(x => x.DEPLOYMENT_TYPE_ID == depTypeId).FirstOrDefault();
        //        if (thisDp != null)
        //            depName = thisDp.METHOD;


        //    }
        //    return depName;
        //}
        //private string GetEvent(STNEntities2 aSTNE, decimal eventId)
        //{
        //    string eventName = string.Empty;
        //    if (eventId > 0)
        //    {
        //        EVENT ev = aSTNE.EVENTS.Where(x => x.EVENT_ID == eventId).FirstOrDefault();
        //        if (ev != null)
        //            eventName = ev.EVENT_NAME;
        //    }
        //    return eventName;
        //}
        //private string GetLocationDesc(string desc)
        //{
        //    string locDesc = string.Empty;

        //    if (desc.Length > 100)
        //    {
        //        locDesc = desc.Substring(0, 100);
        //    }
        //    else
        //    {
        //        locDesc = desc;
        //    }

        //    return locDesc;
        //}
        //private string GetCollectConditions(STNEntities2 aSTNE, decimal cc)
        //{
        //    string collCond = string.Empty;
        //    INSTR_COLLECTION_CONDITIONS icc = aSTNE.INSTR_COLLECTION_CONDITIONS.Where(x => x.ID == cc).FirstOrDefault();
        //    if (icc != null)
        //        collCond = icc.CONDITION;

        //    return collCond;
        //}
        //private string GetHouseType(STNEntities2 aSTNE, decimal houseType)
        //{
        //    string htName = string.Empty;
        //    HOUSING_TYPE ht = aSTNE.HOUSING_TYPE.Where(x => x.HOUSING_TYPE_ID == houseType).FirstOrDefault();
        //    if (ht != null)
        //        htName = ht.TYPE_NAME;

        //    return htName;
        //}
        //private string GetSensorBrand(STNEntities2 aSTNE, decimal sb)
        //{
        //    string sensName = string.Empty;
        //    SENSOR_BRAND hb = aSTNE.SENSOR_BRAND.Where(x => x.SENSOR_BRAND_ID == sb).FirstOrDefault();
        //    if (hb != null)
        //        sensName = hb.BRAND_NAME;

        //    return sensName;
        //}
        //private string GetSensorStatus(STNEntities2 aSTNE, decimal instId)
        //{
        //    string stat = string.Empty;
        //    INSTRUMENT_STATUS instrStat = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instId).OrderByDescending(y => y.TIME_STAMP).FirstOrDefault();
        //    if (instrStat != null)
        //    {
        //        switch (Convert.ToInt32(instrStat.STATUS_TYPE_ID.Value))
        //        {
        //            case 1:
        //                stat = "Deployed";
        //                break;
        //            case 2:
        //                stat = "Retrieved";
        //                break;
        //            case 3:
        //                stat = "Lost";
        //                break;
        //            default:
        //                stat = "Proposed";
        //                break;
        //        }
        //    }
        //    return stat;
        //}
        //private string GetSensorStatDate(STNEntities2 aSTNE, decimal instId)
        //{
        //    string dt = string.Empty;
        //    INSTRUMENT_STATUS instrStat = aSTNE.INSTRUMENT_STATUS.Where(x => x.INSTRUMENT_ID == instId).OrderByDescending(y => y.TIME_STAMP).FirstOrDefault();
        //    if (instrStat != null)
        //    {
        //        if (instrStat.TIME_STAMP != null)
        //        {
        //            dt = ((DateTime)(instrStat.TIME_STAMP)).ToString() + " " + instrStat.TIME_ZONE;
        //        }
        //    }
        //    return dt;
        //}
        //private string BuildFilePath(FILES uploadFile, string fileName)
        //{
        //    try
        //    {
        //        //determine default object name
        //        // ../SITE/3043/ex.jpg
        //        List<string> objectName = new List<string>();
        //        objectName.Add("SITE");
        //        objectName.Add(uploadFile.SITE_ID.ToString());

        //        if (uploadFile.HWM_ID != null && uploadFile.HWM_ID > 0)
        //        {
        //            // ../SITE/3043/HWM/7956/ex.jpg
        //            objectName.Add("HWM");
        //            objectName.Add(uploadFile.HWM_ID.ToString());
        //        }
        //        else if (uploadFile.DATA_FILE_ID != null && uploadFile.DATA_FILE_ID > 0)
        //        {
        //            // ../SITE/3043/DATA_FILE/7956/ex.txt
        //            objectName.Add("DATA_FILE");
        //            objectName.Add(uploadFile.DATA_FILE_ID.ToString());
        //        }
        //        else if (uploadFile.INSTRUMENT_ID != null && uploadFile.INSTRUMENT_ID > 0)
        //        {
        //            // ../SITE/3043/INSTRUMENT/7956/ex.jpg
        //            objectName.Add("INSTRUMENT");
        //            objectName.Add(uploadFile.INSTRUMENT_ID.ToString());
        //        }
        //        objectName.Add(fileName);

        //        return string.Join("/", objectName);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
        //#endregion

        #endregion
    }//end class InstrumentHandler
}//end namespace