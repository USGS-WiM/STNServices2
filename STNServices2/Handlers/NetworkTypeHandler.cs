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
    public class NetworkTypeHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<network_type> entities;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<network_type>().OrderBy(netType => netType.network_type_id)
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
            network_type anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<network_type>().SingleOrDefault(
                                            netName => netName.network_type_id == entityId);
                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteNetworkTypes")]
        public OperationResult GetSiteNetworkTypes(Int32 siteId)
        {
            List<network_type> entities;

            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<network_type_site>().Where(nt => nt.site_id == siteId)
                                                                        .Select(nt => nt.network_type).ToList();
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
        public OperationResult Post(network_type anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.network_type_name)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<network_type>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

   
        //[STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        //[HttpOperation(HttpMethod.POST, ForUriName = "AddSiteNetworkType")]
        //public OperationResult AddSiteNetworkType(Int32 siteId, NETWORK_TYPE aNetworkType)
        //{
        //    NETWORK_TYPE_SITE aSiteNetworkType = null;
        //    List<NETWORK_TYPE> networkTypeList = null;
        //    //Return BadRequest if missing required fields
        //    if (siteId <= 0 || String.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME))
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

        //                if (!Exists(aSTNE.NETWORK_TYPE, ref aNetworkType))
        //                {
        //                    //set ID
        //                    aSTNE.NETWORK_TYPE.AddObject(aNetworkType);
        //                    aSTNE.SaveChanges();
        //                }//end if

        //                //add to site
        //                //first check if site already contains this networktype
        //                if (aSTNE.NETWORK_TYPE_SITE.FirstOrDefault(nt => nt.NETWORK_TYPE_ID == aNetworkType.NETWORK_TYPE_ID &&
        //                                                                    nt.SITE_ID == siteId) == null)
        //                {//create one
        //                    aSiteNetworkType = new NETWORK_TYPE_SITE();
        //                    aSiteNetworkType.SITE_ID = siteId;
        //                    aSiteNetworkType.NETWORK_TYPE_ID = aNetworkType.NETWORK_TYPE_ID;
        //                    aSTNE.NETWORK_TYPE_SITE.AddObject(aSiteNetworkType);
        //                    aSTNE.SaveChanges();
        //                }//end if

        //                //return list of network types
        //                networkTypeList = aSTNE.NETWORK_TYPE.Where(nt => nt.NETWORK_TYPE_SITE.Any(nts => nts.SITE_ID == siteId)).ToList();

        //                if (networkTypeList != null)
        //                    networkTypeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
        //            }//end using
        //        }//end using
        //        //return object to verify persistance
        //        return new OperationResult.OK { ResponseResource = networkTypeList };
        //    }
        //    catch
        //    { return new OperationResult.BadRequest(); }
        //}

        #endregion
        #region PutMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, network_type anEntity)
        {
            try
            {
                if (entityId <= 0 || string.IsNullOrEmpty(anEntity.network_type_name)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<network_type>(entityId, anEntity);
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
                        network_type ObjectToBeDeleted = sa.Select<network_type>().SingleOrDefault(n => n.network_type_id == entityId);

                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException();
                        sa.Delete<network_type>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE

        //[STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        //[HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkType")]
        //public OperationResult RemoveSiteNetworkType(Int32 siteId, NETWORK_TYPE aNetworkType)
        //{
        //    //Return BadRequest if missing required fields
        //    if (siteId <= 0 || String.IsNullOrEmpty(aNetworkType.NETWORK_TYPE_NAME))
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
        //                NETWORK_TYPE_SITE thisNetTypeSite = aSTNE.NETWORK_TYPE_SITE.FirstOrDefault(nts => nts.NETWORK_TYPE_ID == aNetworkType.NETWORK_TYPE_ID &&
        //                                                                                     nts.SITE_ID == siteId);

        //                if (thisNetTypeSite != null)
        //                {
        //                    //remove it
        //                    aSTNE.NETWORK_TYPE_SITE.DeleteObject(thisNetTypeSite);
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