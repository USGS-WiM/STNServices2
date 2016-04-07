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

    public class HWMQualitiesHandler : STNHandlerBase
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

    public class HWMTypesHandler : STNHandlerBase
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

    public class InstrCollectConditionsHandler : STNHandlerBase
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

    public class LandOwnerHandler : STNHandlerBase
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

    public class MarkerHandler : STNHandlerBase
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

    public class ObjectivePointTypeHandler : STNHandlerBase
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

    public class OP_QualityHandler : STNHandlerBase
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

    public class RoleHandler : STNHandlerBase
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

    public class SensorBrandHandler : STNHandlerBase
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

    public class SensorDeploymentHandler : STNHandlerBase
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


    public class SensorTypeHandler : STNHandlerBase
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

    public class SourceHandler : STNHandlerBase
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

   


}//end namespace