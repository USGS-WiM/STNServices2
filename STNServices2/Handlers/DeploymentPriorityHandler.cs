﻿//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 03.28.16 - JKN - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Authentication;

namespace STNServices2.Handlers
{
    public class DeploymentPriorityHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<deployment_priority> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<deployment_priority>().OrderBy(e => e.priority_id).ToList();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            deployment_priority anEntity = null;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<deployment_priority>().FirstOrDefault(e => e.priority_id == entityId);

                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSitePriority")]
        public OperationResult GetSitePriority(Int32 siteId)
        {
            deployment_priority mdeployment_priority = null;

            //Return BadRequest if there is no ID
            if (siteId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    mdeployment_priority = sa.Select<site>().FirstOrDefault(i => i.site_id == siteId).deployment_priority;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = mdeployment_priority, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET
        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(deployment_priority anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<deployment_priority>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, deployment_priority anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<deployment_priority>(anEntity);
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

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            deployment_priority anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<deployment_priority>().FirstOrDefault(i => i.priority_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<deployment_priority>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end deployment_priorityHandler
}