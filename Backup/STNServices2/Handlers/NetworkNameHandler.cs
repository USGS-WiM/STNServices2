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
// 09.29.14 - TR - Created
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
    public class NetworkNameHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "NETWORKNAME"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<NETWORK_NAME> networkNameList = new List<NETWORK_NAME>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    networkNameList = aSTNE.NETWORK_NAME.OrderBy(netType => netType.NETWORK_NAME_ID)
                                    .ToList();

                    if (networkNameList != null)
                        networkNameList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = networkNameList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        /// 
        /// Force the user to provide authentication 
        /// 
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            NETWORK_NAME aNetworkName;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aNetworkName = aSTNE.NETWORK_NAME.SingleOrDefault(
                                            netName => netName.NETWORK_NAME_ID == entityId);

                    if (aNetworkName != null)
                        aNetworkName.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aNetworkName };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteNetworkNames")]
        public OperationResult GetSiteNetworkNames(Int32 siteId)
        {
            List<NETWORK_NAME> networkNameList;

            //return Badrequest if there is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    networkNameList = aSTNE.NETWORK_NAME_SITE.Where(nt => nt.SITE_ID == siteId)
                                                                        .Select(nt => nt.NETWORK_NAME)
                                                                        .OrderBy(n => n.NETWORK_NAME_ID).ToList();
                }//end using

                if (networkNameList != null)
                    networkNameList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                return new OperationResult.OK { ResponseResource = networkNameList };
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
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateNetworkName")]
        public OperationResult Post(NETWORK_NAME aNetworkName)
        {
            //Return BadRequest if missing required fields
            if (string.IsNullOrEmpty(aNetworkName.NAME))
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
                        if (!Exists(aSTNE.NETWORK_NAME, ref aNetworkName))
                        {
                            aSTNE.NETWORK_NAME.AddObject(aNetworkName);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aNetworkName };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.POST

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSiteNetworkName")]
        public OperationResult AddSiteNetworkName(Int32 siteId, NETWORK_NAME aNetworkName)
        {
            NETWORK_NAME_SITE aSiteNetworkName = null;
            List<NETWORK_NAME> networkNameList = null;
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aNetworkName.NAME))
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

                        if (!Exists(aSTNE.NETWORK_NAME, ref aNetworkName))
                        {
                            //set ID
                            aSTNE.NETWORK_NAME.AddObject(aNetworkName);
                            aSTNE.SaveChanges();
                        }//end if

                        //add to site
                        //first check if site already contains this networktype
                        if (aSTNE.NETWORK_NAME_SITE.FirstOrDefault(nt => nt.NETWORK_NAME_ID == aNetworkName.NETWORK_NAME_ID &&
                                                                            nt.SITE_ID == siteId) == null)
                        {//create one
                            aSiteNetworkName = new NETWORK_NAME_SITE();
                            aSiteNetworkName.SITE_ID = siteId;
                            aSiteNetworkName.NETWORK_NAME_ID = aNetworkName.NETWORK_NAME_ID;
                            aSTNE.NETWORK_NAME_SITE.AddObject(aSiteNetworkName);
                            aSTNE.SaveChanges();
                        }//end if

                        //return list of network types
                        networkNameList = aSTNE.NETWORK_NAME.Where(nt => nt.NETWORK_NAME_SITE.Any(nts => nts.SITE_ID == siteId)).ToList();

                        if (networkNameList != null)
                            networkNameList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                    }//end using
                }//end using
                //return object to verify persistance
                return new OperationResult.OK { ResponseResource = networkNameList };
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
        public OperationResult Put(Int32 entityId, NETWORK_NAME aNetworkName)
        {
            NETWORK_NAME networkNameToUpdate = null;
            //Return BadRequest if missing required fields
            if (aNetworkName.NETWORK_NAME_ID <= 0 || entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //Grab the network Type row to update
                        networkNameToUpdate = aSTNE.NETWORK_NAME.SingleOrDefault(
                                           netName => netName.NETWORK_NAME_ID == entityId);
                        //Update fields
                        networkNameToUpdate.NAME = (string.IsNullOrEmpty(aNetworkName.NAME) ?
                            networkNameToUpdate.NAME : aNetworkName.NAME);

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                if (aNetworkName != null)
                    aNetworkName.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                return new OperationResult.OK { ResponseResource = aNetworkName };
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
                        NETWORK_NAME ObjectToBeDeleted = aSTNE.NETWORK_NAME.SingleOrDefault(netName => netName.NETWORK_NAME_ID == entityId);
                        //delete it

                        aSTNE.NETWORK_NAME.DeleteObject(ObjectToBeDeleted);

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
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkName")]
        public OperationResult RemoveSiteNetworkName(Int32 siteId, NETWORK_NAME aNetworkName)
        {
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aNetworkName.NAME))
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
                        NETWORK_NAME_SITE thisNetNameSite = aSTNE.NETWORK_NAME_SITE.FirstOrDefault(nts => nts.NETWORK_NAME_ID == aNetworkName.NETWORK_NAME_ID &&
                                                                                             nts.SITE_ID == siteId);

                        if (thisNetNameSite != null)
                        {
                            //remove it
                            aSTNE.NETWORK_NAME_SITE.DeleteObject(thisNetNameSite);
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
        private bool Exists(ObjectSet<NETWORK_NAME> entityRDS, ref NETWORK_NAME anEntity)
        {
            NETWORK_NAME existingEntity;
            NETWORK_NAME thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.NAME.ToUpper() == thisEntity.NAME.ToUpper());


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