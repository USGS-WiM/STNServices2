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
// 04.18.16 - JKN - updated and adapted from original STNServices
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class InstrumentStatusHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<instrument_status> entities;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument_status>().OrderBy(instStat => instStat.instrument_status_id).ToList();
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            instrument_status anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument_status>().SingleOrDefault(inst => inst.instrument_status_id == entityId);
                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET,ForUriName = "GetInstrumentStatusLog")]
        public OperationResult GetInstrumentStatusLog(Int32 instrumentId)
        {
            List<instrument_status> entities;
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<instrument_status>().AsEnumerable()
                                .Where(instStat => instStat.instrument_id == instrumentId)
                                .OrderByDescending(instStat => instStat.time_stamp)
                                .ToList();

                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET 

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentStatus")]
        public OperationResult GetInstrumentStatus(Int32 instrumentId)
        {
            instrument_status anEntity;
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument_status>().AsEnumerable()
                                .Where(instStat => instStat.instrument_id == instrumentId)
                                .OrderByDescending(instStat => instStat.time_stamp).FirstOrDefault();

                    sm(sa.Messages);
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET
        #endregion
        #region PostMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "CreateInstrumentStatus")]
        public OperationResult Post(instrument_status anEntity)
        {
            try
            {
                if (!anEntity.instrument_id.HasValue ||
                    !anEntity.time_stamp.HasValue ||
                    !anEntity.status_type_id.HasValue ||
                    !anEntity.member_id.HasValue ||
                    string.IsNullOrEmpty(anEntity.time_zone)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<instrument_status>(anEntity);                        
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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, instrument_status anEntity)
        {
            try
            {
                if (!anEntity.instrument_id.HasValue ||
                    !anEntity.time_stamp.HasValue ||
                    !anEntity.status_type_id.HasValue ||
                    !anEntity.member_id.HasValue ||
                    string.IsNullOrEmpty(anEntity.time_zone)) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<instrument_status>(entityId, anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.PUT

        #endregion
        #region DeleteMethods
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        instrument_status ObjectToBeDeleted = sa.Select<instrument_status>()
                            .SingleOrDefault(instStat => instStat.instrument_status_id == entityId);

                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException("Item Not found");
                        sa.Delete<instrument_status>(ObjectToBeDeleted);

                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion
    }//end class InstrumentStatusHandler
}//end namespace