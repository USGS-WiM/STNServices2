//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

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
// 03.28.16 - JKN - Created
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

namespace STNServices2.Handlers
{
    public class EventsHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET, ForUriName="GetAllEvents")]
        public OperationResult Get()
        {
            List<events> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<events>().OrderBy(e => e.event_id).ToList();

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
        public OperationResult Get(string entityId)
        {
            events anEntity = null;

            try
            {
                if (string.IsNullOrEmpty(entityId)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    // why is this get so complicated, instead of just passing in Int32 eventId and getting it based on that??
                    anEntity = sa.Select<events>().FirstOrDefault(e => String.Equals(e.event_id.ToString().Trim().ToLower(), entityId.Trim().ToLower()) || 
                        String.Equals(e.event_name.Trim().Replace(" ", "").ToLower(), entityId.Trim().ToLower()));

                    if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEvents")]
        public OperationResult GetSiteEvents(Int32 siteId)
        {
            List<events> entities = null;

            try
            { 
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<events>().Include(e=> e.hwms).Include(e=> e.instruments).Where(e => e.hwms.Any(h => h.site_id == siteId) || e.instruments.Any(inst => inst.site_id == siteId)).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventTypeEvents")]
        public OperationResult GetEventTypeEvents(Int32 eventTypeId)
        {
            List<events> entities = null;
            
            try
            {
                if (eventTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<events>().Where(e => e.event_type_id == eventTypeId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventStatusEvents")]
        public OperationResult GetEventStatusEvents(Int32 eventStatusId)
        {
            List<events> entities = null;

            try
            {
                if (eventStatusId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<events>().Where(e => e.event_status_id == eventStatusId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMEvent")]
        public OperationResult GetHWMEvent(Int32 hwmId)
        {
            events anEntity = null;

            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<hwm>().Include(h=>h.@event).FirstOrDefault(h => h.hwm_id == hwmId).@event;
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentEvent")]
        public OperationResult GetInstrumentEvent(Int32 instrumentId)
        {
            events anEntity = null;
            
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<instrument>().Include(i => i.@event).FirstOrDefault(i => i.instrument_id == instrumentId).@event;
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredEvents")]
        public OperationResult GetFilteredEvents([Optional]string date, [Optional]string eventTypeId, [Optional] string stateName)
        {
            IQueryable<events> query;           
            List<events> entities = new List<events>();
            DateTime? fromDate;
            try
            {
                fromDate = ValidDate(date);
                using (STNAgent sa = new STNAgent())
                {
                    //get all events
                    query = sa.Select<events>().Include("instruments.site").Include("hwms.site");

                    //events from this day forward
                    if (fromDate.HasValue)  
                        query = query.Where(s => s.event_start_date >= fromDate);

                    //for this eventType only
                    if (!string.IsNullOrEmpty(eventTypeId) && Convert.ToInt32(eventTypeId) > 0)
                    {
                        Int32 eventTypdID = Convert.ToInt32(eventTypeId);
                        query = query.Where(s => s.event_type_id == eventTypdID);
                    }

                    //in this state only
                    if (!string.IsNullOrEmpty(stateName))
                    {
                        query = query.Where(e => e.instruments.Any(i => i.site.state == stateName.ToUpper()) || e.hwms.Any(h => h.site.state == stateName.ToUpper()));                    
                    }
                    entities = query.Distinct().ToList();

                    entities = entities.Select(ev => new events
                    {
                        event_id = ev.event_id,
                        event_name = ev.event_name,
                        event_start_date = ev.event_start_date,
                        event_end_date = ev.event_end_date,
                        event_description = ev.event_description,
                        event_type_id = ev.event_type_id,
                        event_status_id = ev.event_status_id,
                        event_coordinator = ev.event_coordinator
                    }).ToList();
                }

                return new OperationResult.OK { ResponseResource = entities };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(events anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            {
                if (string.IsNullOrEmpty(anEntity.event_name)|| anEntity.event_type_id <=0 || 
                    anEntity.event_status_id<=0 || anEntity.event_coordinator <=0)
                        throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last_updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Add<events>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, events anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            {
                if (entityId <=0||string.IsNullOrEmpty(anEntity.event_name) || anEntity.event_id <= 0 ||
                    anEntity.event_status_id <= 0 || anEntity.event_coordinator <= 0)
                        throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last_updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<events>(entityId, anEntity);
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
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            events anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<events>().FirstOrDefault(i => i.event_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<events>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end eventsHandler
}
