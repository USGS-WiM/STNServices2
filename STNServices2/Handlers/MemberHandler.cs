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
//     

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

using STNServices2.Resources;
using STNServices2.Authentication;

using OpenRasta.Web;
using OpenRasta.Security;
using OpenRasta.Diagnostics;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;


namespace STNServices2.Handlers
{
    public class MemberHandler : STNHandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "MEMBERS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<MEMBER> memberList = new List<MEMBER>();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        memberList = aSTNE.MEMBERS.OrderBy(m => m.MEMBER_ID)
                                     .ToList();

                        if (memberList != null)
                            memberList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = memberList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetEventMembers")]
        public OperationResult GetEventMembers(Int32 eventId)
        {
            List<MEMBER> memberList = new List<MEMBER>();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //give me all members that deployed sensor or did hwm for this event
                        memberList = aSTNE.MEMBERS.Where(m => m.HWMs.Any(h => h.EVENT_ID == eventId) ||
                            m.INSTRUMENT_STATUS.Any(inst => inst.INSTRUMENT.EVENT_ID == eventId)).ToList();

                        //memberList = aSTNE.MEMBERS.Where(mt => mt.COLLECT_TEAM.HWMs.Any(h => h.EVENT_ID == eventId) ||
                        //                                            mt.COLLECT_TEAM.INSTRUMENT_STATUS.Any(instS => instS.INSTRUMENT.EVENT_ID == eventId))
                        //                                    .Select(m => m.MEMBER).Distinct().OrderBy(m => m.MEMBER_ID).ToList();

                        if (memberList != null)
                            memberList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = memberList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET       

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetAgencyMembers")]
        public OperationResult GetAgencyMembers(Int32 agencyId)
        {
            List<MEMBER> memberList = new List<MEMBER>();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        memberList = aSTNE.AGENCIES.FirstOrDefault(a => a.AGENCY_ID == agencyId).MEMBERs.ToList();

                        if (memberList != null)
                            memberList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = memberList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(ForUriName = "GetRoleMembers")]
        public OperationResult GetRoleMembers(Int32 roleId)
        {
            List<MEMBER> memberList = new List<MEMBER>();
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        memberList = aSTNE.ROLES.FirstOrDefault(a => a.ROLE_ID == roleId).MEMBERs.ToList();

                        if (memberList != null)
                            memberList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = memberList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.MEMBERS.SingleOrDefault(
                                  m => m.MEMBER_ID == entityId);

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventCoordinator")]
        public OperationResult GetEventCoordinator(Int32 eventId)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (eventId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.EVENTS.SingleOrDefault(
                                  e => e.EVENT_ID == eventId).MEMBER;

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetByUserName")]
        public OperationResult Get(string userName)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (userName == null)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.MEMBERS.SingleOrDefault(
                                  m => String.Equals(m.USERNAME.ToUpper(), userName.ToUpper()));

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovingOfficial")]
        public OperationResult GetApprovalOfficial(Int32 ApprovalId)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (ApprovalId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.APPROVALs.FirstOrDefault(
                                  a => a.APPROVAL_ID == ApprovalId).MEMBER;

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileProcessor")]
        public OperationResult GetDataFileProcessor(Int32 dataFileId)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (dataFileId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.DATA_FILE.FirstOrDefault(
                                  a => a.DATA_FILE_ID == dataFileId).MEMBER;

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryProcessor")]
        public OperationResult GetPeakSummaryProcessor(Int32 peakSummaryId)
        {
            MEMBER aMember;

            //Return BadRequest if there is no ID
            if (peakSummaryId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        aMember = aSTNE.PEAK_SUMMARY.FirstOrDefault(
                                  a => a.PEAK_SUMMARY_ID == peakSummaryId).MEMBER;

                        if (aMember != null)
                            aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "ChangeMembersPassword")]
        public OperationResult ChangeMembersPassword(string userName, string newPassword)
        {
            MEMBER aMember;

            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(newPassword)))
            { return new OperationResult.BadRequest() { ResponseResource = "Invalid arguments" }; }

            //have to be admin or change their own password (both have to be false to get inside here)
            if (!IsAuthorized(AdminRole) && Context.User.Identity.Name != username)
            { return new OperationResult.Forbidden(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        aMember = aSTNE.MEMBERS.SingleOrDefault(
                                                                m => m.USERNAME == userName);

                        if (aMember == null)
                        { return new OperationResult.BadRequest() { ResponseResource = "no member exists" }; }

                        // edit user profile using db stored procedure
                        aSTNE.USERPROFILE_EDITPASSWORD(aMember.USERNAME, newPassword);

                    }//end using
                }//end using

                if (aMember != null)
                    aMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddMember")]
        public OperationResult AddMember(string pass, MEMBER aMember)
        {
            MEMBER createdMember = new MEMBER();

            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(aMember.USERNAME) || aMember.ROLE_ID <= 0))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        if (!Exists(aSTNE.MEMBERS, ref aMember))
                        {
                            //Prior to running stored procedure, check if username exists
                            if (aSTNE.MEMBERS.FirstOrDefault(m => String.Equals(m.USERNAME.ToUpper().Trim(),
                                                                                aMember.USERNAME.ToUpper().Trim())) != null)
                                return new OperationResult.BadRequest { Description = "Username exists" };

                            // Create user profile using db stored procedure
                            aSTNE.USERPROFILE_ADD(aMember.USERNAME, pass);
                            aSTNE.USERPROFILE_ADDROLE(aMember.USERNAME, aMember.ROLE_ID);
                            aSTNE.MEMBERS.AddObject(aMember);
                            aSTNE.SaveChanges();

                        }//end if
                        createdMember = aSTNE.MEMBERS.FirstOrDefault(m => String.Equals(m.USERNAME.ToUpper().Trim(),
                                                                                aMember.USERNAME.ToUpper().Trim()));
                        if (createdMember != null)
                            createdMember.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }//end using

                }//end using

                return new OperationResult.OK { ResponseResource = createdMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key
         * 
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, MEMBER aMember)
        {
            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(aMember.USERNAME) || aMember.ROLE_ID <= 0))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        //fetch the object to be updated (assuming that it exists)
                        MEMBER ObjectToBeUpdated = aSTNE.MEMBERS.Single(m => m.MEMBER_ID == entityId);

                        //return bad request if member logged in is not admin (can update anyone) or is not the member they are editing(can only update yourself)
                        if (Context.User.Identity.Name != ObjectToBeUpdated.USERNAME && Context.User.IsInRole("Admin") == false)
                            return new OperationResult.BadRequest();


                        //FirstName
                        ObjectToBeUpdated.FNAME = (string.IsNullOrEmpty(aMember.FNAME) ?
                            ObjectToBeUpdated.FNAME : aMember.FNAME);
                        //Last Name
                        ObjectToBeUpdated.LNAME = (string.IsNullOrEmpty(aMember.LNAME) ?
                            ObjectToBeUpdated.LNAME : aMember.LNAME);
                        //AgencyID
                        ObjectToBeUpdated.AGENCY_ID = (Decimal.Equals(aMember.AGENCY_ID, ObjectToBeUpdated.AGENCY_ID) ?
                            ObjectToBeUpdated.AGENCY_ID : aMember.AGENCY_ID);
                        //Phone
                        ObjectToBeUpdated.PHONE = (string.IsNullOrEmpty(aMember.PHONE) ?
                            ObjectToBeUpdated.PHONE : aMember.PHONE);
                        //Email
                        ObjectToBeUpdated.EMAIL = (string.IsNullOrEmpty(aMember.EMAIL) ?
                            ObjectToBeUpdated.EMAIL : aMember.EMAIL);
                        //RSSFeed
                        ObjectToBeUpdated.RSSFEED = (string.IsNullOrEmpty(aMember.RSSFEED) ?
                            ObjectToBeUpdated.RSSFEED : aMember.RSSFEED);
                        //EMERGENCY_CONTACT_NAME
                        ObjectToBeUpdated.EMERGENCY_CONTACT_NAME = (string.IsNullOrEmpty(aMember.EMERGENCY_CONTACT_NAME) ?
                            ObjectToBeUpdated.EMERGENCY_CONTACT_NAME : aMember.EMERGENCY_CONTACT_NAME);
                        //EMERGENCY_CONTACT_PHONE
                        ObjectToBeUpdated.EMERGENCY_CONTACT_PHONE = (string.IsNullOrEmpty(aMember.EMERGENCY_CONTACT_PHONE) ?
                            ObjectToBeUpdated.EMERGENCY_CONTACT_PHONE : aMember.EMERGENCY_CONTACT_PHONE);

                        aSTNE.SaveChanges();
                        //build alter statement to pass to resource file                 
                        /*string currentRole = ObjectToBeUpdated.ROLE.ROLE_NAME;
                        string FR_role = string.Empty;

                        string newRole = RoleName(aMember.ROLE_ID);
                        string FR_newRole = string.Empty;

                        switch (currentRole)
                        {
                            case "Admin": FR_role = "FR_ADMIN_ROLE"; break;
                            case "Manager": FR_role = "FR_MGMT_ROLE"; break;
                            case "Field": FR_role = "FR_FIELD_ROLE"; break;
                        }

                        switch (newRole)
                        {
                            case "Admin": FR_newRole = "FR_ADMIN_ROLE"; break;
                            case "Manager": FR_newRole = "FR_MGMT_ROLE"; break;
                            case "Field": FR_newRole = "FR_FIELD_ROLE"; break;
                        }*/

                        //GRANT "FR_MGMT_ROLE" TO "ELOOPER" ; REVOKE "FR_FIELD_ROLE" FROM "ELOOPER"; ALTER USER "ELOOPER" DEFAULT ROLE "FR_MGMT_ROLE","CONNECT";
                        //string query = @"REVOKE " + FR_role + " FROM " + ObjectToBeUpdated.USERNAME.ToUpper() + ";" +
                        //    "GRANT \"" + FR_newRole + "\" TO \"" + ObjectToBeUpdated.USERNAME.ToUpper() + "\";" +
                        //    "ALTER USER \"" + ObjectToBeUpdated.USERNAME.ToUpper() + "\" DEFAULT ROLE \"CONNECT\", \"" + FR_newRole + "\";";
                        //string query = @"DEFAULT ROLE "" + FR_newRole + "";";
                        //aSTNE.USERPROFILE_ALTER(ObjectToBeUpdated.USERNAME, query);

                        //ROLE_ID
                        //ObjectToBeUpdated.ROLE_ID = (Decimal.Equals(aMember.ROLE_ID, ObjectToBeUpdated.ROLE_ID) ?
                        //  ObjectToBeUpdated.ROLE_ID : aMember.ROLE_ID);aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aMember };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
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
            MEMBER ObjectToBeDeleted = null;

            //Return BadRequest if missing required fields
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (STNEntities2 aSTNE = GetRDS(securedPassword))
                {
                    // create user profile using db stored proceedure
                    try
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = aSTNE.MEMBERS.SingleOrDefault(
                                                m => m.MEMBER_ID == entityId);

                        //Try to remove user profile first
                        aSTNE.USERPROFILE_REMOVE(ObjectToBeDeleted.USERNAME);

                        //delete it
                        aSTNE.MEMBERS.DeleteObject(ObjectToBeDeleted);
                        aSTNE.SaveChanges();
                        //Return object to verify persisitance

                        return new OperationResult.OK { };

                    }
                    catch (Exception)
                    {
                        //TODO: relay failure type message 
                        // EX. if profile failed to be removed
                        return new OperationResult.BadRequest();
                    }

                }// end using
            } //end using
        }//end HTTP.DELETE
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<MEMBER> entityRDS, ref MEMBER anEntity)
        {
            MEMBER existingMember;
            MEMBER thisMember = anEntity as MEMBER;
            //check if it exists
            try
            {

                existingMember = entityRDS.FirstOrDefault(m => string.Equals(m.USERNAME.ToUpper(), thisMember.USERNAME.ToUpper()) &&
                                                                        (string.Equals(m.FNAME.ToUpper(), thisMember.FNAME.ToUpper()) || string.IsNullOrEmpty(thisMember.FNAME)) &&
                                                                        (string.Equals(m.LNAME.ToUpper(), thisMember.LNAME.ToUpper()) || string.IsNullOrEmpty(thisMember.LNAME)));

                //(m.AGENCY_ID == thisMember.AGENCY_ID || thisMember.AGENCY_ID <= 0 || thisMember.AGENCY_ID == null) &&
                //(string.Equals(m.PHONE.ToUpper(), thisMember.PHONE.ToUpper()) || string.IsNullOrEmpty(thisMember.PHONE)) &&
                //(string.Equals(m.EMERGENCY_CONTACT_NAME.ToUpper(), thisMember.EMERGENCY_CONTACT_NAME.ToUpper()) || string.IsNullOrEmpty(thisMember.EMERGENCY_CONTACT_NAME)) &&
                //(string.Equals(m.EMERGENCY_CONTACT_PHONE.ToUpper(), thisMember.EMERGENCY_CONTACT_PHONE.ToUpper()) || string.IsNullOrEmpty(thisMember.EMERGENCY_CONTACT_PHONE)));


                if (existingMember == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingMember;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private string RoleName(decimal? roleId)
        {
            using (STNEntities2 aSTNE = GetRDS())
            {
                ROLE thisRole = aSTNE.ROLES.FirstOrDefault(x => x.ROLE_ID == roleId);
                return thisRole.ROLE_NAME;
            }
        }
        private string buildDefaultPassword(MEMBER dm)
        {
            //LaMPDefau1t+numbercharInlastname+first2letterFirstName
            return "STNDefau1t" + dm.LNAME.Count() + dm.FNAME.Substring(0, 2);
        }//end buildDefaultPassword
        #endregion


    }//end class PeakSummaryHandler
}//end namespace