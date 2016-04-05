//------------------------------------------------------------------------------
//----- NetworkTypeHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles NetworkType resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 10.28.13 - TR - Created
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

namespace STNServices2.Handlers
{
    public class NetworkTypeHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "NETWORKTYPE"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<NETWORK_TYPE> networkTypeList = new List<NETWORK_TYPE>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    networkTypeList = aSTNE.NETWORK_TYPE.OrderBy(netType => netType.NETWORK_TYPE_ID)
                                    .ToList();

                    if (networkTypeList != null)
                        networkTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = networkTypeList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            NETWORK_TYPE aNetworkType;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aNetworkType = aSTNE.NETWORK_TYPE.SingleOrDefault(
                                            netType => netType.NETWORK_TYPE_ID == entityId);

                    if (aNetworkType != null)
                        aNetworkType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aNetworkType };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteNetworkTypes")]
        public OperationResult GetSiteNetworkTypes(Int32 siteId)
        {
            List<NETWORK_TYPE> networkTypeList;

            //return Badrequest if there is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    networkTypeList = aSTNE.NETWORK_TYPE_SITE.Where(nt => nt.SITE_ID == siteId)
                                                                        .Select(nt => nt.NETWORK_TYPE)
                                                                        .OrderBy(n => n.NETWORK_TYPE_ID).ToList();
                }//end using

                if (networkTypeList != null)
                    networkTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = networkTypeList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateNetworkType")]
        public OperationResult Post(NETWORK_TYPE aNetworkType)
        {
            //Return BadRequest if missing required fields
            if (string.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME))
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
                        if (!Exists(aSTNE.NETWORK_TYPE, ref aNetworkType))
                        {
                            aSTNE.NETWORK_TYPE.AddObject(aNetworkType);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aNetworkType };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.POST

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSiteNetworkType")]
        public OperationResult AddSiteNetworkType(Int32 siteId, NETWORK_TYPE aNetworkType)
        {
            NETWORK_TYPE_SITE aSiteNetworkType = null;
            List<NETWORK_TYPE> networkTypeList = null;
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid site
                        if (aSTNE.SITES.First(s => s.SITE_ID == siteId) == null)
                            return new OperationResult.BadRequest { Description = "Site does not exists" };

                        if (!Exists(aSTNE.NETWORK_TYPE, ref aNetworkType))
                        {
                            //set ID
                            aSTNE.NETWORK_TYPE.AddObject(aNetworkType);
                            aSTNE.SaveChanges();
                        }//end if

                        //add to site
                        //first check if site already contains this networktype
                        if (aSTNE.NETWORK_TYPE_SITE.FirstOrDefault(nt => nt.NETWORK_TYPE_ID == aNetworkType.NETWORK_TYPE_ID &&
                                                                            nt.SITE_ID == siteId) == null)
                        {//create one
                            aSiteNetworkType = new NETWORK_TYPE_SITE();
                            aSiteNetworkType.SITE_ID = siteId;
                            aSiteNetworkType.NETWORK_TYPE_ID = aNetworkType.NETWORK_TYPE_ID;
                            aSTNE.NETWORK_TYPE_SITE.AddObject(aSiteNetworkType);
                            aSTNE.SaveChanges();
                        }//end if

                        //return list of network types
                        networkTypeList = aSTNE.NETWORK_TYPE.Where(nt => nt.NETWORK_TYPE_SITE.Any(nts => nts.SITE_ID == siteId)).ToList();

                        if (networkTypeList != null)
                            networkTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                    }//end using
                }//end using
                //return object to verify persistance
                return new OperationResult.OK { ResponseResource = networkTypeList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }

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
        public OperationResult Put(Int32 entityId, NETWORK_TYPE aNetworkType)
        {
            NETWORK_TYPE networkTypeToUpdate = null;
            //Return BadRequest if missing required fields
            if (aNetworkType.NETWORK_TYPE_ID <= 0 || entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //Grab the network Type row to update
                        networkTypeToUpdate = aSTNE.NETWORK_TYPE.SingleOrDefault(
                                           netType => netType.NETWORK_TYPE_ID == entityId);
                        //Update fields
                        networkTypeToUpdate.NETWORK_TYPE_NAME = (string.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME) ?
                            networkTypeToUpdate.NETWORK_TYPE_NAME : aNetworkType.NETWORK_TYPE_NAME);

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                if (aNetworkType != null)
                    aNetworkType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                return new OperationResult.OK { ResponseResource = aNetworkType };
            }
            catch
            { return new OperationResult.BadRequest(); }
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
                        NETWORK_TYPE ObjectToBeDeleted = aSTNE.NETWORK_TYPE.SingleOrDefault(netType => netType.NETWORK_TYPE_ID == entityId);
                        //delete it

                        aSTNE.NETWORK_TYPE.DeleteObject(ObjectToBeDeleted);

                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HTTP.DELETE

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkType")]
        public OperationResult RemoveSiteNetworkType(Int32 siteId, NETWORK_TYPE aNetworkType)
        {
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid site
                        if (aSTNE.SITES.First(s => s.SITE_ID == siteId) == null)
                            return new OperationResult.BadRequest { Description = "Site does not exist" };

                        //remove from network type
                        NETWORK_TYPE_SITE thisNetTypeSite = aSTNE.NETWORK_TYPE_SITE.FirstOrDefault(nts => nts.NETWORK_TYPE_ID == aNetworkType.NETWORK_TYPE_ID &&
                                                                                             nts.SITE_ID == siteId);

                        if (thisNetTypeSite != null)
                        {
                            //remove it
                            aSTNE.NETWORK_TYPE_SITE.DeleteObject(thisNetTypeSite);
                            aSTNE.SaveChanges();
                        }//end if
                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<NETWORK_TYPE> entityRDS, ref NETWORK_TYPE anEntity)
        {
            NETWORK_TYPE existingEntity;
            NETWORK_TYPE thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.NETWORK_TYPE_ID == thisEntity.NETWORK_TYPE_ID &&
                                                               e.NETWORK_TYPE_NAME == thisEntity.NETWORK_TYPE_NAME);


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

    }//end class NetworkTypeHandler
}//end namespace