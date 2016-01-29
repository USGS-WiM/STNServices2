//------------------------------------------------------------------------------
//----- InstrumentStatusHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles InstrumentStatus resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 08.02.12 - JKN - Created
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
    public class InstrumentStatusHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "INSTRUMENTSTATUS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<INSTRUMENT_STATUS> instrumentStatusList = new List<INSTRUMENT_STATUS>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentStatusList = aSTNE.INSTRUMENT_STATUS.OrderBy(instStat => instStat.INSTRUMENT_STATUS_ID).ToList();

                    if (instrumentStatusList != null)
                        instrumentStatusList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = instrumentStatusList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            INSTRUMENT_STATUS anInstrumentStatus;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anInstrumentStatus = aSTNE.INSTRUMENT_STATUS.SingleOrDefault(inst => inst.INSTRUMENT_STATUS_ID == entityId);

                    if (anInstrumentStatus != null)
                        anInstrumentStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anInstrumentStatus };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentStatusLog")]
        public OperationResult GetInstrumentStatusLog(Int32 instrumentId)
        {
            List<INSTRUMENT_STATUS> instrumentStatusList = new List<INSTRUMENT_STATUS>();

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentStatusList = aSTNE.INSTRUMENT_STATUS.AsEnumerable()
                                .Where(instStat => instStat.INSTRUMENT_ID == instrumentId)
                                .OrderByDescending(instStat => instStat.TIME_STAMP)
                                .ToList<INSTRUMENT_STATUS>();

                    instrumentStatusList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using            

                return new OperationResult.OK { ResponseResource = instrumentStatusList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[HttpOperation(ForUriName = "GetCollectionTeamInstrumentStatuses")]
        //public OperationResult GetCollectionTeamInstrumentStatuses(Int32 teamId)
        //{
        //    List<INSTRUMENT_STATUS> instrumentStatusList = new List<INSTRUMENT_STATUS>();

        //    //Return BadRequest if there is no ID
        //    if (teamId <= 0)
        //    {
        //        return new OperationResult.BadRequest();
        //    }

        //    try
        //    {
        //        using (STNEntities2 aSTNE = GetRDS())
        //        {
        //            instrumentStatusList = aSTNE.COLLECT_TEAM.FirstOrDefault(ct => ct.COLLECT_TEAM_ID == teamId).INSTRUMENT_STATUS.ToList();
        //            if (instrumentStatusList != null)
        //                instrumentStatusList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

        //        }//end using

        //        return new OperationResult.OK { ResponseResource = instrumentStatusList };
        //    }
        //    catch
        //    {
        //        return new OperationResult.BadRequest();
        //    }
        //}//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentStatus")]
        public OperationResult GetInstrumentStatus(Int32 instrumentId)
        {
            INSTRUMENT_STATUS instrumentStatus;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    instrumentStatus = aSTNE.INSTRUMENT_STATUS.AsEnumerable()
                                .Where(instStat => instStat.INSTRUMENT_ID == instrumentId)
                                .OrderByDescending(instStat => instStat.TIME_STAMP).FirstOrDefault();

                    instrumentStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = instrumentStatus };
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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateInstrumentStatus")]
        public OperationResult Post(INSTRUMENT_STATUS anInstrumentStatus)
        {
            //Return BadRequest if missing required fields
            if (anInstrumentStatus.INSTRUMENT_ID <= 0 || !anInstrumentStatus.INSTRUMENT_ID.HasValue ||
                anInstrumentStatus.STATUS_TYPE_ID <= 0 || !anInstrumentStatus.STATUS_TYPE_ID.HasValue)
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
                        if (!Exists(aSTNE.INSTRUMENT_STATUS, ref anInstrumentStatus))
                        {
                            aSTNE.INSTRUMENT_STATUS.AddObject(anInstrumentStatus);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anInstrumentStatus };
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
        public OperationResult Put(Int32 entityId, INSTRUMENT_STATUS anInstrumentStatus)
        {

            INSTRUMENT_STATUS instrumentStatusToUpdate = null;
            //Return BadRequest if missing required fields
            if (anInstrumentStatus.INSTRUMENT_STATUS_ID <= 0 || entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        //Grab the instrument row to update
                        instrumentStatusToUpdate = aSTNE.INSTRUMENT_STATUS.SingleOrDefault(
                                           instrumentStat => instrumentStat.INSTRUMENT_STATUS_ID == entityId);
                        //Update fields
                        instrumentStatusToUpdate.INSTRUMENT_STATUS_ID = anInstrumentStatus.INSTRUMENT_STATUS_ID;
                        instrumentStatusToUpdate.INSTRUMENT_ID = anInstrumentStatus.INSTRUMENT_ID;
                        instrumentStatusToUpdate.TIME_STAMP = anInstrumentStatus.TIME_STAMP;
                        instrumentStatusToUpdate.NOTES = anInstrumentStatus.NOTES;
                        instrumentStatusToUpdate.COLLECTION_TEAM_ID = anInstrumentStatus.COLLECTION_TEAM_ID;
                        instrumentStatusToUpdate.TIME_ZONE = anInstrumentStatus.TIME_ZONE;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anInstrumentStatus };
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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteInstrumentStatus")]
        public OperationResult Delete(Int32 instrumentStatusId)
        {
            //Return BadRequest if missing required fields
            if (instrumentStatusId <= 0)
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
                        INSTRUMENT_STATUS ObjectToBeDeleted = aSTNE.INSTRUMENT_STATUS.SingleOrDefault(instStat => instStat.INSTRUMENT_STATUS_ID == instrumentStatusId);
                        //delete it

                        aSTNE.INSTRUMENT_STATUS.DeleteObject(ObjectToBeDeleted);
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
        private bool Exists(ObjectSet<INSTRUMENT_STATUS> entityRDS, ref INSTRUMENT_STATUS anEntity)
        {
            INSTRUMENT_STATUS existingEntity;
            INSTRUMENT_STATUS thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.INSTRUMENT_ID == thisEntity.INSTRUMENT_ID &&
                                                               e.STATUS_TYPE_ID == thisEntity.STATUS_TYPE_ID &&
                                                              (DateTime.Equals(e.TIME_STAMP.Value, thisEntity.TIME_STAMP.Value) || !thisEntity.TIME_STAMP.HasValue) &&
                                                              (!thisEntity.COLLECTION_TEAM_ID.HasValue || e.COLLECTION_TEAM_ID == thisEntity.COLLECTION_TEAM_ID || thisEntity.COLLECTION_TEAM_ID <= 0) &&
                                                              (string.Equals(e.NOTES.ToUpper(), thisEntity.NOTES.ToUpper()) || string.IsNullOrEmpty(thisEntity.NOTES)) &&
                                                               (string.Equals(e.TIME_ZONE.ToUpper(), thisEntity.TIME_ZONE.ToUpper()) || string.IsNullOrEmpty(thisEntity.TIME_ZONE)));


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

    }//end class InstrumentStatusHandler
}//end namespace