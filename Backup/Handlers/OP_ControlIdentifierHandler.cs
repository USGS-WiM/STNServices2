//------------------------------------------------------------------------------
//----- OP_ControlIdentifierHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
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
// 05.25.14 - TR - Created

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
    public class OP_ControlIdentifierHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "OP_CONTROL_IDENTIFIER"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<OP_CONTROL_IDENTIFIER> OPcontrolIdentifiers = new List<OP_CONTROL_IDENTIFIER>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    OPcontrolIdentifiers = aSTNE.OP_CONTROL_IDENTIFIER.OrderBy(m => m.OP_CONTROL_IDENTIFIER_ID)
                                    .ToList();

                    if (OPcontrolIdentifiers != null)
                        OPcontrolIdentifiers.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = OPcontrolIdentifiers };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {

            OP_CONTROL_IDENTIFIER anOPControl;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anOPControl = aSTNE.OP_CONTROL_IDENTIFIER.SingleOrDefault(
                                m => m.OP_CONTROL_IDENTIFIER_ID == entityId);

                    if (anOPControl != null)
                        anOPControl.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anOPControl };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "OPControls")]
        public OperationResult OPControls(Int32 objectivePointId)
        {
            List<OP_CONTROL_IDENTIFIER> OP_controls;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
            { return new OperationResult.BadRequest(); }
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    OP_controls = aSTNE.OP_CONTROL_IDENTIFIER.Where(m => m.OBJECTIVE_POINT_ID == objectivePointId).ToList();

                    if (OP_controls != null)
                        OP_controls.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = OP_controls };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddOPControls")]
        public OperationResult AddOPControls(Int32 objectivePointId, OP_CONTROL_IDENTIFIER opCI)
        {
            //Return BadRequest if there is no ID
            if (objectivePointId <= 0 || String.IsNullOrEmpty(opCI.IDENTIFIER))
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
                        opCI.OBJECTIVE_POINT_ID = objectivePointId;
                        if (!Exists(aSTNE.OP_CONTROL_IDENTIFIER, ref opCI))
                        {
                            aSTNE.OP_CONTROL_IDENTIFIER.AddObject(opCI);
                            aSTNE.SaveChanges();
                        }//end if
                    }
                }
                return new OperationResult.OK();
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT, ForUriName = "Put")]
        public OperationResult Put(Int32 entityId, OP_CONTROL_IDENTIFIER anOPControl)
        {
            //Return BadRequest if missing required fields
            if (anOPControl.OBJECTIVE_POINT_ID <= 0 || string.IsNullOrEmpty(anOPControl.IDENTIFIER_TYPE))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        OP_CONTROL_IDENTIFIER ObjectToBeUpdated = aSTNE.OP_CONTROL_IDENTIFIER.Single(m => m.OP_CONTROL_IDENTIFIER_ID == entityId);

                        ObjectToBeUpdated.IDENTIFIER = anOPControl.IDENTIFIER != ObjectToBeUpdated.IDENTIFIER ? anOPControl.IDENTIFIER : ObjectToBeUpdated.IDENTIFIER;
                        ObjectToBeUpdated.IDENTIFIER_TYPE = anOPControl.IDENTIFIER_TYPE != ObjectToBeUpdated.IDENTIFIER_TYPE ? anOPControl.IDENTIFIER_TYPE : ObjectToBeUpdated.IDENTIFIER_TYPE;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.PUT

        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            OP_CONTROL_IDENTIFIER ObjectToBeDeleted = null;

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
                        ObjectToBeDeleted = aSTNE.OP_CONTROL_IDENTIFIER.SingleOrDefault(
                                                m => m.OP_CONTROL_IDENTIFIER_ID == entityId);

                        //delete it
                        aSTNE.OP_CONTROL_IDENTIFIER.DeleteObject(ObjectToBeDeleted);
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
        private bool Exists(ObjectSet<OP_CONTROL_IDENTIFIER> entityRDS, ref OP_CONTROL_IDENTIFIER anEntity)
        {
            OP_CONTROL_IDENTIFIER existingSH;
            OP_CONTROL_IDENTIFIER thisEntity = anEntity as OP_CONTROL_IDENTIFIER;
            //check if it exists
            try
            {

                existingSH = entityRDS.FirstOrDefault(e => e.OBJECTIVE_POINT_ID == thisEntity.OBJECTIVE_POINT_ID &&
                                                               (string.Equals(e.IDENTIFIER.ToUpper(), thisEntity.IDENTIFIER.ToUpper())) &&
                                                               (string.Equals(e.IDENTIFIER_TYPE.ToUpper(), thisEntity.IDENTIFIER_TYPE.ToUpper())));


                if (existingSH == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingSH;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


    }//end class PeakSummaryHandler
}//end namespace