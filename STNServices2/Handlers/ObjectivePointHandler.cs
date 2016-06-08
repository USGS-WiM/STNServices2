//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles Objective Point resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.08.16 - TR -Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class ObjectivePointHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<objective_point> entities = null;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<objective_point>().OrderBy(e => e.objective_point_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            objective_point anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<objective_point>().FirstOrDefault(e => e.objective_point_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVDatumOPs")]
        public OperationResult GetVDatumOPs(Int32 vdatumId)
        {
            List<objective_point> entities = null;

            try
            {
                if (vdatumId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<objective_point>().Where(op => op.vdatum_id == vdatumId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteObjectivePoints")]
        public OperationResult GetSiteObjectivePoints(Int32 siteId)
        {
            List<objective_point> entities = null;

            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<objective_point>().Where(rp => rp.site_id == siteId).OrderBy(rp => rp.objective_point_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(objective_point anEntity)
        {            
            try
            {
                if (string.IsNullOrEmpty(anEntity.name) || string.IsNullOrEmpty(anEntity.description) || anEntity.op_type_id <= 0 ||
                    !anEntity.date_established.HasValue || anEntity.site_id <= 0 || anEntity.vdatum_id <= 0)
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<objective_point>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, objective_point anEntity)
        {
            try
            {
                if (entityId <= 0 || string.IsNullOrEmpty(anEntity.name) || string.IsNullOrEmpty(anEntity.description) || anEntity.op_type_id <= 0 ||
                    !anEntity.date_established.HasValue || anEntity.site_id <= 0 || anEntity.vdatum_id <= 0) 
                    throw new BadRequestException("Invalid input parameters");
                
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<objective_point>(anEntity);
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
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        objective_point ObjectToBeDeleted = sa.Select<objective_point>().Include(i => i.files).Include(i => i.op_control_identifier).FirstOrDefault(i => i.objective_point_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();
                                               
                        //remove files
                        ObjectToBeDeleted.files.ToList().ForEach(f => sa.RemoveFileItem(f));
                        ObjectToBeDeleted.files.Clear();
                        
                        //delete op_control_identifiers for this op
                        ObjectToBeDeleted.op_control_identifier.Clear();
                       
                        sa.Delete<objective_point>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
    }//end class ObjectivePointHandler
}//end namespace