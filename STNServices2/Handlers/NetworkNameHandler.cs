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
using System.Data.Entity;

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
                    entities = sa.Select<network_name>().OrderBy(netType => netType.network_name_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
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
                    anEntity = sa.Select<network_name>().SingleOrDefault(nn => nn.network_name_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException();
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

                    entities = sa.Select<network_name_site>().Include(n => n.network_name).Include(n => n.site).Where(nns => nns.site_id == siteId).Select(nn => nn.network_name).ToList();
                    sm(MessageType.info, "Count: " + entities.Count()); 
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
        [HttpOperation(HttpMethod.POST)]
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

        //posts relationship, then returns list of network_names for the anEntity.site_id
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "addSiteNetworkName")]
        public OperationResult AddNetworkName(Int32 siteId, Int32 networkNameId)
        {
            network_name_site anEntity = null;
            List<network_name> networkNameList = null;
            try
            {
                if (siteId <= 0 || networkNameId <= 0) throw new BadRequestException("Invalid input parameters");
                
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword, true))
                    {
                        if (sa.Select<site>().First(s => s.site_id == siteId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<network_name>().First(n=> n.network_name_id == networkNameId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<network_name_site>().FirstOrDefault(nt => nt.network_name_id == networkNameId && nt.site_id == siteId) == null)
                        {
                            anEntity = new network_name_site();
                            anEntity.site_id = siteId;
                            anEntity.network_name_id = networkNameId;
                            anEntity = sa.Add<network_name_site>(anEntity);
                            sm(sa.Messages);
                        }
                        //return list of network types
                        networkNameList = sa.Select<network_name>().Where(nn => nn.network_name_site.Any(nns => nns.site_id == siteId)).ToList();
                        
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = networkNameList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

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
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
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

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkName")]
        public OperationResult RemoveNetworkName(Int32 siteId, Int32 networkNameId)
        {           
            try
            {
                if (siteId <= 0 || networkNameId <= 0) throw new BadRequestException("Invalid input parameters");
                
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (sa.Select<site>().First(s => s.site_id == siteId) == null)
                            throw new NotFoundRequestException();

                        network_name_site ObjectToBeDeleted = sa.Select<network_name_site>().SingleOrDefault(nns => nns.site_id == siteId && nns.network_name_id == networkNameId);
                      
                        if (ObjectToBeDeleted != null) 
                            sa.Delete<network_name_site>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        #endregion

    }//end class NetworkTypeHandler
}//end namespace