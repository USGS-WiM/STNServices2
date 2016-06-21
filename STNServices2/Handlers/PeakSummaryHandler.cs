//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles Peak summary resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.08.16 - TR -Created

#endregion

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Resources;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;


namespace STNServices2.Handlers
{
    public class PeakSummaryHandler : STNHandlerBase
    {
        //needed for peakDownloadable populating of related site
        public site globalPeakSite = new site();

        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName="GetAllPeaks")]
        public OperationResult Get()
        {
            List<peak_summary> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<peak_summary>().ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            peak_summary anEntity = null;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<peak_summary>().FirstOrDefault(e => e.peak_summary_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMPeakSummary")]
        public OperationResult GetHWMPeakSummary(Int32 hwmId)
        {
            peak_summary anEntity = null;

            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        // return only approved list
                        anEntity = sa.Select<hwm>().Include(i=>i.peak_summary).FirstOrDefault(i => i.hwm_id == hwmId && i.approval_id > 0).peak_summary;
                    }
                    else
                    {
                        anEntity = sa.Select<hwm>().Include(i => i.peak_summary).FirstOrDefault(i => i.hwm_id == hwmId).peak_summary;
                    }
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFilePeakSummary")]
        public OperationResult GetDataFilePeakSummary(Int32 dataFileId)
        {
            peak_summary anEntity = null;

            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent(true))
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        // return only approved list
                        anEntity = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId && df.approval_id > 0).peak_summary;
                    }
                    else
                    {
                        anEntity = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId).peak_summary;
                    }
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }                 
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventPeakSummaries")]
        public OperationResult GetEventPeakSummaries(Int32 eventId)
        {
            List<peak_summary> entities = null;

            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<peak_summary>().Include(ps => ps.hwms).Include("data_file.instrument")
                                .Where(ps => ps.hwms.Any(hwm => hwm.event_id == eventId) || ps.data_file.Any(d => d.instrument.event_id == eventId)).ToList();
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSitePeakSummaries")]
        public OperationResult GetSitePeakSummaries(Int32 siteId)
        {
            List<peak_summary> entities = null;

            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<peak_summary>().Include(ps => ps.hwms).Include("data_file.instrument")
                        .Where(ps => ps.hwms.Any(hwm => hwm.site_id == siteId) || ps.data_file.Any(d => d.instrument.site_id == siteId)).ToList();
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryViewBySite")]
        public OperationResult GetSitePeakSummarView(Int32 siteId)
        {           
            List<peak_view> entities = null;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.getTable<peak_view>(new Object[1] {null}).Where(p => p.site_id == siteId).ToList();
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredPeaks")]
        public OperationResult FilteredPeaks([Optional]string eventIds, [Optional] string eventTypeIDs, [Optional] string eventStatusID,
                                                [Optional] string states, [Optional] string counties, [Optional] string startDate, [Optional] string endDate)
        {
            List<peak_summary> entities = null;
            try
            {
                char[] delimiterChars = { ';', ',', ' ' };
                char[] countydelimiterChars = { ';', ',' };
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<string> countyList = !string.IsNullOrEmpty(counties) ? counties.Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

                DateTime? FromDate = ValidDate(startDate);
                DateTime? ToDate = ValidDate(endDate);
                if (!ToDate.HasValue) ToDate = DateTime.Now;

                using (STNAgent sa = new STNAgent())
                {
                    IQueryable<peak_summary> query;
                    query = sa.Select<peak_summary>().Include(p => p.hwms).Include("hwms.site").Include("data_file.instrument.site").Include("hwms.site.network_name_site.network_name")
                        .Include("data_file.instrument.site.network_name_site.network_name").Include(p => p.vertical_datums).Include(p => p.member).Include(p => p.data_file).Where(s => s.peak_summary_id > 0);

                    if (eventIdList != null && eventIdList.Count > 0)
                        query = query.Where(ps => (ps.hwms.Any(hwm => eventIdList.Contains(hwm.event_id.Value)) || (ps.data_file.Any(d => eventIdList.Contains(d.instrument.event_id.Value)))));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(ps => (ps.hwms.Any(hwm => eventTypeList.Contains(hwm.@event.event_type_id.Value)) || (ps.data_file.Any(d => eventTypeList.Contains(d.instrument.@event.event_type_id.Value)))));

                    if (!string.IsNullOrEmpty(eventStatusID))
                    {
                        if (Convert.ToInt32(eventStatusID) > 0)
                        {
                            Int32 evStatID = Convert.ToInt32(eventStatusID);
                            query = query.Where(ps => (ps.hwms.Any(hwm => hwm.@event.event_status_id.Value == evStatID)) || (ps.data_file.Any(d => d.instrument.@event.event_status_id.Value == evStatID)));
                        }
                    }

                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(ps => (ps.hwms.Any(hwm => stateList.Contains(hwm.site.state)) || (ps.data_file.Any(d => stateList.Contains(d.instrument.site.state)))));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(ps => (ps.hwms.Any(hwm => countyList.Contains(hwm.site.county)) || (ps.data_file.Any(d => countyList.Contains(d.instrument.site.county)))));

                    if (FromDate.HasValue)
                        query = query.Where(ps => ps.peak_date >= FromDate);

                    if (ToDate.HasValue)
                        query = query.Where(ps => ps.peak_date.Value <= ToDate.Value);

                    entities = query.AsEnumerable().Select(
                        p => new PeakResource
                        {
                            peak_summary_id = p.peak_summary_id,
                            member_name = p.member_id.HasValue ? p.member.fname + " " + p.member.lname : "",
                            peak_date = p.peak_date,
                            is_peak_estimated = p.is_peak_estimated,
                            time_zone = p.time_zone,
                            is_peak_time_estimated = p.is_peak_time_estimated,
                            peak_stage = p.peak_stage,
                            is_peak_stage_estimated = p.is_peak_stage_estimated,
                            peak_discharge = p.peak_discharge,
                            is_peak_discharge_estimated = p.is_peak_discharge_estimated,
                            height_above_gnd = p.height_above_gnd,
                            is_hag_estimated = p.is_hag_estimated,
                            aep = p.aep,
                            aep_lowci = p.aep_lowci,
                            aep_upperci = p.aep_upperci,
                            aep_range = p.aep_range,
                            calc_notes = p.calc_notes,
                            vdatum = p.vertical_datums != null ? p.vertical_datums.datum_name : "",
                            site_id = GetThisPeaksSiteID(p),
                            site_no = globalPeakSite.site_no,
                            latitude = globalPeakSite.latitude_dd,
                            longitude = globalPeakSite.longitude_dd,
                            description = globalPeakSite.site_description,
                            networks = globalPeakSite.network_name_site.Count > 0 ? globalPeakSite.network_name_site.Select(x=>x.network_name.name).Distinct().Aggregate((x,j) => x + ", " + j): "",
                            state = globalPeakSite.state,
                            county = globalPeakSite.county,
                            waterbody = globalPeakSite.waterbody,
                            horizontal_datum = globalPeakSite.horizontal_datums != null ? globalPeakSite.horizontal_datums.datum_name : "",
                            priority = globalPeakSite.deployment_priority != null ? globalPeakSite.deployment_priority.priority_name : "",
                            zone = globalPeakSite.zone,
                            horizontal_collection_method = globalPeakSite.horizontal_collect_methods != null ? globalPeakSite.horizontal_collect_methods.hcollect_method : "",
                            perm_housing_installed = globalPeakSite.is_permanent_housing_installed == null || globalPeakSite.is_permanent_housing_installed == "No" ? "No" : "Yes",
                            site_notes = globalPeakSite.site_notes
                        }).ToList<peak_summary>();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };            
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(peak_summary anEntity)
        {
            try
            {
                if ((!anEntity.member_id.HasValue || anEntity.member_id <= 0) || !anEntity.peak_date.HasValue || string.IsNullOrEmpty(anEntity.time_zone))
                    throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<peak_summary>(anEntity);
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
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, peak_summary anEntity)
        {            
            try
            {
                if ((!anEntity.member_id.HasValue || anEntity.member_id <= 0) || !anEntity.peak_date.HasValue || string.IsNullOrEmpty(anEntity.time_zone))
                    throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<peak_summary>(anEntity);
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
        [RequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            peak_summary anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<peak_summary>().FirstOrDefault(i => i.peak_summary_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<peak_summary>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
            
        }//end HTTP.DELETE
        #endregion

        #region Helper Methods
       
        private decimal GetThisPeaksSiteID(peak_summary peak)
        {
            decimal siteID = 0;
            if (peak.hwms.Count > 0)
            {
                globalPeakSite = peak.hwms.FirstOrDefault().site;
                siteID = Convert.ToDecimal(peak.hwms.FirstOrDefault().site_id);
            } else
            {
                globalPeakSite = peak.data_file.FirstOrDefault().instrument.site;
                siteID = Convert.ToDecimal(peak.data_file.FirstOrDefault().instrument.site_id);
            }           
            
            return siteID;
        }
        
        #endregion
    }//end class PeakSummaryHandler
}//end namespace