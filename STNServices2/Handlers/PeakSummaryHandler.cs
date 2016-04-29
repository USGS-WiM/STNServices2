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

        [HttpOperation(HttpMethod.GET)]
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
                    if (Context.User.IsInRole(PublicRole))
                    {
                        // return only approved list
                        anEntity = sa.Select<hwm>().FirstOrDefault(i => i.hwm_id == hwmId && i.approval_id > 0).peak_summary;
                    }
                    else
                    {
                        anEntity = sa.Select<hwm>().FirstOrDefault(i => i.hwm_id == hwmId).peak_summary;
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

                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User.IsInRole(PublicRole))
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
                    entities = sa.Select<peak_summary>().Where(ps => ps.hwms.Any(hwm => hwm.event_id == eventId) || ps.data_file.Any(d => d.instrument.event_id == eventId)).ToList();
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

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<peak_summary>().Where(ps => ps.hwms.Any(hwm => hwm.site_id == siteId) || ps.data_file.Any(d => d.instrument.site_id == siteId)).ToList();
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
           // List<peak_summary> entities = null;
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummariesByStateName")]
        public OperationResult GetPeakSummariesByStateName(string stateName)
        {
            List<peak_summary> entities = null;

            try
            {
                if (string.IsNullOrEmpty(stateName)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<peak_summary>().Where(ps => ps.hwms.Any(hwm => string.Equals(hwm.site.state, stateName, StringComparison.OrdinalIgnoreCase)) ||
                        ps.data_file.Any(d => string.Equals(d.instrument.site.state, stateName, StringComparison.OrdinalIgnoreCase))).ToList();
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

        //[HttpOperation(ForUriName = "GetFilteredPeaks")]
        //public OperationResult FilteredPeaks([Optional]string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID,
        //                                        [Optional] string states, [Optional] string counties, [Optional] string startDate, [Optional] string endDate)
        //{
        //    List<PeakDownloadable> peakList = new List<PeakDownloadable>();
        //    try
        //    {
        //        char[] delimiterChars = { ';', ',', ' ' };
        //        //parse the requests
        //        List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
        //        List<string> countyList = !string.IsNullOrEmpty(counties) ? counties.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

        //        DateTime? FromDate = ValidDate(startDate);
        //        DateTime? ToDate = ValidDate(endDate);
        //        if (!ToDate.HasValue) ToDate = DateTime.Now;

        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<PEAK_SUMMARY> query;
        //            query = aSTNE.PEAK_SUMMARY.Where(s => s.PEAK_SUMMARY_ID > 0);

        //            if (eventIdList != null && eventIdList.Count > 0)
        //                //query = query.Where(ps => ps.HWMs.Any(hwm => eventIdList.Contains(hwm.EVENT_ID.Value)));
        //                query = query.Where(ps => (ps.HWMs.Any(hwm => eventIdList.Contains(hwm.EVENT_ID.Value)) || (ps.DATA_FILE.Any(d => eventIdList.Contains(d.INSTRUMENT.EVENT_ID.Value)))));

        //            if (eventTypeList != null && eventTypeList.Count > 0)
        //                query = query.Where(ps => (ps.HWMs.Any(hwm => eventTypeList.Contains(hwm.EVENT.EVENT_TYPE_ID.Value)) || (ps.DATA_FILE.Any(d => eventTypeList.Contains(d.INSTRUMENT.EVENT.EVENT_TYPE_ID.Value)))));

        //            if (eventStatusID > 0)
        //                query = query.Where(ps => (ps.HWMs.Any(hwm => hwm.EVENT.EVENT_STATUS_ID.Value == eventStatusID)) || (ps.DATA_FILE.Any(d => d.INSTRUMENT.EVENT.EVENT_STATUS_ID.Value == eventStatusID)));

        //            if (stateList != null && stateList.Count > 0)
        //                query = query.Where(ps => (ps.HWMs.Any(hwm => stateList.Contains(hwm.SITE.STATE)) || (ps.DATA_FILE.Any(d => stateList.Contains(d.INSTRUMENT.SITE.STATE)))));

        //            if (countyList != null && countyList.Count > 0)
        //                query = query.Where(ps => (ps.HWMs.Any(hwm => countyList.Contains(hwm.SITE.COUNTY)) || (ps.DATA_FILE.Any(d => countyList.Contains(d.INSTRUMENT.SITE.COUNTY)))));

        //            if (FromDate.HasValue)
        //                query = query.Where(ps => ps.PEAK_DATE >= FromDate);

        //            if (ToDate.HasValue)
        //                query = query.Where(ps => ps.PEAK_DATE.Value <= ToDate.Value);

        //            peakList = query.AsEnumerable().Select(
        //                p => new PeakDownloadable
        //                {
        //                    PEAK_SUMMARY_ID = p.PEAK_SUMMARY_ID,
        //                    MEMBER_NAME = p.MEMBER_ID.HasValue ? GetMember(aSTNE, p.MEMBER_ID) : "",
        //                    PEAK_DATE = p.PEAK_DATE.HasValue ? p.PEAK_DATE.Value.ToShortDateString() : "",
        //                    IS_PEAK_DATE_ESTIMATED = p.IS_PEAK_ESTIMATED > 0 ? "Yes" : "No",
        //                    PEAK_TIME = p.PEAK_DATE.HasValue ? p.PEAK_DATE.Value.TimeOfDay.ToString() : "",
        //                    TIME_ZONE = !string.IsNullOrEmpty(p.TIME_ZONE) ? p.TIME_ZONE : "",
        //                    IS_PEAK_TIME_ESTIMATED = p.IS_PEAK_TIME_ESTIMATED > 0 ? "Yes" : "No",
        //                    PEAK_STAGE = p.PEAK_STAGE.HasValue ? p.PEAK_STAGE.Value : 0,
        //                    IS_PEAK_STAGE_ESTIMATED = p.IS_PEAK_STAGE_ESTIMATED > 0 ? "Yes" : "No",
        //                    PEAK_DISCHARGE = p.PEAK_DISCHARGE.HasValue ? p.PEAK_DISCHARGE.Value : 0,
        //                    IS_PEAK_DISCHARGE_ESTIMATED = p.IS_PEAK_DISCHARGE_ESTIMATED > 0 ? "Yes" : "No",
        //                    HEIGHT_ABOVE_GND = p.HEIGHT_ABOVE_GND.HasValue ? p.HEIGHT_ABOVE_GND.Value : 0,
        //                    IS_HAG_ESTIMATED = p.IS_HAG_ESTIMATED.HasValue && p.IS_HAG_ESTIMATED.Value > 0 ? "Yes" : "No",
        //                    AEP = p.AEP.Value,
        //                    AEP_LOWCI = p.AEP_LOWCI.Value,
        //                    AEP_UPPERCI = p.AEP_UPPERCI.Value,
        //                    AEP_RANGE = p.AEP_RANGE.Value,
        //                    CALC_NOTES = p.CALC_NOTES,
        //                    VERTICAL_DATUM = p.VDATUM_ID.HasValue ? GetVDatum(aSTNE, p.VDATUM_ID.Value) : "",
        //                    SITE_ID = GetThisPeaksSiteID(aSTNE, p.PEAK_SUMMARY_ID),
        //                    SITE_NO = globalPeakSite.SITE_NO,
        //                    LATITUDE = globalPeakSite.LATITUDE_DD,
        //                    LONGITUDE = globalPeakSite.LONGITUDE_DD,
        //                    DESCRIPTION = globalPeakSite.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(globalPeakSite.SITE_DESCRIPTION) : "",
        //                    NETWORK = SiteHandler.GetSiteNetwork(aSTNE, globalPeakSite.SITE_ID),
        //                    STATE = globalPeakSite.STATE,
        //                    COUNTY = globalPeakSite.COUNTY,
        //                    WATERBODY = globalPeakSite.WATERBODY,
        //                    HORIZONTAL_DATUM = globalPeakSite.HDATUM_ID > 0 ? SiteHandler.GetHDatum(aSTNE, globalPeakSite.HDATUM_ID) : "",
        //                    PRIORITY = globalPeakSite.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, globalPeakSite.PRIORITY_ID.Value) : "",
        //                    ZONE = globalPeakSite.ZONE,
        //                    HORIZONTAL_COLLECT_METHOD = globalPeakSite.HCOLLECT_METHOD_ID.HasValue ? SiteHandler.GetHCollMethod(aSTNE, globalPeakSite.HCOLLECT_METHOD_ID.Value) : "",
        //                    PERM_HOUSING_INSTALLED = globalPeakSite.IS_PERMANENT_HOUSING_INSTALLED == null || globalPeakSite.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
        //                    SITE_NOTES = !string.IsNullOrEmpty(globalPeakSite.SITE_NOTES) ? SiteHandler.GetSiteNotes(globalPeakSite.SITE_NOTES) : ""

        //                }
        //                ).ToList();

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = peakList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}


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
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
            
        }//end HTTP.DELETE
        #endregion

        #region Helper Methods
        
        
        

        #region PeakDownloadable calls
        /*private string GetMember(STNEntities2 aSTNE, decimal? memberId)
        {
            string memberName = string.Empty;
            MEMBER thisMember = aSTNE.MEMBERS.Where(x => x.MEMBER_ID == memberId).FirstOrDefault();
            if (thisMember != null)
            {
                if (thisMember.FNAME.Contains("Historic"))
                {
                    memberName = thisMember.FNAME;
                }
                else
                {
                    memberName = thisMember.FNAME + " " + thisMember.LNAME;
                }
            }

            return memberName;
        }

        private string GetVDatum(STNEntities2 aSTNE, decimal vdId)
        {
            string vD = string.Empty;
            if (vdId > 0)
            {
                VERTICAL_DATUMS thisVD = aSTNE.VERTICAL_DATUMS.Where(x => x.DATUM_ID == vdId).FirstOrDefault();
                if (thisVD != null)
                    vD = thisVD.DATUM_NAME;
            }
            return vD;
        }

        private decimal GetThisPeaksSiteID(STNEntities2 aSTNE, decimal peakId)
        {
            List<HWM> peakHWMs = new List<HWM>();
            List<DATA_FILE> peakDatas = new List<DATA_FILE>();
            decimal siteID = 0;

            peakHWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.PEAK_SUMMARY_ID == peakId).OrderBy(hwm => hwm.HWM_ID).ToList<HWM>();
            peakDatas = aSTNE.DATA_FILE.AsEnumerable().Where(df => df.PEAK_SUMMARY_ID == peakId).OrderBy(df => df.DATA_FILE_ID).ToList<DATA_FILE>();

            if (peakHWMs.Count > 0)
            {
                SITE theSite = peakHWMs.FirstOrDefault().SITE;
                if (theSite != null)
                {
                    siteID = theSite.SITE_ID;
                    globalPeakSite = theSite;
                }
            }
            if (peakDatas.Count > 0)
            {
                SITE theSite = peakDatas.FirstOrDefault().INSTRUMENT.SITE;
                if (theSite != null)
                {
                    siteID = theSite.SITE_ID;
                    globalPeakSite = theSite;
                }
            }
            return siteID;
        }

        */
        #endregion PeakDownloadable calls

        #endregion
    }//end class PeakSummaryHandler
}//end namespace