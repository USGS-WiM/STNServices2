//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

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
// 03.28.16 - JKN - Created
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

using WiM.Authentication;

namespace STNServices2.Handlers
{
    public class EventsHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
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
        public OperationResult Get(Int32 entityId)
        {
            events anEntity = null;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<events>().FirstOrDefault(e => e.event_id == entityId);
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

        [HttpOperation(ForUriName = "GetEventsBySite")]
        public OperationResult GetEventsBySite(Int32 siteId)
        {
            List<events> eventList = null;

            //Return BadRequest if there is no ID
            if (siteId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    eventList = sa.Select<events>().Where(e => e.hwms.Any(h => h.site_id == siteId) ||
                                                    e.instruments.Any(inst => inst.site_id == siteId))
                                            .ToList();
                    sm(MessageType.info, "Count: " + eventList.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = eventList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventTypeEvents")]
        public OperationResult GetEventTypeEvents(Int32 eventTypeId)
        {
            List<events> eventList = null;

            //Return BadRequest if there is no ID
            if (eventTypeId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    eventList = sa.Select<event_type>().FirstOrDefault(e => e.event_type_id== eventTypeId).events.ToList();
                                            
                    sm(MessageType.info, "Count: " + eventList.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = eventList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventbyStatus")]
        public OperationResult GetEventbyStatus(Int32 eventStatusId)
        {
            List<events> eventList = null;

            //Return BadRequest if there is no ID
            if (eventStatusId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    eventList = sa.Select<event_status>().FirstOrDefault(e=>e.event_status_id == eventStatusId).events
                                            .ToList();
                    sm(MessageType.info, "Count: " + eventList.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = eventList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetHWMEvent")]
        public OperationResult GetHWMEvent(Int32 hwmId)
        {
            events anEvent = null;

            //Return BadRequest if there is no ID
            if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    anEvent = sa.Select<hwm>().FirstOrDefault(e => e.hwm_id == hwmId).@event;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEvent, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentEvent")]
        public OperationResult GetInstrumentEvent(Int32 instrumentId)
        {
            events anEvent = null;

            //Return BadRequest if there is no ID
            if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    anEvent = sa.Select<instrument>().FirstOrDefault(e => e.instrument_id == instrumentId).@event;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEvent, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredEvents")]
        public OperationResult GetFilteredEvents([Optional]string date, [Optional]Int32 eventTypeId, [Optional] string stateName)
        {
            IQueryable<events> query;
            List<events> eventList = new List<events>();
            DateTime? fromDate;
            try
            {
                fromDate = ValidDate(date);
                using (STNAgent sa = new STNAgent())
                {
                    query = sa.Select<events>();

                    if (fromDate.HasValue)  
                        query = query.Where(s => s.event_start_date >= fromDate);                    
                    if (eventTypeId > 0) 
                        query = query.Where(s => s.event_type_id == eventTypeId);
                    
                    if (!string.IsNullOrEmpty(stateName))
                    {
                        query = query.Where(e => e.instruments.Any(i => i.site.state == stateName));
                        query = query.Where(e => e.hwms.Any(h => h.site.state == stateName));
                    }

                    eventList = query.Distinct().ToList();

                }

                return new OperationResult.OK { ResponseResource = eventList };
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
            try
            {
                if (string.IsNullOrEmpty(anEntity.event_name)|| anEntity.event_type_id <=0 || 
                    anEntity.event_status_id<=0 || anEntity.event_coordinator <=0)
                        throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
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
            try
            {
                if (entityId <=0||string.IsNullOrEmpty(anEntity.event_name) || anEntity.event_id <= 0 ||
                    anEntity.event_status_id <= 0 || anEntity.event_coordinator <= 0)
                        throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<events>(anEntity);
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
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end eventsHandler
}
