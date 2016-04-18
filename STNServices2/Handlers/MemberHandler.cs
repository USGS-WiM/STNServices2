//------------------------------------------------------------------------------
//----- MemberHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Peak summary resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     https://www.owasp.org/index.php/REST_Security_Cheat_Sheet

#region Comments
// 02.13.13 - JKN - added endpoint to return all members for an event, and updated stored procedure for adding new member
// 02.12.13 - JKN - added endpoint to add member to team (/CollectionTeams/{teamId}/AddMember)
// 01.25.13 - TR - added endpoint for members/{memberName}
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 10.24.12 - JB - Fixed check for null username
// 08.23.12 - JB - Fix from JKN on PUT method
// 07.16.12 - JKN - Added GetHWMPeakSummary and GetDataFilePeakSummary GET methods
// 07.09.12 - JKN - Created

#endregion

using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using System.Text;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using OpenRasta.Security;
using WiM.Security;
using STNServices2.Security;

namespace STNServices2.Handlers
{
    public class MemberHandler : STNHandlerBase
    {

        #region GetMethods

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<member> memberList = null;

            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        memberList = sa.Select<member>().OrderBy(m => m.member_id)
                                     .ToList();
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = memberList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        //[RequiresAuthentication]
        //[HttpOperation(ForUriName = "GetEventMembers")]
        //public OperationResult GetEventMembers(Int32 eventId)
        //{
        //    List<member> memberList = null;

        //    try
        //    {
        //        //Get basic authentication password
        //        using (EasySecureString securedPassword = GetSecuredPassword())
        //        {
        //            using (STNEntities2 aSTNE = GetRDS(securedPassword))
        //            {
        //                //give me all members that deployed sensor or did hwm for this event
        //                memberList = aSTNE.MEMBERS.Where(m => m.HWMs.Any(h => h.EVENT_ID == eventId) ||
        //                    m.INSTRUMENT_STATUS.Any(inst => inst.INSTRUMENT.EVENT_ID == eventId)).ToList();

        //                //memberList = aSTNE.MEMBERS.Where(mt => mt.COLLECT_TEAM.HWMs.Any(h => h.EVENT_ID == eventId) ||
        //                //                                            mt.COLLECT_TEAM.INSTRUMENT_STATUS.Any(instS => instS.INSTRUMENT.EVENT_ID == eventId))
        //                //                                    .Select(m => m.MEMBER).Distinct().OrderBy(m => m.MEMBER_ID).ToList();

        //                if (memberList != null)
        //                    memberList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //            }//end using
        //        }//end using

        //        return new OperationResult.OK { ResponseResource = memberList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end HttpMethod.GET       

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetAgencyMembers")]
        public OperationResult GetAgencyMembers(Int32 agencyId)
        {
            List<member> memberList = null;
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {

                        memberList = sa.Select<agency>().FirstOrDefault(a => a.agency_id == agencyId).members.ToList();
                        sm(MessageType.info, "Count: " + memberList.Count());
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = memberList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetRoleMembers")]
        public OperationResult GetRoleMembers(Int32 roleId)
        {
            List<member> memberList = null;
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {

                        memberList = sa.Select<role>().FirstOrDefault(a => a.role_id == roleId).members.ToList();
                        sm(MessageType.info, "Count: " + memberList.Count());
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = memberList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            member anEntity = null;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<member>().FirstOrDefault(e => e.member_id == entityId);
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventCoordinator")]
        public OperationResult GetEventCoordinator(Int32 eventId)
        {
            member anEntity = null;

            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<events>().FirstOrDefault(e => e.event_id == eventId).member;
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        //[HttpOperation(HttpMethod.GET, ForUriName = "GetByUserName")]
        //public OperationResult Get(string userName)
        //{
        //    MEMBER aMember;

        //    //Return BadRequest if there is no ID
        //    if (userName == null)
        //    { return new OperationResult.BadRequest(); }

        //    try
        //    {
        //        //Get basic authentication password
        //        using (EasySecureString securedPassword = GetSecuredPassword())
        //        {
        //            using (STNEntities2 aSTNE = GetRDS(securedPassword))
        //            {
        //                aMember = aSTNE.MEMBERS.SingleOrDefault(
        //                          m => String.Equals(m.USERNAME.ToUpper(), userName.ToUpper()));

        //                if (aMember != null)
        //                    aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

        //            }//end using
        //        }//end using

        //        return new OperationResult.OK { ResponseResource = aMember };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovingOfficial")]
        public OperationResult GetApprovalOfficial(Int32 ApprovalId)
        {            
            member anEntity = null;

            try
            {
                if (ApprovalId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<events>().FirstOrDefault(e => e.event_id == ApprovalId).member;
                        sm(sa.Messages);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileProcessor")]
        public OperationResult GetDataFileProcessor(Int32 dataFileId)
        {
            member anEntity = null;

            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<data_file>().FirstOrDefault(e => e.data_file_id == dataFileId).member;
                        sm(sa.Messages);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryProcessor")]
        public OperationResult GetPeakSummaryProcessor(Int32 peakSummaryId)
        {
            member anEntity = null;

            try
            {
                if (peakSummaryId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<peak_summary>().FirstOrDefault(e => e.peak_summary_id == peakSummaryId).member;
                        sm(sa.Messages);

                    }//end using
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
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddMember")]
        public OperationResult POST(member anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.fname) || string.IsNullOrEmpty(anEntity.lname) ||
                    string.IsNullOrEmpty(anEntity.username) || string.IsNullOrEmpty(anEntity.phone)||
                    string.IsNullOrEmpty(anEntity.email)|| anEntity.agency_id < 0 )
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (string.IsNullOrEmpty(anEntity.password))
                            anEntity.password = generateDefaultPassword(anEntity);
                        else
                            anEntity.password = Encoding.UTF8.GetString(Convert.FromBase64String(anEntity.password));

                        if (anEntity.role_id <= 0) anEntity.role_id = sa.Select<role>().SingleOrDefault(r => r.role_name == FieldRole).role_id;
                        anEntity.salt =  Cryptography.CreateSalt();
                        anEntity.password = Cryptography.GenerateSHA256Hash(anEntity.password, anEntity.salt);
                        anEntity = sa.Add<member>(anEntity);
                        sm(sa.Messages);
                        
                        //remove info not relevant
                        anEntity.password = string.Empty;
                        anEntity.salt = string.Empty;

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
        [RequiresAuthentication]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, member anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.fname) || string.IsNullOrEmpty(anEntity.lname) ||
                    string.IsNullOrEmpty(anEntity.username) || string.IsNullOrEmpty(anEntity.phone) ||
                    string.IsNullOrEmpty(anEntity.email) || anEntity.agency_id < 0)
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {


                        //fetch the object to be updated (assuming that it exists)
                        member ObjectToBeUpdated = sa.Select<member>().SingleOrDefault(m => m.member_id == entityId);
                        if (ObjectToBeUpdated == null) throw new NotFoundRequestException("Requested member not found.");

                        if (!IsAuthorizedToEdit(ObjectToBeUpdated.username))
                            return new OperationResult.Forbidden { Description="Not authorized to edit specified user" };

                        ObjectToBeUpdated.username = anEntity.username;
                        ObjectToBeUpdated.fname = anEntity.fname;
                        ObjectToBeUpdated.lname = anEntity.lname;
                        ObjectToBeUpdated.agency_id = anEntity.agency_id;
                        ObjectToBeUpdated.phone = anEntity.phone;
                        ObjectToBeUpdated.email = anEntity.email;
                        ObjectToBeUpdated.emergency_contact_name = (string.IsNullOrEmpty(anEntity.emergency_contact_name) ?
                            ObjectToBeUpdated.emergency_contact_name : anEntity.emergency_contact_name);
                        ObjectToBeUpdated.emergency_contact_phone = (string.IsNullOrEmpty(anEntity.emergency_contact_phone) ?
                            ObjectToBeUpdated.emergency_contact_phone : anEntity.emergency_contact_phone);
                        
                        //admin can only change role
                        if (IsAuthorized(AdminRole) && anEntity.role_id > 0)
                            ObjectToBeUpdated.role_id = anEntity.role_id;


                        if (!string.IsNullOrEmpty(anEntity.password) && !Cryptography
                            .VerifyPassword(Encoding.UTF8.GetString(Convert.FromBase64String(anEntity.password)), 
                                                                    ObjectToBeUpdated.salt, ObjectToBeUpdated.password))
                        {
                            ObjectToBeUpdated.password = Cryptography.GenerateSHA256Hash(Encoding.UTF8
                                .GetString(Convert.FromBase64String(anEntity.password)), ObjectToBeUpdated.salt);
                            sm(MessageType.info, "Password updated.");
                        }

                        anEntity = sa.Update<member>(entityId, ObjectToBeUpdated);
                        sm(sa.Messages);
                        //remove info not relevant
                        anEntity.password = string.Empty;
                        anEntity.salt = string.Empty;

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
        [STNRequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            member anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<member>().FirstOrDefault(i => i.member_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<member>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

        #region Helper Methods

        private string generateDefaultPassword(member dm)
        {   
            //STNDefau1t+numbercharInlastname+first2letterFirstName
            string generatedPassword = "STNDefau1t" + dm.lname.Count() + dm.fname.Substring(0, 2);
            sm(MessageType.info, "Generated Password: " + generatedPassword);
            
            return generatedPassword;
        }//end buildDefaultPassword
        #endregion

    }//end class PeakSummaryHandler
}//end namespace