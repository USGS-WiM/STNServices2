﻿//------------------------------------------------------------------------------
//----- HWMHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

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

using STNServices2.Resources;
using STNServices2.Authentication;
using STNServices2.Utilities;

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

    public class HWMHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HWMs"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            HWMList hWMs = new HWMList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.APPROVAL_ID > 0)
                                    .Select(hwm => new SimpleHWM
                                    {
                                        HWM_ID = Convert.ToInt32(hwm.HWM_ID),
                                        LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
                                        LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
                                        HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
                                        ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
                                    }).ToList<SimpleHWM>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        hWMs.HWMs = aSTNE.HWMs.AsEnumerable()
                             .Select(hwm => new SimpleHWM
                             {
                                 HWM_ID = Convert.ToInt32(hwm.HWM_ID),
                                 LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
                                 LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
                                 HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
                                 ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
                             }).ToList<SimpleHWM>();
                    }

                    if (hWMs != null)
                        hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMByApproval")]
        public OperationResult Get(string boolean)
        {
            try
            {
                List<HWM> hWMs = new List<HWM>();
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(boolean, out isApprovedStatus);

                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (isApprovedStatus)
                    {
                        hWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.APPROVAL_ID > 0).ToList();
                    }
                    else
                    {
                        //return all non approved hwms
                        hWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.APPROVAL_ID < 0 || !hwm.APPROVAL_ID.HasValue).ToList();
                    }

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedHWMs")]
        public OperationResult GetApprovedHWMs(Int32 ApprovalId)
        {
            List<HWM> hWMs = new List<HWM>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hWMs = aSTNE.APPROVALs.FirstOrDefault(a => a.APPROVAL_ID == ApprovalId).HWMs.ToList();

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetApprovedHWMRSSFeed")]
        public OperationResult GetApprovedHWMRSSFeed()
        {
            try
            {
                SyndicationFeed aFeed = new SyndicationFeed("Ohio Flood Response High Water Marks Feed", "Feed Description", new Uri("http://wim.usgs.gov"), "FeedID", DateTime.Now);

                //SyndicationPerson sp = new SyndicationPerson("mpeppler@usgs.gov", "Marie Peppler", "http://wi.water.usgs.gov/professional-pages/peppler.html");
                //aFeed.Authors.Add(sp);

                aFeed.Description = new TextSyndicationContent("A feed showing provisional HWMs");
                aFeed.Language = "en-us";
                aFeed.LastUpdatedTime = DateTime.Now;
                aFeed.Generator = "WiM Flood Response Services";
                aFeed.Id = "aFeedID";
                aFeed.ImageUrl = new Uri("http://wim.usgs.gov/FIMI/assets/images/WiM_logo_sm.gif");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aFeed.Items = aSTNE.HWMs.AsEnumerable()
                                    .Where(hwm => hwm.APPROVAL_ID < 0)
                                    .Select(hwm => new SyndicationItem
                                    {
                                        Title = new TextSyndicationContent("ID " + hwm.HWM_ID + " at " + hwm.LATITUDE_DD + "," + hwm.LONGITUDE_DD),
                                        Summary = new TextSyndicationContent("ID " + hwm.HWM_ID + " at " + hwm.LATITUDE_DD + "," + hwm.LONGITUDE_DD + " on waterbody " + hwm.WATERBODY)
                                    }
                                    ).ToList<SyndicationItem>();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aFeed };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteHWMs")]
        public OperationResult GetSiteHWMs(Int32 siteId)
        {
            List<HWM> hWMs = new List<HWM>();

            //Return BadRequest if there is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        hWMs = aSTNE.HWMs.AsEnumerable()
                                    .Where(hwm => hwm.SITE_ID == siteId && hwm.APPROVAL_ID > 0)
                                    .OrderBy(hwm => hwm.HWM_ID)
                                    .ToList<HWM>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        hWMs = aSTNE.HWMs.AsEnumerable()
                            .Where(hwm => hwm.SITE_ID == siteId)
                            .OrderBy(hwm => hwm.HWM_ID)
                            .ToList<HWM>();
                    }

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVDatumHWMs")]
        public OperationResult GetVDatumHWMs(Int32 vdatumId)
        {
            List<HWM> hWMs = new List<HWM>();

            //Return BadRequest if there is no ID
            if (vdatumId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hWMs = aSTNE.VERTICAL_DATUMS.FirstOrDefault(vd => vd.DATUM_ID == vdatumId).HWMs.ToList();

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetVmethodHWMs")]
        public OperationResult GetVmethodHWMs(Int32 vmethodId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (vmethodId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.VERTICAL_COLLECT_METHODS.FirstOrDefault(i => i.VCOLLECT_METHOD_ID == vmethodId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetCollectionTeamHWMs")]
        public OperationResult GetCollectionTeamHWMs(Int32 teamId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (teamId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.COLLECT_TEAM.FirstOrDefault(i => i.COLLECT_TEAM_ID == teamId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMQualityHWMs")]
        public OperationResult GetHWMQualityHWMs(Int32 hwmQualityId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (hwmQualityId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.HWM_QUALITIES.FirstOrDefault(i => i.HWM_QUALITY_ID == hwmQualityId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMTypeHWMs")]
        public OperationResult GetHWMTypeHWMs(Int32 hwmTypeId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (hwmTypeId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.HWM_TYPES.FirstOrDefault(i => i.HWM_TYPE_ID == hwmTypeId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHmethodHWMs")]
        public OperationResult GetHMethodHWMs(Int32 hmethodId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (hmethodId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.HORIZONTAL_COLLECT_METHODS.FirstOrDefault(i => i.HCOLLECT_METHOD_ID == hmethodId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMarkerHWMs")]
        public OperationResult GetMarkerHWMs(Int32 markerId)
        {
            List<HWM> hwmList = new List<HWM>();

            //Return BadRequest if there is no ID
            if (markerId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmList = aSTNE.MARKERS.FirstOrDefault(i => i.MARKER_ID == markerId).HWMs.ToList();

                    if (hwmList != null)
                        hwmList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hwmList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryHWMs")]
        public OperationResult GetPeakSummaryHWMs(Int32 peakSummaryId)
        {
            List<HWM> hWMs = new List<HWM>();

            //Return BadRequest if there is no ID
            if (peakSummaryId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        hWMs = aSTNE.HWMs.AsEnumerable()
                                    .Where(hwm => hwm.PEAK_SUMMARY_ID == peakSummaryId && hwm.APPROVAL_ID > 0)
                                    .OrderBy(hwm => hwm.HWM_ID)
                                    .ToList<HWM>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        hWMs = aSTNE.HWMs.AsEnumerable()
                            .Where(hwm => hwm.PEAK_SUMMARY_ID == peakSummaryId)
                            .OrderBy(hwm => hwm.HWM_ID)
                            .ToList<HWM>();
                    }

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSiteHWMsBySiteNo")]
        public OperationResult GetSiteHWMsBySiteNo(String siteNo)
        {
            List<HWM> hWMs = new List<HWM>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    var aSite = aSTNE.SITES.Where(site => site.SITE_NO == siteNo)
                                        .Select(s => s.SITE_ID).FirstOrDefault();

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        hWMs = aSTNE.HWMs.AsEnumerable()
                                    .Where(hwm => hwm.SITE_ID == aSite && hwm.APPROVAL_ID.HasValue)
                                    .OrderBy(hwm => hwm.HWM_ID)
                                    .ToList<HWM>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        hWMs = aSTNE.HWMs.AsEnumerable()
                                .Where(hwm => hwm.SITE_ID == aSite)
                                .OrderBy(hwm => hwm.HWM_ID)
                                .ToList<HWM>();
                    }

                    if (hWMs != null)
                        hWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWM")]
        public OperationResult Get(Int32 entityId)
        {
            HWM aHWM;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aHWM = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == entityId);

                    //load links
                    if (aHWM != null)
                        aHWM.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aHWM };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileHWM")]
        public OperationResult GetFileHWM(Int32 fileId)
        {
            HWM aHWM;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aHWM = aSTNE.FILES.FirstOrDefault(f => f.FILE_ID == fileId).HWM;

                    //load links
                    if (aHWM != null)
                        aHWM.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aHWM };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventSimpleHWMs")]
        public OperationResult GetEventSimpleHWMs(Int32 eventId)
        {
            HWMList hWMs = new HWMList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.EVENT_ID == eventId)
                        .Select(hwm => new SimpleHWM
                        {
                            HWM_ID = Convert.ToInt32(hwm.HWM_ID),
                            LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
                            LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
                            HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
                            ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT)
                        }).ToList<SimpleHWM>();

                    if (hWMs != null)
                        hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventHWMs")]
        public OperationResult Get(Int32 siteId, Int32 eventId)
        {
            HWMList hWMs = new HWMList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hWMs.HWMs = aSTNE.HWMs.AsEnumerable().Where(hwm => hwm.EVENT_ID == eventId
                        && hwm.SITE_ID == siteId)
                        .Select(hwm => new SimpleHWM
                        {
                            HWM_ID = Convert.ToInt32(hwm.HWM_ID),
                            LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
                            LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
                            HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION,
                            ELEV_FT = Convert.ToDecimal(hwm.ELEV_FT),
                            SURVEY_DATE = hwm.SURVEY_DATE,
                            FLAG_DATE = hwm.FLAG_DATE
                        }).ToList<SimpleHWM>();

                    if (hWMs != null)
                        hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovalHWMs")]
        public OperationResult GetFilteredHWMs(string approved, [Optional] Int32 eventId, [Optional] Int32 memberId, [Optional] string state)
        {
            try
            {
                HWMList hWMs = new HWMList();
                //set defaults
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();
                Int32 filterMember = (memberId > 0) ? memberId : -1;
                Int32 filterEvent = (eventId > 0) ? eventId : -1;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    //Because 'Where' is producing an IQueryable, 
                    //the execution is deferred until the ToList so you can chain 'Wheres' together.
                    IQueryable<HWM> query;
                    if (isApprovedStatus)
                        query = aSTNE.HWMs.Where(h => h.APPROVAL_ID > 0);
                    else
                        query = aSTNE.HWMs.Where(h => h.APPROVAL_ID <= 0 || !h.APPROVAL_ID.HasValue);

                    if (filterEvent > 0)
                        query = query.Where(h => h.EVENT_ID == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(h => h.SITE.STATE == filterState);

                    if (filterMember > 0)
                        query = query.Where(h => h.COLLECT_TEAM.MEMBERS_TEAM.Any(mt => mt.MEMBER_ID == filterMember));

                    hWMs.HWMs = query.AsEnumerable().Select(hwm => new SimpleHWM
                    {
                        HWM_ID = Convert.ToInt32(hwm.HWM_ID),
                        LATITUDE_DD = Convert.ToDecimal(hwm.LATITUDE_DD),
                        LONGITUDE_DD = Convert.ToDecimal(hwm.LONGITUDE_DD),
                        HWM_LOCATIONDESCRIPTION = hwm.HWM_LOCATIONDESCRIPTION != null ? hwm.HWM_LOCATIONDESCRIPTION : "",
                        ELEV_FT = hwm.ELEV_FT != null ? Convert.ToDecimal(hwm.ELEV_FT) : 0,
                        SITE_NO = hwm.SITE_ID > 0 ? hwm.SITE.SITE_NO : string.Empty
                    }).ToList<SimpleHWM>();

                    if (hWMs != null)
                        hWMs.HWMs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = hWMs };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(ForUriName = "GetFilteredHWMs")]
        public OperationResult FilteredHWMs([Optional]string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID,
                                                      [Optional] string states, [Optional] string counties, [Optional] string hwmTypeIDs,
                                                      [Optional] string hwmQualIDs, [Optional] string hwmEnvironment, [Optional] string surveyComplete, [Optional] string stillWater)
        {
            List<HWMDownloadable> hwmsList = new List<HWMDownloadable>();
            try
            {
                char[] delimiterChars = { ';', ',', ' ' };
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

                List<decimal> hwmTypeIdList = !string.IsNullOrEmpty(hwmTypeIDs) ? hwmTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> hwmQualIdList = !string.IsNullOrEmpty(hwmQualIDs) ? hwmQualIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<HWM> query;
                    query = aSTNE.HWMs.Where(s => s.HWM_ID > 0);

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

                    if (hwmTypeIdList != null && hwmTypeIdList.Count > 0)
                        query = query.Where(i => hwmTypeIdList.Contains(i.HWM_TYPE_ID));

                    if (hwmQualIdList != null && hwmQualIdList.Count > 0)
                        query = query.Where(i => hwmQualIdList.Contains(i.HWM_QUALITY_ID));

                    if (!string.IsNullOrEmpty(hwmEnvironment))
                        query = query.Where(i => i.HWM_ENVIRONMENT == hwmEnvironment);

                    if (!string.IsNullOrEmpty(surveyComplete))
                    {
                        if (surveyComplete == "True")
                            query = query.Where(i => i.SURVEY_DATE.HasValue);
                        else
                            query = query.Where(i => !i.SURVEY_DATE.HasValue);
                    }

                    //1 = yes, 0 = no
                    if (!string.IsNullOrEmpty(stillWater))
                        if (stillWater == "True")
                            query = query.Where(i => i.STILLWATER.HasValue && i.STILLWATER.Value == 1);
                        else
                            query = query.Where(i => !i.STILLWATER.HasValue || i.STILLWATER.Value == 0);

                    hwmsList = query.AsEnumerable().Select(
                        hwmD => new HWMDownloadable
                        {
                            HWM_ID = hwmD.HWM_ID,
                            WATERBODY = hwmD.WATERBODY,
                            SITE_ID = hwmD.SITE_ID.Value,
                            EVENT = hwmD.EVENT_ID.HasValue ? GetEvent(aSTNE, hwmD.EVENT_ID.Value) : "",
                            HWM_TYPE = hwmD.HWM_TYPE_ID > 0 ? GetHWMType(aSTNE, hwmD.HWM_TYPE_ID) : "",
                            HWM_QUALITY = hwmD.HWM_QUALITY_ID > 0 ? GetHWMQuality(aSTNE, hwmD.HWM_QUALITY_ID) : "",
                            HWM_LOCATION_DESCRIPTION = hwmD.HWM_LOCATIONDESCRIPTION != null ? GetHWMLocation(hwmD.HWM_LOCATIONDESCRIPTION) : "",
                            LATITUDE = hwmD.LATITUDE_DD.Value,
                            LONGITUDE = hwmD.LONGITUDE_DD.Value,
                            ELEV_FT = hwmD.ELEV_FT.HasValue ? hwmD.ELEV_FT.Value : 0,
                            VERTICAL_DATUM = hwmD.VDATUM_ID.HasValue && hwmD.VDATUM_ID.Value > 0 ? GetVDatum(aSTNE, hwmD.VDATUM_ID.Value) : "",
                            HORIZONTAL_DATUM = hwmD.HDATUM_ID.HasValue && hwmD.HDATUM_ID.Value > 0 ? GetHDatum(aSTNE, hwmD.HDATUM_ID.Value) : "",
                            VERTICAL_COLLECT_METHOD = hwmD.VCOLLECT_METHOD_ID.HasValue && hwmD.VCOLLECT_METHOD_ID.Value > 0 ? GetVCollMethod(aSTNE, hwmD.VCOLLECT_METHOD_ID.Value) : "",
                            HORIZONTAL_COLLECT_METHOD = hwmD.HCOLLECT_METHOD_ID.HasValue && hwmD.HCOLLECT_METHOD_ID.Value > 0 ? GetHCollectMethod(aSTNE, hwmD.HCOLLECT_METHOD_ID.Value) : "",
                            BANK = hwmD.BANK,
                            APPROVAL_MEMBER = hwmD.APPROVAL_ID != null || hwmD.APPROVAL_ID > 0 ? GetApprovingMember(aSTNE, hwmD.APPROVAL_ID) : "",
                            MARKER_NAME = hwmD.MARKER_ID != null && hwmD.MARKER_ID > 0 ? GetMarkerName(aSTNE, hwmD.MARKER_ID) : "",
                            HEIGHT_ABV_GND = hwmD.HEIGHT_ABOVE_GND.HasValue ? hwmD.HEIGHT_ABOVE_GND.Value : 0,
                            HWM_NOTES = !string.IsNullOrEmpty(hwmD.HWM_NOTES) ? GetNotes(aSTNE, hwmD.HWM_NOTES) : "",
                            HWM_ENVIRONMENT = !string.IsNullOrEmpty(hwmD.HWM_ENVIRONMENT) ? hwmD.HWM_ENVIRONMENT : "",
                            FLAG_DATE = hwmD.FLAG_DATE.HasValue ? hwmD.FLAG_DATE.Value.ToShortDateString() : "",
                            SURVEY_DATE = hwmD.SURVEY_DATE.HasValue ? hwmD.SURVEY_DATE.Value.ToShortDateString() : "",
                            STILLWATER = hwmD.STILLWATER.HasValue && hwmD.STILLWATER.Value > 0 ? "Yes" : "No",
                            FLAG_TEAM_NAME = hwmD.FLAG_TEAM_ID.HasValue && hwmD.FLAG_TEAM_ID.Value > 0 ? GetFlagTeam(aSTNE, hwmD.FLAG_TEAM_ID) : "",
                            SURVEY_TEAM_NAME = hwmD.SURVEY_TEAM_ID.HasValue && hwmD.SURVEY_TEAM_ID.Value > 0 ? GetSurveyTeam(aSTNE, hwmD.SURVEY_TEAM_ID) : "",
                            SITE_NO = hwmD.SITE.SITE_NO,
                            DESCRIPTION = hwmD.SITE.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(hwmD.SITE.SITE_DESCRIPTION) : "",
                            NETWORK = SiteHandler.GetSiteNetwork(aSTNE, hwmD.SITE_ID.Value),
                            STATE = hwmD.SITE.STATE,
                            COUNTY = hwmD.SITE.COUNTY,
                            PRIORITY = hwmD.SITE.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, hwmD.SITE.PRIORITY_ID.Value) : "",
                            ZONE = hwmD.SITE.ZONE,
                            PERM_HOUSING_INSTALLED = hwmD.SITE.IS_PERMANENT_HOUSING_INSTALLED == null || hwmD.SITE.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
                            SITE_NOTES = !string.IsNullOrEmpty(hwmD.SITE.SITE_NOTES) ? SiteHandler.GetSiteNotes(hwmD.SITE.SITE_NOTES) : ""


                        }).ToList();


                }//end using

                return new OperationResult.OK { ResponseResource = hwmsList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateHWM")]
        public OperationResult Post(HWM aHWM)
        {
            //Return BadRequest if missing required fields
            if ((aHWM.FLAG_DATE == null) || (aHWM.HWM_TYPE_ID <= 0) || (aHWM.HWM_QUALITY_ID <= 0) || aHWM.SITE_ID <= 0)
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
                        if (!Exists(aSTNE.HWMs, ref aHWM))
                        {
                            aSTNE.HWMs.AddObject(aHWM);
                            aSTNE.SaveChanges();
                        }//end if

                        if (aHWM != null)
                            aHWM.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aHWM };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HWM aHWM)
        {
            HWM updatedHWM;

            //Return BadRequest if missing required fields
            if ((entityId <= 0) || (aHWM.FLAG_DATE == null) || (aHWM.HWM_TYPE_ID <= 0) || (aHWM.HWM_QUALITY_ID <= 0))
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
                        updatedHWM = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == entityId);

                        updatedHWM.WATERBODY = aHWM.WATERBODY;
                        updatedHWM.SITE_ID = aHWM.SITE_ID;
                        updatedHWM.EVENT_ID = aHWM.EVENT_ID;
                        updatedHWM.HWM_TYPE_ID = aHWM.HWM_TYPE_ID;
                        updatedHWM.HWM_QUALITY_ID = aHWM.HWM_QUALITY_ID;
                        updatedHWM.HWM_LOCATIONDESCRIPTION = aHWM.HWM_LOCATIONDESCRIPTION;
                        updatedHWM.LATITUDE_DD = aHWM.LATITUDE_DD;
                        updatedHWM.LONGITUDE_DD = aHWM.LONGITUDE_DD;
                        updatedHWM.SURVEY_DATE = aHWM.SURVEY_DATE;
                        updatedHWM.FLAG_DATE = aHWM.FLAG_DATE;
                        updatedHWM.ELEV_FT = aHWM.ELEV_FT;
                        updatedHWM.VDATUM_ID = aHWM.VDATUM_ID;
                        updatedHWM.HDATUM_ID = aHWM.HDATUM_ID;
                        updatedHWM.FLAG_TEAM_ID = aHWM.FLAG_TEAM_ID;
                        updatedHWM.SURVEY_TEAM_ID = aHWM.SURVEY_TEAM_ID;
                        updatedHWM.VCOLLECT_METHOD_ID = aHWM.VCOLLECT_METHOD_ID;
                        updatedHWM.BANK = aHWM.BANK;
                        updatedHWM.APPROVAL_ID = aHWM.APPROVAL_ID;
                        updatedHWM.MARKER_ID = aHWM.MARKER_ID;
                        updatedHWM.HEIGHT_ABOVE_GND = aHWM.HEIGHT_ABOVE_GND;
                        updatedHWM.HCOLLECT_METHOD_ID = aHWM.HCOLLECT_METHOD_ID;
                        updatedHWM.PEAK_SUMMARY_ID = aHWM.PEAK_SUMMARY_ID;
                        updatedHWM.HWM_NOTES = aHWM.HWM_NOTES;
                        updatedHWM.HWM_ENVIRONMENT = aHWM.HWM_ENVIRONMENT;
                        updatedHWM.STILLWATER = aHWM.STILLWATER;

                        aSTNE.SaveChanges();

                        if (aHWM != null)
                            aHWM.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aHWM };
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
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteSite")]
        public OperationResult Delete(Int32 hwmId)
        {
            //Return BadRequest if missing required fields
            if (hwmId <= 0)
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
                        //fetch the object to be updated (assuming that it exists)
                        HWM ObjectToBeDeleted = aSTNE.HWMs.SingleOrDefault(c => c.HWM_ID == hwmId);

                        //see if it has approval to delete
                        if (ObjectToBeDeleted.APPROVAL_ID.HasValue)
                        {
                            //remove the approval
                            APPROVAL thisAppr = aSTNE.APPROVALs.Where(x => x.APPROVAL_ID == ObjectToBeDeleted.APPROVAL_ID).FirstOrDefault();
                            aSTNE.APPROVALs.DeleteObject(thisAppr);
                            aSTNE.SaveChanges();
                        }
                        //see if there's a peak to delete
                        if (ObjectToBeDeleted.PEAK_SUMMARY != null)
                        {
                            PEAK_SUMMARY thisPeak = aSTNE.PEAK_SUMMARY.Where(x => x.PEAK_SUMMARY_ID == ObjectToBeDeleted.PEAK_SUMMARY_ID).FirstOrDefault();
                            aSTNE.PEAK_SUMMARY.DeleteObject(thisPeak);
                            aSTNE.SaveChanges();
                        }

                        //see if there's any files to delete
                        List<FILE> HWMFiles = aSTNE.FILES.Where(x => x.HWM_ID == hwmId).ToList();
                        if (HWMFiles.Count >= 1)
                        {
                            foreach (FILE f in HWMFiles)
                            {
                                aSTNE.DeleteObject(f);
                                aSTNE.SaveChanges();
                            }
                        }
                        //delete HWM now
                        aSTNE.HWMs.DeleteObject(ObjectToBeDeleted);
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

        private bool PostHWMLayer(HWM aHWM)
        {
            Feature flayer;
            Features fLayers;
            AGSServiceAgent agsAgent;
            try
            {

                flayer = new Feature(new HWMLayer(aHWM));
                fLayers = new Features();

                fLayers.features.Add(flayer);



                agsAgent = new AGSServiceAgent();

                return agsAgent.PostFeature(fLayers, "AddHWM/GPServer/AddHWM/execute", "New_HWM=");
            }
            catch (Exception)
            {
                return false;
            }
        }//end PostHWMLayer
        private bool Exists(ObjectSet<HWM> entityRDS, ref HWM anEntity)
        {
            HWM existingEntity;
            HWM thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.HWM_TYPE_ID == thisEntity.HWM_TYPE_ID &&
                                                               e.HWM_QUALITY_ID == thisEntity.HWM_QUALITY_ID &&
                                                               e.SITE_ID.Value == thisEntity.SITE_ID.Value &&
                                                               (e.EVENT_ID == thisEntity.EVENT_ID || thisEntity.EVENT_ID <= 0 || thisEntity.EVENT_ID == null) &&
                                                               DateTime.Equals(e.FLAG_DATE.Value, thisEntity.FLAG_DATE.Value) &&
                                                               (string.Equals(e.WATERBODY.ToUpper(), thisEntity.WATERBODY.ToUpper()) || string.IsNullOrEmpty(thisEntity.WATERBODY)) &&
                                                               (string.Equals(e.BANK.ToUpper(), thisEntity.BANK.ToUpper()) || string.IsNullOrEmpty(thisEntity.BANK)) &&
                                                               (e.LATITUDE_DD.Value == thisEntity.LATITUDE_DD.Value || thisEntity.LATITUDE_DD.Value <= 0 || !thisEntity.LATITUDE_DD.HasValue) &&
                                                               (e.LONGITUDE_DD.Value == thisEntity.LONGITUDE_DD.Value || thisEntity.LONGITUDE_DD.Value <= 0 || !thisEntity.LONGITUDE_DD.HasValue) &&
                                                               (e.ELEV_FT.Value == thisEntity.ELEV_FT.Value || thisEntity.ELEV_FT.Value <= 0 || !thisEntity.ELEV_FT.HasValue) &&
                                                               (e.VDATUM_ID.Value == thisEntity.VDATUM_ID.Value || thisEntity.VDATUM_ID.Value <= 0 || !thisEntity.VDATUM_ID.HasValue) &&
                                                               (e.HDATUM_ID.Value == thisEntity.HDATUM_ID.Value || thisEntity.HDATUM_ID.Value <= 0 || !thisEntity.HDATUM_ID.HasValue) &&
                                                               (e.FLAG_TEAM_ID.Value == thisEntity.FLAG_TEAM_ID.Value || thisEntity.FLAG_TEAM_ID.Value <= 0 || !thisEntity.FLAG_TEAM_ID.HasValue) &&
                                                               (e.VCOLLECT_METHOD_ID.Value == thisEntity.VCOLLECT_METHOD_ID.Value || thisEntity.VCOLLECT_METHOD_ID.Value <= 0 || !thisEntity.VCOLLECT_METHOD_ID.HasValue) &&
                                                               (e.APPROVAL_ID.Value == thisEntity.APPROVAL_ID.Value || thisEntity.APPROVAL_ID.Value <= 0 || !thisEntity.APPROVAL_ID.HasValue) &&
                                                               (e.MARKER_ID.Value == thisEntity.MARKER_ID.Value || thisEntity.MARKER_ID.Value <= 0 || !thisEntity.MARKER_ID.HasValue) &&
                                                               (e.HEIGHT_ABOVE_GND.Value == thisEntity.HEIGHT_ABOVE_GND.Value || thisEntity.HEIGHT_ABOVE_GND.Value <= 0 || !thisEntity.HEIGHT_ABOVE_GND.HasValue));


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

        #region hwmDownloadable calls
        private string MadeIt()
        {
            return "2";
        }
        private string GetEvent(STNEntities2 aSTNE, decimal eventId)
        {
            string eventName = string.Empty;
            EVENT ev = aSTNE.EVENTS.Where(x => x.EVENT_ID == eventId).FirstOrDefault();
            if (ev != null)
                eventName = ev.EVENT_NAME;

            return eventName;
        }
        private string GetHWMType(STNEntities2 aSTNE, decimal hwmTypeId)
        {
            string hwmType = string.Empty;
            HWM_TYPES ht = aSTNE.HWM_TYPES.Where(x => x.HWM_TYPE_ID == hwmTypeId).FirstOrDefault();
            if (ht != null)
                hwmType = ht.HWM_TYPE;

            return hwmType;
        }
        private string GetHWMQuality(STNEntities2 aSTNE, decimal hwmQualId)
        {
            string hwmQual = string.Empty;
            HWM_QUALITIES ht = aSTNE.HWM_QUALITIES.Where(x => x.HWM_QUALITY_ID == hwmQualId).FirstOrDefault();
            if (ht != null)
                hwmQual = ht.HWM_QUALITY;

            return hwmQual;
        }
        private string GetHWMLocation(string desc)
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
        private string GetVDatum(STNEntities2 aSTNE, decimal? vdID)
        {
            string vdName = string.Empty;
            VERTICAL_DATUMS vd = aSTNE.VERTICAL_DATUMS.Where(x => x.DATUM_ID == vdID).FirstOrDefault();
            if (vd != null)
                vdName = vd.DATUM_NAME;

            return vdName;
        }
        private string GetHDatum(STNEntities2 aSTNE, decimal? hdID)
        {
            string hdName = string.Empty;
            HORIZONTAL_DATUMS hd = aSTNE.HORIZONTAL_DATUMS.Where(x => x.DATUM_ID == hdID).FirstOrDefault();
            if (hd != null)
                hdName = hd.DATUM_NAME;

            return hdName;
        }
        private string GetVCollMethod(STNEntities2 aSTNE, decimal? vcmID)
        {
            string vCollMethod = string.Empty;
            VERTICAL_COLLECT_METHODS vcm = aSTNE.VERTICAL_COLLECT_METHODS.Where(x => x.VCOLLECT_METHOD_ID == vcmID).FirstOrDefault();
            if (vcm != null)
                vCollMethod = vcm.VCOLLECT_METHOD;

            return vCollMethod;
        }
        private string GetHCollectMethod(STNEntities2 aSTNE, decimal? hcmID)
        {
            string hCollMethod = string.Empty;
            HORIZONTAL_COLLECT_METHODS hcm = aSTNE.HORIZONTAL_COLLECT_METHODS.Where(x => x.HCOLLECT_METHOD_ID == hcmID).FirstOrDefault();
            if (hcm != null)
                hCollMethod = hcm.HCOLLECT_METHOD;

            return hCollMethod;
        }
        private string GetApprovingMember(STNEntities2 aSTNE, decimal? approveId)
        {
            string approveName = string.Empty;
            MEMBER m = aSTNE.MEMBERS.Where(x => x.MEMBER_ID == approveId).FirstOrDefault();
            if (m != null)
                approveName = m.FNAME + " " + m.LNAME;

            return approveName;
        }
        private string GetMarkerName(STNEntities2 aSTNE, decimal? markerId)
        {
            string markName = string.Empty;
            MARKER ma = aSTNE.MARKERS.Where(x => x.MARKER_ID == markerId).FirstOrDefault();
            if (ma != null)
                markName = ma.MARKER1;

            return markName;
        }
        private string GetNotes(STNEntities2 aSTNE, string notes)
        {
            string n = string.Empty;

            if (notes.Length > 100)
            {
                n = notes.Substring(0, 100);
            }
            else
            {
                n = notes;
            }

            return n;
        }
        private string GetFlagTeam(STNEntities2 aSTNE, decimal? flagId)
        {
            string teamName = string.Empty;
            COLLECT_TEAM ct = aSTNE.COLLECT_TEAM.Where(x => x.COLLECT_TEAM_ID == flagId).FirstOrDefault();
            if (ct != null)
                teamName = ct.DESCRIPTION;

            return teamName;
        }
        private string GetSurveyTeam(STNEntities2 aSTNE, decimal? surveyId)
        {
            string teamName = string.Empty;
            COLLECT_TEAM ct = aSTNE.COLLECT_TEAM.Where(x => x.COLLECT_TEAM_ID == surveyId).FirstOrDefault();
            if (ct != null)
                teamName = ct.DESCRIPTION;

            return teamName;
        }

        #endregion hwmDownloadable calls

        #endregion
    }//end class HWMHandler

}//end namespace