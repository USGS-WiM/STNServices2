//------------------------------------------------------------------------------
//----- EntityObjectsHandler ---------------------------------------------------
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
// 04.18.13 - JKN - Added CollectionMethodsHandler
// 04.17.13 - JKN -Created

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
    public class AgencyHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "AGENCIES"; }
        }
        #endregion
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<AGENCY>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMemberAgency")]
        public OperationResult GetMemberAgency(Int32 memberId)
        {
            AGENCY mAgency = null;

            //Return BadRequest if there is no ID
            if (memberId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    mAgency = aSTNE.MEMBERS.FirstOrDefault(i => i.MEMBER_ID == memberId).AGENCY;

                    if (mAgency != null)
                        mAgency.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = mAgency };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSourceAgency")]
        public OperationResult GetSourceAgency(Int32 sourceId)
        {
            AGENCY sAgency = null;

            //Return BadRequest if there is no ID
            if (sourceId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    sAgency = aSTNE.SOURCES.FirstOrDefault(i => i.SOURCE_ID == sourceId).AGENCY;

                    if (sAgency != null)
                        sAgency.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = sAgency };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(AGENCY anEntity)
        {
            try
            {
                anEntity.STATE = base.GetStateByName(anEntity.STATE).ToString();
                if (!Exists(ref anEntity))
                {
                    anEntity = base.Post(anEntity) as AGENCY;
                }

                return new OperationResult.OK { ResponseResource = anEntity };

            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, AGENCY entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref AGENCY anEntity)
        {
            AGENCY existingAgency;
            AGENCY thisAgency = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingAgency = aSTNE.AGENCIES.FirstOrDefault(a => string.Equals(a.AGENCY_NAME.ToUpper(), thisAgency.AGENCY_NAME.ToUpper()) &&
                                                                            (string.Equals(a.ADDRESS.ToUpper(), thisAgency.ADDRESS.ToUpper()) || string.IsNullOrEmpty(thisAgency.ADDRESS)) &&
                                                                            (string.Equals(a.CITY.ToUpper(), thisAgency.CITY.ToUpper()) || string.IsNullOrEmpty(thisAgency.CITY)) &&
                                                                            (string.Equals(a.STATE.ToUpper(), thisAgency.STATE.ToUpper()) || string.IsNullOrEmpty(thisAgency.STATE)) &&
                                                                            (string.Equals(a.ZIP.ToUpper(), thisAgency.ZIP.ToUpper()) || string.IsNullOrEmpty(thisAgency.ZIP)) &&
                                                                            (string.Equals(a.PHONE.ToUpper(), thisAgency.PHONE.ToUpper()) || string.IsNullOrEmpty(thisAgency.PHONE)));

                }//end using
                if (existingAgency == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingAgency;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }//end AgencyHandler

    public class CollectionTeamsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "COLLECT_TEAM"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<COLLECT_TEAM>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventTeams")]
        public OperationResult GetEventTeams(Int32 eventId)
        {
            List<COLLECT_TEAM> entities = new List<COLLECT_TEAM>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    entities = aSTNE.COLLECT_TEAM.Where(ct => ct.HWMs.Any(h => h.EVENT_ID == eventId) ||
                                                        ct.INSTRUMENT_STATUS.Any(instat => instat.INSTRUMENT.EVENT_ID == eventId))
                                                    .ToList();

                    if (entities != null)
                        entities.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


                }//end using

                return new OperationResult.OK { ResponseResource = entities };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMemberTeams")]
        public OperationResult GetMemberTeams(Int32 memberId)
        {
            List<COLLECT_TEAM> collectionTeamList = new List<COLLECT_TEAM>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    collectionTeamList = aSTNE.MEMBERS_TEAM.Where(mt => mt.MEMBER_ID == memberId)
                                                    .Select(m => m.COLLECT_TEAM)
                                                    .ToList();

                    if (collectionTeamList != null)
                        collectionTeamList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = collectionTeamList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentStatusCollectionTeam")]
        public OperationResult GetInstrumentStatusCollectionTeam(Int32 instrumentStatusId)
        {
            COLLECT_TEAM collectionTeam;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    collectionTeam = aSTNE.INSTRUMENT_STATUS.FirstOrDefault(i => i.INSTRUMENT_STATUS_ID == instrumentStatusId).COLLECT_TEAM;


                    if (collectionTeam != null)
                        collectionTeam.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = collectionTeam };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET


        [HttpOperation(ForUriName = "GetHWMFlagTeam")]
        public OperationResult GetHWMFlagTeam(Int32 hwmId)
        {
            COLLECT_TEAM collectionTeam;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    HWM thisHWM = aSTNE.HWMs.FirstOrDefault(i => i.HWM_ID == hwmId);

                    collectionTeam = aSTNE.COLLECT_TEAM.FirstOrDefault(a => a.COLLECT_TEAM_ID == thisHWM.FLAG_TEAM_ID);

                    if (collectionTeam != null)
                        collectionTeam.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
                return new OperationResult.OK { ResponseResource = collectionTeam };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMSurveyTeam")]
        public OperationResult GetHWMSurveyTeam(Int32 hwmId)
        {
            COLLECT_TEAM collectionTeam;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    HWM thisHWM = aSTNE.HWMs.FirstOrDefault(i => i.HWM_ID == hwmId);

                    collectionTeam = aSTNE.COLLECT_TEAM.FirstOrDefault(a => a.COLLECT_TEAM_ID == thisHWM.SURVEY_TEAM_ID);

                    if (collectionTeam != null)
                        collectionTeam.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = collectionTeam };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET
        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(COLLECT_TEAM anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, COLLECT_TEAM entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref COLLECT_TEAM anEntity)
        {
            COLLECT_TEAM existingEntity;
            COLLECT_TEAM thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.COLLECT_TEAM.FirstOrDefault(mt => string.Equals(mt.DESCRIPTION.ToUpper(), thisEntity.DESCRIPTION.ToUpper()));
                }
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
        #endregion
    }//end CollectionTeamsHandler

    public class ContactTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "CONTACT_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<CONTACT_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(ForUriName = "GetContactsContactType")]
        public OperationResult GetContactContactType(Int32 contactId)
        {
            CONTACT_TYPE aContactType = null;

            //Return BadRequest if there is no ID
            if (contactId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aContactType = aSTNE.REPORTMETRIC_CONTACT.FirstOrDefault(c => c.CONTACT_ID == contactId).CONTACT_TYPE;

                    if (aContactType != null)
                        aContactType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
                return new OperationResult.OK { ResponseResource = aContactType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET
        #endregion

        #region PostMethods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(CONTACT_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.POST

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddReportMetricContact")]
        public OperationResult AddReportMetricContact(REPORTMETRIC_CONTACT aRepMetricContact)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(aRepMetricContact) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }
        #endregion Post

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, CONTACT_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion

        #region Helper Methods
        private bool Exists(ref CONTACT_TYPE anEntity)
        {
            CONTACT_TYPE existingEntity;
            CONTACT_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.CONTACT_TYPE.FirstOrDefault(mt => string.Equals(mt.TYPE.ToUpper(), thisEntity.TYPE.ToUpper()));
                }
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
        #endregion
    }//end ContactTypeHandler

    public class CountiesHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "COUNTIES"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<COUNTIES>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetStateCountiesById")]
        public OperationResult GetStateCounties(Int32 stateId)
        {
            List<COUNTIES> stateCountyList = null;

            //Return BadRequest if there is no ID
            if (stateId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    stateCountyList = aSTNE.STATES.FirstOrDefault(i => i.STATE_ID == stateId).COUNTIES.ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = stateCountyList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetStateCountiesByAbbrev")]
        public OperationResult GetStateCounties(string stateAbbrev)
        {
            List<COUNTIES> stateCountyList = null;

            //Return BadRequest if there is no ID
            if (string.IsNullOrEmpty(stateAbbrev))
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    stateCountyList = aSTNE.STATES.FirstOrDefault(i => i.STATE_ABBREV == stateAbbrev).COUNTIES.ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = stateCountyList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetStateSiteCounties")]
        public OperationResult StateSiteCounties(string stateAbbrev)
        {
            List<COUNTIES> SiteCounties;
            STATES thisState;
            //Return BadRequest if there is no ID
            if (stateAbbrev == null)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    thisState = aSTNE.STATES.Where(st => st.STATE_ABBREV == stateAbbrev).FirstOrDefault();
                    List<SITE> allSitesInThisState = aSTNE.SITES.Where(p => p.STATE == stateAbbrev).ToList();
                    List<string> uniqueCoList = allSitesInThisState.GroupBy(p => p.COUNTY).Select(g => g.FirstOrDefault()).Select(y => y.COUNTY).ToList();
                    SiteCounties = aSTNE.COUNTIES.Where(co => uniqueCoList.Contains(co.COUNTY_NAME) && co.STATE_ID == thisState.STATE_ID).ToList();
                }//end using            

                return new OperationResult.OK { ResponseResource = SiteCounties };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(COUNTIES anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, COUNTIES entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref COUNTIES anEntity)
        {
            COUNTIES existingEntity;
            COUNTIES thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.COUNTIES.FirstOrDefault(ct => string.Equals(ct.COUNTY_NAME.ToUpper(), thisEntity.COUNTY_NAME.ToUpper()) &&
                                                                        ct.STATE_ID == thisEntity.STATE_ID);
                }
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
        #endregion
    }//end CountiesHandler

    public class DeploymentPriorityHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "DEPLOYMENT_PRIORITY"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<DEPLOYMENT_PRIORITY>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSitePriority")]
        public OperationResult GetSitePriorities(decimal siteId)
        {
            DEPLOYMENT_PRIORITY pri;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    pri = aSTNE.SITES.FirstOrDefault(h => h.SITE_ID == siteId).DEPLOYMENT_PRIORITY;

                    if (pri != null)
                        pri.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = pri };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(DEPLOYMENT_PRIORITY anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, DEPLOYMENT_PRIORITY entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref DEPLOYMENT_PRIORITY anEntity)
        {
            DEPLOYMENT_PRIORITY existingEntity;
            DEPLOYMENT_PRIORITY thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.DEPLOYMENT_PRIORITY.FirstOrDefault(mt => string.Equals(mt.PRIORITY_NAME.ToUpper(), thisEntity.PRIORITY_NAME.ToUpper()));
                }
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
        #endregion
    }//end DeploymentPriorityHandler

    public class DepolymentTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "DEPLOYMENT_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<DEPLOYMENT_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentDeploymentType")]
        public OperationResult GetInstrumentDeploymentType(Int32 instrumentId)
        {
            DEPLOYMENT_TYPE aDeploymentType = null;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aDeploymentType = aSTNE.INSTRUMENTs.FirstOrDefault(i => i.INSTRUMENT_ID == instrumentId).DEPLOYMENT_TYPE;

                    if (aDeploymentType != null)
                        aDeploymentType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aDeploymentType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSensorDeploymentTypes")]
        public OperationResult GetSensorDeploymentTypes(Int32 sensorTypeId)
        {
            List<DEPLOYMENT_TYPE> deploymentTypeList = new List<DEPLOYMENT_TYPE>();

            //Return BadRequest if there is no ID
            if (sensorTypeId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //give me all the deployment types that have this sensor deployment
                    deploymentTypeList = aSTNE.DEPLOYMENT_TYPE.AsEnumerable().Where(i => i.SENSOR_DEPLOYMENT.Any(a => a.SENSOR_TYPE_ID == sensorTypeId)).ToList();

                    if (deploymentTypeList != null)
                        deploymentTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = deploymentTypeList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(DEPLOYMENT_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSensorDeployment")]
        public OperationResult AddSensorDeployment(Int32 sensorTypeId, DEPLOYMENT_TYPE aDeploymentType)
        {
            SENSOR_DEPLOYMENT aSensorDeploymentType = null;
            List<DEPLOYMENT_TYPE> deploymentTypeList = null;
            //Return BadRequest if missing required fields
            if (sensorTypeId <= 0 || String.IsNullOrEmpty(aDeploymentType.METHOD))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid Sensor Type
                        if (aSTNE.SENSOR_TYPE.First(s => s.SENSOR_TYPE_ID == sensorTypeId) == null)
                            return new OperationResult.BadRequest { Description = "Sensor Type does not exists" };

                        if (!Exists(ref aDeploymentType))
                        {
                            //set ID
                            aSTNE.DEPLOYMENT_TYPE.AddObject(aDeploymentType);
                            aSTNE.SaveChanges();
                        }//end if

                        //add to SensorDeployment
                        //first check if sensor already contains this deployment type
                        if (aSTNE.SENSOR_DEPLOYMENT.FirstOrDefault(nt => nt.DEPLOYMENT_TYPE_ID == aDeploymentType.DEPLOYMENT_TYPE_ID &&
                                                                            nt.SENSOR_TYPE_ID == sensorTypeId) == null)
                        {//create one
                            aSensorDeploymentType = new SENSOR_DEPLOYMENT();
                            aSensorDeploymentType.SENSOR_TYPE_ID = sensorTypeId;
                            aSensorDeploymentType.DEPLOYMENT_TYPE_ID = aDeploymentType.DEPLOYMENT_TYPE_ID;
                            aSTNE.SENSOR_DEPLOYMENT.AddObject(aSensorDeploymentType);
                            aSTNE.SaveChanges();
                        }//end if

                        //return list of network types
                        deploymentTypeList = aSTNE.DEPLOYMENT_TYPE.Where(nt => nt.SENSOR_DEPLOYMENT.Any(nts => nts.SENSOR_TYPE_ID == sensorTypeId)).ToList();

                        if (deploymentTypeList != null)
                            deploymentTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                    }//end using
                }//end using
                //return object to verify persistance
                return new OperationResult.OK { ResponseResource = deploymentTypeList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "RemoveSensorDeployment")]
        public OperationResult RemoveSensorDeployment(Int32 sensorTypeId, DEPLOYMENT_TYPE aDeploymentType)
        {
            //Return BadRequest if missing required fields
            if (sensorTypeId <= 0 || String.IsNullOrEmpty(aDeploymentType.METHOD))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid Sensor Type
                        if (aSTNE.SENSOR_TYPE.First(s => s.SENSOR_TYPE_ID == sensorTypeId) == null)
                            return new OperationResult.BadRequest { Description = "Sensor Type does not exists" };

                        //remove from Sensor_Deployment
                        SENSOR_DEPLOYMENT aSensorDeploymentType = aSTNE.SENSOR_DEPLOYMENT.FirstOrDefault(sd => sd.DEPLOYMENT_TYPE_ID == aDeploymentType.DEPLOYMENT_TYPE_ID &&
                                                                                                sd.SENSOR_TYPE_ID == sensorTypeId);

                        if (aSensorDeploymentType != null)
                        {
                            //remove it
                            aSTNE.SENSOR_DEPLOYMENT.DeleteObject(aSensorDeploymentType);
                            aSTNE.SaveChanges();
                        }
                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }

        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, DEPLOYMENT_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref DEPLOYMENT_TYPE anEntity)
        {
            DEPLOYMENT_TYPE existingEntity;
            DEPLOYMENT_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.DEPLOYMENT_TYPE.FirstOrDefault(mt => string.Equals(mt.METHOD.ToUpper(), thisEntity.METHOD.ToUpper()));
                }
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
        #endregion
    }//end DepolymentTypeHandler

    public class EventsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "EVENTS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<EVENT>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventsBySite")]
        public OperationResult GetEventsBySite(Int32 siteId)
        {

            List<EVENT> events = new List<EVENT>();
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    events = aSTNE.EVENTS.Where(e => e.HWMs.Any(h => h.SITE_ID == siteId) ||
                                                    e.INSTRUMENTs.Any(inst => inst.SITE_ID == siteId))
                                            .ToList();

                    if (events != null)
                        events.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


                }//end using

                return new OperationResult.OK { ResponseResource = events };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventTypeEvents")]
        public OperationResult GetEventTypeEvents(Int32 eventTypeId)
        {
            List<EVENT> eventList = new List<EVENT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    eventList = aSTNE.EVENT_TYPE.FirstOrDefault(et => et.EVENT_TYPE_ID == eventTypeId).EVENTs.ToList();

                    if (eventList != null)
                        eventList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = eventList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventStatusEvents")]
        public OperationResult GetEventStatusEvents(Int32 eventStatusId)
        {
            List<EVENT> eventList = new List<EVENT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    eventList = aSTNE.EVENT_STATUS.FirstOrDefault(et => et.EVENT_STATUS_ID == eventStatusId).EVENTs.ToList();

                    if (eventList != null)
                        eventList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = eventList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMEvent")]
        public OperationResult GetHWMEvent(Int32 hwmId)
        {
            EVENT anEvent;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anEvent = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).EVENT;

                    if (anEvent != null)
                        anEvent.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = anEvent };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentEvent")]
        public OperationResult GetInstrumentEvent(Int32 instrumentId)
        {
            EVENT aEvent;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aEvent = aSTNE.INSTRUMENTs.FirstOrDefault(
                                        i => i.INSTRUMENT_ID == instrumentId).EVENT;

                    if (aEvent != null)
                        aEvent.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }

                return new OperationResult.OK { ResponseResource = aEvent };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredEvents")]
        public OperationResult GetFilteredEvents([Optional]string date, [Optional]Int32 eventTypeId, [Optional] string stateName)
        {
            IQueryable<EVENT> query;
            List<EVENT> eventList = new List<EVENT>();
            DateTime fromDate;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    query = aSTNE.EVENTS;

                    if (!string.IsNullOrEmpty(date))
                    {
                        fromDate = Convert.ToDateTime(date);

                        query = query.Where(s => s.EVENT_START_DATE >= fromDate);
                    }
                    if (eventTypeId > 0)
                    {
                        query = query.Where(s => s.EVENT_TYPE_ID == eventTypeId);
                    }
                    if (!string.IsNullOrEmpty(stateName))
                    {
                        //give me all the events that have taken place in this state ... sites have states.. events are on instruments and hwms
                        query = query.Where(e => e.INSTRUMENTs.Any(i => i.SITE.STATE == stateName));
                        query = query.Where(e => e.HWMs.Any(h => h.SITE.STATE == stateName));
                        //var x = query.Where(e => e.INSTRUMENTs.Any(i => i.SITE.STATE == stateName || e.HWMs.Any(h => h.SITE.STATE == stateName))).ToList();

                    }

                    eventList = query.Distinct().ToList();

                }

                return new OperationResult.OK { ResponseResource = eventList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }


        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(EVENT anEntity)
        {
            try
            {
                if (!Exists(ref anEntity))
                {
                    anEntity = base.Post(anEntity) as EVENT;
                }
                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, EVENT entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref EVENT anEntity)
        {
            EVENT existingEntity;
            EVENT thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.EVENTS.FirstOrDefault(e => string.Equals(e.EVENT_NAME, thisEntity.EVENT_NAME) &&
                                                                (DateTime.Equals(e.EVENT_START_DATE.Value, thisEntity.EVENT_START_DATE.Value) || !thisEntity.EVENT_START_DATE.HasValue) &&
                                                                (DateTime.Equals(e.EVENT_END_DATE.Value, thisEntity.EVENT_END_DATE.Value) || !thisEntity.EVENT_END_DATE.HasValue) &&
                                                                (e.EVENT_TYPE_ID == thisEntity.EVENT_TYPE_ID || thisEntity.EVENT_TYPE_ID <= 0 || thisEntity.EVENT_TYPE_ID == null));

                }
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
        #endregion
    }//end EventHandler

    public class EventStatusHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "EVENT_STATUS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<EVENT_STATUS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventStatus")]
        public OperationResult GetEventStatus(Int32 eventId)
        {
            EVENT_STATUS eStatus = null;

            //Return BadRequest if there is no ID
            if (eventId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    eStatus = aSTNE.EVENTS.FirstOrDefault(i => i.EVENT_ID == eventId).EVENT_STATUS;

                    if (eStatus != null)
                        eStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using                

                return new OperationResult.OK { ResponseResource = eStatus };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(EVENT_STATUS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, EVENT_STATUS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref EVENT_STATUS anEntity)
        {
            EVENT_STATUS existingEntity;
            EVENT_STATUS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.EVENT_STATUS.FirstOrDefault(mt => string.Equals(mt.STATUS.ToUpper(), thisEntity.STATUS.ToUpper()));
                }
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
        #endregion
    }//end EventStatusHandler

    public class EventTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "EVENT_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<EVENT_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventType")]
        public OperationResult GetEventType(Int32 eventId)
        {
            EVENT_TYPE eType = null;

            //Return BadRequest if there is no ID
            if (eventId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    eType = aSTNE.EVENTS.FirstOrDefault(i => i.EVENT_ID == eventId).EVENT_TYPE;

                    if (eType != null)
                        eType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = eType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(EVENT_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, EVENT_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref EVENT_TYPE anEntity)
        {
            EVENT_TYPE existingEntity;
            EVENT_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.EVENT_TYPE.FirstOrDefault(mt => string.Equals(mt.TYPE.ToUpper(), thisEntity.TYPE.ToUpper()));
                }
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
        #endregion
    }//end EventTypeHandler

    public class FileTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "FILE_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<FILE_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetFileType")]
        public OperationResult GetFileType(Int32 fileId)
        {
            FILE_TYPE fType = null;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    fType = aSTNE.FILES.FirstOrDefault(i => i.FILE_ID == fileId).FILE_TYPE;

                    if (fType != null)
                        fType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = fType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(FILE_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, FILE_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref FILE_TYPE anEntity)
        {
            FILE_TYPE existingEntity;
            FILE_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.FILE_TYPE.FirstOrDefault(mt => string.Equals(mt.FILETYPE.ToUpper(), thisEntity.FILETYPE.ToUpper()));
                }
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
        #endregion
    }//end FileTypeHandler

    public class HorizontalCollectMethodsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HORIZONTAL_COLLECT_METHODS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<HORIZONTAL_COLLECT_METHODS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMHorizontalMethods")]
        public OperationResult GetHWMHorizontalMethods(Int32 hwmId)
        {
            HORIZONTAL_COLLECT_METHODS hwmHCollMethod;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmHCollMethod = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).HORIZONTAL_COLLECT_METHODS;


                    if (hwmHCollMethod != null)
                        hwmHCollMethod.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
                return new OperationResult.OK { ResponseResource = hwmHCollMethod };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(HORIZONTAL_COLLECT_METHODS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HORIZONTAL_COLLECT_METHODS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref HORIZONTAL_COLLECT_METHODS anEntity)
        {
            HORIZONTAL_COLLECT_METHODS existingEntity;
            HORIZONTAL_COLLECT_METHODS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.HORIZONTAL_COLLECT_METHODS.FirstOrDefault(mt => string.Equals(mt.HCOLLECT_METHOD.ToUpper(), thisEntity.HCOLLECT_METHOD.ToUpper()));
                }
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
        #endregion
    }//end HorizontalCollectMethodsHandler

    public class HorizontalDatumHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HORIZONTAL_DATUMS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<HORIZONTAL_DATUMS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteHdatum")]
        public OperationResult GetSiteHdatum(Int32 siteId)
        {
            HORIZONTAL_DATUMS hDatum = null;

            //Return BadRequest if there is no ID
            if (siteId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hDatum = aSTNE.SITES.FirstOrDefault(i => i.SITE_ID == siteId).HORIZONTAL_DATUMS;

                    if (hDatum != null)
                        hDatum.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = hDatum };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(HORIZONTAL_DATUMS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HORIZONTAL_DATUMS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref HORIZONTAL_DATUMS anEntity)
        {
            HORIZONTAL_DATUMS existingEntity;
            HORIZONTAL_DATUMS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.HORIZONTAL_DATUMS.FirstOrDefault(mt => (string.Equals(mt.DATUM_NAME.ToUpper(), thisEntity.DATUM_NAME.ToUpper()) || string.IsNullOrEmpty(thisEntity.DATUM_NAME)) &&
                                                                                  (string.Equals(mt.DATUM_ABBREVIATION.ToUpper(), thisEntity.DATUM_ABBREVIATION.ToUpper()) || string.IsNullOrEmpty(thisEntity.DATUM_ABBREVIATION)));
                }
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
        #endregion
    }//end HorizontalDatumHandler

    public class HousingTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HOUSING_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<HOUSING_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "InstrumentHousingType")]
        public OperationResult InstrumentHousingType(Int32 instrumentId)
        {
            HOUSING_TYPE housingType;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    housingType = aSTNE.INSTRUMENTs.FirstOrDefault(h => h.INSTRUMENT_ID == instrumentId).HOUSING_TYPE;

                    if (housingType != null)
                        housingType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
                return new OperationResult.OK { ResponseResource = housingType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(HOUSING_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HOUSING_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref HOUSING_TYPE anEntity)
        {
            HOUSING_TYPE existingEntity;
            HOUSING_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.HOUSING_TYPE.FirstOrDefault(mt => string.Equals(mt.TYPE_NAME.ToUpper(), thisEntity.TYPE_NAME.ToUpper()));
                }
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
        #endregion
    }//end HousingTypeHandler

    public class HWMQualitiesHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HWM_QUALITIES"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<HWM_QUALITIES>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMQuality")]
        public OperationResult GetHWMQuality(Int32 hwmId)
        {
            HWM_QUALITIES hwmQuality;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmQuality = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).HWM_QUALITIES;


                    if (hwmQuality != null)
                        hwmQuality.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = hwmQuality };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(HWM_QUALITIES anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HWM_QUALITIES entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref HWM_QUALITIES anEntity)
        {
            HWM_QUALITIES existingEntity;
            HWM_QUALITIES thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.HWM_QUALITIES.FirstOrDefault(mt => string.Equals(mt.HWM_QUALITY.ToUpper(), thisEntity.HWM_QUALITY.ToUpper()));
                }
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
        #endregion
    }//end CollectionMethodsHandler

    public class HWMTypesHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "HWM_TYPES"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<HWM_TYPES>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMType")]
        public OperationResult GetHWMType(Int32 hwmId)
        {
            HWM_TYPES hwmType;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmType = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).HWM_TYPES;


                    if (hwmType != null)
                        hwmType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = hwmType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(HWM_TYPES anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, HWM_TYPES entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref HWM_TYPES anEntity)
        {
            HWM_TYPES existingEntity;
            HWM_TYPES thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.HWM_TYPES.FirstOrDefault(mt => string.Equals(mt.HWM_TYPE.ToUpper(), thisEntity.HWM_TYPE.ToUpper()));
                }
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
        #endregion
    }//end HWMTypesHandler

    public class InstrCollectConditionsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "INSTR_COLLECTION_CONDITIONS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<INSTR_COLLECTION_CONDITIONS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentCondition")]
        public OperationResult GetInstrumentCondition(Int32 instrumentId)
        {
            INSTR_COLLECTION_CONDITIONS cCondition = null;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    cCondition = aSTNE.INSTRUMENTs.FirstOrDefault(i => i.INSTRUMENT_ID == instrumentId).INSTR_COLLECTION_CONDITIONS;

                    if (cCondition != null)
                        cCondition.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = cCondition };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(INSTR_COLLECTION_CONDITIONS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, INSTR_COLLECTION_CONDITIONS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref INSTR_COLLECTION_CONDITIONS anEntity)
        {
            INSTR_COLLECTION_CONDITIONS existingEntity;
            INSTR_COLLECTION_CONDITIONS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.INSTR_COLLECTION_CONDITIONS.FirstOrDefault(mt => string.Equals(mt.CONDITION.ToUpper(), thisEntity.CONDITION.ToUpper()));
                }
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
        #endregion
    }//

    public class LandOwnerHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "LANDOWNERCONTACTs"; }
        }
        #endregion
        #region GetMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<LANDOWNERCONTACT>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(ForUriName = "GetSiteLandOwner")]
        public OperationResult GetSiteLandOwner(Int32 siteId)
        {
            LANDOWNERCONTACT landOwner = null;

            //Return BadRequest if there is no ID
            if (siteId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        landOwner = aSTNE.SITES.FirstOrDefault(i => i.SITE_ID == siteId).LANDOWNERCONTACT;

                        if (landOwner != null)
                            landOwner.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = landOwner };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(LANDOWNERCONTACT anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception ex)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, LANDOWNERCONTACT entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref LANDOWNERCONTACT anEntity)
        {
            LANDOWNERCONTACT existingEntity;
            LANDOWNERCONTACT thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.LANDOWNERCONTACTs.FirstOrDefault(mt => (string.Equals(mt.FNAME.ToUpper(), thisEntity.FNAME.ToUpper()) || string.IsNullOrEmpty(thisEntity.FNAME)) &&
                                                                                  (string.Equals(mt.LNAME.ToUpper(), thisEntity.LNAME.ToUpper()) || string.IsNullOrEmpty(thisEntity.LNAME)) &&
                                                                                  (string.Equals(mt.ADDRESS.ToUpper(), thisEntity.ADDRESS.ToUpper()) || string.IsNullOrEmpty(thisEntity.ADDRESS))
                                                                                  );
                }
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
        #endregion

    }//end LandOwnerHandler

    public class MarkerHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "MARKERS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<MARKER>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMMarker")]
        public OperationResult GetHWMMarker(Int32 hwmId)
        {
            MARKER hwmMarker;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    hwmMarker = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).MARKER;

                    if (hwmMarker != null)
                        hwmMarker.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = hwmMarker };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(MARKER anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, MARKER entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref MARKER anEntity)
        {
            MARKER existingEntity;
            MARKER thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.MARKERS.FirstOrDefault(mt => string.Equals(mt.MARKER1.ToUpper(), thisEntity.MARKER1.ToUpper()));
                }
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
        #endregion
    }//end MarkerHandler

    public class ObjectivePointTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "OBJECTIVE_POINT_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<OBJECTIVE_POINT_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetOPType")]
        public OperationResult GetOPType(Int32 objectivePointId)
        {
            OBJECTIVE_POINT_TYPE opType = null;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    opType = aSTNE.OBJECTIVE_POINT.FirstOrDefault(i => i.OBJECTIVE_POINT_ID == objectivePointId).OBJECTIVE_POINT_TYPE;

                    if (opType != null)
                        opType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = opType };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(OBJECTIVE_POINT_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, OBJECTIVE_POINT_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref OBJECTIVE_POINT_TYPE anEntity)
        {
            OBJECTIVE_POINT_TYPE existingEntity;
            OBJECTIVE_POINT_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.OBJECTIVE_POINT_TYPE.FirstOrDefault(mt => string.Equals(mt.OP_TYPE.ToUpper(), thisEntity.OP_TYPE.ToUpper()));
                }
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
        #endregion
    }//end ObjectivePointTypeTypeHandler

    public class OP_QualityHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "OP_QUALITY"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<OP_QUALITY>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetObjectivePointQuality")]
        public OperationResult GetObjectivePointQuality(Int32 objectivePointId)
        {
            OP_QUALITY opQuality = null;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    opQuality = aSTNE.OBJECTIVE_POINT.FirstOrDefault(i => i.OBJECTIVE_POINT_ID == objectivePointId).OP_QUALITY;

                    if (opQuality != null)
                        opQuality.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = opQuality };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(OP_QUALITY anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, OP_QUALITY entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref OP_QUALITY anEntity)
        {
            OP_QUALITY existingEntity;
            OP_QUALITY thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.OP_QUALITY.FirstOrDefault(mt => string.Equals(mt.QUALITY.ToUpper(), thisEntity.QUALITY.ToUpper()));
                }
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
        #endregion
    }//end OP_QualityHandler

    public class RoleHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "ROLES"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<ROLE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetMemberRole")]
        public OperationResult GetMemberRole(Int32 memberId)
        {
            ROLE mRole = null;

            //Return BadRequest if there is no ID
            if (memberId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    mRole = aSTNE.MEMBERS.FirstOrDefault(i => i.MEMBER_ID == memberId).ROLE;

                    if (mRole != null)
                        mRole.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = mRole };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion

        #region Helper Methods

        #endregion
    }//end RoleHandler

    public class SensorBrandHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SENSOR_BRAND"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<SENSOR_BRAND>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentSensorBrand")]
        public OperationResult GetInstrumentSensorBrand(Int32 instrumentId)
        {
            SENSOR_BRAND aSensorBrand = null;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aSensorBrand = aSTNE.INSTRUMENTs.FirstOrDefault(i => i.INSTRUMENT_ID == instrumentId).SENSOR_BRAND;

                    if (aSensorBrand != null)
                        aSensorBrand.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aSensorBrand };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(SENSOR_BRAND anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, SENSOR_BRAND entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref SENSOR_BRAND anEntity)
        {
            SENSOR_BRAND existingEntity;
            SENSOR_BRAND thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.SENSOR_BRAND.FirstOrDefault(mt => string.Equals(mt.BRAND_NAME.ToUpper(), thisEntity.BRAND_NAME.ToUpper()));
                }
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
        #endregion
    }//end SensorBrandHandler

    public class SensorDeploymentHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SENSOR_DEPLOYMENT"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            return new OperationResult.OK { ResponseResource = base.GetList<SENSOR_DEPLOYMENT>() };
        }//end HttpMethod.GET


        #endregion

    }//end SensorDeploymentHandler


    public class SensorTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SENSOR_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            return new OperationResult.OK { ResponseResource = base.GetList<SENSOR_TYPE>() };
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentSensorType")]
        public OperationResult GetInstrumentSensorType(Int32 instrumentId)
        {
            SENSOR_TYPE aSensorType = null;

            // Return BadRequest if there is no ID
            if (instrumentId <= 0)
                return new OperationResult.BadRequest();

            using (STNEntities2 aSTNE = GetRDS())
            {
                aSensorType = aSTNE.INSTRUMENTs.FirstOrDefault(i => i.INSTRUMENT_ID == instrumentId).SENSOR_TYPE;

                if (aSensorType != null)
                    aSensorType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using

            return new OperationResult.OK { ResponseResource = aSensorType };
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetDeploymentSensorType")]
        public OperationResult GetDeploymentSensorType(Int32 deploymentTypeId)
        {
            SENSOR_TYPE aSensorType = null;

            // Return BadRequest if there is no ID
            if (deploymentTypeId <= 0)
                return new OperationResult.BadRequest();

            using (STNEntities2 aSTNE = GetRDS())
            {
                //need the sensor type that this deployment type is connected to.
                aSensorType = aSTNE.SENSOR_DEPLOYMENT.FirstOrDefault(i => i.DEPLOYMENT_TYPE_ID == deploymentTypeId).SENSOR_TYPE;

                if (aSensorType != null)
                    aSensorType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using

            return new OperationResult.OK { ResponseResource = aSensorType };
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(SENSOR_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, SENSOR_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref SENSOR_TYPE anEntity)
        {
            SENSOR_TYPE existingEntity;
            SENSOR_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.SENSOR_TYPE.FirstOrDefault(mt => string.Equals(mt.SENSOR.ToUpper(), thisEntity.SENSOR.ToUpper()));
                }
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
        #endregion
    }//end SensorTypeHandler

    public class SourceHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SOURCES"; }
        }
        #endregion
        #region GetMethods

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<SOURCE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetAgencySources")]
        public OperationResult GetAgencySources(Int32 agencyId)
        {
            List<SOURCE> sourceList = new List<SOURCE>();

            //Return BadRequest if there is no ID
            if (agencyId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        sourceList = aSTNE.AGENCIES.FirstOrDefault(i => i.AGENCY_ID == agencyId).SOURCEs.ToList();

                        if (sourceList != null)
                            sourceList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = sourceList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetFileSource")]
        public OperationResult GetFileSource(Int32 fileId)
        {
            SOURCE fSource = null;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        fSource = aSTNE.FILES.FirstOrDefault(i => i.FILE_ID == fileId).SOURCE;

                        if (fSource != null)
                            fSource.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = fSource };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(SOURCE anEntity)
        {
            try
            {
                if (!Exists(ref anEntity))
                {
                    anEntity = base.Post(anEntity) as SOURCE;
                }//endi if

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, SOURCE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref SOURCE anEntity)
        {
            SOURCE existingEntity;
            SOURCE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.SOURCES.FirstOrDefault(e => string.Equals(e.SOURCE_NAME.ToUpper(), thisEntity.SOURCE_NAME.ToUpper()) &&
                                                                                    (e.AGENCY_ID == thisEntity.AGENCY_ID || thisEntity.AGENCY_ID <= 0 || thisEntity.AGENCY_ID == null) &&
                                                                                    (DateTime.Equals(e.SOURCE_DATE.Value, thisEntity.SOURCE_DATE.Value) || !thisEntity.SOURCE_DATE.HasValue));

                }
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
        #endregion

    }//end SourceHandler

    public class StatusTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "STATUS_TYPE"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<STATUS_TYPE>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentStatusStatus")]
        public OperationResult GetInstrumentStatusStatus(Int32 instrumentStatusId)
        {
            STATUS_TYPE aStatus = null;

            //Return BadRequest if there is no ID
            if (instrumentStatusId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aStatus = aSTNE.INSTRUMENT_STATUS.FirstOrDefault(i => i.INSTRUMENT_STATUS_ID == instrumentStatusId).STATUS_TYPE;

                    if (aStatus != null)
                        aStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aStatus };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET
        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(STATUS_TYPE anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, STATUS_TYPE entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref STATUS_TYPE anEntity)
        {
            STATUS_TYPE existingEntity;
            STATUS_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.STATUS_TYPE.FirstOrDefault(mt => string.Equals(mt.STATUS.ToUpper(), thisEntity.STATUS.ToUpper()));
                }
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
        #endregion
    }//end StatusTypeHandler

    public class VerticalCollectionMethodsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "VERTICAL_COLLECT_METHODS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<VERTICAL_COLLECT_METHODS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMVerticalMethod")]
        public OperationResult GetHWMVerticalMethod(Int32 hwmId)
        {
            VERTICAL_COLLECT_METHODS VcollectMethod;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    VcollectMethod = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).VERTICAL_COLLECT_METHODS;

                    if (VcollectMethod != null)
                        VcollectMethod.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = VcollectMethod };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(VERTICAL_COLLECT_METHODS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, VERTICAL_COLLECT_METHODS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref VERTICAL_COLLECT_METHODS anEntity)
        {
            VERTICAL_COLLECT_METHODS existingEntity;
            VERTICAL_COLLECT_METHODS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.VERTICAL_COLLECT_METHODS.FirstOrDefault(e => string.Equals(e.VCOLLECT_METHOD.ToUpper(), thisEntity.VCOLLECT_METHOD.ToUpper()));

                }
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
        #endregion
    }//end VerticalCollectionMethodsHandler

    public class VerticalDatumHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "VERTICAL_DATUMS"; }
        }
        #endregion
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<VERTICAL_DATUMS>() };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "getOPVDatum")]
        public OperationResult getOPVDatum(Int32 objectivePointId)
        {
            VERTICAL_DATUMS vDatum = null;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    vDatum = aSTNE.OBJECTIVE_POINT.FirstOrDefault(i => i.OBJECTIVE_POINT_ID == objectivePointId).VERTICAL_DATUMS;

                    if (vDatum != null)
                        vDatum.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = vDatum };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMVDatum")]
        public OperationResult GetHWMVDatum(Int32 hwmId)
        {
            VERTICAL_DATUMS vDatum = null;

            //Return BadRequest if there is no ID
            if (hwmId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    vDatum = aSTNE.HWMs.FirstOrDefault(i => i.HWM_ID == hwmId).VERTICAL_DATUMS;

                    if (vDatum != null)
                        vDatum.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = vDatum };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(VERTICAL_DATUMS anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, VERTICAL_DATUMS entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref VERTICAL_DATUMS anEntity)
        {
            VERTICAL_DATUMS existingEntity;
            VERTICAL_DATUMS thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.VERTICAL_DATUMS.FirstOrDefault(mt => (string.Equals(mt.DATUM_NAME.ToUpper(), thisEntity.DATUM_NAME.ToUpper()) || string.IsNullOrEmpty(thisEntity.DATUM_NAME)) &&
                                                                                (string.Equals(mt.DATUM_ABBREVIATION.ToUpper(), thisEntity.DATUM_ABBREVIATION.ToUpper()) || string.IsNullOrEmpty(thisEntity.DATUM_ABBREVIATION)));
                }
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
        #endregion
    }//end VerticalDatumHandler

}//end namespace