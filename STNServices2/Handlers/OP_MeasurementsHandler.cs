//------------------------------------------------------------------------------
//----- MemberHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Reference Points/measurements to Instrument status resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.22.14 - TR - Created

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
    public class OP_MeasurementsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "OP_MEASUREMENTS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<OP_MEASUREMENTS> RP_measurList = new List<OP_MEASUREMENTS>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    RP_measurList = aSTNE.OP_MEASUREMENTS.OrderBy(m => m.OP_MEASUREMENTS_ID).ToList();

                    if (RP_measurList != null)
                        RP_measurList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = RP_measurList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            OP_MEASUREMENTS anOPMeas;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anOPMeas = aSTNE.OP_MEASUREMENTS.SingleOrDefault(m => m.OP_MEASUREMENTS_ID == entityId);

                    if (anOPMeas != null)
                        anOPMeas.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = anOPMeas };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentStatOPMeasurements")]
        public OperationResult InstrMeasurements(Int32 instrumentStatusId)
        {
            List<OP_MEASUREMENTS> opMeasurements;

            //Return BadRequest if there is no ID
            if (instrumentStatusId <= 0)
            { return new OperationResult.BadRequest(); }
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    opMeasurements = aSTNE.OP_MEASUREMENTS.Where(m => m.INSTRUMENT_STATUS_ID == instrumentStatusId).ToList();

                    if (opMeasurements != null)
                        opMeasurements.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = opMeasurements };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOPOPMeasurements")]
        public OperationResult OPOPMeasurements(Int32 objectivePointId)
        {
            List<OP_MEASUREMENTS> opMeasurements;

            //Return BadRequest if there is no ID
            if (objectivePointId <= 0)
            { return new OperationResult.BadRequest(); }
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    opMeasurements = aSTNE.OP_MEASUREMENTS.Where(m => m.OBJECTIVE_POINT_ID == objectivePointId).ToList();

                    if (opMeasurements != null)
                        opMeasurements.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = opMeasurements };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET
        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddInstrumentStatOPMeasurements")]
        public OperationResult AddInstrumentStatOPMeasurements(Int32 instrumentStatusId, OP_MEASUREMENTS anOPMeasurement)
        {
            //Return BadRequest if there is no ID
            if (instrumentStatusId <= 0 || anOPMeasurement.OBJECTIVE_POINT_ID <= 0)
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
                        anOPMeasurement.INSTRUMENT_STATUS_ID = instrumentStatusId;
                        if (!Exists(aSTNE.OP_MEASUREMENTS, ref anOPMeasurement))
                        {
                            aSTNE.OP_MEASUREMENTS.AddObject(anOPMeasurement);
                            aSTNE.SaveChanges();
                        }//end if
                    }
                }
                return new OperationResult.OK();
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT, ForUriName = "Put")]
        public OperationResult Put(Int32 entityId, OP_MEASUREMENTS anOPmeas)
        {
            //Return BadRequest if missing required fields
            if (anOPmeas.OBJECTIVE_POINT_ID <= 0 || anOPmeas.INSTRUMENT_STATUS_ID <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        OP_MEASUREMENTS ObjectToBeUpdated = aSTNE.OP_MEASUREMENTS.Single(m => m.OP_MEASUREMENTS_ID == entityId);                        
                        ObjectToBeUpdated.WATER_SURFACE = anOPmeas.WATER_SURFACE != ObjectToBeUpdated.WATER_SURFACE ? anOPmeas.WATER_SURFACE : ObjectToBeUpdated.WATER_SURFACE;
                        ObjectToBeUpdated.GROUND_SURFACE = anOPmeas.GROUND_SURFACE != ObjectToBeUpdated.GROUND_SURFACE ? anOPmeas.GROUND_SURFACE : ObjectToBeUpdated.GROUND_SURFACE;
                        ObjectToBeUpdated.OFFSET_CORRECTION = anOPmeas.OFFSET_CORRECTION != ObjectToBeUpdated.OFFSET_CORRECTION ? anOPmeas.OFFSET_CORRECTION : ObjectToBeUpdated.OFFSET_CORRECTION;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { };
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
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            OP_MEASUREMENTS ObjectToBeDeleted = null;

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
                        ObjectToBeDeleted = aSTNE.OP_MEASUREMENTS.SingleOrDefault(
                                                m => m.OP_MEASUREMENTS_ID == entityId);

                        //delete it
                        aSTNE.OP_MEASUREMENTS.DeleteObject(ObjectToBeDeleted);
                        aSTNE.SaveChanges();
                        //Return object to verify persisitance

                        return new OperationResult.OK { };

                    }
                    catch (Exception)
                    {
                        return new OperationResult.BadRequest();
                    }

                }// end using
            } //end using
        }//end HTTP.DELETE

        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<OP_MEASUREMENTS> entityRDS, ref OP_MEASUREMENTS anEntity)
        {
            OP_MEASUREMENTS existingRPM;
            OP_MEASUREMENTS thisEntity = anEntity as OP_MEASUREMENTS;
            //check if it exists
            try
            {

                existingRPM = entityRDS.FirstOrDefault(e => e.OBJECTIVE_POINT_ID == thisEntity.OBJECTIVE_POINT_ID &&
                                                               e.INSTRUMENT_STATUS_ID == thisEntity.INSTRUMENT_STATUS_ID &&                                                              
                                                               e.WATER_SURFACE == thisEntity.WATER_SURFACE &&
                                                               e.GROUND_SURFACE == thisEntity.GROUND_SURFACE &&
                                                               e.OFFSET_CORRECTION == thisEntity.OFFSET_CORRECTION);


                if (existingRPM == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingRPM;
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