//------------------------------------------------------------------------------
//----- ApprovalHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+
//------------------------------------------------------------------------------
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
// 03.23.16 - JKN - Created
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
using WiM.Security;
using STNServices2.Security;

namespace STNServices2.Handlers
{
    public class ApprovalHandler : STNHandlerBase
    {

        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<approval> entities = null;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<approval>().OrderBy(e => e.approval_id).ToList();

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
            approval thisEntity = null;

            //Return BadRequest if there is no ID
            if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    thisEntity = sa.Select<approval>().FirstOrDefault(i => i.approval_id == entityId);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = thisEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMApproval")]
        public OperationResult GetHWMApproval(Int32 hwmId)
        {
            approval thisEntity;

            //Return BadRequest if there is no ID
            if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    thisEntity = sa.Select<hwm>().FirstOrDefault(i => i.hwm_id == hwmId).approval;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = thisEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileApproval")]
        public OperationResult GetDataFileApproval(Int32 datafileId)
        {
            approval thisEntity;

            //Return BadRequest if there is no ID
            if (datafileId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    thisEntity = sa.Select<data_file>().FirstOrDefault(i => i.data_file_id == datafileId).approval;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = thisEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET
        #endregion

        #region PostMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveHWM")]
        public OperationResult ApproveHWM(Int32 hwmId)
        {
            hwm aHWM = null;
            approval anApproval = null;
            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

                anApproval = new approval();

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        aHWM = sa.Select<hwm>().SingleOrDefault(hwm => hwm.hwm_id == hwmId);
                        if (aHWM == null) throw new BadRequestException("invalid HWMID" );

                        //get logged in user
                        member user = sa.Select<member>().FirstOrDefault(u => String.Equals(u.username.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (user == null) throw new BadRequestException("invalid username response");

                        //set time and user of approval
                        anApproval.approval_date = DateTime.UtcNow;
                        anApproval.member_id = user.member_id;

                        anApproval = sa.Add<approval>(anApproval);
                        
                        //set HWM approvalID
                        aHWM.approval_id = anApproval.approval_id;
                        sa.Update<hwm>(aHWM);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anApproval };
            }
            catch(Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.POST

        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveDataFile")]
        public OperationResult ApproveDataFile(Int32 dataFileId)
        {
            data_file aDataFile = null;
            approval anApproval = null;
            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                anApproval = new approval();

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId);
                        if (aDataFile == null) throw new BadRequestException("invalid HWMID");

                        //get logged in user
                        member user = sa.Select<member>().FirstOrDefault(u => String.Equals(u.username.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (user == null) throw new BadRequestException("invalid username response");

                        //set time and user of approval
                        anApproval.approval_date = DateTime.UtcNow;
                        anApproval.member_id = user.member_id;

                        anApproval = sa.Add<approval>(anApproval);

                        //set HWM approvalID
                        aDataFile.approval_id = anApproval.approval_id;
                        sa.Update<data_file>(aDataFile);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anApproval };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST
        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "Delete")]
        public OperationResult Delete(Int32 approvalId)
        {
            approval ObjectToBeDeleted = null;
            try
            {
                if (approvalId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = sa.Select<approval>().SingleOrDefault(appr => appr.approval_id == approvalId);

                        //remove id from HWM or DF
                        if (ObjectToBeDeleted.hwms.Count > 0) ObjectToBeDeleted.hwms.ToList().ForEach(x => { x.approval_id = null; sa.Update<hwm>(x); });
                        if (ObjectToBeDeleted.data_file.Count > 0) ObjectToBeDeleted.data_file.ToList().ForEach(x => { x.approval_id = null; sa.Update<data_file>(x); });

                        //delete it
                        sa.Delete<approval>(ObjectToBeDeleted);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HTTP.DELETE

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "UnApproveHWM")]
        public OperationResult UnApproveHWM(Int32 hwmId)
        {
            hwm aHWM = null;
            try
            {
                //Return BadRequest if missing required fields
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        aHWM = sa.Select<hwm>().SingleOrDefault(hwm => hwm.hwm_id == hwmId);
                        //remove approval
                        sa.Delete<approval>(aHWM.approval);
                        //remove id from hwm
                        aHWM.approval_id = null;
                        sa.Update<hwm>(aHWM);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE

        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "UnApproveDataFile")]
        public OperationResult UnApproveDataFile(Int32 dataFileId)
        {
            data_file aDataFile = null;
            try
            {
                //Return BadRequest if missing required fields
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId);
                        //remove approval
                        sa.Delete<approval>(aDataFile.approval);
                        //remove id from hwm
                        aDataFile.approval_id = null;
                        sa.Update<data_file>(aDataFile);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
        
    }//end class ApprovalHandler
}//end namespace