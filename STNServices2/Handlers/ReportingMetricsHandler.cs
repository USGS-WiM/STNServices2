//------------------------------------------------------------------------------
//----- ReportingMetricsHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Resource Metrics resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 02.13.14 - TR -Created

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
using System.Runtime.InteropServices;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Web;


namespace STNServices2.Handlers
{
    public class ReportingMetricsHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "REPORTING_METRICS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<REPORTING_METRICS> reportMetList = new List<REPORTING_METRICS>();

            try
            {
                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    reportMetList = aSTNE.REPORTING_METRICS.OrderBy(ps => ps.REPORTING_METRICS_ID)
                                     .ToList();

                    if (reportMetList != null)
                        reportMetList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = reportMetList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            REPORTING_METRICS aReportMet;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aReportMet = aSTNE.REPORTING_METRICS.SingleOrDefault(
                                  df => df.REPORTING_METRICS_ID == entityId);

                    if (aReportMet != null)
                        aReportMet.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = aReportMet };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportModel")]
        public OperationResult GetReportModel(Int32 entityId)
        {
            ReportResource aReportMetModel;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
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
                        aReportMetModel = aSTNE.REPORTING_METRICS.Where(df => df.REPORTING_METRICS_ID == entityId).Select(r => new ReportResource
                        {
                            Report = r,
                            ReportContacts = getReportContacts(r)
                        }).FirstOrDefault();

                        //if (aReportMet != null)
                        //    aReportMet.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aReportMetModel };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetMemberReports")]
        public OperationResult GetMemberReports(Int32 memberId)
        {
            List<REPORTING_METRICS> MemberReports = null;

            //Return BadRequest if there is no ID
            if (memberId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    MemberReports = aSTNE.REPORTING_METRICS.Where(mr => mr.MEMBER_ID == memberId).ToList();

                    if (MemberReports != null)
                        MemberReports.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = MemberReports };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportsByEventAndState")]
        public OperationResult GetReportsByEventAndState(Int32 eventId, string stateName)
        {
            List<REPORTING_METRICS> ReportList = null;

            //Return BadRequest if there is no ID
            if (eventId <= 0 || stateName == string.Empty)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    ReportList = aSTNE.REPORTING_METRICS.Where(mr => mr.EVENT_ID == eventId && mr.STATE == stateName).ToList();

                    if (ReportList != null)
                        ReportList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
                // }//end using

                return new OperationResult.OK { ResponseResource = ReportList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventReports")]
        public OperationResult GetEventReports(Int32 eventId)
        {
            List<REPORTING_METRICS> ReportList = null;

            //Return BadRequest if there is no ID
            if (eventId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    ReportList = aSTNE.REPORTING_METRICS.Where(mr => mr.EVENT_ID == eventId).ToList();

                    if (ReportList != null)
                        ReportList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = ReportList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportsByDate")]
        public OperationResult ReportsByDate(string aDate)
        {
            List<REPORTING_METRICS> ReportList = null;

            //Return BadRequest if there is no ID
            if (aDate == string.Empty)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                DateTime formattedDate = Convert.ToDateTime(aDate).Date;

                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    ReportList = aSTNE.REPORTING_METRICS.Where(mr => mr.REPORT_DATE == formattedDate).ToList();

                    if (ReportList != null)
                        ReportList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = ReportList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDailyReportTotals")]
        public OperationResult GetDailyReportTotals(string date, Int32 eventId, string stateName)
        {
            try
            {
                REPORTING_METRICS ReportForTotals = new REPORTING_METRICS();

                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    DateTime aDate = Convert.ToDateTime(date).Date;

                    //statusType 1 : Deployed, statusType 2 : Retrieved, statusType 3 : lost

                    //query instruments with specified eventid, state (input can handle state name or abbr name)
                    IQueryable<INSTRUMENT> sensorQuery = aSTNE.INSTRUMENTs.Where(i => i.EVENT_ID == eventId &&
                                                                                      i.SITE.STATE.Equals(stateName));

                    //query instruments by the date being less than status date
                    List<INSTRUMENT> sensorList = sensorQuery.AsEnumerable().Where(i => i.INSTRUMENT_STATUS.OrderByDescending(instStat => instStat.INSTRUMENT_STATUS_ID)
                                                                    .FirstOrDefault().TIME_STAMP.Value <= aDate).ToList();

                    List<INSTRUMENT> depINSTs = sensorList.Where(i => i.INSTRUMENT_STATUS.OrderByDescending(inst => inst.TIME_STAMP).FirstOrDefault().STATUS_TYPE_ID == 1).ToList();
                    List<INSTRUMENT> recINSTs = sensorList.Where(i => i.INSTRUMENT_STATUS.OrderByDescending(inst => inst.TIME_STAMP).FirstOrDefault().STATUS_TYPE_ID == 2).ToList();
                    List<INSTRUMENT> lostINSTs = sensorList.Where(i => i.INSTRUMENT_STATUS.OrderByDescending(inst => inst.TIME_STAMP).FirstOrDefault().STATUS_TYPE_ID == 3).ToList();

                    //RDG totals ( S(5) )
                    Int32 DEPrapidDeploymentGageCount = depINSTs.Where(s => s.SENSOR_TYPE_ID == 5).ToList().Count();
                    Int32 RECrapidDeploymentGageCount = recINSTs.Where(s => s.SENSOR_TYPE_ID == 5).ToList().Count();
                    Int32 LOSTrapidDeploymentGageCount = lostINSTs.Where(s => s.SENSOR_TYPE_ID == 5).ToList().Count();

                    //water level ( S(1), D(1) )
                    Int32 DEPwaterLevelCount = depINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 1).ToList().Count();
                    Int32 RECwaterLevelCount = recINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 1).ToList().Count();
                    Int32 LOSTwaterLevelCount = lostINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 1).ToList().Count();

                    //wave height ( S(1), D(2) )
                    Int32 DEPwaveHeightCount = depINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 2).ToList().Count();
                    Int32 RECwaveHeightCount = recINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 2).ToList().Count();
                    Int32 LOSTwaveHeightCount = lostINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 2).ToList().Count();

                    //Barometric totals ( S(1), D(3) )
                    Int32 DEPbarometricCount = depINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 3).ToList().Count();
                    Int32 RECbarometricCount = recINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 3).ToList().Count();
                    Int32 LOSTbarometricCount = lostINSTs.Where(s => s.SENSOR_TYPE_ID == 1 && s.DEPLOYMENT_TYPE_ID == 3).ToList().Count();

                    //Met totals ( S(2) )
                    Int32 DEPmetCount = depINSTs.Where(s => s.SENSOR_TYPE_ID == 2).ToList().Count();
                    Int32 RECmetCount = recINSTs.Where(s => s.SENSOR_TYPE_ID == 2).ToList().Count();
                    Int32 LOSTmetCount = lostINSTs.Where(s => s.SENSOR_TYPE_ID == 2).ToList().Count();

                    //now go get the HWMs for Flagged and Collected
                    IQueryable<HWM> hwmQuery = aSTNE.HWMs.Where(i => i.EVENT_ID == eventId &&
                                                                                      i.SITE.STATE.Equals(stateName));

                    //query instruments by the date being less than status date
                    hwmQuery = hwmQuery.Where(i => i.EVENT.EVENT_START_DATE <= aDate.Date);

                    Int32 hwmFlagged = hwmQuery.Where(h => h.FLAG_DATE != null).ToList().Count();
                    Int32 hwmCollected = hwmQuery.Where(h => h.ELEV_FT != null).ToList().Count();

                    //now populate the report to pass back
                    ReportForTotals.DEP_RAPDEPL_GAGE = DEPrapidDeploymentGageCount >= 1 ? DEPrapidDeploymentGageCount : 0;
                    ReportForTotals.REC_RAPDEPL_GAGE = RECrapidDeploymentGageCount >= 1 ? RECrapidDeploymentGageCount : 0;
                    ReportForTotals.LOST_RAPDEPL_GAGE = LOSTrapidDeploymentGageCount >= 1 ? LOSTrapidDeploymentGageCount : 0;
                    ReportForTotals.DEP_WTRLEV_SENSOR = DEPwaterLevelCount >= 1 ? DEPwaterLevelCount : 0;
                    ReportForTotals.REC_WTRLEV_SENSOR = RECwaterLevelCount >= 1 ? RECwaterLevelCount : 0;
                    ReportForTotals.LOST_WTRLEV_SENSOR = LOSTwaterLevelCount >= 1 ? LOSTwaterLevelCount : 0;
                    ReportForTotals.DEP_WV_SENS = DEPwaveHeightCount >= 1 ? DEPwaveHeightCount : 0;
                    ReportForTotals.REC_WV_SENS = RECwaveHeightCount >= 1 ? RECwaveHeightCount : 0;
                    ReportForTotals.LOST_WV_SENS = LOSTwaveHeightCount >= 1 ? LOSTwaveHeightCount : 0;
                    ReportForTotals.DEP_BAROMETRIC = DEPbarometricCount >= 1 ? DEPbarometricCount : 0;
                    ReportForTotals.REC_BAROMETRIC = RECbarometricCount >= 1 ? RECbarometricCount : 0;
                    ReportForTotals.LOST_BAROMETRIC = LOSTbarometricCount >= 1 ? LOSTbarometricCount : 0;
                    ReportForTotals.DEP_METEOROLOGICAL = DEPmetCount >= 1 ? DEPmetCount : 0;
                    ReportForTotals.REC_METEOROLOGICAL = RECmetCount >= 1 ? RECmetCount : 0;
                    ReportForTotals.LOST_METEOROLOGICAL = LOSTmetCount >= 1 ? LOSTmetCount : 0;
                    ReportForTotals.HWM_FLAGGED = hwmFlagged >= 1 ? hwmFlagged : 0;
                    ReportForTotals.HWM_COLLECTED = hwmCollected >= 1 ? hwmCollected : 0;


                }//end using
                //}//end using

                return new OperationResult.OK { ResponseResource = ReportForTotals };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.Get

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReports")]
        public OperationResult GetFilteredReports([Optional] int eventId, [Optional] string stateNames, string aDate)
        {
            List<REPORTING_METRICS> ReportList = null;
            IQueryable<REPORTING_METRICS> query = null;

            if (aDate == string.Empty)
                return new OperationResult.BadRequest();

            try
            {
                DateTime? thisDate = (DateTime?)Convert.ToDateTime(aDate);
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (stateNames != string.Empty)
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 eventID = eventId > 0 ? eventId : -1;

                //Get basic authentication password
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (STNEntities2 aSTNE = GetRDS())
                {
                    //get all the reports first to then narrow down
                    query = aSTNE.REPORTING_METRICS;

                    //do we have an EVENT?
                    if (eventID > 0)
                    {
                        query = aSTNE.REPORTING_METRICS.Where(e => e.EVENT_ID == eventID);
                    }

                    //query DATE
                    query = query.Where(e => e.REPORT_DATE == thisDate.Value);

                    if (states.Count >= 2)
                    {

                        //multiple STATES
                        query = from q in query where states.Any(s => q.STATE.Contains(s.Trim())) select q;
                    }

                    if (states.Count == 1)
                    {
                        if (states[0] != "All States" && states[0] != "0")
                        {
                            string thisState = states[0];
                            thisState = GetStateByName(thisState).ToString();
                            query = query.Where(r => r.STATE == thisState);
                        }
                    }

                    //only completed reports please
                    ReportList = query.AsEnumerable().Where(x => x.COMPLETE == 1).ToList();

                    if (ReportList != null)
                        ReportList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
                //   }//end using

                return new OperationResult.OK { ResponseResource = ReportList };
            } //end try
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET


        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredReportsModel")]
        public OperationResult GetFilteredReportModel(int eventId, string aDate, [Optional] string stateNames)
        {
            //this returns only completed reports

            List<ReportResource> ReportList = null;
            IQueryable<REPORTING_METRICS> query = null;

            if (aDate == string.Empty)
                return new OperationResult.BadRequest();

            try
            {
                DateTime? thisDate = (DateTime?)Convert.ToDateTime(aDate);
                List<string> states = new List<string>();
                char[] delimiter = { ',' };
                //stateNames will be a list that is comma separated. Need to parse out
                if (stateNames != string.Empty)
                {
                    states = stateNames.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                Int32 eventID = eventId > 0 ? eventId : -1;

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //get all the reports first to then narrow down
                        query = aSTNE.REPORTING_METRICS;

                        //do we have an EVENT?
                        if (eventID > 0)
                        {
                            query = aSTNE.REPORTING_METRICS.Where(e => e.EVENT_ID == eventID);
                        }

                        //query DATE
                        query = query.Where(e => e.REPORT_DATE == thisDate.Value);

                        if (states.Count >= 2)
                        {

                            //multiple STATES
                            query = from q in query where states.Any(s => q.STATE.Contains(s.Trim())) select q;
                        }

                        if (states.Count == 1)
                        {
                            if (states[0] != "All States" && states[0] != "0")
                            {
                                string thisState = states[0];
                                thisState = GetStateByName(thisState).ToString();
                                query = query.Where(r => r.STATE == thisState);
                            }
                        }

                        //only completed reports please
                        ReportList = query.AsEnumerable().Where(x => x.COMPLETE == 1).Select(x => new ReportResource
                        {
                            Report = x,
                            ReportContacts = getReportContacts(x)
                        }).ToList();

                        //if (ReportList != null)
                        //    ReportList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = ReportList };
            } //end try
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        private List<ReportContactModel> getReportContacts(REPORTING_METRICS x)
        {
            var contactList = x.REPORTMETRIC_CONTACT.Select(c => new ReportContactModel
            {
                ContactId = c.CONTACT_ID,
                FNAME = c.CONTACT.FNAME,
                LNAME = c.CONTACT.LNAME,
                PHONE = c.CONTACT.PHONE,
                ALT_PHONE = c.CONTACT.ALT_PHONE,
                EMAIL = c.CONTACT.EMAIL,
                TYPE = c.CONTACT_TYPE.TYPE
            }).ToList();
            return contactList;

        }//end HttpMethod.GET


        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "PostReportMetrics")]
        public OperationResult Post(REPORTING_METRICS aReportMetrics)
        {
            //Return BadRequest if missing required fields
            if ((aReportMetrics.EVENT_ID <= 0 || aReportMetrics.STATE == null || aReportMetrics.MEMBER_ID <= 0))
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
                        if (!Exists(aSTNE.REPORTING_METRICS, ref aReportMetrics))
                        {
                            aSTNE.REPORTING_METRICS.AddObject(aReportMetrics);
                        }

                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aReportMetrics };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, REPORTING_METRICS aReport)
        {
            REPORTING_METRICS ReportToUpdate = null;
            //Return BadRequest if missing required fields
            if (aReport.REPORTING_METRICS_ID <= 0 || entityId <= 0)
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
                        //Grab the report row to update
                        ReportToUpdate = aSTNE.REPORTING_METRICS.SingleOrDefault(
                                           ps => ps.REPORTING_METRICS_ID == entityId);
                        //Update fields
                        ReportToUpdate.REPORT_DATE = aReport.REPORT_DATE;
                        ReportToUpdate.EVENT_ID = aReport.EVENT_ID;
                        ReportToUpdate.STATE = aReport.STATE;
                        ReportToUpdate.SW_FIELDPERS_NOTACCT = aReport.SW_FIELDPERS_NOTACCT;
                        ReportToUpdate.WQ_FIELDPERS_NOTACCT = aReport.WQ_FIELDPERS_NOTACCT;
                        ReportToUpdate.SW_YEST_FIELDPERS = aReport.SW_YEST_FIELDPERS;
                        ReportToUpdate.WQ_YEST_FIELDPERS = aReport.WQ_YEST_FIELDPERS;
                        ReportToUpdate.SW_TOD_FIELDPERS = aReport.SW_TOD_FIELDPERS;
                        ReportToUpdate.WQ_TOD_FIELDPERS = aReport.WQ_TOD_FIELDPERS;
                        ReportToUpdate.SW_TMW_FIELDPERS = aReport.SW_TMW_FIELDPERS;
                        ReportToUpdate.WQ_TMW_FIELDPERS = aReport.WQ_TMW_FIELDPERS;
                        ReportToUpdate.SW_YEST_OFFICEPERS = aReport.SW_YEST_OFFICEPERS;
                        ReportToUpdate.WQ_YEST_OFFICEPERS = aReport.WQ_YEST_OFFICEPERS;
                        ReportToUpdate.SW_TOD_OFFICEPERS = aReport.SW_TOD_OFFICEPERS;
                        ReportToUpdate.WQ_TOD_OFFICEPERS = aReport.WQ_TOD_OFFICEPERS;
                        ReportToUpdate.SW_TMW_OFFICEPERS = aReport.SW_TMW_OFFICEPERS;
                        ReportToUpdate.WQ_TMW_OFFICEPERS = aReport.WQ_TMW_OFFICEPERS;
                        ReportToUpdate.SW_AUTOS_DEPL = aReport.SW_AUTOS_DEPL;
                        ReportToUpdate.WQ_AUTOS_DEPL = aReport.WQ_AUTOS_DEPL;
                        ReportToUpdate.SW_BOATS_DEPL = aReport.SW_BOATS_DEPL;
                        ReportToUpdate.WQ_BOATS_DEPL = aReport.WQ_BOATS_DEPL;
                        ReportToUpdate.SW_OTHER_DEPL = aReport.SW_OTHER_DEPL;
                        ReportToUpdate.WQ_OTHER_DEPL = aReport.WQ_OTHER_DEPL;
                        ReportToUpdate.GAGE_VISIT = aReport.GAGE_VISIT;
                        ReportToUpdate.GAGE_DOWN = aReport.GAGE_DOWN;
                        ReportToUpdate.TOT_DISCHARGE_MEAS = aReport.TOT_DISCHARGE_MEAS;
                        ReportToUpdate.PLAN_DISCHARGE_MEAS = aReport.PLAN_DISCHARGE_MEAS;
                        ReportToUpdate.TOT_CHECK_MEAS = aReport.TOT_CHECK_MEAS;
                        ReportToUpdate.PLAN_CHECK_MEAS = aReport.PLAN_CHECK_MEAS;
                        ReportToUpdate.PLAN_INDIRECT_MEAS = aReport.PLAN_INDIRECT_MEAS;
                        ReportToUpdate.RATING_EXTENS = aReport.RATING_EXTENS;
                        ReportToUpdate.GAGE_PEAK_RECORD = aReport.GAGE_PEAK_RECORD;
                        ReportToUpdate.PLAN_RAPDEPL_GAGE = aReport.PLAN_RAPDEPL_GAGE;
                        ReportToUpdate.DEP_RAPDEPL_GAGE = aReport.DEP_RAPDEPL_GAGE;
                        ReportToUpdate.REC_RAPDEPL_GAGE = aReport.REC_RAPDEPL_GAGE;
                        ReportToUpdate.LOST_RAPDEPL_GAGE = aReport.LOST_RAPDEPL_GAGE;
                        ReportToUpdate.PLAN_WTRLEV_SENSOR = aReport.PLAN_WTRLEV_SENSOR;
                        ReportToUpdate.DEP_WTRLEV_SENSOR = aReport.DEP_WTRLEV_SENSOR;
                        ReportToUpdate.REC_WTRLEV_SENSOR = aReport.REC_WTRLEV_SENSOR;
                        ReportToUpdate.LOST_WTRLEV_SENSOR = aReport.LOST_WTRLEV_SENSOR;
                        ReportToUpdate.PLAN_WV_SENS = aReport.PLAN_WV_SENS;
                        ReportToUpdate.DEP_WV_SENS = aReport.DEP_WV_SENS;
                        ReportToUpdate.REC_WV_SENS = aReport.REC_WV_SENS;
                        ReportToUpdate.LOST_WV_SENS = aReport.LOST_WV_SENS;
                        ReportToUpdate.PLAN_BAROMETRIC = aReport.PLAN_BAROMETRIC;
                        ReportToUpdate.DEP_BAROMETRIC = aReport.DEP_BAROMETRIC;
                        ReportToUpdate.REC_BAROMETRIC = aReport.REC_BAROMETRIC;
                        ReportToUpdate.LOST_BAROMETRIC = aReport.LOST_BAROMETRIC;
                        ReportToUpdate.PLAN_METEOROLOGICAL = aReport.PLAN_METEOROLOGICAL;
                        ReportToUpdate.DEP_METEOROLOGICAL = aReport.DEP_METEOROLOGICAL;
                        ReportToUpdate.REC_METEOROLOGICAL = aReport.REC_METEOROLOGICAL;
                        ReportToUpdate.LOST_METEOROLOGICAL = aReport.LOST_METEOROLOGICAL;
                        ReportToUpdate.HWM_FLAGGED = aReport.HWM_FLAGGED;
                        ReportToUpdate.HWM_COLLECTED = aReport.HWM_COLLECTED;
                        ReportToUpdate.QW_GAGE_VISIT = aReport.QW_GAGE_VISIT;
                        ReportToUpdate.QW_CONT_GAGEVISIT = aReport.QW_CONT_GAGEVISIT;
                        ReportToUpdate.QW_GAGE_DOWN = aReport.QW_GAGE_DOWN;
                        ReportToUpdate.QW_DISCR_SAMPLES = aReport.QW_DISCR_SAMPLES;
                        ReportToUpdate.COLL_SEDSAMPLES = aReport.COLL_SEDSAMPLES;
                        ReportToUpdate.NOTES = aReport.NOTES;
                        ReportToUpdate.MEMBER_ID = aReport.MEMBER_ID;
                        ReportToUpdate.COMPLETE = aReport.COMPLETE;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aReport };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.PUT

        #endregion

        #region DeleteMethods

        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<REPORTING_METRICS> entityRDS, ref REPORTING_METRICS anEntity)
        {
            REPORTING_METRICS existingEntity;
            REPORTING_METRICS thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.MEMBER_ID == thisEntity.MEMBER_ID &&
                                                              (DateTime.Equals(e.REPORT_DATE, thisEntity.REPORT_DATE)) &&
                                                              (e.EVENT_ID == thisEntity.EVENT_ID) && (e.STATE == thisEntity.STATE) && (e.SW_FIELDPERS_NOTACCT == thisEntity.SW_FIELDPERS_NOTACCT) &&
                                                              (e.WQ_FIELDPERS_NOTACCT == thisEntity.WQ_FIELDPERS_NOTACCT) && (e.SW_YEST_FIELDPERS == thisEntity.SW_YEST_FIELDPERS) &&
                                                              (e.WQ_YEST_FIELDPERS == thisEntity.WQ_YEST_FIELDPERS) && (e.SW_TOD_FIELDPERS == thisEntity.SW_TOD_FIELDPERS == null) &&
                                                              (e.WQ_TOD_FIELDPERS == thisEntity.WQ_TOD_FIELDPERS) && (e.SW_TMW_FIELDPERS == thisEntity.SW_TMW_FIELDPERS) && (e.WQ_TMW_FIELDPERS == thisEntity.WQ_TMW_FIELDPERS) &&
                                                              (e.SW_YEST_OFFICEPERS == thisEntity.SW_YEST_OFFICEPERS) && (e.WQ_YEST_OFFICEPERS == thisEntity.WQ_YEST_OFFICEPERS) &&
                                                              (e.SW_TOD_OFFICEPERS == thisEntity.SW_TOD_OFFICEPERS) && (e.WQ_TOD_OFFICEPERS == thisEntity.WQ_TOD_OFFICEPERS) &&
                                                              (e.SW_TMW_OFFICEPERS == thisEntity.SW_TMW_OFFICEPERS) && (e.WQ_TMW_OFFICEPERS == thisEntity.WQ_TMW_OFFICEPERS) &&
                                                              (e.SW_AUTOS_DEPL == thisEntity.SW_AUTOS_DEPL) && (e.WQ_AUTOS_DEPL == thisEntity.WQ_AUTOS_DEPL) &&
                                                              (e.SW_BOATS_DEPL == thisEntity.SW_BOATS_DEPL) && (e.WQ_BOATS_DEPL == thisEntity.WQ_BOATS_DEPL) &&
                                                              (e.SW_OTHER_DEPL == thisEntity.SW_OTHER_DEPL) && (e.WQ_OTHER_DEPL == thisEntity.WQ_OTHER_DEPL) &&
                                                              (e.GAGE_VISIT == thisEntity.GAGE_VISIT) && (e.GAGE_DOWN == thisEntity.GAGE_DOWN) && (e.TOT_DISCHARGE_MEAS == thisEntity.TOT_DISCHARGE_MEAS) &&
                                                              (e.PLAN_DISCHARGE_MEAS == thisEntity.PLAN_DISCHARGE_MEAS) && (e.TOT_CHECK_MEAS == thisEntity.TOT_CHECK_MEAS) && (e.PLAN_CHECK_MEAS == thisEntity.PLAN_CHECK_MEAS) &&
                                                              (e.PLAN_INDIRECT_MEAS == thisEntity.PLAN_INDIRECT_MEAS) && (e.RATING_EXTENS == thisEntity.RATING_EXTENS) && (e.GAGE_PEAK_RECORD == thisEntity.GAGE_PEAK_RECORD) &&
                                                              (e.PLAN_RAPDEPL_GAGE == thisEntity.PLAN_RAPDEPL_GAGE) && (e.DEP_RAPDEPL_GAGE == thisEntity.DEP_RAPDEPL_GAGE) && (e.REC_RAPDEPL_GAGE == thisEntity.REC_RAPDEPL_GAGE) &&
                                                              (e.LOST_RAPDEPL_GAGE == thisEntity.LOST_RAPDEPL_GAGE) && (e.PLAN_WTRLEV_SENSOR == thisEntity.PLAN_WTRLEV_SENSOR) && (e.DEP_WTRLEV_SENSOR == thisEntity.DEP_WTRLEV_SENSOR) &&
                                                              (e.REC_WTRLEV_SENSOR == thisEntity.REC_WTRLEV_SENSOR) && (e.LOST_WTRLEV_SENSOR == thisEntity.LOST_WTRLEV_SENSOR) && (e.PLAN_WV_SENS == thisEntity.PLAN_WV_SENS) &&
                                                              (e.DEP_WV_SENS == thisEntity.DEP_WV_SENS) && (e.REC_WV_SENS == thisEntity.REC_WV_SENS) && (e.LOST_WV_SENS == thisEntity.LOST_WV_SENS) && (e.PLAN_BAROMETRIC == thisEntity.PLAN_BAROMETRIC) &&
                                                              (e.DEP_BAROMETRIC == thisEntity.DEP_BAROMETRIC) && (e.REC_BAROMETRIC == thisEntity.REC_BAROMETRIC) && (e.LOST_BAROMETRIC == thisEntity.LOST_BAROMETRIC) && (e.PLAN_METEOROLOGICAL == thisEntity.PLAN_METEOROLOGICAL) &&
                                                              (e.DEP_METEOROLOGICAL == thisEntity.DEP_METEOROLOGICAL) && (e.REC_METEOROLOGICAL == thisEntity.REC_METEOROLOGICAL) && (e.LOST_METEOROLOGICAL == thisEntity.LOST_METEOROLOGICAL) &&
                                                              (e.HWM_COLLECTED == thisEntity.HWM_COLLECTED) && (e.HWM_FLAGGED == thisEntity.HWM_FLAGGED) && (e.QW_GAGE_VISIT == thisEntity.QW_GAGE_VISIT) &&
                                                              (e.QW_CONT_GAGEVISIT == thisEntity.QW_CONT_GAGEVISIT) && (e.QW_GAGE_DOWN == thisEntity.QW_GAGE_DOWN) && (e.QW_DISCR_SAMPLES == thisEntity.QW_DISCR_SAMPLES) &&
                                                              (e.COLL_SEDSAMPLES == thisEntity.COLL_SEDSAMPLES) && (e.NOTES == thisEntity.NOTES || thisEntity.NOTES == null));

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

    }//end class PeakSummaryHandler
}//end namespace