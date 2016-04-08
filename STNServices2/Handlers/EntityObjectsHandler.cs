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

    
}//end namespace