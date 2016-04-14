//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
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
// 04.07.16 - TR - Created
//#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using WiM.Security;

namespace STNServices2.Handlers
{
    public class SiteHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllSites")]
        public OperationResult GetAllSites()
        {
            List<site> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<site>().ToList();
                    
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        #region siteList and sitelocationquery
        //[HttpOperation(HttpMethod.GET, ForUriName = "GetDetailSites")]
        //public OperationResult GetDetailSites()
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.SITES.AsEnumerable().Select(
        //                site => new DetailSite
        //                {
        //                    SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                    siteDetail = site,
        //                    hWMDetail = aSTNE.HWMs.AsEnumerable()
        //                        .Where(hwm => hwm.SITE_ID == site.SITE_ID)
        //                        .ToList<HWM>()
        //                }
        //            ).ToList<SiteBase>();

        //            if (sites != null)
        //                sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using 

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET

        //[HttpOperation(ForUriName = "GetPoints")]
        //public OperationResult GetPoints()
        //{
        //    SiteList sites = new SiteList();
        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.SITES.AsEnumerable().Select(
        //                site => new SitePoint { 
        //                    SITE_ID = Convert.ToInt32(site.SITE_ID), 
        //                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "", 
        //                    latitude = site.LATITUDE_DD, 
        //                    longitude = site.LONGITUDE_DD 
        //                }
        //            ).ToList<SiteBase>();
        //        }

        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };

        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}

        //[HttpOperation(ForUriName = "GetHWMLocationSites")]
        //public OperationResult GetHWMLocationSites()
        //{
        //    List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //Site with no sensor deployments AND
        //            //Site with no proposed sensors AND 
        //            //(Site does not have permanent housing installed OR
        //            //  Site has had HWM collected at it in the past OR
        //            //  Site has “Sensor not appropriate” box checked)

        //            IQueryable<SITE> query = aSTNE.SITES.Where(s => !s.INSTRUMENTs.Any() &&
        //                    ((s.IS_PERMANENT_HOUSING_INSTALLED == "No" || s.IS_PERMANENT_HOUSING_INSTALLED == null) || s.HWMs.Any() || s.SENSOR_NOT_APPROPRIATE == 1));


        //            sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
        //            {
        //                SITE_ID = site.SITE_ID,
        //                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                Description = site.SITE_DESCRIPTION,
        //                County = site.COUNTY,
        //                State = site.STATE,
        //                Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
        //                RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.OBJECTIVE_POINT_ID).FirstOrDefault(),
        //                Events = site.INSTRUMENTs.Where(i => i.EVENT_ID != null && i.DATA_FILE.Count() > 0).Select(e => e.EVENT).Distinct().ToList<EVENT>()
        //            }).ToList<SiteLocationQuery>();
        //        }

        //        return new OperationResult.OK { ResponseResource = sites };

        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}

        //[HttpOperation(ForUriName = "GetSensorLocationSites")]
        //public OperationResult GetSensorLocationSites()
        //{
        //    List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //Site with previous sensor deployment  
        //            //OR Sites with housing types 1-4 indicated //OR Sites with permanent housing installed 
        //            //OR Site with proposed sensor indicated of any type

        //            IQueryable<SITE> query = aSTNE.SITES.Where(s => s.IS_PERMANENT_HOUSING_INSTALLED == "Yes" ||
        //                    s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID > 0 && h.HOUSING_TYPE_ID < 5) || s.INSTRUMENTs.Any());


        //            sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
        //            {
        //                SITE_ID = site.SITE_ID,
        //                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                County = site.COUNTY,
        //                State = site.STATE,
        //                Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
        //                RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
        //                Events = site.INSTRUMENTs.Where(i => i.EVENT_ID != null && i.DATA_FILE.Count() > 0).Select(e => e.EVENT).Distinct().ToList<EVENT>()
        //            }).ToList<SiteLocationQuery>();
        //        }

        //        return new OperationResult.OK { ResponseResource = sites };

        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}

        //[HttpOperation(ForUriName = "GetRDGLocationSites")]
        //public OperationResult GetRDGLocationSites()
        //{
        //    List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //Site with previous RDG sensor type deployed
        //            //OR Site with RDG housing type listed (type 5) 
        //            //OR Site with RDG checked as a proposed sensor

        //            IQueryable<SITE> query = aSTNE.SITES.Where(s => s.INSTRUMENTs.Any(inst => inst.SENSOR_TYPE_ID == 5) ||
        //                    s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID == 5));


        //            sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
        //            {
        //                SITE_ID = site.SITE_ID,
        //                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                County = site.COUNTY,
        //                State = site.STATE,
        //                Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
        //                RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
        //                Events = aSTNE.EVENTS.Where(e => e.HWMs.Any(h => h.SITE_ID == site.SITE_ID) || e.INSTRUMENTs.Any(inst => inst.SITE_ID == site.SITE_ID)).ToList()
        //            }).ToList<SiteLocationQuery>();
        //        }

        //        return new OperationResult.OK { ResponseResource = sites };

        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}

        //[HttpOperation(ForUriName = "GetFilteredSites")]
        //public OperationResult GetFilteredSites([Optional]Int32 eventId, [Optional] string stateNames, [Optional] Int32 sensorTypeId, [Optional] Int32 opDefined, [Optional] Int32 networkNameId, [Optional] Int32 hwmOnlySites, [Optional] Int32 sensorOnlySites, [Optional] Int32 rdgOnlySites)
        //{
        //    try
        //    {
        //        List<SiteLocationQuery> sites = new List<SiteLocationQuery>();
                
        //        List<string> states = new List<string>();
        //        char[] delimiter = { ',' };
        //        //stateNames will be a list that is comma separated. Need to parse out
        //        if (!string.IsNullOrEmpty(stateNames))
        //        {
        //            states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
        //        }

        //        Int32 filterEvent = (eventId > 0) ? eventId : -1;
        //        Int32 filterSensorType = (sensorTypeId > 0) ? sensorTypeId : -1;
        //        Int32 filternetworkname = (networkNameId > 0) ? networkNameId : -1;
        //        Boolean OPhasBeenDefined = opDefined > 0 ? true : false;
        //        Boolean hwmOnly = hwmOnlySites > 0 ? true : false;
        //        Boolean sensorOnly = sensorOnlySites > 0 ? true : false;
        //        Boolean rdgOnly = rdgOnlySites > 0 ? true : false;

        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<SITE> query;

        //            //query = aSTNE.SITES.Where(s => s.SITE_ID > 0);
        //            query = aSTNE.SITES;

        //            if (filterEvent > 0)
        //                query = query.Where(s => s.INSTRUMENTs.Any(i => i.EVENT_ID == filterEvent) || s.HWMs.Any(h => h.EVENT_ID == filterEvent));

        //            if (states.Count >= 2)
        //            {
        //                //multiple STATES
        //                query = from q in query where states.Any(s => q.STATE.Contains(s.Trim())) select q;
        //            }
        //            if (states.Count == 1)
        //            {
        //                string thisState = states[0];
        //                thisState = GetStateByName(thisState).ToString();
        //                query = query.Where(r => string.Equals(r.STATE.ToUpper(), thisState.ToUpper()));
        //            }
        //            if (OPhasBeenDefined)
        //                query = query.Where(s => s.OBJECTIVE_POINT.Any());

        //            if (filterSensorType > 0)
        //                query = query.Where(s => s.INSTRUMENTs.Any(i => i.SENSOR_TYPE_ID == filterSensorType));

        //            if (filternetworkname > 0)
        //                query = query.Where(s => s.NETWORK_NAME_SITE.Any(i => i.NETWORK_NAME_ID == filternetworkname));


        //            if (hwmOnly)
        //            {
        //                //Site with no sensor deployments AND Site with no proposed sensors AND 
        //                //(Site does not have permanent housing installed OR Site has had HWM collected at it in the past OR Site has “Sensor not appropriate” box checked)
        //                query = query.Where(s => !s.INSTRUMENTs.Any() && ((s.IS_PERMANENT_HOUSING_INSTALLED == "No" || s.IS_PERMANENT_HOUSING_INSTALLED == null) || s.HWMs.Any() || s.SENSOR_NOT_APPROPRIATE == 1));
        //            }

        //            if (sensorOnly)
        //            {
        //                //Site with previous sensor deployment OR Sites with housing types 1-4 indicated //OR Sites with permanent housing installed 
        //                //OR Site with proposed sensor indicated of any type
        //                query = query.Where(s => s.IS_PERMANENT_HOUSING_INSTALLED == "Yes" || s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID > 0 && h.HOUSING_TYPE_ID < 5) || s.INSTRUMENTs.Any());
        //            }

        //            if (rdgOnly)
        //            {
        //                //Site with previous RDG sensor type deployed OR Site with RDG housing type listed (type 5) OR Site with RDG checked as a proposed sensor
        //                query = query.Where(s => s.INSTRUMENTs.Any(inst => inst.SENSOR_TYPE_ID == 5) || s.SITE_HOUSING.Any(h => h.HOUSING_TYPE_ID == 5));
        //            }

        //            sites = query.Distinct().AsEnumerable().Select(site => new SiteLocationQuery
        //                    {
        //                        SITE_ID = site.SITE_ID,
        //                        Description = site.SITE_DESCRIPTION,
        //                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                        latitude = site.LATITUDE_DD,
        //                        longitude = site.LONGITUDE_DD,
        //                        County = site.COUNTY,
        //                        State = site.STATE,
        //                        Networks = site.NETWORK_NAME_SITE.Select(n => n.NETWORK_NAME).ToList<NETWORK_NAME>(),
        //                        RecentOP = site.OBJECTIVE_POINT.OrderByDescending(x => x.DATE_ESTABLISHED).FirstOrDefault(),
        //                        Events = aSTNE.EVENTS.Where(e => e.HWMs.Any(h => h.SITE_ID == site.SITE_ID) || e.INSTRUMENTs.Any(inst => inst.SITE_ID == site.SITE_ID)).ToList()
        //                    }).ToList<SiteLocationQuery>();
        //        }//end using

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //}//end HttpMethod.Get

        //[HttpOperation(HttpMethod.GET)]
        //public OperationResult GetSitesByStateName(string stateName)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.SITES.Where(s => string.Equals(s.STATE.ToUpper(),
        //                                            stateName.ToUpper())).AsEnumerable()
        //                                        .Select(
        //                                        site => new SimpleSite
        //                                        {
        //                                            SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : ""
        //                                        }
        //                                        ).ToList<SiteBase>();

        //            if (sites != null)
        //                sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }// end using

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET

        //[HttpOperation(HttpMethod.GET)]
        //public OperationResult GetSites(Int32 eventId, Int32 sensorTypeId, Int32 deploymentTypeId)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.SITES.Where(s => s.INSTRUMENTs.Any(ins => ins.SENSOR_TYPE_ID == sensorTypeId &&
        //                                                                        ins.DEPLOYMENT_TYPE_ID == deploymentTypeId &&
        //                                                                        ins.EVENT_ID == eventId))
        //                                        .AsEnumerable()
        //                                        .Select(
        //                                        site => new SimpleSite
        //                                        {
        //                                            SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : ""
        //                                        }
        //                                        ).ToList<SiteBase>();

        //            if (sites != null)
        //                sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


        //        }// end using

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET

        //[HttpOperation(ForUriName = "GetSitesByLatLong")]
        //public OperationResult GetSitesByLatLong(decimal latitude, decimal longitude, [Optional]decimal buffer)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.SITES.Where(s => (s.LATITUDE_DD >= latitude - buffer && s.LATITUDE_DD <= latitude + buffer) &&
        //                                                    (s.LONGITUDE_DD >= longitude - buffer && s.LONGITUDE_DD <= longitude + buffer))
        //                                        .AsEnumerable()
        //                                        .Select(
        //                                        site => new SitePoint
        //                                        {
        //                                            SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                            SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                                            latitude = site.LATITUDE_DD,
        //                                            longitude = site.LONGITUDE_DD
        //                                        }).ToList<SiteBase>();

        //            if (sites != null)
        //                sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }// end using

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET
 
        //[HttpOperation(HttpMethod.GET, ForUriName = "getNetworkTypeSites")]
        //public OperationResult getNetworkTypeSites(Int32 networkTypeId)
        //{
        //    SiteList sites = new SiteList();

        //    //Return BadRequest if there is no ID
        //    if (networkTypeId <= 0)
        //    { return new OperationResult.BadRequest(); }

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //give me all the sites that have this network type sites = from all sites ==> look at networktypesite table, where siteId 
        //            //matches and network type id matches
        //            sites.Sites = aSTNE.SITES.AsEnumerable().Where(s => s.NETWORK_TYPE_SITE.Any(i => i.NETWORK_TYPE_ID == networkTypeId))
        //                                                    .Select(site => new SitePoint
        //                                                    {
        //                                                        SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                                                        latitude = site.LATITUDE_DD,
        //                                                        longitude = site.LONGITUDE_DD
        //                                                    }).ToList<SiteBase>();
        //        }

        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}

        //[HttpOperation(HttpMethod.GET, ForUriName = "getNetworkNameSites")]
        //public OperationResult getNetworkNameSites(Int32 networkNameId)
        //{
        //    SiteList sites = new SiteList();

        //    //Return BadRequest if there is no ID
        //    if (networkNameId <= 0)
        //    { return new OperationResult.BadRequest(); }

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            //give me all the sites that have this network type sites = from all sites ==> look at networktypesite table, where siteId 
        //            //matches and network type id matches
        //            sites.Sites = aSTNE.SITES.AsEnumerable().Where(s => s.NETWORK_NAME_SITE.Any(i => i.NETWORK_NAME_ID == networkNameId))
        //                                                    .Select(site => new SitePoint
        //                                                    {
        //                                                        SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                                        SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                                                        latitude = site.LATITUDE_DD,
        //                                                        longitude = site.LONGITUDE_DD
        //                                                    }).ToList<SiteBase>();
        //        }
        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}
 
        //[HttpOperation(ForUriName = "GetEventSites")]
        //public OperationResult GetEventSites(Int32 eventId)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            IQueryable<SITE> query;
        //            query = aSTNE.SITES;

        //            query = query.Where(s => s.INSTRUMENTs.Any(i => i.EVENT_ID == eventId) || s.HWMs.Any(h => h.EVENT_ID == eventId));

        //            sites.Sites = query.AsEnumerable().Select(site => new SitePoint
        //            {
        //                SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                latitude = site.LATITUDE_DD,
        //                longitude = site.LONGITUDE_DD
        //            }).ToList<SiteBase>();
                    
        //        }

        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };

        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end httpMethod.GET
 
        //[HttpOperation(ForUriName = "GetHDatumSites")]
        //public OperationResult GetHDatumSites(Int32 hdatumId)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.HORIZONTAL_DATUMS.FirstOrDefault(hd => hd.DATUM_ID == hdatumId).SITEs
        //                                .Select(site => new SitePoint
        //                                {
        //                                    SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                                    latitude = site.LATITUDE_DD,
        //                                    longitude = site.LONGITUDE_DD
        //                                })
        //                                .ToList<SiteBase>();
        //        }

        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //}//end httpMethod.GET
 
        //[HttpOperation(ForUriName = "GetLandOwnserSites")]
        //public OperationResult GetLandOwnserSites(Int32 landOwnerId)
        //{
        //    SiteList sites = new SiteList();

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            sites.Sites = aSTNE.LANDOWNERCONTACTs.FirstOrDefault(l => l.LANDOWNERCONTACTID == landOwnerId).SITES
        //                                .Select(site => new SitePoint
        //                                {
        //                                    SITE_ID = Convert.ToInt32(site.SITE_ID),
        //                                    SITE_NO = (site.SITE_NO != null) ? site.SITE_NO : "",
        //                                    latitude = site.LATITUDE_DD,
        //                                    longitude = site.LONGITUDE_DD
        //                                })
        //                                .ToList<SiteBase>();
        //        }

        //        if (sites != null)
        //            sites.Sites.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        return new OperationResult.OK { ResponseResource = sites };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //}//end httpMethod.GET

        #endregion siteList and sitelocationquery
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            site anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<site>().FirstOrDefault(e => e.site_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOPSite")]
        public OperationResult GetOPSite(Int32 objectivePointId)
        {
            site anEntity = null;
            
            try
            {
                if (objectivePointId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<objective_point>().FirstOrDefault(op => op.objective_point_id == objectivePointId).site;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentSite")]
        public OperationResult GetInstrumentSite(Int32 instrumentId)
        {
            site anEntity = null;
            
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument>().FirstOrDefault(i => i.instrument_id == instrumentId).site;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "getHWMSite")]
        public OperationResult getHWMSite(Int32 hwmId)
        {
            site anEntity = null;

            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<hwm>().FirstOrDefault(h => h.hwm_id == hwmId).site;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
           
        [HttpOperation(ForUriName = "GetFileSite")]
        public OperationResult GetFileSite(Int32 fileId)
        {
            site anEntity = null;            

            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).site;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(ForUriName = "GetSiteBySiteNo")]
        public OperationResult GetSiteBySiteNo([Optional] String siteNo, [Optional] String siteName, [Optional] String siteId)
        {
            site anEntity = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    //only one will be provided
                    if (!string.IsNullOrEmpty(siteNo))
                    {
                        anEntity = sa.Select<site>().SingleOrDefault(s => string.Equals(s.site_no, siteNo, StringComparison.OrdinalIgnoreCase));
                        if (anEntity == null) throw new NotFoundRequestException(); 
                        sm(sa.Messages);
                    }
                    if (!string.IsNullOrEmpty(siteName))
                    {
                        anEntity = sa.Select<site>().SingleOrDefault(s => string.Equals(s.site_name, siteName, StringComparison.OrdinalIgnoreCase));
                        if (anEntity == null) throw new NotFoundRequestException(); 
                        sm(sa.Messages);
                    }
                    if (!string.IsNullOrEmpty(siteId))
                    {
                        Int32 sid = Convert.ToInt32(siteId);
                        anEntity = sa.Select<site>().SingleOrDefault(s => s.site_id == sid);
                        if (anEntity == null) throw new NotFoundRequestException(); 
                        sm(sa.Messages);
                    }

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }                
        }//end httpMethod.GET             
        #endregion

        #region PostMethods

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(site anEntity)
        {           
            try
            {
                if (string.IsNullOrEmpty(anEntity.site_description) || anEntity.latitude_dd <= 0 || anEntity.longitude_dd >= 0 || anEntity.hdatum_id <= 0 ||
                    anEntity.hcollect_method_id <= 0 || string.IsNullOrEmpty(anEntity.state) || string.IsNullOrEmpty(anEntity.county) ||
                    string.IsNullOrEmpty(anEntity.waterbody) || (anEntity.member_id <= 0))
                { 
                    throw new BadRequestException("Invalid input parameters"); 
                }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //aSite.STATE = this.GetStateByName(aSite.STATE).ToString();
                        anEntity = sa.Add<site>(anEntity);                       

                        anEntity.site_no = buildSiteNO(sa, anEntity.state, anEntity.county, Convert.ToInt32(anEntity.site_id), anEntity.site_name);
                        //if siteName contains historic name (with dashes) leave it as is..coming from uploader.. else assign the no to the name too
                        anEntity.site_name = string.IsNullOrEmpty(anEntity.site_name) ? anEntity.site_no : anEntity.site_name;
                        
                        //now hit the update to save this
                        anEntity = sa.Update<site>(anEntity);
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
        public OperationResult Put(Int32 entityId, site anEntity)
        {
            site updatedSite;
            
            try
            {
                if (entityId <= 0 || string.IsNullOrEmpty(anEntity.site_description) || anEntity.latitude_dd <= 0 || anEntity.longitude_dd >= 0 || 
                    anEntity.hdatum_id <= 0 || anEntity.hcollect_method_id <= 0 || string.IsNullOrEmpty(anEntity.state) || 
                    string.IsNullOrEmpty(anEntity.county) || string.IsNullOrEmpty(anEntity.waterbody) || (anEntity.member_id <= 0))
                {
                    throw new BadRequestException("Invalid input parameters");
                }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        updatedSite = sa.Select<site>().SingleOrDefault(s => s.site_id == entityId);
                        updatedSite.site_description = "testing Here";
                        ////update the site_no if it was changed during edit
                        //if ((!string.Equals(anEntity.state, updatedSite.state, StringComparison.OrdinalIgnoreCase)) || 
                        //    (!string.Equals(anEntity.county, updatedSite.county, StringComparison.OrdinalIgnoreCase)))
                        //    anEntity.site_no = buildSiteNO(sa, anEntity.state, anEntity.county, Convert.ToInt32(anEntity.site_id), anEntity.site_name);
                        
                        //update state to be the abbrv. state
                        //aSite.STATE = this.GetStateByName(aSite.STATE).ToString();

                        anEntity = sa.Update<site>(updatedSite);
                        sm(sa.Messages);                        
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
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
            site anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<site>().FirstOrDefault(i => i.site_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        //Delete all site-related stuff too
                        //  Files, DataFiles, 
                        #region site files to delete
                        //List<file> deleteFiles = sa.Select<file>().Where(f => f.site_id == entityId).ToList();
                        //List<data_file> deleteDFs = null;
                        ////foreach file, get datafile if exists, and peak summary
                        //foreach (file f in deleteFiles)
                        //{
                        //    if (f.data_file_id.HasValue)
                        //    {
                        //        data_file df = sa.Select<data_file>().SingleOrDefault(dataf => dataf.data_file_id == f.data_file_id);
                        //        if (df.peak_summary_id.HasValue)
                        //        {
                        //            peak_summary pk = sa.Select<peak_summary>().SingleOrDefault(p => p.peak_summary_id == df.peak_summary_id);
                        //            sa.Delete<peak_summary>(pk);
                        //        }
                                
                        //        sa.Delete<data_file>(df);
                        //    }
                        //    sa. aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                            
                        //    aBucket.DeleteObject(BuildFilePath(f, f.PATH));
                        //    aSTNE.FILES.DeleteObject(f);
                        //}
                        //aSTNE.SaveChanges();
                        #endregion
                       
                        //HWMs 
                        #region hwms
                        List<hwm> deleteHWMs = sa.Select<hwm>().Where(h => h.site_id == entityId).ToList();
                        foreach (hwm h in deleteHWMs)
                        {
                            if (h.peak_summary_id.HasValue)
                            {
                                peak_summary pk = sa.Select<peak_summary>().SingleOrDefault(p => p.peak_summary_id == h.peak_summary_id);
                                sa.Delete<peak_summary>(pk);
                            }
                            sa.Delete<hwm>(h);
                        }
                        #endregion
                        
                        //Sensors and Sensor statuses
                        #region sensors and sensor status
                        List<instrument> deleteInst = sa.Select<instrument>().Where(i => i.site_id == entityId).ToList();
                        List<instrument_status> eachStat = null;
                        foreach (instrument instr in deleteInst)
                        {
                            eachStat = sa.Select<instrument_status>().Where(instrStat => instrStat.instrument_id == instr.instrument_id).ToList();
                            eachStat.ForEach(insSTAT => sa.Delete<instrument_status>(insSTAT));
                            sa.Delete<instrument>(instr);
                        }
                        #endregion

                        //Network_name_sites, Network_type_sites
                        #region Networks
                        List<network_name_site> deleteNNS = sa.Select<network_name_site>().Where(nns => nns.site_id == entityId).ToList();
                        deleteNNS.ForEach(nnsite => sa.Delete<network_name_site>(nnsite));
                        List<network_type_site> deleteNTS = sa.Select<network_type_site>().Where(nts => nts.site_id == entityId).ToList();
                        deleteNTS.ForEach(ntsite => sa.Delete<network_type_site>(ntsite));
                        #endregion

                        //OPs and OP_Control_Identifiers, OP_measurements 
                        #region OP stuff
                        List<objective_point> deleteOPs = sa.Select<objective_point>().Where(op => op.site_id == entityId).ToList();
                        foreach (objective_point op in deleteOPs)
                        {
                            //get OP_CONTROL_IDENTIFIERS
                            List<op_control_identifier> opcontrols = sa.Select<op_control_identifier>().Where(opci => opci.objective_point_id == op.objective_point_id).ToList();
                            opcontrols.ForEach(o => sa.Delete<op_control_identifier>(o));
                            //get OP_MEASURMENTS
                            List<op_measurements> opmeasures = sa.Select<op_measurements>().Where(opm => opm.objective_point_id == op.objective_point_id).ToList();
                            opmeasures.ForEach(opmeas => sa.Delete<op_measurements>(opmeas));
                            sa.Delete<objective_point>(op);
                        }
                        #endregion

                        //site_housing
                        #region sitehousings
                        List<site_housing> deleteSH = sa.Select<site_housing>().Where(sh => sh.site_id == entityId).ToList();
                        deleteSH.ForEach(shouse => sa.Delete<site_housing>(shouse));
                        #endregion
                        
                        //lastly delete the site
                        sa.Delete<site>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.DELETE

        #endregion

        #region Helpers

        private string buildSiteNO(STNAgent sa, string siteState, string siteCounty, Int32 siteID, string siteName)
        {
            String siteNo;
            //[a-zA-Z]{2}
            if (!string.IsNullOrEmpty(siteName) && Regex.IsMatch(siteName, @"^[a-zA-Z]{3}[-][a-zA-Z]{2}[-][a-zA-Z]{3}[-][0-9]{3}"))
            {
                string[] substring = siteName.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                siteNo = substring[1] + substring[2] + "00" + Regex.Replace(substring[3], "[^0-9.]", "");

                int incr = 0;
                while (sa.Select<site>().FirstOrDefault(s => s.site_id == siteID) != null)
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
        //private bool PostSiteLayer(SITE aSite)
        //{
        //    Feature flayer;
        //    Features fLayers;
        //    AGSServiceAgent agsAgent;
        //    try
        //    {
        //        flayer = new Feature(new SiteLayer(aSite));

        //        fLayers = new Features();
        //        fLayers.features.Add(flayer);

        //        agsAgent = new AGSServiceAgent();
        //        return agsAgent.PostFeature(fLayers, "AddSite/GPServer/AddSite/execute", "New_SITE=");
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}//end PostHWMLayer
       
        //public static string GetSiteDesc(string desc)
        //{
        //    string siteDesc = string.Empty;
        //    if (desc.Length > 100)
        //    {
        //        siteDesc = desc.Substring(0, 100);
        //    }
        //    else
        //    {
        //        siteDesc = desc;
        //    }

        //    return siteDesc;
        //}
        //public static string GetSiteNetwork(STNEntities2 aSTNE, decimal siteId)
        //{
        //    string netName = string.Empty;
        //    List<NETWORK_NAME_SITE> allSiteNets = aSTNE.NETWORK_NAME_SITE.Where(x => x.SITE_ID == siteId).ToList();
        //    string delimiter = ", ";
        //    //comma separate fancy way
        //    if (allSiteNets.Count >= 1)
        //        netName = allSiteNets.Select(x => x.NETWORK_NAME.NAME).Distinct().Aggregate((x, j) => x + delimiter + j);

        //    return netName;
        //}
        //public static string GetHDatum(STNEntities2 aSTNE, decimal hdatumId)
        //{
        //    string hd = string.Empty;
        //    HORIZONTAL_DATUMS hdatum = aSTNE.HORIZONTAL_DATUMS.Where(x => x.DATUM_ID == hdatumId).FirstOrDefault();
        //    if (hdatum != null)
        //        hd = hdatum.DATUM_NAME;

        //    return hd;
        //}
        //public static string GetSitePriority(STNEntities2 aSTNE, decimal priorityID)
        //{
        //    string sitePriority = string.Empty;
        //    DEPLOYMENT_PRIORITY dp = aSTNE.DEPLOYMENT_PRIORITY.Where(x => x.PRIORITY_ID == priorityID).FirstOrDefault();
        //    if (dp != null)
        //        sitePriority = dp.PRIORITY_NAME;

        //    return sitePriority;
        //}
        //public static string GetHCollMethod(STNEntities2 aSTNE, decimal hcollectID)
        //{
        //    string hcolmethod = string.Empty;
        //    HORIZONTAL_COLLECT_METHODS hcm = aSTNE.HORIZONTAL_COLLECT_METHODS.Where(x => x.HCOLLECT_METHOD_ID == hcollectID).FirstOrDefault();
        //    if (hcm != null)
        //        hcolmethod = hcm.HCOLLECT_METHOD;

        //    return hcolmethod;
        //}
        //public static string GetSiteNotes(string notes)
        //{
        //    string siteNotes = string.Empty;
        //    if (notes.Length > 100)
        //    {
        //        siteNotes = notes.Substring(0, 100);
        //    }
        //    else
        //    {
        //        siteNotes = notes;
        //    }

        //    return siteNotes;
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
        #endregion
        #endregion
    }//end class SiteHandler

}//end namespace