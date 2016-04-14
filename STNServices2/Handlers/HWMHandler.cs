//------------------------------------------------------------------------------
//----- HWMHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
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
// 03.29.16 - JKN - Major update.
// 02.07.13 - JKN - Added query to get HWMs by eventId and siteID
// 01.31.13 - JKN - Get(string boolean) method to return only approved or non approved hwms
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 11.07.12 - JKN - Added GetEventSimpleHWMs method
// 07.03.12 - JKN - Role authorization, and moved context to base class
// 06.19.12 - JKN - Replaced HWMList Get method with List<HWM> method
// 05.29.12 - jkn - Added connection string, added Delete method
// 02.17.12 - JB - Turned on pass through authentication to DB
// 02.16.12 - JB - Added required fields check
// 02.10.12 - JB - Added PUT and POST methods for creation and update
// 02.07.12 - JB - Switched to site_id as primary identifier 
// 01.25.12 - JB - Added "Provisonal" site listing for RSS Feed
// 01.19.11 - JB - Created
#endregion

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
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

        //[HttpOperation(HttpMethod.GET)]
        //public OperationResult Get()
        //{
        //    HWMList hWMs = new HWMList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
        //            {
        //                //they are the general public..only approved ones
        //                hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.APPROVAL_ID > 0)
        //                            .Select(hwm => new SimpleHWM
        //                            {
        //                                HWM_ID = Convert.ToInt32(hwm.HWM_ID),
        //                                LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
        //                                LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
        //                                HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
        //                                ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
        //                            }).ToList<SimpleHWM>();
        //            }
        //            else
        //            {
        //                //they are logged in, give them all
        //                hWMs.HWMs = aSTNE.HWMs.AsEnumerable()
        //                     .Select(hwm => new SimpleHWM
        //                     {
        //                         HWM_ID = Convert.ToInt32(hwm.HWM_ID),
        //                         LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
        //                         LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
        //                         HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
        //                         ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
        //                     }).ToList<SimpleHWM>();
        //            }

        //            if (hWMs != null)
        //                hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = hWMs };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMByApproval")]
        public OperationResult Get(string boolean)
        {
            List<hwm> hwms = null;
            try
            {
                
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(boolean, out isApprovedStatus);

                using (STNAgent sa = new STNAgent())
                {
                    if (isApprovedStatus) 
                        hwms = sa.Select<hwm>().Where(hwm => hwm.approval_id > 0).ToList();
                    else//return all non approved hwms
                        hwms = sa.Select<hwm>().Where(hwm => hwm.approval_id < 0 || !hwm.approval_id.HasValue).ToList();

                    sm(sa.Messages);

                }//end using
                return new OperationResult.Created { ResponseResource = hwms, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedHWMs")]
        public OperationResult GetApprovedHWMs(Int32 ApprovalId)
        {
            List<hwm> hwms = null;
            try
            {
                if (ApprovalId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwms = sa.Select<approval>().FirstOrDefault(a => a.approval_id == ApprovalId).hwms.ToList();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.Created { ResponseResource = hwms, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteHWMs")]
        public OperationResult GetSiteHWMs(Int32 siteId)
        {
            IQueryable<hwm> hWMs = null;
            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {                    
                    hWMs = hWMs.Where(h => h.site_id == siteId)
                        .OrderBy(hwm => hwm.hwm_id);

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                        hWMs = sa.Select<hwm>().Where(h=>h.approval_id > 0);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hWMs.ToList(), Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVDatumHWMs")]
        public OperationResult GetVDatumHWMs(Int32 vdatumId)
        {
            List<hwm> hWMs = null;
            try
            {
                if (vdatumId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hWMs = sa.Select<vertical_datums>().FirstOrDefault(vd => vd.datum_id == vdatumId).hwms.ToList();
                    sm(sa.Messages);
               }//end using

                return new OperationResult.OK { ResponseResource = hWMs, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetVmethodHWMs")]
        public OperationResult GetVmethodHWMs(Int32 vmethodId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (vmethodId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<vertical_collect_methods>().FirstOrDefault(i => i.vcollect_method_id == vmethodId).hwms.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMemberHWMs")]
        public OperationResult GetMemberHWMs(Int32 memberId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (memberId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<hwm>().Where(i => i.flag_member_id == memberId || i.survey_member_id == memberId).ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMQualityHWMs")]
        public OperationResult GetHWMQualityHWMs(Int32 hwmQualityId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (hwmQualityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<hwm_qualities>().FirstOrDefault(i => i.hwm_quality_id == hwmQualityId).hwms.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMTypeHWMs")]
        public OperationResult GetHWMTypeHWMs(Int32 hwmTypeId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (hwmTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<hwm_types>().FirstOrDefault(i => i.hwm_type_id == hwmTypeId).hwms.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHmethodHWMs")]
        public OperationResult GetHMethodHWMs(Int32 hmethodId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (hmethodId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<horizontal_collect_methods>().FirstOrDefault(i => i.hcollect_method_id == hmethodId).hwms.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMarkerHWMs")]
        public OperationResult GetMarkerHWMs(Int32 markerId)
        {
            List<hwm> hwmList = null;
            try
            {
                if (markerId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    hwmList = sa.Select<marker>().FirstOrDefault(i => i.marker_id == markerId).hwms.ToList();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryHWMs")]
        public OperationResult GetPeakSummaryHWMs(Int32 peakSummaryId)
        {
            IQueryable<hwm> hWMs = null;
            try
            {
                if (peakSummaryId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {

                    hWMs = sa.Select<hwm>().Where(hwm => hwm.peak_summary_id == peakSummaryId)
                                                .OrderBy(hwm => hwm.hwm_id);

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                        hWMs = hWMs.Where(h => h.approval_id > 0);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hWMs, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSiteHWMsBySiteNo")]
        public OperationResult GetSiteHWMsBySiteNo(String siteNo)
        {
            IQueryable<hwm> hWMs = null;
            try
            {
                if (string.IsNullOrEmpty(siteNo)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {

                    hWMs = sa.Select<hwm>().Include(h=>h.site).Where(h =>string.Equals(h.site.site_no,siteNo,StringComparison.InvariantCultureIgnoreCase))
                                                .OrderBy(hwm => hwm.hwm_id);

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                        hWMs = hWMs.Where(h => h.approval_id > 0);

                    sm(MessageType.info,"Count :" + hWMs.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = hWMs.ToList(), Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWM")]
        public OperationResult Get(Int32 entityId)
        {
            hwm aHWM;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    aHWM = sa.Select<hwm>().SingleOrDefault(hwm => hwm.hwm_id == entityId);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = aHWM, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileHWM")]
        public OperationResult GetFileHWM(Int32 fileId)
        {
            hwm aHWM;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    aHWM = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).hwm;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = aHWM, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetEventSimpleHWMs")]
        //public OperationResult GetEventSimpleHWMs(Int32 eventId)
        //{
        //    HWMList hWMs = new HWMList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.EVENT_ID == eventId)
        //                .Select(hwm => new SimpleHWM
        //                {
        //                    HWM_ID = Convert.ToInt32(hwm.HWM_ID),
        //                    LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
        //                    LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
        //                    HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
        //                    ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
        //                }).ToList<SimpleHWM>();

        //            if (hWMs != null)
        //                hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = hWMs };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}// end HttpMethod.Get

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventHWMs")]
        //public OperationResult Get(Int32 siteId, Int32 eventId)
        //{
        //    HWMList hWMs = new HWMList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.EVENT_ID == eventId
        //                && hwm.SITE_ID == siteId)
        //                .Select(hwm => new SimpleHWM
        //                {
        //                    HWM_ID = Convert.ToInt32(hwm.HWM_ID),
        //                    LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
        //                    LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
        //                    HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
        //                    ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT),
        //                    SURVEY_DATE = hwm.SURVEY_DATE,
        //                    FLAG_DATE = hwm.FLAG_DATE
        //                }).ToList<SimpleHWM>();

        //            if (hWMs != null)
        //                hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = hWMs };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}// end HttpMethod.Get

        //[HttpOperation(HttpMethod.GET, ForUriName = "GetApprovalHWMs")]
        //public OperationResult GetFilteredHWMs(string approved, [Optional] Int32 eventId, [Optional] Int32 memberId, [Optional] string state)
        //{
        //    try
        //    {
        //        HWMList hWMs = new HWMList();
        //        //set defaults
        //        //default to false
        //        bool isApprovedStatus = false;
        //        Boolean.TryParse(approved, out isApprovedStatus);
        //        string filterState = GetStateByName(state).ToString();
        //        Int32 filterMember = (memberId > 0) ? memberId : -1;
        //        Int32 filterEvent = (eventId > 0) ? eventId : -1;

        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //Because 'Where' is producing an IQueryable, 
        //            //the execution is deferred until the ToList so you can chain 'Wheres' together.
        //            IQueryable<HWM> query;
        //            if (isApprovedStatus)
        //                query = aSTNE.HWMs.Where(h => h.APPROVAL_ID > 0);
        //            else
        //                query = aSTNE.HWMs.Where(h => h.APPROVAL_ID <= 0 || !h.APPROVAL_ID.HasValue);

        //            if (filterEvent > 0)
        //                query = query.Where(h => h.EVENT_ID == filterEvent);

        //            if (filterState != State.UNSPECIFIED.ToString())
        //                query = query.Where(h => h.SITE.STATE == filterState);

        //            if (filterMember > 0)
        //                query = query.Where(h => h.FLAG_MEMBER_ID == filterMember || h.SURVEY_MEMBER_ID == filterMember);

        //            hWMs.HWMs = query.AsEnumerable().Select(hwm => new SimpleHWM
        //            {
        //                HWM_ID = Convert.ToInt32(hwm.HWM_ID),
        //                LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
        //                LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
        //                HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION != null ? hwm.HWM_LOCATIONDESCRIPTION : "",
        //                ELEV_FT = hwm.ELEV_FT != null ? Convert.ToDecimal(hwm.ELEV_FT) : 0,
        //                SITE_ID = Convert.ToInt32(hwm.SITE_ID),
        //                SITE_NO = hwm.SITE_ID > 0 ? hwm.SITE.SITE_NO : string.Empty
        //            }).ToList<SimpleHWM>();

        //            if (hWMs != null)
        //                hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = hWMs };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}// end HttpMethod.Get

        //[HttpOperation(HttpMethod.GET, ForUriName = "FilteredHWMs")]
        //public OperationResult FilteredHWMs([Optional]string eventIds, [Optional] string eventTypeIDs, Int32 eventStatusID,
        //                                              [Optional] string states, [Optional] string counties, [Optional] string hwmTypeIDs,
        //                                              [Optional] string hwmQualIDs, [Optional] string hwmEnvironment, [Optional] string surveyComplete, [Optional] string stillWater)
        //{
        //    //TODO: Add FLAG_MEMBER_ID and remove FLAG_TEAM_ID
        //    //TODO: Add SURVEY_MEMBER_ID and remove SURVEY_TEAM_ID
        //    List<HWMDownloadable> hwmsList = new List<HWMDownloadable>();
        //    try
        //    {
        //        char[] delimiterChars = { ';', ',', ' ' };
        //        //parse the requests
        //        List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
        //        List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

        //        List<decimal> hwmTypeIdList = !string.IsNullOrEmpty(hwmTypeIDs) ? hwmTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
        //        List<decimal> hwmQualIdList = !string.IsNullOrEmpty(hwmQualIDs) ? hwmQualIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<HWM> query;
        //            query = aSTNE.HWMs.Where(s => s.HWM_ID > 0);

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

        //            if (hwmTypeIdList != null && hwmTypeIdList.Count > 0)
        //                query = query.Where(i => hwmTypeIdList.Contains(i.HWM_TYPE_ID));

        //            if (hwmQualIdList != null && hwmQualIdList.Count > 0)
        //                query = query.Where(i => hwmQualIdList.Contains(i.HWM_QUALITY_ID));

        //            if (!string.IsNullOrEmpty(hwmEnvironment))
        //                query = query.Where(i => i.HWM_ENVIRONMENT == hwmEnvironment);

        //            if (!string.IsNullOrEmpty(surveyComplete))
        //            {
        //                if (surveyComplete == "True")
        //                    query = query.Where(i => i.SURVEY_DATE.HasValue);
        //                else
        //                    query = query.Where(i => !i.SURVEY_DATE.HasValue);
        //            }

        //            //1 = yes, 0 = no
        //            if (!string.IsNullOrEmpty(stillWater))
        //                if (stillWater == "True")
        //                    query = query.Where(i => i.STILLWATER.HasValue && i.STILLWATER.Value == 1);
        //                else
        //                    query = query.Where(i => !i.STILLWATER.HasValue || i.STILLWATER.Value == 0);

        //            hwmsList = query.AsEnumerable().Select(
        //                hwmD => new HWMDownloadable
        //                {
        //                    HWM_ID = hwmD.HWM_ID,
        //                    WATERBODY = hwmD.WATERBODY,
        //                    SITE_ID = hwmD.SITE_ID.Value,
        //                    EVENT_ID = hwmD.EVENT_ID.Value,
        //                    EVENT = hwmD.EVENT_ID.HasValue ? GetEvent(aSTNE, hwmD.EVENT_ID.Value) : "",
        //                    HWM_TYPE_ID = hwmD.HWM_TYPE_ID,
        //                    HWM_TYPE = hwmD.HWM_TYPE_ID > 0 ? GetHWMType(aSTNE, hwmD.HWM_TYPE_ID) : "",
        //                    HWM_QUALITY_ID = hwmD.HWM_QUALITY_ID,
        //                    HWM_QUALITY = hwmD.HWM_QUALITY_ID > 0 ? GetHWMQuality(aSTNE, hwmD.HWM_QUALITY_ID) : "",
        //                    HWM_LOCATION_DESCRIPTION = hwmD.HWM_LOCATIONDESCRIPTION != null ? GetHWMLocation(hwmD.HWM_LOCATIONDESCRIPTION) : "",
        //                    LATITUDE = hwmD.LATITUDE_DD.Value,
        //                    LONGITUDE = hwmD.LONGITUDE_DD.Value,
        //                    ELEV_FT = hwmD.ELEV_FT.HasValue ? hwmD.ELEV_FT.Value : 0,
        //                    VDATUM_ID = hwmD.VDATUM_ID.HasValue ? hwmD.VDATUM_ID.Value : 0,
        //                    VERTICAL_DATUM = hwmD.VDATUM_ID.HasValue && hwmD.VDATUM_ID.Value > 0 ? GetVDatum(aSTNE, hwmD.VDATUM_ID.Value) : "",
        //                    HDATUM_ID = hwmD.HDATUM_ID.HasValue ? hwmD.HDATUM_ID.Value : 0,
        //                    HORIZONTAL_DATUM = hwmD.HDATUM_ID.HasValue && hwmD.HDATUM_ID.Value > 0 ? GetHDatum(aSTNE, hwmD.HDATUM_ID.Value) : "",
        //                    VCOLLECT_METHOD_ID = hwmD.VCOLLECT_METHOD_ID.HasValue ? hwmD.VCOLLECT_METHOD_ID.Value : 0,
        //                    VERTICAL_COLLECT_METHOD = hwmD.VCOLLECT_METHOD_ID.HasValue && hwmD.VCOLLECT_METHOD_ID.Value > 0 ? GetVCollMethod(aSTNE, hwmD.VCOLLECT_METHOD_ID.Value) : "",
        //                    HCOLLECT_METHOD_ID = hwmD.HCOLLECT_METHOD_ID.HasValue ? hwmD.HCOLLECT_METHOD_ID.Value : 0,
        //                    HORIZONTAL_COLLECT_METHOD = hwmD.HCOLLECT_METHOD_ID.HasValue && hwmD.HCOLLECT_METHOD_ID.Value > 0 ? GetHCollectMethod(aSTNE, hwmD.HCOLLECT_METHOD_ID.Value) : "",
        //                    BANK = hwmD.BANK,
        //                    APPROVAL_ID = hwmD.APPROVAL_ID.HasValue ? hwmD.APPROVAL_ID.Value : 0,
        //                    APPROVAL_MEMBER = hwmD.APPROVAL_ID.HasValue || hwmD.APPROVAL_ID > 0 ? GetApprovingMember(aSTNE, hwmD.APPROVAL_ID) : "",
        //                    MARKER_ID = hwmD.MARKER_ID.HasValue ? hwmD.MARKER_ID.Value: 0,
        //                    MARKER_NAME = hwmD.MARKER_ID.HasValue && hwmD.MARKER_ID > 0 ? GetMarkerName(aSTNE, hwmD.MARKER_ID) : "",
        //                    HEIGHT_ABV_GND = hwmD.HEIGHT_ABOVE_GND.HasValue ? hwmD.HEIGHT_ABOVE_GND.Value : 0,
        //                    HWM_NOTES = !string.IsNullOrEmpty(hwmD.HWM_NOTES) ? GetNotes(aSTNE, hwmD.HWM_NOTES) : "",
        //                    HWM_ENVIRONMENT = !string.IsNullOrEmpty(hwmD.HWM_ENVIRONMENT) ? hwmD.HWM_ENVIRONMENT : "",
        //                    FLAG_DATE = hwmD.FLAG_DATE.HasValue ? hwmD.FLAG_DATE.Value.ToShortDateString() : "",
        //                    SURVEY_DATE = hwmD.SURVEY_DATE.HasValue ? hwmD.SURVEY_DATE.Value.ToShortDateString() : "",
        //                    STILLWATER = hwmD.STILLWATER.HasValue && hwmD.STILLWATER.Value > 0 ? "Yes" : "No",
        //                    FLAG_MEMBER_ID = hwmD.FLAG_MEMBER_ID.HasValue ? hwmD.FLAG_MEMBER_ID.Value : 0,
        //                    FLAG_MEMBER_NAME = hwmD.FLAG_MEMBER_ID.HasValue && hwmD.FLAG_MEMBER_ID.Value > 0 ? GetHWMMember(aSTNE, hwmD.FLAG_MEMBER_ID) : "",
        //                    SURVEY_MEMBER_ID = hwmD.SURVEY_MEMBER_ID.HasValue ? hwmD.SURVEY_MEMBER_ID.Value : 0,
        //                    SURVEY_MEMBER_NAME = hwmD.SURVEY_MEMBER_ID.HasValue && hwmD.SURVEY_MEMBER_ID.Value > 0 ? GetHWMMember(aSTNE, hwmD.SURVEY_MEMBER_ID) : "",
        //                    PEAK_SUMMARY_ID = hwmD.PEAK_SUMMARY_ID.HasValue ? hwmD.PEAK_SUMMARY_ID.Value : 0,
        //                    SITE_NO = hwmD.SITE.SITE_NO,
        //                    DESCRIPTION = hwmD.SITE.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(hwmD.SITE.SITE_DESCRIPTION) : "",
        //                    NETWORK = SiteHandler.GetSiteNetwork(aSTNE, hwmD.SITE_ID.Value),
        //                    STATE = hwmD.SITE.STATE,
        //                    COUNTY = hwmD.SITE.COUNTY,
        //                    PRIORITY = hwmD.SITE.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, hwmD.SITE.PRIORITY_ID.Value) : "",
        //                    ZONE = hwmD.SITE.ZONE,
        //                    PERM_HOUSING_INSTALLED = hwmD.SITE.IS_PERMANENT_HOUSING_INSTALLED == null || hwmD.SITE.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
        //                    SITE_NOTES = !string.IsNullOrEmpty(hwmD.SITE.SITE_NOTES) ? SiteHandler.GetSiteNotes(hwmD.SITE.SITE_NOTES) : ""


        //                }).ToList();


        //        }//end using

        //        return new OperationResult.OK { ResponseResource = hwmsList };
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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateHWM")]
        public OperationResult Post(hwm anEntity)
        {
            try
            {
                if (anEntity.site_id <= 0|| anEntity.event_id <= 0 || anEntity.hwm_type_id <= 0 || 
                    anEntity.hwm_quality_id <= 0 || string.IsNullOrEmpty(anEntity.hwm_environment) || anEntity.hdatum_id <=0 ||
                    anEntity.flag_member_id <=0 || anEntity.hcollect_method_id <=0)
                        throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
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
                if (anEntity.site_id <= 0 || anEntity.event_id <= 0 || anEntity.hwm_type_id <= 0 ||
                   anEntity.hwm_quality_id <= 0 || string.IsNullOrEmpty(anEntity.hwm_environment) || anEntity.hdatum_id <= 0 ||
                   anEntity.flag_member_id <= 0 || anEntity.hcollect_method_id <= 0)
                    throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<hwm>(anEntity);
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
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteSite")]
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
                        hwm ObjectToBeDeleted = sa.Select<hwm>().SingleOrDefault(c => c.hwm_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<hwm>(ObjectToBeDeleted);
                        #region Cascadedelete?
                        //sm(sa.Messages);

                        ////see if it has approval to delete
                        //if (ObjectToBeDeleted.APPROVAL_ID.HasValue)
                        //{
                        //    //remove the approval
                        //    APPROVAL thisAppr = aSTNE.APPROVALs.Where(x => x.APPROVAL_ID == ObjectToBeDeleted.APPROVAL_ID).FirstOrDefault();
                        //    aSTNE.APPROVALs.DeleteObject(thisAppr);
                        //    aSTNE.SaveChanges();
                        //}
                        ////see if there's a peak to delete
                        //if (ObjectToBeDeleted.PEAK_SUMMARY != null)
                        //{
                        //    PEAK_SUMMARY thisPeak = aSTNE.PEAK_SUMMARY.Where(x => x.PEAK_SUMMARY_ID == ObjectToBeDeleted.PEAK_SUMMARY_ID).FirstOrDefault();
                        //    aSTNE.PEAK_SUMMARY.DeleteObject(thisPeak);
                        //    aSTNE.SaveChanges();
                        //}

                        ////see if there's any files to delete
                        //List<FILES> HWMFiles = aSTNE.FILES.Where(x => x.HWM_ID == hwmId).ToList();
                        //if (HWMFiles.Count >= 1)
                        //{
                        //    foreach (FILES f in HWMFiles)
                        //    {
                        //        //delete the file item from s3
                        //        S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                        //        aBucket.DeleteObject(BuildFilePath(f, f.PATH));
                        //        //delete the file
                        //        aSTNE.DeleteObject(f);
                        //        aSTNE.SaveChanges();
                        //    }
                        //}
                        ////delete HWM now
                        //aSTNE.HWMs.DeleteObject(ObjectToBeDeleted);
                        //aSTNE.SaveChanges();
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

        //#region Helper Methods

        //private bool PostHWMLayer(HWM aHWM)
        //{
        //    Feature flayer;
        //    Features fLayers;
        //    AGSServiceAgent agsAgent;
        //    try
        //    {

        //        flayer = new Feature(new HWMLayer(aHWM));
        //        fLayers = new Features();

        //        fLayers.features.Add(flayer);



        //        agsAgent = new AGSServiceAgent();

        //        return agsAgent.PostFeature(fLayers, "AddHWM/GPServer/AddHWM/execute", "New_HWM=");
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}//end PostHWMLayer

        //#region hwmDownloadable calls
        //private string MadeIt()
        //{
        //    return "2";
        //}
        //private string GetEvent(STNEntities2 aSTNE, decimal eventId)
        //{
        //    string eventName = string.Empty;
        //    EVENT ev = aSTNE.EVENTS.Where(x => x.EVENT_ID == eventId).FirstOrDefault();
        //    if (ev != null)
        //        eventName = ev.EVENT_NAME;

        //    return eventName;
        //}
        //private string GetHWMType(STNEntities2 aSTNE, decimal hwmTypeId)
        //{
        //    string hwmType = string.Empty;
        //    HWM_TYPES ht = aSTNE.HWM_TYPES.Where(x => x.HWM_TYPE_ID == hwmTypeId).FirstOrDefault();
        //    if (ht != null)
        //        hwmType = ht.HWM_TYPE;

        //    return hwmType;
        //}
        //private string GetHWMQuality(STNEntities2 aSTNE, decimal hwmQualId)
        //{
        //    string hwmQual = string.Empty;
        //    HWM_QUALITIES ht = aSTNE.HWM_QUALITIES.Where(x => x.HWM_QUALITY_ID == hwmQualId).FirstOrDefault();
        //    if (ht != null)
        //        hwmQual = ht.HWM_QUALITY;

        //    return hwmQual;
        //}
        //private string GetHWMLocation(string desc)
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
        //private string GetVDatum(STNEntities2 aSTNE, decimal? vdID)
        //{
        //    string vdName = string.Empty;
        //    VERTICAL_DATUMS vd = aSTNE.VERTICAL_DATUMS.Where(x => x.DATUM_ID == vdID).FirstOrDefault();
        //    if (vd != null)
        //        vdName = vd.DATUM_NAME;

        //    return vdName;
        //}
        //private string GetHDatum(STNEntities2 aSTNE, decimal? hdID)
        //{
        //    string hdName = string.Empty;
        //    HORIZONTAL_DATUMS hd = aSTNE.HORIZONTAL_DATUMS.Where(x => x.DATUM_ID == hdID).FirstOrDefault();
        //    if (hd != null)
        //        hdName = hd.DATUM_NAME;

        //    return hdName;
        //}
        //private string GetVCollMethod(STNEntities2 aSTNE, decimal? vcmID)
        //{
        //    string vCollMethod = string.Empty;
        //    VERTICAL_COLLECT_METHODS vcm = aSTNE.VERTICAL_COLLECT_METHODS.Where(x => x.VCOLLECT_METHOD_ID == vcmID).FirstOrDefault();
        //    if (vcm != null)
        //        vCollMethod = vcm.VCOLLECT_METHOD;

        //    return vCollMethod;
        //}
        //private string GetHCollectMethod(STNEntities2 aSTNE, decimal? hcmID)
        //{
        //    string hCollMethod = string.Empty;
        //    HORIZONTAL_COLLECT_METHODS hcm = aSTNE.HORIZONTAL_COLLECT_METHODS.Where(x => x.HCOLLECT_METHOD_ID == hcmID).FirstOrDefault();
        //    if (hcm != null)
        //        hCollMethod = hcm.HCOLLECT_METHOD;

        //    return hCollMethod;
        //}
        //private string GetApprovingMember(STNEntities2 aSTNE, decimal? approveId)
        //{
        //    string approveName = string.Empty;
        //    MEMBER m = aSTNE.MEMBERS.Where(x => x.MEMBER_ID == approveId).FirstOrDefault();
        //    if (m != null)
        //        approveName = m.FNAME + " " + m.LNAME;

        //    return approveName;
        //}
        //private string GetMarkerName(STNEntities2 aSTNE, decimal? markerId)
        //{
        //    string markName = string.Empty;
        //    MARKER ma = aSTNE.MARKERS.Where(x => x.MARKER_ID == markerId).FirstOrDefault();
        //    if (ma != null)
        //        markName = ma.MARKER1;

        //    return markName;
        //}
        //private string GetNotes(STNEntities2 aSTNE, string notes)
        //{
        //    string n = string.Empty;

        //    if (notes.Length > 100)
        //    {
        //        n = notes.Substring(0, 100);
        //    }
        //    else
        //    {
        //        n = notes;
        //    }

        //    return n;
        //}
        //private string GetHWMMember(STNEntities2 aSTNE, decimal? memberId)
        //{
        //    string memberName = string.Empty;
        //    MEMBER ct = aSTNE.MEMBERS.Where(x => x.MEMBER_ID == memberId).FirstOrDefault();
        //    if (ct != null)
        //        memberName = ct.FNAME + " " + ct.LNAME;
        //    return memberName;
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
        //#endregion hwmDownloadable calls

        //#endregion
    }//end class HWMHandler

}//end namespace