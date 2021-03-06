﻿//------------------------------------------------------------------------------
//----- ApprovalHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+
//------------------------------------------------------------------------------
// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
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
using System.Data.Entity;
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
                    if (thisEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

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
            approval anEntity;

            //Return BadRequest if there is no ID
            if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<hwm>().Include(h=>h.approval).FirstOrDefault(h => h.hwm_id == hwmId).approval;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileApproval")]
        public OperationResult GetDataFileApproval(Int32 datafileId)
        {
            approval anEntity;

            //Return BadRequest if there is no ID
            if (datafileId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<data_file>().Include(df=>df.approval).FirstOrDefault(df => df.data_file_id == datafileId).approval;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
            approval anEntity = null;
            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

                anEntity = new approval();

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
                        anEntity.approval_date = DateTime.UtcNow;
                        anEntity.member_id = user.member_id;

                        anEntity = sa.Add<approval>(anEntity);
                        
                        //set HWM approvalID
                        aHWM.approval_id = anEntity.approval_id;

                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        aHWM.last_updated = DateTime.Now;
                        aHWM.last_updated_by = loggedInUserId;

                        sa.Update<hwm>(aHWM);
                        sm(sa.Messages);
                
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString  };
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
            approval anEntity = null;
            Int32 loggedInUserId = 0;
            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                anEntity = new approval();

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;

                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId);
                        if (aDataFile == null) throw new BadRequestException("invalid HWMID");

                        //get logged in user
                        member user = sa.Select<member>().FirstOrDefault(u => String.Equals(u.username.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (user == null) throw new BadRequestException("invalid username response");

                        //set time and user of approval
                        anEntity.approval_date = DateTime.UtcNow;
                        anEntity.member_id = user.member_id;

                        anEntity = sa.Add<approval>(anEntity);

                        //set datafile approvalID
                        aDataFile.approval_id = anEntity.approval_id;
                        // last_update parts
                        aDataFile.last_updated = DateTime.Now;
                        aDataFile.last_updated_by = loggedInUserId;

                        sa.Update<data_file>(aDataFile);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveNWISDataFile")]
        public OperationResult ApproveNWISDataFile(Int32 dataFileId)
        {
            data_file aDataFile = null;
            approval anEntity = null;
            Int32 loggedInUserId = 0;
            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                anEntity = new approval();

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;

                        aDataFile = sa.Select<data_file>().Include(df=> df.instrument).SingleOrDefault(df => df.data_file_id == dataFileId);
                        if (aDataFile == null) throw new BadRequestException("invalid datafileId");

                        //get event coordinator to set as approver for NWIS datafile
                        member eventCoor = sa.Select<member>().Include(m => m.events).FirstOrDefault(m => m.events.Any(e => e.event_id == aDataFile.instrument.event_id));
                        if (eventCoor == null) throw new BadRequestException("invalid event coordinator");

                        //set time and user of approval
                        anEntity.approval_date = DateTime.UtcNow;
                        anEntity.member_id = eventCoor.member_id;

                        anEntity = sa.Add<approval>(anEntity);

                        //set datafile approvalID
                        aDataFile.approval_id = anEntity.approval_id;
                        aDataFile.last_updated = DateTime.Now;
                        aDataFile.last_updated_by = loggedInUserId;

                        sa.Update<data_file>(aDataFile);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
            approval anEntity = null;
            Int32 loggedInUserId = 0;

            try
            {
                if (approvalId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;

                        //fetch the object to be updated (assuming that it exists)
                        anEntity = sa.Select<approval>().Include(a=> a.hwms).Include(a=> a.data_file).SingleOrDefault(appr => appr.approval_id == approvalId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
                        
                        //remove id from HWM or DF
                        if (anEntity.hwms.Count > 0) anEntity.hwms.ToList().ForEach(x => 
                            {
                                x.approval_id = null;
                                // last updated parts                               
                                x.last_updated = DateTime.Now;
                                x.last_updated_by = loggedInUserId;
                                sa.Update<hwm>(x);
                            });
                        if (anEntity.data_file.Count > 0) anEntity.data_file.ToList().ForEach(x => 
                            {
                                x.approval_id = null;
                                x.last_updated = DateTime.Now;
                                x.last_updated_by = loggedInUserId;
                                sa.Update<data_file>(x);
                            });

                        //delete it
                        sa.Delete<approval>(anEntity);
                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
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
                        aHWM = sa.Select<hwm>().SingleOrDefault(h => h.hwm_id == hwmId);
                        if (aHWM == null) throw new WiM.Exceptions.NotFoundRequestException();

                        Int32 apprId = aHWM.approval_id.HasValue ? aHWM.approval_id.Value : 0;
                        
                        //remove id from hwm
                        aHWM.approval_id = null;
                        
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        aHWM.last_updated = DateTime.Now;
                        aHWM.last_updated_by = loggedInUserId;

                        sa.Update<hwm>(aHWM);

                        approval anApproval = sa.Select<approval>().FirstOrDefault(a => a.approval_id == apprId);
                        //remove approval     
                        sa.Delete<approval>(anApproval);
                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
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
            Int32 loggedInUserId = 0;
            try
            {
                //Return BadRequest if missing required fields
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;

                        //fetch the object to be updated (assuming that it exists)
                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == dataFileId);
                        if (aDataFile == null) throw new WiM.Exceptions.NotFoundRequestException();

                        Int32 apprId = aDataFile.approval_id.HasValue ? aDataFile.approval_id.Value : 0;
                        //remove id from hwm
                        aDataFile.approval_id = null;
                        aDataFile.last_updated = DateTime.Now;
                        aDataFile.last_updated_by = loggedInUserId;
                        sa.Update<data_file>(aDataFile);
                        
                        approval anApproval = sa.Select<approval>().FirstOrDefault(a => a.approval_id == apprId);
                        //remove approval     
                        sa.Delete<approval>(anApproval);
                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
        
    }//end class ApprovalHandler
}//end namespace