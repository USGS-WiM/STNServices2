﻿//------------------------------------------------------------------------------

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
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Authentication;

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
                    entities = sa.Select<vertical_datums>().FirstOrDefault(vd => vd.datum_id == vdatumId).objective_point.ToList();
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

        [HttpOperation(ForUriName = "GetSiteObjectivePoints")]
        public OperationResult GetSiteObjectivePoints(Int32 siteId)
        {
            List<objective_point> entities = null;

            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<objective_point>().Where(rp => rp.site.site_id == siteId).OrderBy(rp => rp.objective_point_id).ToList();
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
                    !anEntity.date_established.HasValue || anEntity.site_id <= 0)
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
                    !anEntity.date_established.HasValue || anEntity.site_id <= 0) 
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
            objective_point anEntity = null;
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    { 
                        //delete files associated with this op
                        //List<FILES> opFiles = aSTNE.FILES.Where(x => x.OBJECTIVE_POINT_ID == entityId).ToList();
                        //if (opFiles.Count >= 1)
                        //{
                        //    foreach (FILES f in opFiles)
                        //    {

                        //        //delete the file item from s3
                        //        S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                        //        aBucket.DeleteObject(BuildFilePath(f, f.PATH));

                        //        //delete the file
                        //        aSTNE.FILES.DeleteObject(f);
                        //        aSTNE.SaveChanges();
                        //    }
                        //}
                        ////delete op_control_identifiers for this op
                        //List<OP_CONTROL_IDENTIFIER> opci = aSTNE.OP_CONTROL_IDENTIFIER.Where(x => x.OBJECTIVE_POINT_ID == entityId).ToList();
                        //if (opci.Count >= 1)
                        //{
                        //    foreach (OP_CONTROL_IDENTIFIER o in opci)
                        //    {
                        //        aSTNE.OP_CONTROL_IDENTIFIER.DeleteObject(o);
                        //        aSTNE.SaveChanges();
                        //    }
                        //}

                        anEntity = sa.Select<objective_point>().FirstOrDefault(i => i.objective_point_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<objective_point>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
    }//end class ObjectivePointHandler
}//end namespace