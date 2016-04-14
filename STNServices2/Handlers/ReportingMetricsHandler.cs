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
            //ReportResource aReportMetModel;
            List<reporting_metrics> entities = null;
            
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<reporting_metrics>().Include("reportmetric_contact").ToList();

                    //aReportMetModel = aSTNE.REPORTING_METRICS.Where(df => df.REPORTING_METRICS_ID == entityId).Select(r => new ReportResource
                    //{
                    //    Report = r,
                    //    ReportContacts = getReportContacts(r)
                    //}).FirstOrDefault();

                    //if (aReportMet != null)
                    //    aReportMet.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
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
                    IQueryable<instrument> sensorQuery = sa.Select<instrument>().Where(i => i.event_id == eventId && i.site.state.Equals(stateName));

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
                    IQueryable<hwm> hwmQuery = sa.Select<hwm>().Where(i => i.event_id == eventId && i.site.state.Equals(stateName));

                    //query instruments by the date being less than status date
                    hwmQuery = hwmQuery.Where(i => i.@event.event_start_date <= aDate.Date);

                    Int32 hwmFlagged = hwmQuery.Where(h => h.flag_date != null).ToList().Count();
                    Int32 hwmCollected = hwmQuery.Where(h => h.elev_ft != null).ToList().Count();

                    //now populate the report to pass back
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReports")]
        public OperationResult GetFilteredReports([Optional] int eventId, [Optional] string stateNames, string aDate)
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
                if (stateNames != string.Empty)
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 eventID = eventId > 0 ? eventId : -1;
                                
                using (STNAgent sa = new STNAgent())
                {
                    //get all the reports first to then narrow down
                    query = sa.Select<reporting_metrics>();

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


        [STNRequiresRole(new string[] {AdminRole, ManagerRole, FieldRole})]//[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReportsModel")]
        public OperationResult GetFilteredReportModel(int eventId, string aDate, [Optional] string stateNames)
        {
            //this returns only completed reports

            //List<ReportResource> ReportList = null; 
            List<reporting_metrics> ReportList = null;
            IQueryable<reporting_metrics> query = null;

            try
            {
                if (aDate == string.Empty || eventId <= 0)
                    throw new BadRequestException("Invalid input parameters");                
                                  
                DateTime? thisDate = (DateTime?)Convert.ToDateTime(aDate);
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (stateNames != string.Empty)
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 eventID = eventId > 0 ? eventId : -1;
                
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    { 
                        //get all the reports first to then narrow down
                        query = sa.Select<reporting_metrics>();

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

                        ReportList = query.Include("reportmetric_contact").ToList();

                        //only completed reports please
                        //ReportList = query.AsEnumerable().Where(x => x.complete == 1).Select(x => new ReportResource
                        //{
                        //    Report = x,
                        //    ReportContacts = getReportContacts(x)
                        //}).ToList();                       
                        sm(MessageType.info, "Count: " + ReportList.Count());
                        sm(sa.Messages);
                    }//end using
                }
                return new OperationResult.OK { ResponseResource = ReportList, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        

        //private List<ReportContactModel> getReportContacts(REPORTING_METRICS x)
        //{
        //    var contactList = x.REPORTMETRIC_CONTACT.Select(c => new ReportContactModel
        //    {
        //        ContactId = c.CONTACT_ID,
        //        FNAME = c.CONTACT.FNAME,
        //        LNAME = c.CONTACT.LNAME,
        //        PHONE = c.CONTACT.PHONE,
        //        ALT_PHONE = c.CONTACT.ALT_PHONE,
        //        EMAIL = c.CONTACT.EMAIL,
        //        TYPE = c.CONTACT_TYPE.TYPE
        //    }).ToList();
        //    return contactList;

        //}//end HttpMethod.GET        

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "PostReportMetrics")]
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
                        anEntity = sa.Update<reporting_metrics>(anEntity);
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
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.DELETE

        #endregion
        
    }//end class ReportMetricHandler
}//end namespace