//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles Reporting Metrics resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.07.16 - TR - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Security;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Resources;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class ReportingMetricsHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<reporting_metrics> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().OrderBy(e => e.reporting_metrics_id).ToList();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            reporting_metrics anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<reporting_metrics>().FirstOrDefault(e => e.reporting_metrics_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportModel")]
        public OperationResult GetReportModel(Int32 entityId)
        {
            ReportResource anEntity;
            
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<reporting_metrics>().Include(r => r.reportmetric_contact).Include("reportmetric_contact.contact_type")
                        .Where(rep => rep.reporting_metrics_id == entityId).Select(rm => new ReportResource
                        {
                            reporting_metrics_id = rm.reporting_metrics_id,
                            event_id = rm.event_id,
                            sw_fieldpers_notacct = rm.sw_fieldpers_notacct,
                            wq_fieldpers_notacct = rm.wq_fieldpers_notacct,
                            yest_fieldpers = rm.yest_fieldpers,
                            tod_fieldpers = rm.tod_fieldpers,
                            tmw_fieldpers = rm.tmw_fieldpers,
                            yest_officepers = rm.yest_officepers,
                            tod_officepers = rm.tod_officepers,
                            tmw_officepers = rm.tmw_officepers,
                            gage_visit = rm.gage_visit,
                            gage_down = rm.gage_down,
                            tot_discharge_meas = rm.tot_discharge_meas,
                            plan_discharge_meas = rm.plan_discharge_meas,
                            plan_indirect_meas = rm.plan_indirect_meas,
                            rating_extens = rm.rating_extens,
                            gage_peak_record = rm.gage_peak_record,
                            plan_rapdepl_gage = rm.plan_rapdepl_gage,
                            dep_rapdepl_gage = rm.dep_rapdepl_gage,
                            rec_rapdepl_gage = rm.rec_rapdepl_gage,
                            lost_rapdepl_gage = rm.lost_rapdepl_gage,
                            plan_wtrlev_sensor = rm.plan_wtrlev_sensor,
                            dep_wtrlev_sensor = rm.dep_wtrlev_sensor,
                            rec_wtrlev_sensor = rm.rec_wtrlev_sensor,
                            lost_wtrlev_sensor = rm.lost_wtrlev_sensor,
                            plan_wv_sens = rm.plan_wv_sens,
                            dep_wv_sens = rm.dep_wv_sens,
                            rec_wv_sens = rm.rec_wv_sens,
                            lost_wv_sens = rm.lost_wv_sens,
                            plan_barometric = rm.plan_barometric,
                            dep_barometric = rm.dep_barometric,
                            rec_barometric = rm.rec_barometric,
                            lost_barometric = rm.lost_barometric,
                            plan_meteorological = rm.plan_meteorological,
                            dep_meteorological = rm.dep_meteorological,
                            rec_meteorological = rm.rec_meteorological,
                            lost_meteorological = rm.lost_meteorological,
                            hwm_flagged = rm.hwm_flagged,
                            hwm_collected = rm.hwm_collected,
                            qw_discr_samples = rm.qw_discr_samples,
                            coll_sedsamples = rm.coll_sedsamples,
                            member_id = rm.member_id,
                            complete = rm.complete,
                            notes = rm.notes,
                            report_date = rm.report_date,
                            state = rm.state,
                            ReportContacts = rm.reportmetric_contact.Select(rc => new ReportContactModel
                            {
                                 fname = rc.contact.fname,
                                 lname = rc.contact.lname,
                                 email = rc.contact.email,
                                 phone = rc.contact.phone,
                                 alt_phone = rc.contact.alt_phone,
                                 contact_id = rc.contact.contact_id,
                                 type = rc.contact_type.type
                            }).ToList<ReportContactModel>()
                        }).FirstOrDefault();
                                       

                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetMemberReports")]
        public OperationResult GetMemberReports(Int32 memberId)
        {
            List<reporting_metrics> entities = null;
            try
            {
                if (memberId <= 0) throw new BadRequestException("Invalid input parameters");
                
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().Where(mr => mr.member_id == memberId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET
 
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventReports")]
        public OperationResult GetEventReports(Int32 eventId)
        {
            List<reporting_metrics> entities = null;

            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().Where(mr => mr.event_id == eventId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportsByDate")]
        public OperationResult ReportsByDate(string aDate)
        {
            List<reporting_metrics> entities = null;

            try
            { 
                if (aDate == string.Empty) throw new BadRequestException("Invalid input parameters");
                DateTime formattedDate = Convert.ToDateTime(aDate).Date;
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().Where(mr => mr.report_date == formattedDate).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportsByEventAndState")]
        public OperationResult GetReportsByEventAndState(Int32 eventId, string stateName)
        {
            List<reporting_metrics> entities = null;

            try
            {
                if (eventId <= 0 || string.IsNullOrEmpty(stateName)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().Where(mr => mr.event_id == eventId && mr.state == stateName).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET
    
        //returns a report with just the counts of sensors (dep,rec, lost in all sensor types) and hwms for this date/event/state (<= to date)
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDailyReportTotals")]
        public OperationResult GetDailyReportTotals(string date, Int32 eventId, string stateName)
        {
            reporting_metrics ReportForTotals = null;
            try
            {
                if (date == string.Empty || eventId <= 0 || string.IsNullOrEmpty(stateName))
                    throw new BadRequestException("Invalid input parameters");                
               
                using (STNAgent sa = new STNAgent())
                {                      
                    DateTime aDate = Convert.ToDateTime(date).Date;

                    //statusType 1 : Deployed, statusType 2 : Retrieved, statusType 3 : lost

                    //query instruments with specified eventid, state (input can handle state name or abbr name)                    
                    IQueryable<instrument> sensorQuery = sa.Select<instrument>().Include(i => i.instrument_status).Include(i => i.site).Where(i => i.event_id == eventId && i.site.state.Equals(stateName));

                    //query instruments by the date being less than status date
                    List<instrument> sensorList = sensorQuery.AsEnumerable().Where(i => i.instrument_status.OrderByDescending(instStat => instStat.instrument_status_id)
                                                                    .FirstOrDefault().time_stamp.Value <= aDate).ToList();


                    List<instrument> depINSTs = sensorList.Where(i => i.instrument_status.OrderByDescending(inst => inst.time_stamp).FirstOrDefault().status_type_id == 1).ToList();
                    List<instrument> recINSTs = sensorList.Where(i => i.instrument_status.OrderByDescending(inst => inst.time_stamp).FirstOrDefault().status_type_id == 2).ToList();
                    List<instrument> lostINSTs = sensorList.Where(i => i.instrument_status.OrderByDescending(inst => inst.time_stamp).FirstOrDefault().status_type_id == 3).ToList();

                    //RDG totals ( S(5) )
                    Int32 DEPrapidDeploymentGageCount = depINSTs.Where(s => s.sensor_type_id == 5).ToList().Count();
                    Int32 RECrapidDeploymentGageCount = recINSTs.Where(s => s.sensor_type_id == 5).ToList().Count();
                    Int32 LOSTrapidDeploymentGageCount = lostINSTs.Where(s => s.sensor_type_id == 5).ToList().Count();

                    //water level ( S(1), D(1) )
                    Int32 DEPwaterLevelCount = depINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 1).ToList().Count();
                    Int32 RECwaterLevelCount = recINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 1).ToList().Count();
                    Int32 LOSTwaterLevelCount = lostINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 1).ToList().Count();

                    //wave height ( S(1), D(2) )
                    Int32 DEPwaveHeightCount = depINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 2).ToList().Count();
                    Int32 RECwaveHeightCount = recINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 2).ToList().Count();
                    Int32 LOSTwaveHeightCount = lostINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 2).ToList().Count();

                    //Barometric totals ( S(1), D(3) )
                    Int32 DEPbarometricCount = depINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 3).ToList().Count();
                    Int32 RECbarometricCount = recINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 3).ToList().Count();
                    Int32 LOSTbarometricCount = lostINSTs.Where(s => s.sensor_type_id == 1 && s.deployment_type_id == 3).ToList().Count();

                    //Met totals ( S(2) )
                    Int32 DEPmetCount = depINSTs.Where(s => s.sensor_type_id == 2).ToList().Count();
                    Int32 RECmetCount = recINSTs.Where(s => s.sensor_type_id == 2).ToList().Count();
                    Int32 LOSTmetCount = lostINSTs.Where(s => s.sensor_type_id == 2).ToList().Count();

                    //now go get the HWMs for Flagged and Collected
                    IQueryable<hwm> hwmQuery = sa.Select<hwm>().Include(h=> h.site).Include(h =>h.@event).Where(i => i.event_id == eventId && i.site.state.Equals(stateName));

                    //query instruments by the date being less than status date
                    hwmQuery = hwmQuery.Where(i => i.@event.event_start_date <= aDate.Date);

                    Int32 hwmFlagged = hwmQuery.Where(h => h.flag_date != null).ToList().Count();
                    Int32 hwmCollected = hwmQuery.Where(h => h.elev_ft != null).ToList().Count();

                    //now populate the report to pass back
                    ReportForTotals = new reporting_metrics();
                    ReportForTotals.dep_rapdepl_gage = DEPrapidDeploymentGageCount >= 1 ? DEPrapidDeploymentGageCount : 0;
                    ReportForTotals.rec_rapdepl_gage = RECrapidDeploymentGageCount >= 1 ? RECrapidDeploymentGageCount : 0;
                    ReportForTotals.lost_rapdepl_gage = LOSTrapidDeploymentGageCount >= 1 ? LOSTrapidDeploymentGageCount : 0;
                    ReportForTotals.dep_wtrlev_sensor = DEPwaterLevelCount >= 1 ? DEPwaterLevelCount : 0;
                    ReportForTotals.rec_wtrlev_sensor = RECwaterLevelCount >= 1 ? RECwaterLevelCount : 0;
                    ReportForTotals.lost_wtrlev_sensor = LOSTwaterLevelCount >= 1 ? LOSTwaterLevelCount : 0;
                    ReportForTotals.dep_wv_sens = DEPwaveHeightCount >= 1 ? DEPwaveHeightCount : 0;
                    ReportForTotals.rec_wv_sens = RECwaveHeightCount >= 1 ? RECwaveHeightCount : 0;
                    ReportForTotals.lost_wv_sens = LOSTwaveHeightCount >= 1 ? LOSTwaveHeightCount : 0;
                    ReportForTotals.dep_barometric = DEPbarometricCount >= 1 ? DEPbarometricCount : 0;
                    ReportForTotals.rec_barometric = RECbarometricCount >= 1 ? RECbarometricCount : 0;
                    ReportForTotals.lost_barometric = LOSTbarometricCount >= 1 ? LOSTbarometricCount : 0;
                    ReportForTotals.dep_meteorological = DEPmetCount >= 1 ? DEPmetCount : 0;
                    ReportForTotals.rec_meteorological = RECmetCount >= 1 ? RECmetCount : 0;
                    ReportForTotals.lost_meteorological = LOSTmetCount >= 1 ? LOSTmetCount : 0;
                    ReportForTotals.hwm_flagged = hwmFlagged >= 1 ? hwmFlagged : 0;
                    ReportForTotals.hwm_collected = hwmCollected >= 1 ? hwmCollected : 0;

                    if (ReportForTotals == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = ReportForTotals, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.Get

        //returns completed reports that were done on this date, event, state
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReports")]
        public OperationResult GetFilteredReports( string aDate, [Optional] string eventId, [Optional] string stateNames)
        {
            List<reporting_metrics> ReportList = null;
            IQueryable<reporting_metrics> query = null;

            try
            {
                if (aDate == string.Empty)
                    throw new BadRequestException("Invalid input parameters");     

                DateTime? thisDate = (DateTime?)Convert.ToDateTime(aDate);
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (!string.IsNullOrEmpty(stateNames))
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 eventID = !string.IsNullOrEmpty(eventId) ? Convert.ToInt32(eventId) : -1;
                                
                using (STNAgent sa = new STNAgent())
                {
                    //get all the reports first to then narrow down
                    query = sa.Select<reporting_metrics>();

                    //query DATE
                    query = query.Where(e => e.report_date == thisDate.Value);

                    //do we have an EVENT?
                    if (eventID > 0)
                    {
                        query = query.Where(e => e.event_id == eventID);
                    }                    

                    if (states.Count >= 2)
                    {

                        //multiple STATES
                        query = from q in query where states.Any(s => q.state.Contains(s.Trim())) select q;
                    }

                    if (states.Count == 1)
                    {
                        if (states[0] != "All States" && states[0] != "0")
                        {
                            string thisState = states[0];
                            thisState = GetStateByName(thisState).ToString();
                            query = query.Where(r => r.state == thisState);
                        }
                    }

                    //only completed reports please
                    ReportList = query.AsEnumerable().Where(x => x.complete == 1).ToList();
                    sm(MessageType.info, "Count: " + ReportList.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = ReportList, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReportsModel")]
        public OperationResult GetFilteredReportModel(int eventId, string aDate, [Optional] string stateNames)
        {
            //this returns only completed reports
            List<ReportResource> ReportList = null;            
            IQueryable<reporting_metrics> query = null;

            try
            {
                if (aDate == string.Empty || eventId <= 0)
                    throw new BadRequestException("Invalid input parameters");                
                                  
                DateTime? thisDate = (DateTime?)Convert.ToDateTime(aDate);
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (stateNames != null && stateNames != string.Empty)
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

                Int32 eventID = eventId > 0 ? eventId : -1;                
                using (STNAgent sa = new STNAgent())
                { 
                    //get all the reports first to then narrow down
                    query = sa.Select<reporting_metrics>().Include(r => r.reportmetric_contact).Include("reportmetric_contact.contact_type");

                    //do we have an EVENT?
                    if (eventID > 0)
                    {
                        query = sa.Select<reporting_metrics>().Where(e => e.event_id == eventID);
                    }

                    //query DATE
                    query = query.Where(e => e.report_date == thisDate.Value);

                    if (states.Count >= 2)
                    {

                        //multiple STATES
                        query = from q in query where states.Any(s => q.state.Contains(s.Trim())) select q;
                    }

                    if (states.Count == 1)
                    {
                        if (states[0] != "All States" && states[0] != "0")
                        {
                            string thisState = states[0];
                            thisState = GetStateByName(thisState).ToString();
                            query = query.Where(r => r.state == thisState);
                        }
                    }

                    query = query.Where(x => x.complete == 1);
                    ReportList = query.Select(rm => new ReportResource
                        {
                            reporting_metrics_id = rm.reporting_metrics_id,
                            event_id = rm.event_id,
                            sw_fieldpers_notacct = rm.sw_fieldpers_notacct,
                            wq_fieldpers_notacct = rm.wq_fieldpers_notacct,
                            yest_fieldpers = rm.yest_fieldpers,
                            tod_fieldpers = rm.tod_fieldpers,
                            tmw_fieldpers = rm.tmw_fieldpers,
                            yest_officepers = rm.yest_officepers,
                            tod_officepers = rm.tod_officepers,
                            tmw_officepers = rm.tmw_officepers,
                            gage_visit = rm.gage_visit,
                            gage_down = rm.gage_down,
                            tot_discharge_meas = rm.tot_discharge_meas,
                            plan_discharge_meas = rm.plan_discharge_meas,
                            plan_indirect_meas = rm.plan_indirect_meas,
                            rating_extens = rm.rating_extens,
                            gage_peak_record = rm.gage_peak_record,
                            plan_rapdepl_gage = rm.plan_rapdepl_gage,
                            dep_rapdepl_gage = rm.dep_rapdepl_gage,
                            rec_rapdepl_gage = rm.rec_rapdepl_gage,
                            lost_rapdepl_gage = rm.lost_rapdepl_gage,
                            plan_wtrlev_sensor = rm.plan_wtrlev_sensor,
                            dep_wtrlev_sensor = rm.dep_wtrlev_sensor,
                            rec_wtrlev_sensor = rm.rec_wtrlev_sensor,
                            lost_wtrlev_sensor = rm.lost_wtrlev_sensor,
                            plan_wv_sens = rm.plan_wv_sens,
                            dep_wv_sens = rm.dep_wv_sens,
                            rec_wv_sens = rm.rec_wv_sens,
                            lost_wv_sens = rm.lost_wv_sens,
                            plan_barometric = rm.plan_barometric,
                            dep_barometric = rm.dep_barometric,
                            rec_barometric = rm.rec_barometric,
                            lost_barometric = rm.lost_barometric,
                            plan_meteorological = rm.plan_meteorological,
                            dep_meteorological = rm.dep_meteorological,
                            rec_meteorological = rm.rec_meteorological,
                            lost_meteorological = rm.lost_meteorological,
                            hwm_flagged = rm.hwm_flagged,
                            hwm_collected = rm.hwm_collected,
                            qw_discr_samples = rm.qw_discr_samples,
                            coll_sedsamples = rm.coll_sedsamples,
                            member_id = rm.member_id,
                            complete = rm.complete,
                            notes = rm.notes,
                            report_date = rm.report_date,
                            state = rm.state,
                            ReportContacts = rm.reportmetric_contact.Select(rc => new ReportContactModel
                            {
                                fname = rc.contact.fname,
                                lname = rc.contact.lname,
                                email = rc.contact.email,
                                phone = rc.contact.phone,
                                alt_phone = rc.contact.alt_phone,
                                contact_id = rc.contact.contact_id,
                                type = rc.contact_type.type
                            }).ToList<ReportContactModel>()
                        }).ToList();
                    
                    sm(MessageType.info, "Count: " + ReportList.Count());
                    sm(sa.Messages);
                }//end using
               
                return new OperationResult.OK { ResponseResource = ReportList, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }        

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(reporting_metrics anEntity)
        {
            try
            {
                if (anEntity.report_date == null || anEntity.event_id <= 0 || anEntity.state == null || anEntity.member_id <= 0)
                    throw new BadRequestException("Invalid input parameters");                
                                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    { 
                        anEntity = sa.Add<reporting_metrics>(anEntity);
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
        public OperationResult Put(Int32 entityId, reporting_metrics anEntity)
        {          
            try
            {
                if (anEntity.report_date == null || anEntity.event_id <= 0 || anEntity.state == null || anEntity.member_id <= 0)
                    throw new BadRequestException("Invalid input parameters");                
                                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<reporting_metrics>(entityId, anEntity);
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
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            reporting_metrics anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<reporting_metrics>().FirstOrDefault(i => i.reporting_metrics_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<reporting_metrics>(anEntity);
                        sm(sa.Messages);
                        //TODO:: Delete reportmetric_contact relationship row
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.DELETE

        #endregion
        
    }//end class ReportMetricHandler
}//end namespace