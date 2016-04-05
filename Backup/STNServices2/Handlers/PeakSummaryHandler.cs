//------------------------------------------------------------------------------
//----- PeakSummaryHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
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
// 01.31.13 - JKN - added GetPeakSummariesByStateName method
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.16.12 - JKN -Added GetHWMPeakSummary and GetDataFilePeakSummary GET methods
// 07.09.12 - JKN -Created

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
    public class PeakSummaryHandler : HandlerBase
    {
        //needed for peakDownloadable populating of related site
        public SITE globalPeakSite = new SITE();

        #region Properties
        public override string entityName
        {
            get { return "PEAKSUMMARIES"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<PEAK_SUMMARY> peakSummaryList = new List<PEAK_SUMMARY>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    peakSummaryList = aSTNE.PEAK_SUMMARY.OrderBy(ps => ps.PEAK_SUMMARY_ID).ToList();

                    if (peakSummaryList != null)
                        peakSummaryList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = peakSummaryList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            PEAK_SUMMARY aPeakSummary;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aPeakSummary = aSTNE.PEAK_SUMMARY.SingleOrDefault(df => df.PEAK_SUMMARY_ID == entityId);

                    if (aPeakSummary != null)
                        aPeakSummary.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = aPeakSummary };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMPeakSummary")]
        public OperationResult GetHWMPeakSummary(Int32 hwmId)
        {
            PEAK_SUMMARY PeakSummary = null;

            //Return BadRequest if there is no ID
            if (hwmId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User.IsInRole(PublicRole))
                    {
                        // return only approved list
                        PeakSummary = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == hwmId && hwm.APPROVAL_ID > 0).PEAK_SUMMARY;
                    }
                    else
                    {
                        PeakSummary = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == hwmId).PEAK_SUMMARY;
                    }

                    if (PeakSummary != null)
                        PeakSummary.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = PeakSummary };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetDataFilePeakSummary")]
        public OperationResult GetDataFilePeakSummary(Int32 dataFileId)
        {
            PEAK_SUMMARY PeakSummary = null;

            //Return BadRequest if there is no ID
            if (dataFileId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User.IsInRole(PublicRole))
                    {
                        // return only approved list
                        PeakSummary = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId && df.APPROVAL_ID > 0).PEAK_SUMMARY;
                    }
                    else
                    {
                        PeakSummary = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId).PEAK_SUMMARY;
                    }

                    if (PeakSummary != null)
                        PeakSummary.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = PeakSummary };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventPeakSummaries")]
        public OperationResult GetEventPeakSummaries(Int32 eventId)
        {
            List<PEAK_SUMMARY> peakSummaryList = new List<PEAK_SUMMARY>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    peakSummaryList = aSTNE.PEAK_SUMMARY.Where(ps => ps.HWMs.Any(hwm => hwm.EVENT_ID == eventId) || ps.DATA_FILE.Any(d => d.INSTRUMENT.EVENT_ID == eventId)).ToList();

                    if (peakSummaryList != null)
                        peakSummaryList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = peakSummaryList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSitePeakSummaries")]
        public OperationResult GetSitePeakSummaries(Int32 siteId)
        {
            List<PEAK_SUMMARY> peakSummaryList = new List<PEAK_SUMMARY>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    peakSummaryList = aSTNE.PEAK_SUMMARY.Where(ps => ps.HWMs.Any(hwm => hwm.SITE_ID == siteId) || ps.DATA_FILE.Any(d => d.INSTRUMENT.SITE_ID == siteId)).ToList();

                    if (peakSummaryList != null)
                        peakSummaryList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = peakSummaryList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetPeakSummaryViewBySite")]
        public OperationResult GetSitePeakSummarView(Int32 siteId)
        {
            List<PEAK_VIEW> peakSummaryList = new List<PEAK_VIEW>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //IQueryable<PEAK_SUMMARY> query;
                    //query = aSTNE.PEAK_SUMMARY;

                    peakSummaryList = aSTNE.PEAK_VIEW.Where(ps => ps.SITE_ID == siteId).ToList();
                    //aSTNE.PEAK_SUMMARY.Where(ps => ps.HWMs.Any(hwm => hwm.SITE_ID == siteId) || ps.DATA_FILE.Any(d => d.INSTRUMENT.SITE_ID == siteId)).ToList();
                    
                        

                    if (peakSummaryList != null)
                        peakSummaryList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = peakSummaryList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetPeakSummariesByStateName")]
        public OperationResult GetPeakSummariesByStateName(string stateName)
        {
            List<PEAK_SUMMARY> peakSummaryList = new List<PEAK_SUMMARY>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    peakSummaryList = aSTNE.PEAK_SUMMARY.Where(ps => ps.HWMs.Any(hwm => string.Equals(hwm.SITE.STATE.ToUpper(), stateName.ToUpper())) ||
                                                                        ps.DATA_FILE.Any(d => string.Equals(d.INSTRUMENT.SITE.STATE.ToUpper(), stateName.ToUpper())))
                                                                    .ToList();

                    if (peakSummaryList != null)
                        peakSummaryList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = peakSummaryList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetFilteredPeaks")]
        public OperationResult FilteredPeaks([Optional]string eventIds, [Optional] string eventTypeIDs, [Optional] Int32 eventStatusID,
                                                [Optional] string states, [Optional] string counties, [Optional] string startDate, [Optional] string endDate)
        {
            List<PeakDownloadable> peakList = new List<PeakDownloadable>();
            try
            {
                char[] delimiterChars = { ';', ',', ' ' };
                //parse the requests
                List<decimal> eventIdList = !string.IsNullOrEmpty(eventIds) ? eventIds.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<decimal> eventTypeList = !string.IsNullOrEmpty(eventTypeIDs) ? eventTypeIDs.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
                List<string> stateList = !string.IsNullOrEmpty(states) ? states.ToUpper().Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(st => GetStateByName(st).ToString()).ToList() : null;
                List<string> countyList = !string.IsNullOrEmpty(counties) ? counties.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;

                DateTime? FromDate = ValidDate(startDate);
                DateTime? ToDate = ValidDate(endDate);
                if (!ToDate.HasValue) ToDate = DateTime.Now;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<PEAK_SUMMARY> query;
                    query = aSTNE.PEAK_SUMMARY.Where(s => s.PEAK_SUMMARY_ID > 0);

                    if (eventIdList != null && eventIdList.Count > 0)
                        //query = query.Where(ps => ps.HWMs.Any(hwm => eventIdList.Contains(hwm.EVENT_ID.Value)));
                        query = query.Where(ps => (ps.HWMs.Any(hwm => eventIdList.Contains(hwm.EVENT_ID.Value)) || (ps.DATA_FILE.Any(d => eventIdList.Contains(d.INSTRUMENT.EVENT_ID.Value)))));

                    if (eventTypeList != null && eventTypeList.Count > 0)
                        query = query.Where(ps => (ps.HWMs.Any(hwm => eventTypeList.Contains(hwm.EVENT.EVENT_TYPE_ID.Value)) || (ps.DATA_FILE.Any(d => eventTypeList.Contains(d.INSTRUMENT.EVENT.EVENT_TYPE_ID.Value)))));

                    if (eventStatusID > 0)
                        query = query.Where(ps => (ps.HWMs.Any(hwm => hwm.EVENT.EVENT_STATUS_ID.Value == eventStatusID)) || (ps.DATA_FILE.Any(d => d.INSTRUMENT.EVENT.EVENT_STATUS_ID.Value == eventStatusID)));

                    if (stateList != null && stateList.Count > 0)
                        query = query.Where(ps => (ps.HWMs.Any(hwm => stateList.Contains(hwm.SITE.STATE)) || (ps.DATA_FILE.Any(d => stateList.Contains(d.INSTRUMENT.SITE.STATE)))));

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(ps => (ps.HWMs.Any(hwm => countyList.Contains(hwm.SITE.COUNTY)) || (ps.DATA_FILE.Any(d => countyList.Contains(d.INSTRUMENT.SITE.COUNTY)))));

                    if (FromDate.HasValue)
                        query = query.Where(ps => ps.PEAK_DATE >= FromDate);

                    if (ToDate.HasValue)
                        query = query.Where(ps => ps.PEAK_DATE.Value <= ToDate.Value);

                    peakList = query.AsEnumerable().Select(
                        p => new PeakDownloadable
                        {
                            PEAK_SUMMARY_ID = p.PEAK_SUMMARY_ID,
                            MEMBER_NAME = p.MEMBER_ID.HasValue ? GetMember(aSTNE, p.MEMBER_ID) : "",
                            PEAK_DATE = p.PEAK_DATE.HasValue ? p.PEAK_DATE.Value.ToShortDateString() : "",
                            IS_PEAK_DATE_ESTIMATED = p.IS_PEAK_ESTIMATED > 0 ? "Yes" : "No",
                            PEAK_TIME = p.PEAK_DATE.HasValue ? p.PEAK_DATE.Value.TimeOfDay.ToString() : "",
                            TIME_ZONE = !string.IsNullOrEmpty(p.TIME_ZONE) ? p.TIME_ZONE : "",
                            IS_PEAK_TIME_ESTIMATED = p.IS_PEAK_TIME_ESTIMATED > 0 ? "Yes" : "No",
                            PEAK_STAGE = p.PEAK_STAGE.HasValue ? p.PEAK_STAGE.Value : 0,
                            IS_PEAK_STAGE_ESTIMATED = p.IS_PEAK_STAGE_ESTIMATED > 0 ? "Yes" : "No",
                            PEAK_DISCHARGE = p.PEAK_DISCHARGE.HasValue ? p.PEAK_DISCHARGE.Value : 0,
                            IS_PEAK_DISCHARGE_ESTIMATED = p.IS_PEAK_DISCHARGE_ESTIMATED > 0 ? "Yes" : "No",
                            HEIGHT_ABOVE_GND = p.HEIGHT_ABOVE_GND.HasValue ? p.HEIGHT_ABOVE_GND.Value : 0,
                            IS_HAG_ESTIMATED = p.IS_HAG_ESTIMATED.HasValue && p.IS_HAG_ESTIMATED.Value > 0 ? "Yes" : "No",
                            AEP = p.AEP.Value,
                            AEP_LOWCI = p.AEP_LOWCI.Value,
                            AEP_UPPERCI = p.AEP_UPPERCI.Value,
                            AEP_RANGE = p.AEP_RANGE.Value,
                            CALC_NOTES = p.CALC_NOTES,
                            VERTICAL_DATUM = p.VDATUM_ID.HasValue ? GetVDatum(aSTNE, p.VDATUM_ID.Value) : "",
                            SITE_ID = GetThisPeaksSiteID(aSTNE, p.PEAK_SUMMARY_ID),
                            SITE_NO = globalPeakSite.SITE_NO,
                            LATITUDE = globalPeakSite.LATITUDE_DD,
                            LONGITUDE = globalPeakSite.LONGITUDE_DD,
                            DESCRIPTION = globalPeakSite.SITE_DESCRIPTION != null ? SiteHandler.GetSiteDesc(globalPeakSite.SITE_DESCRIPTION) : "",
                            NETWORK = SiteHandler.GetSiteNetwork(aSTNE, globalPeakSite.SITE_ID),
                            STATE = globalPeakSite.STATE,
                            COUNTY = globalPeakSite.COUNTY,
                            WATERBODY = globalPeakSite.WATERBODY,
                            HORIZONTAL_DATUM = globalPeakSite.HDATUM_ID > 0 ? SiteHandler.GetHDatum(aSTNE, globalPeakSite.HDATUM_ID) : "",
                            PRIORITY = globalPeakSite.PRIORITY_ID.HasValue ? SiteHandler.GetSitePriority(aSTNE, globalPeakSite.PRIORITY_ID.Value) : "",
                            ZONE = globalPeakSite.ZONE,
                            HORIZONTAL_COLLECT_METHOD = globalPeakSite.HCOLLECT_METHOD_ID.HasValue ? SiteHandler.GetHCollMethod(aSTNE, globalPeakSite.HCOLLECT_METHOD_ID.Value) : "",
                            PERM_HOUSING_INSTALLED = globalPeakSite.IS_PERMANENT_HOUSING_INSTALLED == null || globalPeakSite.IS_PERMANENT_HOUSING_INSTALLED == "No" ? "No" : "Yes",
                            SITE_NOTES = !string.IsNullOrEmpty(globalPeakSite.SITE_NOTES) ? SiteHandler.GetSiteNotes(globalPeakSite.SITE_NOTES) : ""

                        }
                        ).ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = peakList };
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
        [HttpOperation(HttpMethod.POST, ForUriName = "PostPeakSummary")]
        public OperationResult Post(PEAK_SUMMARY aPeakSummary)
        {
            //Return BadRequest if missing required fields
            if ((!aPeakSummary.MEMBER_ID.HasValue || aPeakSummary.MEMBER_ID <= 0))
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
                        if (!Exists(aSTNE.PEAK_SUMMARY, ref aPeakSummary))
                        {
                            aSTNE.PEAK_SUMMARY.AddObject(aPeakSummary);
                        }

                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aPeakSummary };
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
        public OperationResult Put(Int32 entityId, PEAK_SUMMARY aPeakSummary)
        {
            PEAK_SUMMARY PeakSummaryToUpdate = null;
            //Return BadRequest if missing required fields
            if (aPeakSummary.PEAK_SUMMARY_ID <= 0 || entityId <= 0)
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
                        PeakSummaryToUpdate = aSTNE.PEAK_SUMMARY.SingleOrDefault(ps => ps.PEAK_SUMMARY_ID == entityId);
                        //Update fields
                        PeakSummaryToUpdate.MEMBER_ID = aPeakSummary.MEMBER_ID;
                        PeakSummaryToUpdate.PEAK_DATE = aPeakSummary.PEAK_DATE;
                        PeakSummaryToUpdate.IS_PEAK_ESTIMATED = aPeakSummary.IS_PEAK_ESTIMATED;
                        PeakSummaryToUpdate.IS_PEAK_TIME_ESTIMATED = aPeakSummary.IS_PEAK_TIME_ESTIMATED;
                        PeakSummaryToUpdate.PEAK_STAGE = aPeakSummary.PEAK_STAGE;
                        PeakSummaryToUpdate.IS_PEAK_STAGE_ESTIMATED = aPeakSummary.IS_PEAK_STAGE_ESTIMATED;
                        PeakSummaryToUpdate.PEAK_DISCHARGE = aPeakSummary.PEAK_DISCHARGE;
                        PeakSummaryToUpdate.IS_PEAK_DISCHARGE_ESTIMATED = aPeakSummary.IS_PEAK_DISCHARGE_ESTIMATED;
                        PeakSummaryToUpdate.VDATUM_ID = aPeakSummary.VDATUM_ID;
                        PeakSummaryToUpdate.HEIGHT_ABOVE_GND = aPeakSummary.HEIGHT_ABOVE_GND;
                        PeakSummaryToUpdate.IS_HAG_ESTIMATED = aPeakSummary.IS_HAG_ESTIMATED;
                        PeakSummaryToUpdate.AEP = aPeakSummary.AEP;
                        PeakSummaryToUpdate.AEP_LOWCI = aPeakSummary.AEP_LOWCI;
                        PeakSummaryToUpdate.AEP_UPPERCI = aPeakSummary.AEP_UPPERCI;
                        PeakSummaryToUpdate.AEP_RANGE = aPeakSummary.AEP_RANGE;
                        PeakSummaryToUpdate.CALC_NOTES = aPeakSummary.CALC_NOTES;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aPeakSummary };
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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            //Return BadRequest if missing required fields
            if (entityId <= 0)
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
                        PEAK_SUMMARY ObjectToBeDeleted = aSTNE.PEAK_SUMMARY.SingleOrDefault(ps => ps.PEAK_SUMMARY_ID == entityId);
                        //delete it
                        aSTNE.PEAK_SUMMARY.DeleteObject(ObjectToBeDeleted);

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
        private bool Exists(ObjectSet<PEAK_SUMMARY> entityRDS, ref PEAK_SUMMARY anEntity)
        {
            PEAK_SUMMARY existingEntity;
            PEAK_SUMMARY thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => (e.MEMBER_ID == thisEntity.MEMBER_ID) &&
                                                                (DateTime.Equals(e.PEAK_DATE.Value, thisEntity.PEAK_DATE.Value) || !thisEntity.PEAK_DATE.HasValue) &&
                                                                (e.IS_PEAK_ESTIMATED == thisEntity.IS_PEAK_ESTIMATED || thisEntity.IS_PEAK_ESTIMATED <= 0 || thisEntity.IS_PEAK_ESTIMATED == null) &&
                                                                (e.IS_PEAK_TIME_ESTIMATED == thisEntity.IS_PEAK_TIME_ESTIMATED || thisEntity.IS_PEAK_TIME_ESTIMATED <= 0 || thisEntity.IS_PEAK_TIME_ESTIMATED == null) &&
                                                                (e.PEAK_STAGE.Value == thisEntity.PEAK_STAGE.Value || thisEntity.PEAK_STAGE.Value <= 0 || thisEntity.PEAK_STAGE.Value == null) &&
                                                                (e.IS_PEAK_STAGE_ESTIMATED == thisEntity.IS_PEAK_STAGE_ESTIMATED || thisEntity.IS_PEAK_STAGE_ESTIMATED <= 0 || thisEntity.IS_PEAK_STAGE_ESTIMATED == null) &&
                                                                (e.PEAK_DISCHARGE.Value == thisEntity.PEAK_DISCHARGE.Value || thisEntity.PEAK_DISCHARGE.Value <= 0 || !thisEntity.PEAK_DISCHARGE.HasValue) &&
                                                                (e.IS_PEAK_DISCHARGE_ESTIMATED == thisEntity.IS_PEAK_DISCHARGE_ESTIMATED || (thisEntity.IS_PEAK_DISCHARGE_ESTIMATED <= 0 || thisEntity.IS_PEAK_DISCHARGE_ESTIMATED == null)) &&
                                                                (e.VDATUM_ID.Value == thisEntity.VDATUM_ID.Value || thisEntity.VDATUM_ID.Value <= 0 || !thisEntity.VDATUM_ID.HasValue) &&
                                                                (e.HEIGHT_ABOVE_GND.Value == thisEntity.HEIGHT_ABOVE_GND.Value || thisEntity.HEIGHT_ABOVE_GND.Value <= 0 || thisEntity.HEIGHT_ABOVE_GND.Value == null) &&
                                                                (e.IS_HAG_ESTIMATED.Value == thisEntity.IS_HAG_ESTIMATED.Value || thisEntity.IS_HAG_ESTIMATED.Value <= 0 || thisEntity.IS_HAG_ESTIMATED.Value == null) &&
                                                                (string.Equals(e.TIME_ZONE, thisEntity.TIME_ZONE) || !string.IsNullOrEmpty(thisEntity.TIME_ZONE)) &&
                                                                (e.AEP.Value == thisEntity.AEP.Value || thisEntity.AEP.Value <= 0 || !thisEntity.AEP.HasValue) &&
                                                                (e.AEP_LOWCI.Value == thisEntity.AEP_LOWCI.Value || thisEntity.AEP_LOWCI.Value <= 0 || !thisEntity.AEP_LOWCI.HasValue) &&
                                                                (e.AEP_UPPERCI.Value == thisEntity.AEP_UPPERCI.Value || thisEntity.AEP_UPPERCI.Value <= 0 || !thisEntity.AEP_UPPERCI.HasValue) &&
                                                                (e.AEP_RANGE.Value == thisEntity.AEP_RANGE.Value || thisEntity.AEP_RANGE.Value <= 0 || !thisEntity.AEP_RANGE.HasValue) &&
                                                                (string.Equals(e.CALC_NOTES.ToUpper(), thisEntity.CALC_NOTES.ToUpper())));


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

        #region PeakDownloadable calls
        private string GetMember(STNEntities2 aSTNE, decimal? memberId)
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


        #endregion PeakDownloadable calls

        #endregion
    }//end class PeakSummaryHandler
}//end namespace