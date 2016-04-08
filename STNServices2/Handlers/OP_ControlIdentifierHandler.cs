//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles OP Control Identifiers resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.08.16 - TR - Created

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
    public class OP_ControlIdentifierHandler : STNHandlerBase
    {   
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<op_control_identifier> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<op_control_identifier>().OrderBy(m => m.op_control_identifier_id).ToList();

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
            op_control_identifier anEntity;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<op_control_identifier>().SingleOrDefault(m => m.op_control_identifier_id == entityId);
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

        [HttpOperation(HttpMethod.GET, ForUriName = "OPControls")]
        public OperationResult OPControls(Int32 objectivePointId)
        {
            List<op_control_identifier> anEntity;

            try
            {
                if (objectivePointId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<op_control_identifier>().Where(m => m.objective_point_id == objectivePointId).ToList();
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

        #endregion

        #region PostMethods

        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(op_control_identifier anEntity)
        {            
            try
            {
                if (anEntity.objective_point_id <= 0 || string.IsNullOrEmpty(anEntity.identifier) || string.IsNullOrEmpty(anEntity.identifier_type))
                    throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent())
                    {
                        anEntity = sa.Add<op_control_identifier>(anEntity);
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
        public OperationResult Put(Int32 entityId, op_control_identifier anEntity)
        {
            try
            {
                if (anEntity.objective_point_id <= 0 || string.IsNullOrEmpty(anEntity.identifier) || string.IsNullOrEmpty(anEntity.identifier_type))
                    throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<op_control_identifier>(anEntity);
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
            op_control_identifier anEntity = null;
            
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

                        anEntity = sa.Select<op_control_identifier>().FirstOrDefault(i => i.op_control_identifier_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<op_control_identifier>(anEntity);
                        sm(sa.Messages);
                    }
                }
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

      
       

    }//end class PeakSummaryHandler
}//end namespace