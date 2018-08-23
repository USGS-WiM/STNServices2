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
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
                    anEntity = sa.Select<instrument_status>().Include(i=>i.instrument).FirstOrDefault(i => i.instrument_status_id == instrumentStatusId).instrument;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
                    anEntity = sa.Select<data_file>().Include(df=>df.instrument).FirstOrDefault(df => df.data_file_id == dataFileId).instrument;
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().Where(s => s.sensor_type_id == sensorTypeId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().Where(s => s.sensor_brand_id == sensorBrandId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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
                    entities = sa.Select<instrument>().Where(s => s.deployment_type_id == deploymentTypeId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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
                    entities = sa.Select<instrument>().Where(e => e.event_id == eventId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
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
                if (siteId <= 0 || eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument>().AsEnumerable().Where(instr => instr.site_id == siteId && instr.event_id == eventId).OrderBy(instr => instr.instrument_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #region sensorViews response
        [HttpOperation(ForUriName = "GetSensorViews")]
        public OperationResult GetSensorViews(string view, [Optional] string eventIDs, [Optional] string eventTypeIDs, [Optional] string eventStatusID, [Optional] string states, [Optional] string counties,
            [Optional] string sensorTypeIDs, [Optional] string statusIDs, [Optional] string collectionConditionIDs, [Optional] string deploymentTypeIDs)
        {            
            List<sensor_view> aViewTable = null;
            IQueryable<sensor_view> query;
            
            try
            {
                if (string.IsNullOrEmpty(view) && 
                    (view.ToLower() != "baro_view" || view.ToLower() != "met_view" || view.ToLower() != "rdg_view" || view.ToLower() != "stormtide_view" || view.ToLower() != "waveheight_view" ||
                    view.ToLower() != "pressuretemp_view" || view.ToLower() != "therm_view" || view.ToLower() != "webcam_view" || view.ToLower() != "raingage_view")) 
                    throw new BadRequestException("Invalid input parameters. ViewType must be either 'baro_view', 'met_view', 'rdg_view', 'stormTide_view' or 'waveheight_view'");

                using (STNAgent sa = new STNAgent())
                {
                    query = sa.getTable<sensor_view>(new Object[1] { view.ToString() });                    

                    char[] delimiterChars = { ';', ',', ' ' }; char[] countydelimiterChars = { ';', ',' };
                    //parse the requests
                    List<string> eventValueList = !string.IsNullOrEmpty(eventIDs) ? eventIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                    List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                    List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                    List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                    List<decimal> sensorTypeIdList = !string.IsNullOrEmpty(sensorTypeIDs) ? sensorTypeIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                    List<decimal> statusIdList = !string.IsNullOrEmpty(statusIDs) ? statusIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                    List<decimal> collectionConditionIdList = !string.IsNullOrEmpty(collectionConditionIDs) ? collectionConditionIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                    List<decimal> deploymentTypeIdList = !string.IsNullOrEmpty(deploymentTypeIDs) ? deploymentTypeIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

                    
                    if (eventValueList != null && eventValueList.Count > 0)
                        query = query.Where(e => eventValueList.Contains(e.event_name.Trim().Replace(" ", "").ToUpper()) || eventValueList.Contains(e.event_id.ToString()));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(i => i.event_type_id.HasValue && eventTypeList.Contains(i.event_type_id.Value));                        
                    
                    if (!string.IsNullOrEmpty(eventStatusID))
                    {
                        if (Convert.ToInt32(eventStatusID) > 0)
                        {
                            Int32 evStatID = Convert.ToInt32(eventStatusID);
                            query = query.Where(i => i.event_status_id.HasValue && i.event_status_id.Value == evStatID);                            
                        }
                    }
                    if (stateList != null && stateList.Count > 0)                    
                        query = query.Where(i => stateList.Contains(i.state));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(i => countyList.Contains(i.county));

                    if (collectionConditionIdList != null && collectionConditionIdList.Count > 0)
                        query = query.Where(i => i.inst_collection_id.HasValue && collectionConditionIdList.Contains(i.inst_collection_id.Value));

                    if (deploymentTypeIdList != null && deploymentTypeIdList.Count > 0)                    
                        query = query.Where(i => i.deployment_type_id.HasValue && deploymentTypeIdList.Contains(i.deployment_type_id.Value));

                    if (sensorTypeIdList != null && sensorTypeIdList.Count > 0)                    
                        query = query.Where(i => i.sensor_type_id.HasValue && sensorTypeIdList.Contains(i.sensor_type_id.Value));

                    if (statusIdList != null && statusIdList.Count > 0)                    
                        query = query.Where(i => i.status_type_id.HasValue && statusIdList.Contains(i.status_type_id.Value));
                                        
                    aViewTable = query.ToList();
                }//end using

                return new OperationResult.OK { ResponseResource = aViewTable, Description = this.MessageString };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET
        #endregion sensorViews response

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredInstruments")]
        public OperationResult GetFilteredInstruments([Optional] string eventIds, [Optional] string eventTypeIDs, [Optional] string eventStatusID, [Optional] string states, [Optional] string counties,
                                                        [Optional] string statusIDs, [Optional] string collectionConditionIDs, [Optional] string sensorTypeIDs, [Optional] string deploymentTypeIDs)
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
                List<decimal> sensorTypeIdList = !string.IsNullOrEmpty(sensorTypeIDs) ? sensorTypeIDs.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
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

                    if (!string.IsNullOrEmpty(eventStatusID))
                    {
                        if (Convert.ToInt32(eventStatusID) > 0){
                            Int32 evStatID = Convert.ToInt32(eventStatusID);
                            query = query.Where(i => i.@event.event_status_id.HasValue && i.@event.event_status_id.Value == evStatID);
                        }
                    }
                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(i => stateList.Contains(i.site.state));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(i => countyList.Contains(i.site.county));

                    if (collectionConditionIdList != null && collectionConditionIdList.Count > 0)
                        query = query.Where(i => i.inst_collection_id.HasValue && collectionConditionIdList.Contains(i.inst_collection_id.Value));

                    if (deploymentTypeIdList != null && deploymentTypeIdList.Count > 0)
                        query = query.Where(i => i.deployment_type_id.HasValue && deploymentTypeIdList.Contains(i.deployment_type_id.Value));

                    if (sensorTypeIdList != null && sensorTypeIdList.Count > 0)
                        query = query.Where(i => i.sensor_type_id.HasValue && sensorTypeIdList.Contains(i.sensor_type_id.Value));

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
                            deploymentType =  inst.deployment_type != null ? inst.deployment_type.method : "",
                            deployment_type_id = inst.deployment_type_id,
                            serial_number = inst.serial_number,
                            housing_serial_number = inst.housing_serial_number,
                            interval = inst.interval,
                            site_id = inst.site_id,
                            eventName = inst.@event != null ? inst.@event.event_name : "",
                            location_description = inst.location_description,
                            collectionCondition = inst.instr_collection_conditions != null ? inst.instr_collection_conditions.condition : "",
                            housingType = inst.housing_type != null ? inst.housing_type.type_name : "",
                            vented = inst.vented,
                            sensorBrand = inst.sensor_brand != null ? inst.sensor_brand.brand_name : "",
                            statusId = (inst.instrument_status != null && inst.instrument_status.Count > 0) ? inst.instrument_status.OrderByDescending(y=>y.time_stamp).FirstOrDefault().status_type_id : null,
                            timeStamp = (inst.instrument_status != null && inst.instrument_status.Count > 0) ? inst.instrument_status.OrderByDescending(y=>y.time_stamp).FirstOrDefault().time_stamp: null,
                            site_no = inst.site.site_no,
                            latitude = inst.site.latitude_dd,
                            longitude = inst.site.longitude_dd,
                            siteDescription = inst.site.site_description,
                            networkNames =  inst.site.network_name_site.Count > 0 ? (inst.site.network_name_site.Where(ns => ns.site_id == inst.site.site_id).ToList()).Select(x => x.network_name.name).Distinct().Aggregate((x, j) => x + ", " + j) : "",
                            stateName = inst.site.state,
                            countyName = inst.site.county,
                            siteWaterbody = inst.site.waterbody,
                            siteHDatum =  inst.site.horizontal_datums != null ? inst.site.horizontal_datums.datum_name : "", 
                            sitePriorityName = inst.site.deployment_priority != null ? inst.site.deployment_priority.priority_name : "",
                            siteZone = inst.site.zone,
                            siteHCollectMethod = inst.site.horizontal_collect_methods != null ? inst.site.horizontal_collect_methods.hcollect_method : "",
                            sitePermHousing = inst.site.is_permanent_housing_installed == null || inst.site.is_permanent_housing_installed == "No" ? "No" : "Yes"
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
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

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
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

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
                        List<instrument_status> insStatList = sa.Select<instrument_status>().Where(ins => ins.instrument_id == entityId).ToList();
                        List<Int32> SIDs = new List<Int32>();
                        //delete the statuses and get the id
                        foreach (instrument_status i in insStatList)
                        {
                            SIDs.Add(i.instrument_status_id);
                            sa.Delete<instrument_status>(i);
                        }
                        
                        //get and delete all op_measurements
                        foreach(Int32 id in SIDs)
                        {
                            sa.Select<op_measurements>().Where(o => o.instrument_status_id == id).ToList().ForEach(op => sa.Delete<op_measurements>(op));
                        }

                        //fetch the object to be updated (assuming that it exists)
                        instrument ObjectToBeDeleted = sa.Select<instrument>().Include(c=>c.files).Include(c=>c.data_files).SingleOrDefault(c => c.instrument_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();
                                                
                        //remove files
                        ObjectToBeDeleted.files.ToList().ForEach(f => sa.RemoveFileItem(f));
                        ObjectToBeDeleted.files.ToList().ForEach(f=> sa.Delete<file>(f));
                        
                        //remove datafile          
                        ObjectToBeDeleted.data_files.ToList().ForEach(df => sa.Delete<data_file>(df));
                        
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
        }

        #endregion
       
    }//end class InstrumentHandler
}//end namespace