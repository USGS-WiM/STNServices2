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
// 04.18.2016 - jkn - Refactored
// 09.29.14 - TR - Created
#endregion

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class NetworkNameHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<network_name> entities;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<network_name>().OrderBy(netType => netType.network_name_id)
                                    .ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            network_name anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<network_name>().SingleOrDefault(
                                            netName => netName.network_name_id == entityId);
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteNetworkNames")]
        public OperationResult GetSiteNetworkNames(Int32 siteId)
        {
            List<network_name> entities;

            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<network_name_site>().Where(nt => nt.site_id == siteId)
                                                                        .Select(nt => nt.network_name).ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end httpMethod get

        #endregion
        #region PostMethods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateNetworkName")]
        public OperationResult Post(network_name anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.name)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<network_name>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        //[STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        //[HttpOperation(HttpMethod.POST, ForUriName = "AddSiteNetworkName")]
        //public OperationResult AddSiteNetworkName(Int32 siteId, network_name aNetworkName)
        //{
        //    NETWORK_NAME_SITE aSiteNetworkName = null;
        //    List<NETWORK_NAME> networkNameList = null;
        //    //Return BadRequest if missing required fields
        //    if (siteId <= 0 || String.IsNullOrEmpty(aNetworkName.NAME))
        //    { return new OperationResult.BadRequest(); }

        //    try
        //    {
        //        //Get basic authentication password
        //        using (EasySecureString securedPassword = GetSecuredPassword())
        //        {
        //            using (STNEntities2 aSTNE = GetRDS(securedPassword))
        //            {
        //                //check if valid site
        //                if (aSTNE.SITES.First(s => s.SITE_ID == siteId) == null)
        //                    return new OperationResult.BadRequest { Description = "Site does not exists" };

        //                if (!Exists(aSTNE.NETWORK_NAME, ref aNetworkName))
        //                {
        //                    //set ID
        //                    aSTNE.NETWORK_NAME.AddObject(aNetworkName);
        //                    aSTNE.SaveChanges();
        //                }//end if

        //                //add to site
        //                //first check if site already contains this networktype
        //                if (aSTNE.NETWORK_NAME_SITE.FirstOrDefault(nt => nt.NETWORK_NAME_ID == aNetworkName.NETWORK_NAME_ID &&
        //                                                                    nt.SITE_ID == siteId) == null)
        //                {//create one
        //                    aSiteNetworkName = new NETWORK_NAME_SITE();
        //                    aSiteNetworkName.SITE_ID = siteId;
        //                    aSiteNetworkName.NETWORK_NAME_ID = aNetworkName.NETWORK_NAME_ID;
        //                    aSTNE.NETWORK_NAME_SITE.AddObject(aSiteNetworkName);
        //                    aSTNE.SaveChanges();
        //                }//end if

        //                //return list of network types
        //                networkNameList = aSTNE.NETWORK_NAME.Where(nt => nt.NETWORK_NAME_SITE.Any(nts => nts.SITE_ID == siteId)).ToList();

        //                if (networkNameList != null)
        //                    networkNameList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
        //            }//end using
        //        }//end using
        //        //return object to verify persistance
        //        return new OperationResult.OK { ResponseResource = networkNameList };
        //    }
        //    catch
        //    { return new OperationResult.BadRequest(); }
        //}

        #endregion
        #region PutMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, network_name anEntity)
        {
            try
            {
                if (entityId <= 0 || string.IsNullOrEmpty(anEntity.name)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<network_name>(entityId, anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Modified { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.PUT

        #endregion
        #region DeleteMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        network_name ObjectToBeDeleted = sa.Select<network_name>().SingleOrDefault(netName => netName.network_name_id == entityId);
                      
                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException();
                        sa.Delete<network_name>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE

        //[STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        //[HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkName")]
        //public OperationResult RemoveSiteNetworkName(Int32 siteId, NETWORK_NAME aNetworkName)
        //{
        //    //Return BadRequest if missing required fields
        //    if (siteId <= 0 || String.IsNullOrEmpty(aNetworkName.NAME))
        //    { return new OperationResult.BadRequest(); }

        //    try
        //    {
        //        //Get basic authentication password
        //        using (EasySecureString securedPassword = GetSecuredPassword())
        //        {
        //            using (STNEntities2 aSTNE = GetRDS(securedPassword))
        //            {
        //                //check if valid site
        //                if (aSTNE.SITES.First(s => s.SITE_ID == siteId) == null)
        //                    return new OperationResult.BadRequest { Description = "Site does not exist" };

        //                //remove from network type
        //                NETWORK_NAME_SITE thisNetNameSite = aSTNE.NETWORK_NAME_SITE.FirstOrDefault(nts => nts.NETWORK_NAME_ID == aNetworkName.NETWORK_NAME_ID &&
        //                                                                                     nts.SITE_ID == siteId);

        //                if (thisNetNameSite != null)
        //                {
        //                    //remove it
        //                    aSTNE.NETWORK_NAME_SITE.DeleteObject(thisNetNameSite);
        //                    aSTNE.SaveChanges();
        //                }//end if
        //            }//end using
        //        }//end using

        //        return new OperationResult.OK { };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}
        #endregion

    }//end class NetworkTypeHandler
}//end namespace