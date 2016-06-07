//------------------------------------------------------------------------------
//----- InstrumentHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
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
// 03.29.16 - JKN - Major update
#endregion

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNServices2.Resources;
using WiM.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;


namespace STNServices2.Handlers
{

    public class InstrumentHandler : STNHandlerBase
    {

        #region GetMethods
        [HttpOperation(HttpMethod.GET, ForUriName="GetAllInstruments")]
        public OperationResult Get()
        {
            List<instrument> entities = null;
            try{
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
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
                    if (anEntity == null) throw new NotFoundRequestException(); 
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
                    anEntity = sa.Select<file>().Include(f=>f.instrument).FirstOrDefault(f => f.file_id == fileId).instrument;
                    if (anEntity == null) throw new NotFoundRequestException(); 
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
                using (STNAgent sa = new STNAgent(true))
                {
                    anEntity = sa.Select<instrument_status>().FirstOrDefault(i => i.instrument_status_id == instrumentStatusId).instrument;
                    if (anEntity == null) throw new NotFoundRequestException(); 
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
                using (STNAgent sa = new STNAgent(true))
                {
                    anEntity = sa.Select<data_file>().FirstOrDefault(f => f.data_file_id == dataFileId).instrument;
                    if (anEntity == null) throw new NotFoundRequestException(); 
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
                    entities = sa.Select<instrument>().AsEnumerable().Where(instr => instr.site_id == siteId).OrderBy(instr => instr.instrument_id).ToList<instrument>();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSensorTypeInstruments")]
        public OperationResult SensorTypeInstruments(Int32 sensorTypeId)
        {
            List<instrument> entities;
            try
            {
                if (sensorTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<sensor_type>().FirstOrDefault(s => s.sensor_type_id == sensorTypeId).instruments.ToList();
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSensorBrandInstruments")]
        public OperationResult SensorBrandInstruments(Int32 sensorBrandId)
        {
            List<instrument> entities;
            try
            {
                if (sensorBrandId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<sensor_brand>().FirstOrDefault(s => s.sensor_brand_id == sensorBrandId).instruments.ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
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
                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<deployment_type>().FirstOrDefault(s => s.deployment_type_id == deploymentTypeId).instruments.ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
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
                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<events>().FirstOrDefault(e => e.event_id == eventId).instruments.ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventInstruments")]
        public OperationResult GetSiteEventInstruments(Int32 siteId, Int32 eventId)
        {
            List<instrument> entities;
            try
            {
                if (siteId <= 0 || eventId <=0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().AsEnumerable().Where(instr => instr.site_id == siteId && instr.event_id == eventId).OrderBy(instr => instr.instrument_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #region sensorViews response
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
        #endregion sensorViews response

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredInstruments")]
        public OperationResult GetFilteredInstruments([Optional] string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID, [Optional] string states, [Optional] string counties, 
                                                        [Optional] string statusIDs, [Optional] string collectionConditionIDs, [Optional] string deploymentTypeIDs)
        {
            List<instrument> entities = null;
            try
            {
                char[] delimiterChars = { ';', ',', ' ' }; char[] countydelimiterChars = { ';', ',' };
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                List<decimal> statusIdList = !string.IsNullOrEmpty(statusIDs) ? statusIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> collectionConditionIdList = !string.IsNullOrEmpty(collectionConditionIDs) ? collectionConditionIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> deploymentTypeIdList = !string.IsNullOrEmpty(deploymentTypeIDs) ? deploymentTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<instrument> query;
                    query = sa.Select<instrument>().Include(i => i.instrument_status).Include(i => i.@event).Include(i => i.sensor_brand).Include(i => i.sensor_type).Include(i => i.instr_collection_conditions).Include(i => i.site)
                        .Include("site.deployment_priority").Include(i => i.housing_type).Include(i => i.deployment_type).Include("site.horizontal_datums")
                        .Include("site.horizontal_collect_methods").Include("site.network_name_site.network_name").Where(s => s.instrument_id > 0);

                    if (eventIdList != null && eventIdList.Count > 0)
                        query = query.Where(i => i.event_id.HasValue && eventIdList.Contains(i.event_id.Value));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(i => i.@event.event_type_id.HasValue && eventTypeList.Contains(i.@event.event_type_id.Value));

                    if (eventStatusID > 0)
                        query = query.Where(i => i.@event.event_status_id.HasValue && i.@event.event_status_id.Value == eventStatusID);

                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(i => stateList.Contains(i.site.state));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(i => countyList.Contains(i.site.county));

                    if (collectionConditionIdList != null && collectionConditionIdList.Count > 0)
                        query = query.Where(i => i.inst_collection_id.HasValue && collectionConditionIdList.Contains(i.inst_collection_id.Value));

                    if (deploymentTypeIdList != null && deploymentTypeIdList.Count > 0)
                        query = query.Where(i => i.deployment_type_id.HasValue && deploymentTypeIdList.Contains(i.deployment_type_id.Value));

                    if (statusIdList != null && statusIdList.Count > 0)
                        query = query.AsEnumerable().Where(i => (i.instrument_status == null || i.instrument_status.Count <= 0) ? false :
                                                                    statusIdList.Contains(i.instrument_status.OrderByDescending(insStat => insStat.time_stamp)
                                                                    .Where(stat => stat != null).FirstOrDefault().status_type_id.Value)).AsQueryable();
                    
                    entities = query.AsEnumerable().Select(
                        inst => new InstrumentDownloadable
                        {
                            instrument_id =  inst.instrument_id,
                            sensorType = inst.sensor_type.sensor,
                            sensor_type_id = inst.sensor_type_id,
                            deploymentType = inst.deployment_type_id.HasValue ? inst.deployment_type.method : "",
                            deployment_type_id = inst.deployment_type_id,
                            serial_number = inst.serial_number,
                            housing_serial_number = inst.housing_serial_number,
                            interval = inst.interval,
                            site_id = inst.site_id,
                            eventName =  inst.event_id.HasValue ? inst.@event.event_name : "",
                            location_description = inst.location_description,
                            collectionCondition = inst.inst_collection_id.HasValue ? inst.instr_collection_conditions.condition : "",
                            housingType = inst.housing_type_id.HasValue ? inst.housing_type.type_name : "",
                            vented = inst.vented,
                            sensorBrand = inst.sensor_brand_id.HasValue ? inst.sensor_brand.brand_name : "",
                            statusId = inst.instrument_status.OrderByDescending(y=>y.time_stamp).FirstOrDefault().status_type_id,
                            timeStamp = inst.instrument_status.OrderByDescending(y=>y.time_stamp).FirstOrDefault().time_stamp,
                            site_no =  inst.site.site_no,
                            latitude = inst.site.latitude_dd,
                            longitude = inst.site.longitude_dd,
                            siteDescription = inst.site.site_description,
                            networkNames =  inst.site.network_name_site.Count > 0 ? (inst.site.network_name_site.Where(ns => ns.site_id == inst.site.site_id).ToList()).Select(x => x.network_name.name).Distinct().Aggregate((x, j) => x + ", " + j) : "",
                            stateName = inst.site.state,
                            countyName = inst.site.county,
                            siteWaterbody =  inst.site.waterbody,
                            siteHDatum =  inst.site.hdatum_id > 0 ? inst.site.horizontal_datums.datum_name : "", //inst.site.horizontal_datums is coming back null even though there's a hdatum_id...
                            sitePriorityName = inst.site.priority_id.HasValue && inst.site.priority_id > 0 ? inst.site.deployment_priority.priority_name : "",
                            siteZone = inst.site.zone,
                            siteHCollectMethod = inst.site.hcollect_method_id.HasValue && inst.site.hcollect_method_id > 0 ? inst.site.horizontal_collect_methods.hcollect_method : "",
                            sitePermHousing = inst.site.is_permanent_housing_installed == null || inst.site.is_permanent_housing_installed == "No" ? "No" : "Yes",
                            siteNotes = inst.site.site_notes
                        }).ToList<instrument>();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFullInstruments")]
        public OperationResult GetFullInstruments(Int32 instrumentId)
        {
            //get the instrument and all instrument_stats together for an instrument
            FullInstrument anEntity = null;
           
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<instrument> instrument = sa.Select<instrument>().Include(i => i.sensor_type).Include(i => i.deployment_type).Include(i => i.instr_collection_conditions)
                        .Include(i => i.housing_type).Include(i => i.sensor_brand).Include(i => i.instrument_status).Include("instrument_status.status_type").Include("instrument_status.vertical_datums")
                        .Where(inst => inst.instrument_id == instrumentId);

                    anEntity = instrument.AsEnumerable().Select(
                        inst => new FullInstrument
                        {
                            instrument_id = inst.instrument_id,
                            sensor_type_id = inst.sensor_type_id,
                            sensorType = inst.sensor_type_id != null ? inst.sensor_type.sensor : "",
                            deployment_type_id = inst.deployment_type_id != null && inst.deployment_type_id != 0 ? inst.deployment_type_id.Value : 0,
                            deploymentType = inst.deployment_type_id != null && inst.deployment_type_id != 0 ? inst.deployment_type.method : "",
                            serial_number = inst.serial_number,
                            housing_serial_number = inst.housing_serial_number,
                            interval = inst.interval != null ? inst.interval.Value : 0,
                            site_id = inst.site_id != null ? inst.site_id.Value : 0,
                            event_id = inst.event_id != null ? inst.event_id.Value : 0,                                
                            location_description = inst.location_description,
                            inst_collection_id = inst.inst_collection_id != null && inst.inst_collection_id > 0 ? inst.inst_collection_id.Value : 0,
                            instCollection = inst.inst_collection_id != null && inst.inst_collection_id > 0 ? inst.instr_collection_conditions.condition : "",
                            housing_type_id = inst.housing_type_id != null && inst.housing_type_id > 0 ? inst.housing_type_id.Value : 0,
                            housingType = inst.housing_type_id != null && inst.housing_type_id > 0 ? inst.housing_type.type_name : "",
                            vented = inst.vented,
                            sensor_brand_id = inst.sensor_brand_id != null ? inst.sensor_brand_id.Value : 0,
                            sensorBrand = inst.sensor_brand_id != null ? inst.sensor_brand.brand_name : "",
                            instrument_status = inst.instrument_status.OrderByDescending(instStat => instStat.time_stamp)
                                .Select(i => new Instrument_Status
                                {
                                    instrument_status_id = i.instrument_status_id,
                                    status_type_id = i.status_type_id,
                                    status = i.status_type_id != null ? i.status_type.status : "",
                                    instrument_id = i.instrument_id,
                                    time_stamp = i.time_stamp.Value,
                                    time_zone = i.time_zone,
                                    notes = i.notes,
                                    member_id = i.member_id != null ? i.member_id.Value : 0,
                                    sensor_elevation = i.sensor_elevation,
                                    ws_elevation = i.ws_elevation,
                                    gs_elevation = i.gs_elevation,
                                    vdatum_id = i.vdatum_id,
                                    vdatum = i.vdatum_id.HasValue && i.vdatum_id > 0 ? i.vertical_datums.datum_name : ""
                                }).ToList<instrument_status>()
                        }).FirstOrDefault();
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);                
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end Get        

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteFullInstrumentList")]
        public OperationResult GetSiteFullInstrumentList(Int32 siteId)
        {
            //get list of instrument and all instrument_stats together for a site
            List<FullInstrument> entities = null; 

            try
            {
                if (siteId <= 0)
                    throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<instrument> instrumentList = sa.Select<instrument>().Include(i => i.sensor_type).Include(i => i.deployment_type).Include(i => i.instr_collection_conditions)
                        .Include(i => i.housing_type).Include(i => i.sensor_brand).Include(i => i.instrument_status).Include("instrument_status.status_type").Include("instrument_status.vertical_datums")
                        .Where(instr => instr.site_id == siteId);

                    entities = instrumentList.AsEnumerable().Select(
                        inst => new FullInstrument
                        {
                            instrument_id = inst.instrument_id,
                            sensor_type_id = inst.sensor_type_id,
                            sensorType = inst.sensor_type_id != null ? inst.sensor_type.sensor : "",
                            deployment_type_id = inst.deployment_type_id != null && inst.deployment_type_id != 0 ? inst.deployment_type_id.Value : 0,
                            deploymentType = inst.deployment_type_id != null && inst.deployment_type_id != 0 ? inst.deployment_type.method : "",
                            serial_number = inst.serial_number,
                            housing_serial_number = inst.housing_serial_number,
                            interval = inst.interval != null ? inst.interval : 0,
                            site_id = inst.site_id != null ? inst.site_id : 0,
                            event_id = inst.event_id != null ? inst.event_id : 0,
                            location_description = inst.location_description,
                            inst_collection_id = inst.inst_collection_id != null && inst.inst_collection_id > 0 ? inst.inst_collection_id.Value : 0,
                            instCollection = inst.inst_collection_id != null && inst.inst_collection_id > 0 ? inst.instr_collection_conditions.condition : "",
                            housing_type_id = inst.housing_type_id != null && inst.housing_type_id > 0 ? inst.housing_type_id.Value : 0,
                            housingType = inst.housing_type_id != null && inst.housing_type_id > 0 ? inst.housing_type.type_name : "",
                            vented = inst.vented,
                            sensor_brand_id = inst.sensor_brand_id != null ? inst.sensor_brand_id : 0,
                            sensorBrand = inst.sensor_brand_id != null ? inst.sensor_brand.brand_name : "",
                            instrument_status = inst.instrument_status.OrderByDescending(instStat => instStat.time_stamp)
                                .Select(i => new Instrument_Status
                                {
                                    instrument_status_id = i.instrument_status_id,
                                    status_type_id = i.status_type_id,
                                    status = i.status_type_id != null ? i.status_type.status : "",
                                    instrument_id = i.instrument_id,
                                    time_stamp = i.time_stamp,
                                    time_zone = i.time_zone,
                                    notes = i.notes,
                                    member_id = i.member_id != null ? i.member_id.Value : 0,
                                    sensor_elevation = i.sensor_elevation,
                                    ws_elevation = i.ws_elevation,
                                    gs_elevation = i.gs_elevation,
                                    vdatum_id = i.vdatum_id,
                                    vdatum = i.vdatum_id.HasValue && i.vdatum_id > 0 ? i.vertical_datums.datum_name : ""
                                }).ToList<instrument_status>()
                        }).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end Get       
       
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(instrument anEntity)
        {
            try
            {
                //if instrument is being proposed, don't need sensor_brand_id and event_id and serial_number
                if (!anEntity.sensor_type_id.HasValue || anEntity.sensor_type_id <= 0 ||
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
                        instrument ObjectToBeDeleted = sa.Select<instrument>().Include(df => df.data_files).Include(f => f.files).Include(s => s.instrument_status).SingleOrDefault(c => c.instrument_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();
                                                
                        //remove files
                        ObjectToBeDeleted.files.ToList().ForEach(f => sa.RemoveFileItem(f));
                        ObjectToBeDeleted.files.Clear();

                        //remove datafile
                        ObjectToBeDeleted.data_files.Clear();

                        //remove instrument status
                        ObjectToBeDeleted.instrument_status.Clear();
                        
                        //delete instrument
                        sa.Delete<instrument>(ObjectToBeDeleted);
                        sm(sa.Messages);
                        
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

        #endregion
    }//end class InstrumentHandler
}//end namespace