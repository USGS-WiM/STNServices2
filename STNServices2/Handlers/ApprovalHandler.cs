//------------------------------------------------------------------------------
//----- ApprovalHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Approval resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.06.12 - JKN - Created
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
    public class ApprovalHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "APPROVALS"; }
        }
        #endregion

        #region Routed Methods

        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<APPROVAL>().ToList() };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            APPROVAL anApproval;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anApproval = aSTNE.APPROVALs.SingleOrDefault(inst => inst.APPROVAL_ID == entityId);

                    if (anApproval != null)
                        anApproval.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using


                return new OperationResult.OK { ResponseResource = anApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET


        #endregion

        #region HWM Methods


        #region GetMethod

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMApproval")]
        public OperationResult GetHWMApproval(Int32 hwmId)
        {
            APPROVAL anApproval;

            //Return BadRequest if there is no ID
            if (hwmId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anApproval = aSTNE.HWMs.FirstOrDefault(h => h.HWM_ID == hwmId).APPROVAL;

                    if (anApproval != null)
                        anApproval.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveHWM")]
        public OperationResult ApproveHWM(Int32 hwmId)
        {
            try
            {
                APPROVAL aApproval = new APPROVAL();

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        HWM aHWM = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == hwmId);
                        if (aHWM == null) return new OperationResult.BadRequest { ResponseResource = "invalid HWMID" };

                        //get logged in user
                        MEMBER user = aSTNE.MEMBERS.FirstOrDefault(u => String.Equals(u.USERNAME.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (user == null) return new OperationResult.BadRequest { ResponseResource = "invalid username response" };

                        //set time and user of approval
                        aApproval.APPROVAL_DATE = DateTime.UtcNow;
                        aApproval.MEMBER_ID = user.MEMBER_ID;

                        if (!Exists(aSTNE.APPROVALs, ref aApproval))
                        {
                            //save approval
                            aSTNE.APPROVALs.AddObject(aApproval);
                            aSTNE.SaveChanges();
                        }//end if

                        //set HWM approvalID
                        aHWM.APPROVAL_ID = aApproval.APPROVAL_ID;
                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST


        #endregion
        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "UnApproveHWM")]
        public OperationResult UnApproveHWM(Int32 hwmId)
        {
            //Return BadRequest if missing required fields
            if (hwmId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        HWM aHWM = aSTNE.HWMs.SingleOrDefault(hwm => hwm.HWM_ID == hwmId);

                        //remove approval
                        this.Delete(Convert.ToInt32(aHWM.APPROVAL_ID));

                        //remove id from hwm
                        aHWM.APPROVAL_ID = null;
                        aSTNE.SaveChanges();

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
        #endregion

        #endregion

        #region DataFile Methods
        #region GetMethod

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileApproval")]
        public OperationResult GetDataFileApproval(Int32 datafileId)
        {
            APPROVAL anApproval;

            //Return BadRequest if there is no ID
            if (datafileId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anApproval = aSTNE.APPROVALs.FirstOrDefault(a => a.DATA_FILE.Any(df => df.DATA_FILE_ID == datafileId));

                    if (anApproval != null)
                        anApproval.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveDataFile")]
        public OperationResult ApproveDataFile(Int32 dataFileId)
        {
            APPROVAL aApproval = new APPROVAL();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        DATA_FILE aDatafile = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId);
                        if (aDatafile == null) return new OperationResult.BadRequest { ResponseResource = "invalid datafileID" };

                        //get logged in user
                        MEMBER user = aSTNE.MEMBERS.FirstOrDefault(u => String.Equals(u.USERNAME.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (user == null) return new OperationResult.BadRequest { ResponseResource = "invalid username response" };

                        //set time and user of approval
                        aApproval.APPROVAL_DATE = DateTime.UtcNow;
                        aApproval.MEMBER_ID = user.MEMBER_ID;

                        if (!Exists(aSTNE.APPROVALs, ref aApproval))
                        {
                            //save approval
                            aSTNE.APPROVALs.AddObject(aApproval);
                            aSTNE.SaveChanges();
                        }//end if
                        //set HWM approvalID
                        aDatafile.APPROVAL_ID = aApproval.APPROVAL_ID;
                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "ApproveNWISDataFile")]
        public OperationResult ApproveNWISDataFile(Int32 dataFileId)
        {
            APPROVAL aApproval = new APPROVAL();

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        DATA_FILE aDatafile = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId);
                        if (aDatafile == null) return new OperationResult.BadRequest { ResponseResource = "invalid datafileID" };

                        //get event coordinator to set as approver for NWIS datafile
                        MEMBER eventCoor = aSTNE.MEMBERS.FirstOrDefault(m => m.EVENTs.Any(e => e.EVENT_ID == aDatafile.INSTRUMENT.EVENT_ID));

                        //MEMBER user = aSTNE.MEMBERS.FirstOrDefault(u => String.Equals(u.USERNAME.ToUpper(), Context.User.Identity.Name.ToUpper()));
                        if (eventCoor == null) return new OperationResult.BadRequest { ResponseResource = "invalid username response" };

                        //set time and user of approval
                        aApproval.APPROVAL_DATE = DateTime.UtcNow;
                        aApproval.MEMBER_ID = eventCoor.MEMBER_ID;

                        if (!Exists(aSTNE.APPROVALs, ref aApproval))
                        {
                            //save approval
                            aSTNE.APPROVALs.AddObject(aApproval);
                            aSTNE.SaveChanges();
                        }//end if
                        //set HWM approvalID
                        aDatafile.APPROVAL_ID = aApproval.APPROVAL_ID;
                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aApproval };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "UnApproveDataFile")]
        public OperationResult UnApproveDataFile(Int32 dataFileId)
        {
            //Return BadRequest if missing required fields
            if (dataFileId <= 0)
            {
                return new OperationResult.BadRequest();
            }
            DATA_FILE aDataFile;

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        aDataFile = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId);

                        //remove approval
                        this.Delete(Convert.ToInt32(aDataFile.APPROVAL_ID));

                        //remove id from hwm
                        aDataFile.APPROVAL_ID = null;
                        aSTNE.SaveChanges();

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
        #endregion
        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [STNRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "Delete")]
        public OperationResult Delete(Int32 approvalId)
        {
            //Return BadRequest if missing required fields
            if (approvalId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        APPROVAL ObjectToBeDeleted = aSTNE.APPROVALs.SingleOrDefault(appr => appr.APPROVAL_ID == approvalId);

                        //remove id from HWM or DF
                        if (ObjectToBeDeleted.HWMs.Count > 0) ObjectToBeDeleted.HWMs.ToList().ForEach(x => x.APPROVAL_ID = null);
                        if (ObjectToBeDeleted.DATA_FILE.Count > 0) ObjectToBeDeleted.DATA_FILE.ToList().ForEach(x => x.APPROVAL_ID = null);

                        //delete it
                        aSTNE.APPROVALs.DeleteObject(ObjectToBeDeleted);
                        aSTNE.SaveChanges();

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
        #endregion

        #endregion

        #region Helper Methods
        private bool Exists(ObjectSet<APPROVAL> entityRDS, ref APPROVAL anEntity)
        {
            APPROVAL existingEntity;
            APPROVAL thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = entityRDS.FirstOrDefault(e => e.MEMBER_ID == thisEntity.MEMBER_ID &&
                                                           (DateTime.Equals(e.APPROVAL_DATE, thisEntity.APPROVAL_DATE) || !thisEntity.APPROVAL_DATE.HasValue));

                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }//end class ApprovalHandler
}//end namespace