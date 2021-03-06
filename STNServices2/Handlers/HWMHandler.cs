﻿//------------------------------------------------------------------------------
//----- HWMHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles HWM resources through the HTTP uniform interface.
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
using STNServices2.Resources;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using STNServices2.Security;
using WiM.Security;
using OpenRasta.Security;

namespace STNServices2.Handlers
{

    public class HWMHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName="GetAllHWMs")]
        public OperationResult Get()
        {
            List<hwm> entities = null;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);      

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            hwm anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<hwm>().SingleOrDefault(hwm => hwm.hwm_id == entityId);                    

                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventHWMs")]
        public OperationResult GetEventHWMs(Int32 eventId)
        {
            List<hwm> entities = null;
            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(h => h.event_id == eventId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages); 
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventHWMs")]
        public OperationResult GetSiteEventHWMs(Int32 siteId, Int32 eventId)
        {
            List<hwm> entities = null;
            try
            {
                if (siteId <= 0 || eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(h => h.event_id == eventId && h.site_id == siteId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovalHWMs")]
        public OperationResult GetApprovalHWMs(string approved, [Optional] string eventId, [Optional] string state, [Optional] string counties)
        {
            try
            {
                if (string.IsNullOrEmpty(approved)) throw new BadRequestException("Invalid input parameters");

                List<hwm> entities = null;
                char[] countydelimiterChars = { ';', ',' };
                
                //set defaults
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();
                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                Int32 filterEvent = (!string.IsNullOrEmpty(eventId)) ? Convert.ToInt32(eventId) : -1;

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<hwm> query;
                    if (isApprovedStatus)
                        query = sa.Select<hwm>().Include(h => h.site).Where(h => h.approval_id > 0);
                    else
                        query = sa.Select<hwm>().Include(h => h.site).Where(h => h.approval_id <= 0 || !h.approval_id.HasValue);                                   

                    if (filterEvent > 0)
                        query = query.Where(h => h.event_id == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(h => h.site.state == filterState);

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(h => countyList.Contains(h.site.county.ToUpper()));

                    entities = query.AsEnumerable().ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                    
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.GET                   

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventStateHWMs")]
        public OperationResult GetEventStateHWMs(string eventId, string state)
        {
            try
            {
                List<hwm> entities = null;

                //set defaults                
                string filterState = GetStateByName(state).ToString();
                Int32 filterEvent = (!string.IsNullOrEmpty(eventId)) ? Convert.ToInt32(eventId) : -1;

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<hwm> query = sa.Select<hwm>().Include(h => h.site);
                    
                    if (filterEvent > 0)
                        query = query.Where(h => h.event_id == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(h => h.site.state == filterState);
                                        
                    entities = query.AsEnumerable().ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.GET                   

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedHWMs")]
        public OperationResult GetApprovedHWMs(Int32 ApprovalId)
        {
            List<hwm> entities = null;
            try
            {
                if (ApprovalId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<approval>().Include(a=>a.hwms).FirstOrDefault(a => a.approval_id == ApprovalId).hwms.ToList();
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

         [HttpOperation(HttpMethod.GET, ForUriName = "GetMemberHWMs")]
        public OperationResult GetMemberHWMs(Int32 memberId)
        {
            List<hwm> entities = null;
            try
            {
                if (memberId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(i => i.flag_member_id == memberId || i.survey_member_id == memberId).ToList();
                    
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMQualityHWMs")]
        public OperationResult GetHWMQualityHWMs(Int32 hwmQualityId)
        {
            List<hwm> entities = null;
            try
            {
                if (hwmQualityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities  = sa.Select<hwm>().Where(i => i.hwm_quality_id == hwmQualityId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMTypeHWMs")]
        public OperationResult GetHWMTypeHWMs(Int32 hwmTypeId)
        {
            List<hwm> entities = null;
            try
            {
                if (hwmTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(i => i.hwm_type_id == hwmTypeId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHmethodHWMs")]
        public OperationResult GetHMethodHWMs(Int32 hmethodId)
        {
            List<hwm> entities = null;
            try
            {
                if (hmethodId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(i => i.hcollect_method_id == hmethodId).ToList();
                    
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVmethodHWMs")]
        public OperationResult GetVmethodHWMs(Int32 vmethodId)
        {
            List<hwm> entities = null;
            try
            {
                if (vmethodId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(h => h.vcollect_method_id == vmethodId).ToList();
 
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteHWMs")]
        public OperationResult GetSiteHWMs(Int32 siteId)
        {
            List<hwm> entities = null;            
            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {                    
                    entities = sa.Select<hwm>().Where(h => h.site_id == siteId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVDatumHWMs")]
        public OperationResult GetVDatumHWMs(Int32 vdatumId)
        {
            List<hwm> entities = null;
            try
            {
                if (vdatumId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(h => h.vdatum_id == vdatumId).ToList();
                    
                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
               }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetMarkerHWMs")]
        public OperationResult GetMarkerHWMs(Int32 markerId)
        {
            List<hwm> entities = null;
            try
            {
                if (markerId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(i => i.marker_id == markerId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryHWMs")]
        public OperationResult GetPeakSummaryHWMs(Int32 peakSummaryId)
        {
            List<hwm> entities = null;
            try
            {
                if (peakSummaryId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<hwm>().Where(hwm => hwm.peak_summary_id == peakSummaryId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count()); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileHWM")]
        public OperationResult GetFileHWM(Int32 fileId)
        {
            hwm anEntity;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<file>().Include(f => f.hwm).FirstOrDefault(f => f.file_id == fileId).hwm;
                    
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "FilteredHWMs")]
        public OperationResult FilteredHWMs([Optional]string eventIds, [Optional] string eventTypeIDs, [Optional] string eventStatusID,
                                            [Optional] string states, [Optional] string counties, [Optional] string hwmTypeIDs,
                                            [Optional] string hwmQualIDs, [Optional] string hwmEnvironment, [Optional] string surveyComplete, [Optional] string stillWater)
        {

            List<hwm> entities = null;
            try
            {
                char[] delimiterChars = { ';', ',', ' ' }; char[] countydelimiterChars = { ';', ','};
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

                List<decimal> hwmTypeIdList = !string.IsNullOrEmpty(hwmTypeIDs) ? hwmTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> hwmQualIdList = !string.IsNullOrEmpty(hwmQualIDs) ? hwmQualIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<hwm> query;
                    query = sa.Select<hwm>().Include(h => h.hwm_types).Include(h => h.@event).Include(h => h.hwm_qualities).Include(h => h.vertical_datums).Include(h => h.horizontal_datums).Include(h => h.survey_member)
                        .Include(h => h.flag_member).Include(h => h.vertical_collect_methods).Include(h => h.horizontal_collect_methods).Include(h => h.approval).Include("approval.member").Include(h => h.marker).Include(h => h.site)
                        .Include("site.network_name_site.network_name").Include("site.deployment_priority").Where(s => s.hwm_id > 0);

                    if (eventIdList != null && eventIdList.Count > 0)
                        query = query.Where(i => i.event_id.HasValue && eventIdList.Contains(i.event_id.Value));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(i => i.@event.event_type_id.HasValue && eventTypeList.Contains(i.@event.event_type_id.Value));

                    if (!string.IsNullOrEmpty(eventStatusID))
                    {
                        if (Convert.ToInt32(eventStatusID) > 0)
                        {
                            Int32 eveStatId = Convert.ToInt32(eventStatusID);
                            query = query.Where(i => i.@event.event_status_id.HasValue && i.@event.event_status_id.Value == eveStatId);
                        }
                    }
                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(i => stateList.Contains(i.site.state));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(i => countyList.Contains(i.site.county.ToUpper()));

                    if (hwmTypeIdList != null && hwmTypeIdList.Count > 0)
                        query = query.Where(i => hwmTypeIdList.Contains(i.hwm_type_id));

                    if (hwmQualIdList != null && hwmQualIdList.Count > 0)
                        query = query.Where(i => hwmQualIdList.Contains(i.hwm_quality_id));

                    if (!string.IsNullOrEmpty(hwmEnvironment))
                        query = query.Where(i => i.hwm_environment == hwmEnvironment);

                    if (!string.IsNullOrEmpty(surveyComplete))
                    {
                        if (surveyComplete == "True")
                            query = query.Where(i => i.survey_date.HasValue);
                        else
                            query = query.Where(i => !i.survey_date.HasValue);
                    }

                    //1 = yes, 0 = no
                    if (!string.IsNullOrEmpty(stillWater))
                        if (stillWater == "True")
                            query = query.Where(i => i.stillwater.HasValue && i.stillwater.Value == 1);
                        else
                            query = query.Where(i => !i.stillwater.HasValue || i.stillwater.Value == 0);

                    entities = query.AsEnumerable().Select(
                        hw => new HWMDownloadable 
                        {
                            latitude = hw.latitude_dd.Value,
                            longitude = hw.longitude_dd.Value,
                            hwm_id = hw.hwm_id,
                            waterbody = hw.waterbody,
                            site_id = hw.site_id,
                            event_id = hw.event_id,
                            eventName = hw.@event != null ? hw.@event.event_name : "",
                            hwm_type_id = hw.hwm_type_id,
                            hwm_label = hw.hwm_label,
                            hwmTypeName = hw.hwm_type_id > 0 && hw.hwm_types != null ? hw.hwm_types.hwm_type : "",
                            hwm_quality_id = hw.hwm_quality_id,
                            hwmQualityName = hw.hwm_quality_id > 0 && hw.hwm_qualities != null ? hw.hwm_qualities.hwm_quality : "",
                            hwm_locationdescription = hw.hwm_locationdescription,
                            latitude_dd = hw.latitude_dd,
                            longitude_dd = hw.longitude_dd,
                            elev_ft = hw.elev_ft,
                            uncertainty = hw.uncertainty,
                            vdatum_id = hw.vdatum_id,
                            verticalDatumName = hw.vdatum_id > 0 && hw.vertical_datums != null ? hw.vertical_datums.datum_name : "",
                            hdatum_id = hw.hdatum_id,
                            horizontalDatumName = hw.hdatum_id > 0 && hw.horizontal_datums != null ? hw.horizontal_datums.datum_name : "",
                            vcollect_method_id = hw.vcollect_method_id,
                            verticalMethodName = hw.vcollect_method_id > 0 && hw.vertical_collect_methods != null ? hw.vertical_collect_methods.vcollect_method : "",
                            hcollect_method_id = hw.hcollect_method_id,
                            horizontalMethodName = hw.hcollect_method_id > 0 && hw.horizontal_collect_methods != null ? hw.horizontal_collect_methods.hcollect_method : "",
                            bank = hw.bank,
                            hwm_uncertainty = hw.hwm_uncertainty,
                            approval_id = hw.approval_id,
                            approvalMember = hw.approval_id > 0  ? getApprovalName(hw) : "",
                            marker_id = hw.marker_id,
                            markerName = hw.marker_id > 0 && hw.marker != null ? hw.marker.marker1 : "",
                            height_above_gnd = hw.height_above_gnd,
                            hwm_notes = hw.hwm_notes,
                            hwm_environment = hw.hwm_environment,
                            flag_date = hw.flag_date,
                            survey_date = hw.survey_date,
                            stillwater = hw.stillwater,
                            flag_member_id = hw.flag_member_id,
                            flagMemberName = hw.flag_member_id > 0 && hw.flag_member != null ? hw.flag_member.fname + " " + hw.flag_member.lname : "",
                            survey_member_id = hw.survey_member_id,
                            surveyMemberName = hw.survey_member_id > 0 && hw.survey_member != null ? hw.survey_member.fname + " " + hw.survey_member.lname : "",
                            peak_summary_id = hw.peak_summary_id,
                            site_no = hw.site != null ? hw.site.site_no : "",
                            site_latitude = hw.site != null? hw.site.latitude_dd: 0,
                            site_longitude = hw.site != null? hw.site.longitude_dd: 0,
                            siteDescription = hw.site != null ? hw.site.site_description : "",
                            networkNames = hw.site != null && hw.site.network_name_site.Count > 0 ? (hw.site.network_name_site.Where(ns => ns.site_id == hw.site.site_id).ToList()).Select(x => x.network_name.name).Distinct().Aggregate((x, j) => x + ", " + j) : "",
                            stateName = hw.site != null ? hw.site.state:"",
                            countyName = hw.site != null ? hw.site.county:"",
                            sitePriorityName = hw.site != null && hw.site.priority_id > 0 && hw.site.deployment_priority != null ? hw.site.deployment_priority.priority_name : "",
                            siteZone = hw.site != null ? hw.site.zone:"",
                            sitePermHousing = hw.site != null && hw.site.is_permanent_housing_installed == null || hw.site.is_permanent_housing_installed == "No" ? "No" : "Yes"
                        }).ToList<hwm>();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }        
                
        #endregion
        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(hwm anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            {
                if (anEntity.site_id <= 0|| anEntity.event_id <= 0 || anEntity.hwm_type_id <= 0 || !anEntity.flag_date.HasValue ||
                    anEntity.hwm_quality_id <= 0 || string.IsNullOrEmpty(anEntity.hwm_environment) || anEntity.hdatum_id <= 0 ||
                    anEntity.flag_member_id <= 0 || anEntity.hcollect_method_id <= 0) // || string.IsNullOrEmpty(anEntity.hwm_label))
                        throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        if (string.IsNullOrEmpty(anEntity.hwm_label)) {
                            anEntity.hwm_label = "no_label";
                        }
                        anEntity = sa.Add<hwm>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        #endregion
        #region PutMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, hwm anEntity)
        {
            try
            {
                if (anEntity.site_id <= 0 || anEntity.event_id <= 0 || anEntity.hwm_type_id <= 0 || !anEntity.flag_date.HasValue ||
                   anEntity.hwm_quality_id <= 0 || string.IsNullOrEmpty(anEntity.hwm_environment) || anEntity.hdatum_id <= 0 ||
                   anEntity.flag_member_id <= 0 || anEntity.hcollect_method_id <= 0) // || string.IsNullOrEmpty(anEntity.hwm_label))
                    throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //remove approval_id from hwm if no approval in db
                        if (anEntity.approval_id > 0 && sa.Select<approval>().FirstOrDefault(a => a.approval_id == anEntity.approval_id) == null)                                                    
                            anEntity.approval_id = null;
                        
                        if (string.IsNullOrEmpty(anEntity.hwm_label)) anEntity.hwm_label = "no_label";

                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<hwm>(entityId, anEntity);
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
            Int32? approvalID = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        hwm ObjectToBeDeleted = sa.Select<hwm>().Include(h => h.files).SingleOrDefault(h => h.hwm_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();
                                                
                        #region Cascadedelete
                        approvalID = ObjectToBeDeleted.approval_id;

                        //remove files
                        ObjectToBeDeleted.files.ToList().ForEach(f => sa.RemoveFileItem(f));
                        ObjectToBeDeleted.files.ToList().ForEach(f => sa.Delete<file>(f));
                        //ObjectToBeDeleted.files.Clear();                        
                        ////delete HWM now
                        sa.Delete(ObjectToBeDeleted);

                        if (approvalID.HasValue)
                        {
                            approval item = sa.Select<approval>().SingleOrDefault(h => h.approval_id == approvalID);
                            sa.Delete<approval>(item);
                        }                          
                        
                        sm(sa.Messages);
                        #endregion
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK {Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
        
        #region Helpers
        private string getApprovalName(hwm h)
        {
            try
            {
                return h.approval.member.fname + " " + h.approval.member.lname;                
            }
            catch
            {
                return "Missing Approval";
            }
        }
        #endregion

    }//end class HWMHandler

}//end namespace