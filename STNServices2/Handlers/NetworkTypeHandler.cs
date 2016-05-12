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
                    entities = sa.Select<network_type>().OrderBy(netType => netType.network_type_id).ToList();
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
            network_type anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<network_type>().SingleOrDefault(netName => netName.network_type_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException();
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
                using (STNAgent sa = new STNAgent(true))
                {
                    entities = sa.Select<network_type_site>().Where(nt => nt.site_id == siteId).Select(nt => nt.network_type).ToList();
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


        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "addSiteNetworkType")]
        public OperationResult AddNetworkType(Int32 siteId, Int32 networkTypeId)
        {
            network_type_site anEntity = null;
            List<network_type> networkTypeList = null;
            try
            {
                if (siteId <= 0 || networkTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword, true))
                    {
                        if (sa.Select<site>().First(s => s.site_id == siteId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<network_type>().First(n => n.network_type_id == networkTypeId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<network_type_site>().FirstOrDefault(nt => nt.network_type_id == networkTypeId && nt.site_id == siteId) == null)
                        {
                            anEntity = new network_type_site();
                            anEntity.site_id = siteId;
                            anEntity.network_type_id = networkTypeId;
                            anEntity = sa.Add<network_type_site>(anEntity);
                            sm(sa.Messages);
                        }
                        //return list of network types
                        networkTypeList = sa.Select<network_type>().Where(nn => nn.network_type_site.Any(nns => nns.site_id == siteId)).ToList();
                        
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = networkTypeList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

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

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteNetworkType")]
        public OperationResult RemoveSiteNetworkType(Int32 siteId, Int32 networkTypeId)
        {
             try
            {
                if (siteId <= 0 || networkTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (sa.Select<site>().First(s => s.site_id == siteId) == null)
                            throw new NotFoundRequestException();

                        network_type_site ObjectToBeDeleted = sa.Select<network_type_site>().SingleOrDefault(nns => nns.site_id == siteId && nns.network_type_id == networkTypeId);
                      
                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException();
                        sa.Delete<network_type_site>(ObjectToBeDeleted);
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