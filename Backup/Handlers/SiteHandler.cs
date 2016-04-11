//------------------------------------------------------------------------------
//----- SiteHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 01.31.13 - JKN - added getsites(eventId, sensorTypeId, deploymenttypeId) method
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.03.12 - JKN - Role authorization, and moved context to base class
// 05.29.12 - JKN - Added connection string, Added Delete method
// 03.13.12 - JB - Added site detail method
// 02.17.12 - JB - Turned on pass through authentication to DB
// 02.16.12 - JB - Added required fields check
// 02.07.12 - JB - Switched to site_id as primary identifier
// 01.19.12 - JB - Created
#endregion


using STNServices2.Resources;
using STNServices2.Authentication;
using STNServices2.Utilities;

using OpenRasta.Web;
using OpenRasta.Security;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Runtime.InteropServices;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Text.RegularExpressions;


namespace STNServices2.Handlers
{
    public class SiteHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SITES"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllSites")]
        public OperationResult GetAllSites()
        {
            List<SITE> allSites = new List<SITE>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    allSites = aSTNE.SITES.ToList();

                    if (allSites != null)
                        allSites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using

                return new OperationResult.OK { ResponseResource = allSites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDetailSites")]
        public OperationResult GetDetailSites()
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.SITES.AsEnumerable().Select(
                        site => new DetailSite
                        {
                            SITE_ID = Convert.ToInt32(site.SITE_ID),
                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                            siteDetail = site,
                            hWMDetail = aSTNE.HWMs.AsEnumerable()
                                .Where(hwm => hwm.SITE_ID == site.SITE_ID)
                                .ToList<HWM>()
                        }
                    ).ToList<SiteBase>();

                    if (sites != null)
                        sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using 

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetPoints")]
        public OperationResult GetPoints()
        {
            SiteList sites = new SiteList();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.SITES.AsEnumerable().Select(
                        site => new SitePoint { 
                            SITE_ID = Convert.ToInt32(site.SITE_ID), 
                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "", 
                            latitude = site.LATITUDE_DD, 
                            longitude = site.LONGITUDE_DD 
                        }
                    ).ToList<SiteBase>();
                }

                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(ForUriName = "GetHWMLocationSites")]
        public OperationResult GetHWMLocationSites()
        {
            List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //Site with no sensor deployments AND
                    //Site with no proposed sensors AND 
                    //(Site does not have permanent housing installed OR
                    //  Site has had HWM collected at it in the past OR
                    //  Site has “Sensor not appropriate” box checked)

                    IQueryable<SITE> query = aSTNE.SITES.Where(s => !s.INSTRUMENTs.Any() &&
                            ((s.IS_PERMANENT_HOUSING_INSTALLED == "No" || s.IS_PERMANENT_HOUSING_INSTALLED == null) || s.HWMs.Any() || s.SENSOR_NOT_APPROPRIATE == 1));


                    sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
                    {
                        SITE_ID = site.SITE_ID,
                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                        Description = site.SITE_DESCRIPTION,
                        County = site.COUNTY,
                        State = site.STATE,
                        Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
                        RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.OBJECTIVE_POINT_ID).FirstOrDefault(),
                        Events = site.INSTRUMENTs.Where(i => i.EVENT_ID != null && i.DATA_FILE.Count() > 0).Select(e => e.EVENT).Distinct().ToList<EVENT>()
                    }).ToList<SiteLocationQuery>();
                }

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(ForUriName = "GetSensorLocationSites")]
        public OperationResult GetSensorLocationSites()
        {
            List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //Site with previous sensor deployment  
                    //OR Sites with housing types 1-4 indicated //OR Sites with permanent housing installed 
                    //OR Site with proposed sensor indicated of any type

                    IQueryable<SITE> query = aSTNE.SITES.Where(s => s.IS_PERMANENT_HOUSING_INSTALLED == "Yes" ||
                            s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID > 0 && h.HOUSING_TYPE_ID < 5) || s.INSTRUMENTs.Any());


                    sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
                    {
                        SITE_ID = site.SITE_ID,
                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                        County = site.COUNTY,
                        State = site.STATE,
                        Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
                        RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
                        Events = site.INSTRUMENTs.Where(i => i.EVENT_ID != null && i.DATA_FILE.Count() > 0).Select(e => e.EVENT).Distinct().ToList<EVENT>()
                    }).ToList<SiteLocationQuery>();
                }

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(ForUriName = "GetRDGLocationSites")]
        public OperationResult GetRDGLocationSites()
        {
            List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //Site with previous RDG sensor type deployed
                    //OR Site with RDG housing type listed (type 5) 
                    //OR Site with RDG checked as a proposed sensor

                    IQueryable<SITE> query = aSTNE.SITES.Where(s => s.INSTRUMENTs.Any(inst => inst.SENSOR_TYPE_ID == 5) ||
                            s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID == 5));


                    sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
                    {
                        SITE_ID = site.SITE_ID,
                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                        County = site.COUNTY,
                        State = site.STATE,
                        Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
                        RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
                        Events = aSTNE.EVENTS.Where(e => e.HWMs.Any(h => h.SITE_ID == site.SITE_ID) || e.INSTRUMENTs.Any(inst => inst.SITE_ID == site.SITE_ID)).ToList()
                    }).ToList<SiteLocationQuery>();
                }

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }


        [HttpOperation(ForUriName = "GetFilteredSites")]
        public OperationResult GetFilteredSites([Optional]Int32 eventId, [Optional] string stateNames, [Optional] Int32 sensorTypeId, [Optional] Int32 opDefined, [Optional] Int32 networkNameId, [Optional] Int32 hwmOnlySites, [Optional] Int32 sensorOnlySites, [Optional] Int32 rdgOnlySites)
        {
            try
            {
                List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
                
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (!string.IsNullOrEmpty(stateNames))
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 filterEvent = (eventId > 0) ? eventId : -1;
                Int32 filterSensorType = (sensorTypeId > 0) ? sensorTypeId : -1;
                Int32 filternetworkname = (networkNameId > 0) ? networkNameId : -1;
                Boolean OPhasBeenDefined = opDefined > 0 ? true : false;
                Boolean hwmOnly = hwmOnlySites > 0 ? true : false;
                Boolean sensorOnly = sensorOnlySites > 0 ? true : false;
                Boolean rdgOnly = rdgOnlySites > 0 ? true : false;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<SITE> query;

                    //query = aSTNE.SITES.Where(s => s.SITE_ID > 0);
                    query = aSTNE.SITES;

                    if (filterEvent > 0)
                        query = query.Where(s => s.INSTRUMENTs.Any(i => i.EVENT_ID == filterEvent) || s.HWMs.Any(h => h.EVENT_ID == filterEvent));

                    if (states.Count >= 2)
                    {
                        //multiple STATES
                        query = from q in query where states.Any(s => q.STATE.Contains(s.Trim())) select q;
                    }
                    if (states.Count == 1)
                    {
                        string thisState = states[0];
                        thisState = GetStateByName(thisState).ToString();
                        query = query.Where(r => string.Equals(r.STATE.ToUpper(), thisState.ToUpper()));
                    }
                    if (OPhasBeenDefined)
                        query = query.Where(s => s.OBJECTIVE_POINT.Any());

                    if (filterSensorType > 0)
                        query = query.Where(s => s.INSTRUMENTs.Any(i => i.SENSOR_TYPE_ID == filterSensorType));

                    if (filternetworkname > 0)
                        query = query.Where(s => s.NETWORK_NAME_SITE.Any(i => i.NETWORK_NAME_ID == filternetworkname));


                    if (hwmOnly)
                    {
                        //Site with no sensor deployments AND Site with no proposed sensors AND 
                        //(Site does not have permanent housing installed OR Site has had HWM collected at it in the past OR Site has “Sensor not appropriate” box checked)
                        query = query.Where(s => !s.INSTRUMENTs.Any() && ((s.IS_PERMANENT_HOUSING_INSTALLED == "No" || s.IS_PERMANENT_HOUSING_INSTALLED == null) || s.HWMs.Any() || s.SENSOR_NOT_APPROPRIATE == 1));
                    }

                    if (sensorOnly)
                    {
                        //Site with previous sensor deployment OR Sites with housing types 1-4 indicated //OR Sites with permanent housing installed 
                        //OR Site with proposed sensor indicated of any type
                        query = query.Where(s => s.IS_PERMANENT_HOUSING_INSTALLED == "Yes" || s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID > 0 && h.HOUSING_TYPE_ID < 5) || s.INSTRUMENTs.Any());
                    }

                    if (rdgOnly)
                    {
                        //Site with previous RDG sensor type deployed OR Site with RDG housing type listed (type 5) OR Site with RDG checked as a proposed sensor
                        query = query.Where(s => s.INSTRUMENTs.Any(inst => inst.SENSOR_TYPE_ID == 5) || s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID == 5));
                    }

                    sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
                            {
                                SITE_ID = site.SITE_ID,
                                Description = site.SITE_DESCRIPTION,
                                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                latitude = site.LATITUDE_DD,
                                longitude = site.LONGITUDE_DD,
                                County = site.COUNTY,
                                State = site.STATE,
                                Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
                                RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
                                Events = aSTNE.EVENTS.Where(e => e.HWMs.Any(h => h.SITE_ID == site.SITE_ID) || e.INSTRUMENTs.Any(inst => inst.SITE_ID == site.SITE_ID)).ToList()
                            }).ToList<SiteLocationQuery>();
                }//end using

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

        }//end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetSitesByStateName(string stateName)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.SITES.Where(s => string.Equals(s.STATE.ToUpper(),
                                                    stateName.ToUpper())).AsEnumerable()
                                                .Select(
                                                site => new SimpleSite
                                                {
                                                    SITE_ID = Convert.ToInt32(site.SITE_ID),
                                                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : ""
                                                }
                                                ).ToList<SiteBase>();

                    if (sites != null)
                        sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetSites(Int32 eventId, Int32 sensorTypeId, Int32 deploymentTypeId)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.SITES.Where(s => s.INSTRUMENTs.Any(ins => ins.SENSOR_TYPE_ID == sensorTypeId &&
                                                                                ins.DEPLOYMENT_TYPE_ID == deploymentTypeId &&
                                                                                ins.EVENT_ID == eventId))
                                                .AsEnumerable()
                                                .Select(
                                                site => new SimpleSite
                                                {
                                                    SITE_ID = Convert.ToInt32(site.SITE_ID),
                                                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : ""
                                                }
                                                ).ToList<SiteBase>();

                    if (sites != null)
                        sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


                }// end using

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetSitesByLatLong")]
        public OperationResult GetSitesByLatLong(decimal latitude, decimal longitude, [Optional]decimal buffer)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.SITES.Where(s => (s.LATITUDE_DD >= latitude - buffer && s.LATITUDE_DD <= latitude + buffer) &&
                                                            (s.LONGITUDE_DD >= longitude - buffer && s.LONGITUDE_DD <= longitude + buffer))
                                                .AsEnumerable()
                                                .Select(
                                                site => new SitePoint
                                                {
                                                    SITE_ID = Convert.ToInt32(site.SITE_ID),
                                                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                                    latitude = site.LATITUDE_DD,
                                                    longitude = site.LONGITUDE_DD
                                                }).ToList<SiteBase>();

                    if (sites != null)
                        sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            SITE aSite;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aSite = aSTNE.SITES.SingleOrDefault(
                                        site => site.SITE_ID == entityId);

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOPSite")]
        public OperationResult GetOPSite(Int32 objectivePointId)
        {
            SITE aSite;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {

                    aSite = aSTNE.OBJECTIVE_POINT.FirstOrDefault(
                                        rp => rp.OBJECTIVE_POINT_ID == objectivePointId).SITE;

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentSite")]
        public OperationResult GetInstrumentSite(Int32 instrumentId)
        {
            SITE aSite;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aSite = aSTNE.INSTRUMENTs.FirstOrDefault(
                                        i => i.INSTRUMENT_ID == instrumentId).SITE;

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "getHWMSite")]
        public OperationResult getHWMSite(Int32 hwmId)
        {
            SITE aSite;

            //Return BadRequest if there is no ID
            if (hwmId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aSite = aSTNE.HWMs.FirstOrDefault(
                                        h => h.HWM_ID == hwmId).SITE;

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "getNetworkTypeSites")]
        public OperationResult getNetworkTypeSites(Int32 networkTypeId)
        {
            SiteList sites = new SiteList();

            //Return BadRequest if there is no ID
            if (networkTypeId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //give me all the sites that have this network type sites = from all sites ==> look at networktypesite table, where siteId 
                    //matches and network type id matches
                    sites.Sites = aSTNE.SITES.AsEnumerable().Where(s => s.NETWORK_TYPE_SITE.Any(i => i.NETWORK_TYPE_ID == networkTypeId))
                                                            .Select(site => new SitePoint
                                                            {
                                                                SITE_ID = Convert.ToInt32(site.SITE_ID),
                                                                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                                                latitude = site.LATITUDE_DD,
                                                                longitude = site.LONGITUDE_DD
                                                            }).ToList<SiteBase>();
                }

                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "getNetworkNameSites")]
        public OperationResult getNetworkNameSites(Int32 networkNameId)
        {
            SiteList sites = new SiteList();

            //Return BadRequest if there is no ID
            if (networkNameId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //give me all the sites that have this network type sites = from all sites ==> look at networktypesite table, where siteId 
                    //matches and network type id matches
                    sites.Sites = aSTNE.SITES.AsEnumerable().Where(s => s.NETWORK_NAME_SITE.Any(i => i.NETWORK_NAME_ID == networkNameId))
                                                            .Select(site => new SitePoint
                                                            {
                                                                SITE_ID = Convert.ToInt32(site.SITE_ID),
                                                                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                                                latitude = site.LATITUDE_DD,
                                                                longitude = site.LONGITUDE_DD
                                                            }).ToList<SiteBase>();
                }
                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(ForUriName = "GetFileSite")]
        public OperationResult GetFileSite(Int32 fileId)
        {
            SITE aSite;
            if (fileId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {

                    aSite = aSTNE.FILES.FirstOrDefault(
                                        f => f.FILE_ID == fileId).SITE;

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        //[HttpOperation(ForUriName = "GetSiteBySiteNo")]
        //public OperationResult GetSiteBySiteNo(String siteNo)
        //{
        //    SITE aSite;

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {

        //            aSite = aSTNE.SITES.SingleOrDefault(
        //                                site => site.SITE_NO.ToUpper() == siteNo.ToUpper());

        //            if (aSite != null)
        //                aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

        //        }//end using

        //        if (aSite != null)
        //        {
        //            return new OperationResult.OK { ResponseResource = aSite };
        //        }
        //        else
        //        {
        //            return new OperationResult.NotFound { };
        //        }
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET

        [HttpOperation(ForUriName = "GetSiteBySiteNo")]
        public OperationResult GetSiteBySiteNo([Optional] String siteNo, [Optional] String siteName, [Optional] String siteId)
        {
            SITE aSite = null;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (!string.IsNullOrEmpty(siteNo))
                    {
                        aSite = aSTNE.SITES.SingleOrDefault(
                                        site => site.SITE_NO.ToUpper() == siteNo.ToUpper());
                    }
                    if (!string.IsNullOrEmpty(siteName))
                    {
                        aSite = aSTNE.SITES.SingleOrDefault(
                                        site => site.SITE_NAME.ToUpper() == siteName.ToUpper());
                    }
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        Int32 sid = Convert.ToInt32(siteId);
                        aSite = aSTNE.SITES.SingleOrDefault(
                                        site => site.SITE_ID == sid);
                    }
                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                if (aSite != null)
                {
                    return new OperationResult.OK { ResponseResource = aSite };
                }
                else
                {
                    return new OperationResult.NotFound { };
                }
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetEventSites")]
        public OperationResult GetEventSites(Int32 eventId)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    IQueryable<SITE> query;
                    query = aSTNE.SITES;

                    query = query.Where(s => s.INSTRUMENTs.Any(i => i.EVENT_ID == eventId) || s.HWMs.Any(h => h.EVENT_ID == eventId));

                    sites.Sites = query.AsEnumerable().Select(site => new SitePoint
                    {
                        SITE_ID = Convert.ToInt32(site.SITE_ID),
                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                        latitude = site.LATITUDE_DD,
                        longitude = site.LONGITUDE_DD
                    }).ToList<SiteBase>();
                    
                }

                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetHDatumSites")]
        public OperationResult GetHDatumSites(Int32 hdatumId)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.HORIZONTAL_DATUMS.FirstOrDefault(hd => hd.DATUM_ID == hdatumId).SITEs
                                        .Select(site => new SitePoint
                                        {
                                            SITE_ID = Convert.ToInt32(site.SITE_ID),
                                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                            latitude = site.LATITUDE_DD,
                                            longitude = site.LONGITUDE_DD
                                        })
                                        .ToList<SiteBase>();
                }

                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetLandOwnserSites")]
        public OperationResult GetLandOwnserSites(Int32 landOwnerId)
        {
            SiteList sites = new SiteList();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sites.Sites = aSTNE.LANDOWNERCONTACTs.FirstOrDefault(l => l.LANDOWNERCONTACTID == landOwnerId).SITES
                                        .Select(site => new SitePoint
                                        {
                                            SITE_ID = Convert.ToInt32(site.SITE_ID),
                                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
                                            latitude = site.LATITUDE_DD,
                                            longitude = site.LONGITUDE_DD
                                        })
                                        .ToList<SiteBase>();
                }

                if (sites != null)
                    sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

        }//end httpMethod.GET

        #endregion

        #region PostMethods

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateSite")]
        public OperationResult Post(SITE aSite)
        {
            //Return BadRequest if missing required fields
            if (aSite.LONGITUDE_DD >= 0 || aSite.LATITUDE_DD <= 0 || aSite.HDATUM_ID <= 0 ||
                string.IsNullOrEmpty(aSite.WATERBODY) || string.IsNullOrEmpty(aSite.STATE) ||
                string.IsNullOrEmpty(aSite.COUNTY) || (!aSite.HCOLLECT_METHOD_ID.HasValue || aSite.HCOLLECT_METHOD_ID <= 0))
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
                        //update state to be the abbrv. state
                        aSite.STATE = this.GetStateByName(aSite.STATE).ToString();

                        //check if exists
                        if (!Exists(aSTNE.SITES, ref aSite))
                        {
                            aSTNE.SITES.AddObject(aSite);
                            aSTNE.SaveChanges();

                            aSite.SITE_NO = buildSiteNO(aSTNE.SITES, aSite.STATE, aSite.COUNTY, Convert.ToInt32(aSite.SITE_ID), aSite.SITE_NAME);

                            //if siteName contains historic name (with dashes) leave it as is..coming from uploader.. else assign the no to the name too
                            aSite.SITE_NAME = string.IsNullOrEmpty(aSite.SITE_NAME) ? aSite.SITE_NO : aSite.SITE_NAME;
                            aSTNE.SaveChanges();
                        }//end if

                        if (aSite != null)
                            aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aSite };
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
        public OperationResult Put(Int32 entityId, SITE aSite)
        {
            SITE updatedSite;

            //Return BadRequest if missing required fields
            if ((entityId <= 0) || (aSite.HDATUM_ID <= 0) || (String.IsNullOrEmpty(aSite.WATERBODY)))
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
                        updatedSite = aSTNE.SITES.SingleOrDefault(
                                            site => site.SITE_ID == entityId);

                        //update state to be the abbrv. state
                        aSite.STATE = this.GetStateByName(aSite.STATE).ToString();

                        if ((!string.Equals(aSite.STATE.ToUpper(), updatedSite.STATE.ToUpper())) || (!string.Equals(aSite.COUNTY.ToUpper(), updatedSite.COUNTY.ToUpper())))
                            updatedSite.SITE_NO = buildSiteNO(aSTNE.SITES, aSite.STATE, aSite.COUNTY, Convert.ToInt32(aSite.SITE_ID), aSite.SITE_NAME);

                        updatedSite.SITE_NAME = string.Equals(aSite.SITE_NAME, aSite.SITE_NO) ? updatedSite.SITE_NO : aSite.SITE_NAME;
                        updatedSite.SITE_DESCRIPTION = aSite.SITE_DESCRIPTION;
                        updatedSite.ADDRESS = aSite.ADDRESS;
                        updatedSite.CITY = aSite.CITY;
                        updatedSite.STATE = aSite.STATE;
                        updatedSite.ZIP = aSite.ZIP;
                        updatedSite.OTHER_SID = aSite.OTHER_SID;
                        updatedSite.COUNTY = aSite.COUNTY;
                        updatedSite.WATERBODY = aSite.WATERBODY;
                        updatedSite.LATITUDE_DD = aSite.LATITUDE_DD;
                        updatedSite.LONGITUDE_DD = aSite.LONGITUDE_DD;
                        updatedSite.HDATUM_ID = aSite.HDATUM_ID;
                        updatedSite.DRAINAGE_AREA_SQMI = aSite.DRAINAGE_AREA_SQMI;
                        updatedSite.LANDOWNERCONTACT_ID = aSite.LANDOWNERCONTACT_ID;
                        updatedSite.PRIORITY_ID = aSite.PRIORITY_ID;
                        updatedSite.ZONE = aSite.ZONE;
                        updatedSite.IS_PERMANENT_HOUSING_INSTALLED = aSite.IS_PERMANENT_HOUSING_INSTALLED;
                        updatedSite.USGS_SID = aSite.USGS_SID;
                        updatedSite.NOAA_SID = aSite.NOAA_SID;
                        updatedSite.HCOLLECT_METHOD_ID = aSite.HCOLLECT_METHOD_ID;
                        updatedSite.SITE_NOTES = aSite.SITE_NOTES;
                        updatedSite.SAFETY_NOTES = aSite.SAFETY_NOTES;
                        updatedSite.ACCESS_GRANTED = aSite.ACCESS_GRANTED;

                        aSTNE.SaveChanges();

                        if (aSite != null)
                            aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.PUT
        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteSite")]
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
                        //check into cascade delete 

                        //fetch the object to be updated (assuming that it exists)
                        SITE ObjectToBeDeleted = aSTNE.SITES.SingleOrDefault(c => c.SITE_ID == entityId);
                        
                        //Delete all site-related stuff too
                        //  Files, DataFiles, 
                        #region site files to delete
                        List<FILES> deleteFiles = aSTNE.FILES.Where(file => file.SITE_ID == entityId).ToList<FILES>();
                        List<DATA_FILE> deleteDFs = new List<DATA_FILE>();
                        //foreach file, get datafile if exists, and peak summary
                        foreach (FILES f in deleteFiles)
                        {
                            if (f.DATA_FILE_ID.HasValue)
                            {
                                DATA_FILE df = aSTNE.DATA_FILE.SingleOrDefault(dataf => dataf.DATA_FILE_ID == f.DATA_FILE_ID);
                                if (df.PEAK_SUMMARY_ID.HasValue)
                                {
                                    PEAK_SUMMARY pk = aSTNE.PEAK_SUMMARY.SingleOrDefault(p => p.PEAK_SUMMARY_ID == df.PEAK_SUMMARY_ID);
                                    aSTNE.PEAK_SUMMARY.DeleteObject(pk);
                                }
                                
                                aSTNE.DATA_FILE.DeleteObject(df);
                            }
                            S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                            
                            aBucket.DeleteObject(BuildFilePath(f, f.PATH));
                            aSTNE.FILES.DeleteObject(f);
                        }
                        aSTNE.SaveChanges();
                        #endregion
                       
                        //HWMs 
                        #region hwms
                        List<HWM> deleteHWMs = aSTNE.HWMs.Where(h => h.SITE_ID == entityId).ToList();
                        foreach (HWM h in deleteHWMs)
                        {
                            if (h.PEAK_SUMMARY_ID.HasValue)
                            {
                                PEAK_SUMMARY pk = aSTNE.PEAK_SUMMARY.SingleOrDefault(p => p.PEAK_SUMMARY_ID == h.PEAK_SUMMARY_ID);
                                aSTNE.PEAK_SUMMARY.DeleteObject(pk);
                            }
                            aSTNE.HWMs.DeleteObject(h);
                        }
                        aSTNE.SaveChanges();
                        #endregion
                        
                        //Sensors and Sensor statuses
                        #region sensors and sensor status
                        List<INSTRUMENT> deleteInst = aSTNE.INSTRUMENTs.Where(i => i.SITE_ID == entityId).ToList();
                        List<INSTRUMENT_STATUS> eachStat = new List<INSTRUMENT_STATUS>();
                        foreach (INSTRUMENT instr in deleteInst)
                        {
                            eachStat = aSTNE.INSTRUMENT_STATUS.Where(instrStat => instrStat.INSTRUMENT_ID == instr.INSTRUMENT_ID).ToList();
                            eachStat.ForEach(insSTAT => aSTNE.INSTRUMENT_STATUS.DeleteObject(insSTAT));
                            aSTNE.INSTRUMENTs.DeleteObject(instr);
                        }
                        aSTNE.SaveChanges();
                        #endregion

                        //Network_name_sites, Network_type_sites
                        #region Networks
                        List<NETWORK_NAME_SITE> deleteNNS = aSTNE.NETWORK_NAME_SITE.Where(nns => nns.SITE_ID == entityId).ToList();
                        deleteNNS.ForEach(nnsite => aSTNE.NETWORK_NAME_SITE.DeleteObject(nnsite));
                        List<NETWORK_TYPE_SITE> deleteNTS = aSTNE.NETWORK_TYPE_SITE.Where(nts => nts.SITE_ID == entityId).ToList();
                        deleteNTS.ForEach(ntsite => aSTNE.NETWORK_TYPE_SITE.DeleteObject(ntsite));
                        aSTNE.SaveChanges();
                        #endregion

                        //OPs and OP_Control_Identifiers, OP_measurements 
                        #region OP stuff
                        List<OBJECTIVE_POINT> deleteOPs = aSTNE.OBJECTIVE_POINT.Where(op => op.SITE_ID == entityId).ToList();
                        foreach (OBJECTIVE_POINT op in deleteOPs)
                        {
                            //get OP_CONTROL_IDENTIFIERS
                            List<OP_CONTROL_IDENTIFIER> opcontrols = aSTNE.OP_CONTROL_IDENTIFIER.Where(opci => opci.OBJECTIVE_POINT_ID == op.OBJECTIVE_POINT_ID).ToList();
                            opcontrols.ForEach(o => aSTNE.OP_CONTROL_IDENTIFIER.DeleteObject(o));
                            //get OP_MEASURMENTS
                            List<OP_MEASUREMENTS> opmeasures = aSTNE.OP_MEASUREMENTS.Where(opm => opm.OBJECTIVE_POINT_ID == op.OBJECTIVE_POINT_ID).ToList();
                            opmeasures.ForEach(opmeas => aSTNE.OP_MEASUREMENTS.DeleteObject(opmeas));
                            aSTNE.OBJECTIVE_POINT.DeleteObject(op);
                        }
                        aSTNE.SaveChanges();
                        #endregion

                        //site_housing
                        #region sitehousings
                        List<SITE_HOUSING> deleteSH = aSTNE.SITE_HOUSING.Where(sh => sh.SITE_ID == entityId).ToList();
                        deleteSH.ForEach(shouse => aSTNE.SITE_HOUSING.DeleteObject(shouse));
                        aSTNE.SaveChanges();
                        #endregion
                        
                        //lastly delete the site
                        aSTNE.SITES.DeleteObject(ObjectToBeDeleted);
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
        }//end HttpMethod.DELETE

        #endregion

        #region Helpers

        private string buildSiteNO(ObjectSet<SITE> entityRDS, string siteState, string siteCounty, Int32 siteID, string siteName)
        {
            String siteNo;
            //[a-zA-Z]{2}
            if (!string.IsNullOrEmpty(siteName) && Regex.IsMatch(siteName, @"^[a-zA-Z]{3}[-][a-zA-Z]{2}[-][a-zA-Z]{3}[-][0-9]{3}"))
            {
                string[] substring = siteName.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                siteNo = substring[1] + substring[2] + "00" + Regex.Replace(substring[3], "[^0-9.]", "");

                int incr = 0;
                while (entityRDS.FirstOrDefault(s => s.SITE_NO.ToUpper() == siteNo.ToUpper()) != null)
                {
                    siteNo += incr;
                    incr++;
                }//end while
            }
            else
            {
                //remove . and space from counties like 'st. lucie'
                siteCounty = siteCounty.Trim(new Char[] { ' ', '.' });
                siteNo = siteState + siteCounty.Substring(0, 3).ToUpper() + siteID.ToString("D5");
            }

            return siteNo;
        }
        private bool PostSiteLayer(SITE aSite)
        {
            Feature flayer;
            Features fLayers;
            AGSServiceAgent agsAgent;
            try
            {
                flayer = new Feature(new SiteLayer(aSite));

                fLayers = new Features();
                fLayers.features.Add(flayer);

                agsAgent = new AGSServiceAgent();
                return agsAgent.PostFeature(fLayers, "AddSite/GPServer/AddSite/execute", "New_SITE=");
            }
            catch (Exception)
            {
                return false;
            }
        }//end PostHWMLayer
        private bool Exists(ObjectSet<SITE> entityRDS, ref SITE anEntity)
        {
            SITE existingEntity;
            SITE thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.LATITUDE_DD == thisEntity.LATITUDE_DD &&
                                                               e.LONGITUDE_DD == thisEntity.LONGITUDE_DD &&
                                                               e.HDATUM_ID == thisEntity.HDATUM_ID &&
                                                               (string.Equals(e.WATERBODY.ToUpper(), thisEntity.WATERBODY.ToUpper())) &&
                                                               (string.Equals(e.STATE.ToUpper(), thisEntity.STATE.ToUpper())) &&
                                                               (string.Equals(e.COUNTY.ToUpper(), thisEntity.COUNTY.ToUpper())) &&
                                                               (string.Equals(e.ADDRESS.ToUpper(), thisEntity.ADDRESS.ToUpper()) || string.IsNullOrEmpty(thisEntity.ADDRESS)) &&
                                                               (string.Equals(e.ZIP.ToUpper(), thisEntity.ZIP.ToUpper()) || string.IsNullOrEmpty(thisEntity.ZIP)) &&
                                                               (string.Equals(e.OTHER_SID.ToUpper(), thisEntity.OTHER_SID.ToUpper()) || string.IsNullOrEmpty(thisEntity.OTHER_SID)) &&
                                                               (string.Equals(e.USGS_SID.ToUpper(), thisEntity.USGS_SID.ToUpper()) || string.IsNullOrEmpty(thisEntity.USGS_SID)) &&
                                                               (string.Equals(e.NOAA_SID.ToUpper(), thisEntity.NOAA_SID.ToUpper()) || string.IsNullOrEmpty(thisEntity.NOAA_SID)) &&
                                                               (e.DRAINAGE_AREA_SQMI == thisEntity.DRAINAGE_AREA_SQMI || thisEntity.DRAINAGE_AREA_SQMI <= 0 || thisEntity.DRAINAGE_AREA_SQMI == null) &&
                                                               (e.LANDOWNERCONTACT_ID == thisEntity.LANDOWNERCONTACT_ID || thisEntity.LANDOWNERCONTACT_ID <= 0 || thisEntity.LANDOWNERCONTACT_ID == null) &&
                                                               (e.HCOLLECT_METHOD_ID == thisEntity.HCOLLECT_METHOD_ID || thisEntity.HCOLLECT_METHOD_ID <= 0 || thisEntity.HCOLLECT_METHOD_ID == null));


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

        public static string GetSiteDesc(string desc)
        {
            string siteDesc = string.Empty;
            if (desc.Length > 100)
            {
                siteDesc = desc.Substring(0, 100);
            }
            else
            {
                siteDesc = desc;
            }

            return siteDesc;
        }
        public static string GetSiteNetwork(STNEntities2 aSTNE, decimal siteId)
        {
            string netName = string.Empty;
            List<NETWORK_NAME_SITE> allSiteNets = aSTNE.NETWORK_NAME_SITE.Where(x => x.SITE_ID == siteId).ToList();
            string delimiter = ", ";
            //comma separate fancy way
            if (allSiteNets.Count >= 1)
                netName = allSiteNets.Select(x => x.NETWORK_NAME.NAME).Distinct().Aggregate((x, j) => x + delimiter + j);

            return netName;
        }
        public static string GetHDatum(STNEntities2 aSTNE, decimal hdatumId)
        {
            string hd = string.Empty;
            HORIZONTAL_DATUMS hdatum = aSTNE.HORIZONTAL_DATUMS.Where(x => x.DATUM_ID == hdatumId).FirstOrDefault();
            if (hdatum != null)
                hd = hdatum.DATUM_NAME;

            return hd;
        }
        public static string GetSitePriority(STNEntities2 aSTNE, decimal priorityID)
        {
            string sitePriority = string.Empty;
            DEPLOYMENT_PRIORITY dp = aSTNE.DEPLOYMENT_PRIORITY.Where(x => x.PRIORITY_ID == priorityID).FirstOrDefault();
            if (dp != null)
                sitePriority = dp.PRIORITY_NAME;

            return sitePriority;
        }
        public static string GetHCollMethod(STNEntities2 aSTNE, decimal hcollectID)
        {
            string hcolmethod = string.Empty;
            HORIZONTAL_COLLECT_METHODS hcm = aSTNE.HORIZONTAL_COLLECT_METHODS.Where(x => x.HCOLLECT_METHOD_ID == hcollectID).FirstOrDefault();
            if (hcm != null)
                hcolmethod = hcm.HCOLLECT_METHOD;

            return hcolmethod;
        }
        public static string GetSiteNotes(string notes)
        {
            string siteNotes = string.Empty;
            if (notes.Length > 100)
            {
                siteNotes = notes.Substring(0, 100);
            }
            else
            {
                siteNotes = notes;
            }

            return siteNotes;
        }
        private string BuildFilePath(FILES uploadFile, string fileName)
        {
            try
            {
                //determine default object name
                // ../SITE/3043/ex.jpg
                List<string> objectName = new List<string>();
                objectName.Add("SITE");
                objectName.Add(uploadFile.SITE_ID.ToString());

                if (uploadFile.HWM_ID != null && uploadFile.HWM_ID > 0)
                {
                    // ../SITE/3043/HWM/7956/ex.jpg
                    objectName.Add("HWM");
                    objectName.Add(uploadFile.HWM_ID.ToString());
                }
                else if (uploadFile.DATA_FILE_ID != null && uploadFile.DATA_FILE_ID > 0)
                {
                    // ../SITE/3043/DATA_FILE/7956/ex.txt
                    objectName.Add("DATA_FILE");
                    objectName.Add(uploadFile.DATA_FILE_ID.ToString());
                }
                else if (uploadFile.INSTRUMENT_ID != null && uploadFile.INSTRUMENT_ID > 0)
                {
                    // ../SITE/3043/INSTRUMENT/7956/ex.jpg
                    objectName.Add("INSTRUMENT");
                    objectName.Add(uploadFile.INSTRUMENT_ID.ToString());
                }
                objectName.Add(fileName);

                return string.Join("/", objectName);
            }
            catch
            {
                return null;
            }
        }
        #endregion
        #endregion
    }//end class SiteHandler

}//end namespace