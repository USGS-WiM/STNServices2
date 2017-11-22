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
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.Entity;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNServices2.Resources;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using WiM.Security;
using System.Configuration;

namespace STNServices2.Handlers
{
    public class SiteHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllSites")]
        public OperationResult Get()
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteBySearch")]
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
                        anEntity = sa.Select<site>().SingleOrDefault(s => s.site_no.ToUpper() == siteNo.ToUpper());
                        if (anEntity == null) throw new NotFoundRequestException(); 
                        sm(sa.Messages);
                    }
                    if (!string.IsNullOrEmpty(siteName))
                    {
                        anEntity = sa.Select<site>().SingleOrDefault(s => s.site_name.ToUpper() == siteName.ToUpper());
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
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileSite")]
        public OperationResult GetFileSite(Int32 fileId)
        {
            site anEntity = null;            

            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<file>().Include(f => f.site).FirstOrDefault(f => f.file_id == fileId).site;
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
 
        [HttpOperation(HttpMethod.GET, ForUriName = "GetOPSite")]
        public OperationResult GetOPSite(Int32 objectivePointId)
        {
            site anEntity = null;
            
            try
            {
                if (objectivePointId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<objective_point>().Include(op => op.site).FirstOrDefault(op => op.objective_point_id == objectivePointId).site;
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

        [HttpOperation(HttpMethod.GET, ForUriName = "getHWMSite")]
        public OperationResult getHWMSite(Int32 hwmId)
        {
            site anEntity = null;

            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<hwm>().Include(h=>h.site).FirstOrDefault(h => h.hwm_id == hwmId).site;
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentSite")]
        public OperationResult GetInstrumentSite(Int32 instrumentId)
        {
            site anEntity = null;
            
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument>().Include(i=>i.site).FirstOrDefault(i => i.instrument_id == instrumentId).site;
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSitesWithoutPeaks")]
        public OperationResult GetPeaklessSites(Int32 eventId)
        {
            List<site> entities = null;

            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {                
                    // all sites with hwms but none of them have a peak for this eventId
                    IQueryable<site> hwmSList = sa.Select<site>().Include(s => s.hwms)
                        .Where(s => s.hwms.Any(h => (h.peak_summary_id <= 0 || !h.peak_summary_id.HasValue) && h.event_id == eventId));

                    // all sites with sensors that have data files but none of them have a peak for this event
                    IQueryable<site> dfSList = sa.Select<site>().Include(s => s.instruments).Include("instruments.data_files")
                        .Where(s => s.instruments.Any(i => i.data_files.Any(d => (d.peak_summary_id <= 0 || !d.peak_summary_id.HasValue) && i.event_id == eventId)));

                    // join them and remove duplicates
                    entities = hwmSList.Union(dfSList).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventSites")]
        public OperationResult GetEventSites(Int32 eventId)
        {
            List<site> entities = null;

            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {                
                    entities = sa.Select<site>().Include(s => s.instruments).Include(s => s.hwms).Where(s => s.instruments.Any(i => i.event_id == eventId) || s.hwms.Any(h => h.event_id == eventId)).ToList();                    
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "getNetworkTypeSites")]
        public OperationResult getNetworkTypeSites(Int32 networkTypeId)
        {
            List<site> entities = null;

            try
            {
                if (networkTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                { 
                    //give me all the sites that have this network type sites
                    entities = sa.Select<site>().Include(s => s.network_type_site).Where(s => s.network_type_site.Any(i => i.network_type_id == networkTypeId)).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "getNetworkNameSites")]
        public OperationResult getNetworkNameSites(Int32 networkNameId)
        {
            List<site> entities = null;

            try
            {
                if (networkNameId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                { 
                    //give me all the sites that have this network type sites 
                    entities = sa.Select<site>().Include(s => s.network_name_site).Where(s => s.network_name_site.Any(i => i.network_name_id == networkNameId)).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSitesByStateName")]
        public OperationResult GetSitesByStateName(string stateAbbrev)
        {
            List<site> entities = null;

            try
            {
                if (string.IsNullOrEmpty(stateAbbrev)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<site>().Where(s => s.state.ToUpper() == stateAbbrev.ToUpper()).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSitesByLatLong")]
        public OperationResult GetSitesByLatLong(double latitude, double longitude, [Optional]double buffer)
        {
            List<site> entities = null;

            try
            {
                if (latitude <= 0 || longitude >= 0) throw new BadRequestException("Invalid input parameters");
                
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<site>().Where(s => (s.latitude_dd >= latitude - buffer && s.latitude_dd <= latitude + buffer) &&
                                                            (s.longitude_dd >= longitude - buffer && s.longitude_dd <= longitude + buffer)).ToList();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHDatumSites")]
        public OperationResult GetHDatumSites(Int32 hdatumId)
        {
            List<site> entities = null;

            try
            {
                if (hdatumId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<horizontal_datums>().Include(hd => hd.sites).FirstOrDefault(hd => hd.datum_id == hdatumId).sites.ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetLandOwnserSites")]
        public OperationResult GetLandOwnserSites(Int32 landOwnerId)
        {
            List<site> entities = null;

            try
            {
                if (landOwnerId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<landownercontact>().Include(l => l.sites).FirstOrDefault(l => l.landownercontactid == landOwnerId).sites.ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end httpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "FilteredSites")]
        public OperationResult GetFilteredSites([Optional] string eventId, [Optional] string stateNames, [Optional] string sensorTypeId, [Optional] string opDefined, [Optional] string networkNameId, 
                                                    [Optional] string hwmOnlySites, [Optional] string surveyedHWMs, [Optional] string sensorOnlySites, [Optional] string rdgOnlySites)
        {
            try
            {
                List<SiteLocationQuery> sites = null;

                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (!string.IsNullOrEmpty(stateNames))
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 filterEvent = (!string.IsNullOrEmpty(eventId)) ? Convert.ToInt32(eventId) : -1;
                Int32 filterSensorType = (!string.IsNullOrEmpty(sensorTypeId)) ? Convert.ToInt32(sensorTypeId) : -1;
                Int32 filternetworkname = (!string.IsNullOrEmpty(networkNameId)) ? Convert.ToInt32(networkNameId) : -1;
                Boolean OPhasBeenDefined = (!string.IsNullOrEmpty(opDefined) && Convert.ToInt32(opDefined) > 0) ? true : false;  
                Boolean hwmOnly = (!string.IsNullOrEmpty(hwmOnlySites) && Convert.ToInt32(hwmOnlySites) > 0) ? true : false;  //this is the problem.. if false, it doesn't remove those with hwms
                Boolean sensorOnly = (!string.IsNullOrEmpty(sensorOnlySites) && Convert.ToInt32(sensorOnlySites) > 0) ? true : false;
                Boolean rdgOnly = (!string.IsNullOrEmpty(rdgOnlySites) && Convert.ToInt32(rdgOnlySites) > 0) ? true : false;

                using (STNAgent sa = new STNAgent(true))
                {
                    IQueryable<site> query = sa.Select<site>().Include(s => s.instruments).Include(s => s.hwms).Include(s => s.objective_points).Include("network_name_site.network_name").Include(s => s.site_housing)
                        .Include("hwms.event").Include("instruments.event");

                    if (filterEvent > 0)
                        query = query.Where(s => s.instruments.Any(i =>i.event_id == filterEvent) || s.hwms.Any(h => h.event_id == filterEvent));

                    if (states.Count >= 2)
                    {
                        //multiple STATES
                        query = from q in query where states.Any(s => q.state.Contains(s.Trim())) select q;
                    }
                    if (states.Count == 1)
                    {
                        string thisState = states[0];
                        thisState = GetStateByName(thisState).ToString();
                        query = query.Where(r => r.state.Equals(thisState, StringComparison.OrdinalIgnoreCase));
                    }
                    if (OPhasBeenDefined)
                        query = query.Where(s => s.objective_points.Any());                    

                    if (filterSensorType > 0)
                        query = query.Where(s => s.instruments.Any(i => i != null && i.sensor_type_id == filterSensorType));

                    if (filternetworkname > 0)
                        query = query.Where(s => s.network_name_site.Any(i => i.network_name_id == filternetworkname));


                    if (hwmOnly)
                    {
                        //Site with no sensor deployments AND Site with no proposed sensors AND 
                        //(Site does not have permanent housing installed OR Site has had HWM collected at it in the past OR Site has “Sensor not appropriate” box checked)
                        query = query.Where(s => !s.instruments.Any() && ((s.is_permanent_housing_installed == "No" || s.is_permanent_housing_installed == null) || s.hwms.Any() || s.sensor_not_appropriate == 1));
                    }
                    if (!string.IsNullOrEmpty(surveyedHWMs))
                    {
                        if (surveyedHWMs == "true")
                            query = query.Where(i => i.hwms.Any() && i.hwms.Any(h => h.survey_date.HasValue));
                        else
                            query = query.Where(i => i.hwms.Any() && i.hwms.Any(h => !h.survey_date.HasValue));
                    }

                    if (sensorOnly)
                    {
                        //Site with previous sensor deployment OR Sites with housing types 1-4 indicated //OR Sites with permanent housing installed 
                        //OR Site with proposed sensor indicated of any type
                        query = query.Where(s => !s.hwms.Any() && (s.is_permanent_housing_installed == "Yes" || s.site_housing.Any(h => h.housing_type_id > 0 && h.housing_type_id < 5) || s.instruments.Any()));
                    }

                    if (rdgOnly)
                    {
                        //Site with previous RDG sensor type deployed OR Site with RDG housing type listed (type 5) OR Site with RDG checked as a proposed sensor
                        query = query.Where(s => s.instruments.Any(inst => inst.sensor_type_id == 5) || s.site_housing.Any(h => h.housing_type_id == 5));
                    }

                    sites = query.Distinct().AsEnumerable().Select(s => new SiteLocationQuery
                            {
                                site_id = s.site_id,
                                site_no = s.site_no,
                                site_name = s.site_name,
                                site_description = s.site_description,
                                address = s.address,
                                city = s.city,
                                state = s.state,
                                zip = s.zip,
                                county= s.county,
                                waterbody = s.waterbody,
                                latitude_dd = s.latitude_dd,
                                longitude_dd = s.longitude_dd,
                                hdatum_id = s.hdatum_id,
                                drainage_area_sqmi = s.drainage_area_sqmi,
                                landownercontact_id = s.landownercontact_id,
                                priority_id = s.priority_id,
                                zone = s.zone,
                                is_permanent_housing_installed = s.is_permanent_housing_installed,
                                usgs_sid = s.usgs_sid,
                                other_sid = s.other_sid,
                                noaa_sid = s.noaa_sid,
                                hcollect_method_id = s.hcollect_method_id,
                                site_notes = s.site_notes, // should this be removed?? (Internal)
                                safety_notes = s.safety_notes,
                                access_granted = s.access_granted,
                                member_id = s.member_id,
                                networkNames = s.network_name_site.Count > 0 ? s.network_name_site.Where(ns => ns.site_id == s.site_id).Select(x => x.network_name.name).Distinct().ToList() : new List<string>(),
                                RecentOP = getRecentOP(s),
                                Events = getSiteEvents(s)
                            }).ToList();
                }//end using

                return new OperationResult.OK { ResponseResource = sites };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

        }//end HttpMethod.Get
       
        [HttpOperation(HttpMethod.GET, ForUriName="GetDataFileScript")]
        public OperationResult GetDataFileScript(Int32 entityId)
        {
            string isRunning = "false";
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    string path1 = ConfigurationManager.AppSettings["STNRepository"].ToString(); 
                    string path2 = entityId.ToString();
                    string combinedPath = System.IO.Path.Combine(path1);
                    
                    string[] subdirs = Directory.GetDirectories(combinedPath, path2+"*");

                    if (subdirs.Length > 0)
                        isRunning = "true";
                    else
                        isRunning = "false";

                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = isRunning, Description = this.MessageString };
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
                        //no duplicate lat/longs allowed
                        List<site> query = sa.Select<site>().Where(s => s.latitude_dd == anEntity.latitude_dd && s.longitude_dd == anEntity.longitude_dd).ToList();
                        if (query.Count > 0)
                            throw new BadRequestException("Lat/Long already exists");

                        //aSite.STATE = this.GetStateByName(aSite.STATE).ToString();
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

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
            site updatedSite = null;
            
            try
            {
                if (entityId <= 0 || string.IsNullOrEmpty(anEntity.site_description) || anEntity.latitude_dd <= 0 || anEntity.longitude_dd >= 0 || 
                    anEntity.hdatum_id <= 0 || anEntity.hcollect_method_id <= 0 || string.IsNullOrEmpty(anEntity.state) || 
                    string.IsNullOrEmpty(anEntity.county) || string.IsNullOrEmpty(anEntity.waterbody))
                {
                    throw new BadRequestException("Invalid input parameters");
                }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //no duplicate lat/longs allowed (just this one)
                        List<site> query = sa.Select<site>().Where(s => s.latitude_dd == anEntity.latitude_dd && s.longitude_dd == anEntity.longitude_dd).ToList();
                        if (query.Count > 1)
                            throw new BadRequestException("Lat/Long already exists");

                        updatedSite = sa.Select<site>().SingleOrDefault(s => s.site_id == entityId);

                        anEntity.state = this.GetStateByName(anEntity.state).ToString();
                        if ((!string.Equals(anEntity.state.ToUpper(), updatedSite.state.ToUpper())) || (!string.Equals(anEntity.county.ToUpper(), updatedSite.county.ToUpper())))
                            anEntity.site_no = buildSiteNO(sa, anEntity.state, anEntity.county, Convert.ToInt32(anEntity.site_id), anEntity.site_name);
                                      
                        updatedSite.site_no = anEntity.site_no;
                        updatedSite.site_name = anEntity.site_name;
                        updatedSite.site_description = anEntity.site_description;
                        updatedSite.address = anEntity.address;
                        updatedSite.city = anEntity.city;
                        updatedSite.state = anEntity.state;
                        updatedSite.zip = anEntity.zip;
                        updatedSite.other_sid = anEntity.other_sid;
                        updatedSite.county = anEntity.county;
                        updatedSite.waterbody = anEntity.waterbody;
                        updatedSite.latitude_dd = anEntity.latitude_dd;
                        updatedSite.longitude_dd = anEntity.longitude_dd;
                        updatedSite.hdatum_id = anEntity.hdatum_id;
                        updatedSite.drainage_area_sqmi = anEntity.drainage_area_sqmi;
                        updatedSite.landownercontact_id = anEntity.landownercontact_id;
                        updatedSite.priority_id = anEntity.priority_id;
                        updatedSite.zone = anEntity.zone;
                        updatedSite.is_permanent_housing_installed = anEntity.is_permanent_housing_installed;
                        updatedSite.usgs_sid = anEntity.usgs_sid;
                        updatedSite.noaa_sid = anEntity.noaa_sid;
                        updatedSite.hcollect_method_id = anEntity.hcollect_method_id;
                        updatedSite.site_notes = anEntity.site_notes;
                        updatedSite.safety_notes = anEntity.safety_notes;
                        updatedSite.access_granted = anEntity.access_granted;
                        updatedSite.member_id = anEntity.member_id;
                        updatedSite.sensor_not_appropriate = anEntity.sensor_not_appropriate;

                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        updatedSite.last_updated = DateTime.Now;
                        updatedSite.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<site>(entityId, updatedSite);
                        sm(sa.Messages);                         
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = updatedSite, Description = this.MessageString };
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
        [HttpOperation(HttpMethod.DELETE)]
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
                        anEntity = sa.Select<site>().Include(s => s.files).Include(s=>s.instruments).Include(s=>s.hwms).Include(s=>s.objective_points)
                            .Include(s => s.network_name_site).Include(s => s.network_type_site).Include(s => s.site_housing)
                            .FirstOrDefault(s => s.site_id == entityId);

                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
                        //check and see if it has anything related (Can't delete if so)
                        if (anEntity.objective_points.Count > 0 || anEntity.files.Count > 0 || anEntity.instruments.Count > 0 || anEntity.hwms.Count > 0)
                        {
                            sm(MessageType.error, "Related Entities");
                            throw new BadRequestException("Please remove attached entities first.");
                        }
                        else
                        {
                            anEntity.site_housing.Clear();
                            anEntity.network_type_site.Clear();
                            anEntity.network_type_site.Clear();
                            sa.Delete<site>(anEntity);
                            sm(sa.Messages);
                        }
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
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
                //remove . and space from counties like 'st. lucie'  this only removes from start and end of name
                siteCounty = siteCounty.Trim(new Char[] { ' ', '.' });
                //if there's a dot, remove it and the space following
                if (siteCounty.IndexOf(".") > -1)
                {
                    siteCounty = siteCounty.Remove(siteCounty.IndexOf("."),2);
                }
                siteNo = siteState + siteCounty.Substring(0, 3).ToUpper() + siteID.ToString("D5");
            }

            return siteNo;
        }
        private recent_op getRecentOP(site se)
        {
            recent_op recentop = null;

            if (se.objective_points.Count > 0)
                recentop = se.objective_points.OrderByDescending(x => x.date_established).Select(o => new recent_op
                {
                    name = o.name,
                    date_established = o.date_established.Value
                }).FirstOrDefault<recent_op>();

            return recentop;
            
        }
        private List<string> getSiteEvents(site se)
        {
            List<string> eventNames = new List<string>();
            if (se.hwms.Count >= 1)
            {
                eventNames.AddRange(se.hwms.Where(h => h.event_id != null).Select(h => h.@event.event_name).ToList());
            }
            if (se.instruments.Count >= 1)
            {
                eventNames.AddRange(se.instruments.Where(i => i.event_id != null).Select(i => i.@event.event_name).ToList());
            }

            return eventNames.Distinct().ToList();
        }
        #endregion
       
    }//end class SiteHandler

}//end namespace