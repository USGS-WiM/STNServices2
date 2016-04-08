using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STNDB;
using WiM.Test;
using WiM.Hypermedia;
using STNServices2;

namespace STNServices2.Test
{
    [TestClass]
    public class EndpointTests:EndpointTestBase
    {
        #region Private Fields
        private string host = "http://localhost/";
        private string basicAuth = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1")
                                .GetBytes("fradmin:***REMOVED***"));
            
        #endregion
        #region Constructor
        public EndpointTests():base(new Configuration()){}
        #endregion
        #region Test Methods
        [TestMethod]
        public void AgencyRequest()
        {
            //GET LIST
            List<agency> RequestList = this.GETRequest<List<agency>>(host + Configuration.agencyResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());
            
            //POST
            agency postObj;
            postObj = this.POSTRequest<agency>(host + Configuration.agencyResource, new agency() { agency_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj,"ID: " + postObj.agency_id.ToString());
            
            //GET POSTed item
            agency RequestObj = this.GETRequest<agency>(host + Configuration.agencyResource + "/" + postObj.agency_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.agency_name = "put-test";
            agency putObj = this.PUTRequest<agency>(host + Configuration.agencyResource + "/" + postObj.agency_id,postObj,basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<agency>(host + Configuration.agencyResource + "/" + postObj.agency_id, basicAuth);
            Assert.IsTrue(success);            
        }//end method
        [TestMethod]
        public void ApprovalRequest()
        {
            List<approval> returnedObject = this.GETRequest<List<approval>>(host + Configuration.approvalResource);
            Assert.IsNotNull(returnedObject, returnedObject.Count.ToString());
            //Assert.IsFalse(true);
        }//end method
        [TestMethod]
        public void ContactRequest()
        {
            //GET LIST
            List<contact> RequestList = this.GETRequest<List<contact>>(host + Configuration.contactResource,basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            contact postObj;
            postObj = this.POSTRequest<contact>(host + Configuration.contactResource, new contact() { fname = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.contact_id.ToString());

            //GET POSTed item
            contact RequestObj = this.GETRequest<contact>(host + Configuration.contactResource + "/" + postObj.contact_id,basicAuth);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.fname = "put-test";
            contact putObj = this.PUTRequest<contact>(host + Configuration.contactResource + "/" + postObj.contact_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<contact>(host + Configuration.contactResource + "/" + postObj.contact_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void ContactTypeRequest()
        {
            //GET LIST
            List<contact_type> RequestList = this.GETRequest<List<contact_type>>(host + Configuration.contacttypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            contact_type postObj;
            postObj = this.POSTRequest<contact_type>(host + Configuration.contacttypeResource, new contact_type() { type = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.contact_type_id.ToString());

            //GET POSTed item
            contact_type RequestObj = this.GETRequest<contact_type>(host + Configuration.contacttypeResource + "/" + postObj.contact_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.type = "put-test";
            contact_type putObj = this.PUTRequest<contact_type>(host + Configuration.contacttypeResource + "/" + postObj.contact_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<contact_type>(host + Configuration.contacttypeResource + "/" + postObj.contact_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void CountyRequest()
        {
            //GET LIST
            List<county> RequestList = this.GETRequest<List<county>>(host + Configuration.countyResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            county postObj;
            postObj = this.POSTRequest<county>(host + Configuration.countyResource, new county() { county_name = "post-test" },basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.county_id.ToString());

            //GET POSTed item
            county RequestObj = this.GETRequest<county>(host + Configuration.countyResource + "/" + postObj.county_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.county_name = "put-test";
            county putObj = this.PUTRequest<county>(host + Configuration.countyResource + "/" + postObj.county_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<county>(host + Configuration.countyResource + "/" + postObj.county_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void DataFileRequest()
        {
            //GET LIST
            List<data_file> RequestList = this.GETRequest<List<data_file>>(host + Configuration.datafileResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            //data_file postObj;
            //postObj = this.POSTRequest<data_file>(host + Configuration.datafileResource, new data_file() { = "post-test" },basicAuth);
            //Assert.IsNotNull(postObj, "ID: " + postObj.data_file_id.ToString());

            ////GET POSTed item
            //data_file RequestObj = this.GETRequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id);
            //Assert.IsNotNull(RequestObj);

            ////PUT POSTed item
            //postObj.data_file_name = "put-test";
            //data_file putObj = this.PUTRequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id, postObj, basicAuth);
            //Assert.IsNotNull(putObj);

            ////Delete POSTed item
            //bool success = this.DELETERequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id, basicAuth);
            //Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void DeploymentPriorityRequest()
        {
            //GET LIST
            List<deployment_priority> RequestList = this.GETRequest<List<deployment_priority>>(host + Configuration.deploymentpriorityResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            deployment_priority postObj;
            postObj = this.POSTRequest<deployment_priority>(host + Configuration.deploymentpriorityResource, new deployment_priority() { priority_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.priority_id.ToString());

            //GET POSTed item
            deployment_priority RequestObj = this.GETRequest<deployment_priority>(host + Configuration.deploymentpriorityResource + "/" + postObj.priority_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.priority_name = "put-test";
            deployment_priority putObj = this.PUTRequest<deployment_priority>(host + Configuration.deploymentpriorityResource + "/" + postObj.priority_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<deployment_priority>(host + Configuration.deploymentpriorityResource + "/" + postObj.priority_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void DeploymentTypeRequest()
        {
            //GET LIST
            List<deployment_type> RequestList = this.GETRequest<List<deployment_type>>(host + Configuration.deploymenttypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            deployment_type postObj;
            postObj = this.POSTRequest<deployment_type>(host + Configuration.deploymenttypeResource, new deployment_type() { method = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.deployment_type_id.ToString());

            //GET POSTed item
            deployment_type RequestObj = this.GETRequest<deployment_type>(host + Configuration.deploymenttypeResource + "/" + postObj.deployment_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.method = "put-test";
            deployment_type putObj = this.PUTRequest<deployment_type>(host + Configuration.deploymenttypeResource + "/" + postObj.deployment_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<deployment_type>(host + Configuration.deploymenttypeResource + "/" + postObj.deployment_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void EventRequest()
        {
            //GET LIST
            List<events> RequestList = this.GETRequest<List<events>>(host + Configuration.eventsResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            events postObj;
            postObj = this.POSTRequest<events>(host + Configuration.eventsResource, new events() { event_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.event_id.ToString());

            //GET POSTed item
            events RequestObj = this.GETRequest<events>(host + Configuration.eventsResource + "/" + postObj.event_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.event_name = "put-test";
            events putObj = this.PUTRequest<events>(host + Configuration.eventsResource + "/" + postObj.event_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<events>(host + Configuration.eventsResource + "/" + postObj.event_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void EventStatusRequest()
        {
            //GET LIST
            List<event_status> RequestList = this.GETRequest<List<event_status>>(host + Configuration.eventstatusResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            event_status postObj;
            postObj = this.POSTRequest<event_status>(host + Configuration.eventstatusResource, new event_status() { status = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.event_status_id.ToString());

            //GET POSTed item
            event_status RequestObj = this.GETRequest<event_status>(host + Configuration.eventstatusResource + "/" + postObj.event_status_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.status = "put-test";
            event_status putObj = this.PUTRequest<event_status>(host + Configuration.eventstatusResource + "/" + postObj.event_status_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<event_status>(host + Configuration.eventstatusResource + "/" + postObj.event_status_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void EventTypeRequest()
        {
            //GET LIST
            List<event_type> RequestList = this.GETRequest<List<event_type>>(host + Configuration.eventtypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            event_type postObj;
            postObj = this.POSTRequest<event_type>(host + Configuration.eventtypeResource, new event_type() { type = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.event_type_id.ToString());

            //GET POSTed item
            event_type RequestObj = this.GETRequest<event_type>(host + Configuration.eventtypeResource + "/" + postObj.event_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.type = "put-test";
            event_type putObj = this.PUTRequest<event_type>(host + Configuration.eventtypeResource + "/" + postObj.event_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<event_type>(host + Configuration.eventtypeResource + "/" + postObj.event_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void FileRequest()
        {
            //GET LIST
            List<file> RequestList = this.GETRequest<List<file>>(host + Configuration.fileResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            file postObj;
            postObj = this.POSTRequest<file>(host + Configuration.fileResource, new file() { site_id = 1, description = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.file_id.ToString());

            //GET POSTed item
            file RequestObj = this.GETRequest<file>(host + Configuration.fileResource + "/" + postObj.file_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.description = "put-test";
            file putObj = this.PUTRequest<file>(host + Configuration.fileResource + "/" + postObj.file_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<file>(host + Configuration.fileResource + "/" + postObj.file_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void FileTypeRequest()
        {
            //GET LIST
            List<file_type> RequestList = this.GETRequest<List<file_type>>(host + Configuration.filetypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            file_type postObj;
            postObj = this.POSTRequest<file_type>(host + Configuration.filetypeResource, new file_type() { filetype = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.filetype_id.ToString());

            //GET POSTed item
            file_type RequestObj = this.GETRequest<file_type>(host + Configuration.filetypeResource + "/" + postObj.filetype_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.filetype = "put-test";
            file_type putObj = this.PUTRequest<file_type>(host + Configuration.filetypeResource + "/" + postObj.filetype_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<file_type>(host + Configuration.filetypeResource + "/" + postObj.filetype_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HorizontalCollectionMethodRequest()
        {
            //GET LIST
            List<horizontal_collect_methods> RequestList = this.GETRequest<List<horizontal_collect_methods>>(host + Configuration.horizontalmethodResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            horizontal_collect_methods postObj;
            postObj = this.POSTRequest<horizontal_collect_methods>(host + Configuration.horizontalmethodResource, new horizontal_collect_methods() { hcollect_method = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.hcollect_method_id.ToString());

            //GET POSTed item
            horizontal_collect_methods RequestObj = this.GETRequest<horizontal_collect_methods>(host + Configuration.horizontalmethodResource + "/" + postObj.hcollect_method_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.hcollect_method = "put-test";
            horizontal_collect_methods putObj = this.PUTRequest<horizontal_collect_methods>(host + Configuration.horizontalmethodResource + "/" + postObj.hcollect_method_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<horizontal_collect_methods>(host + Configuration.horizontalmethodResource + "/" + postObj.hcollect_method_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HorizontalDatumHandlerRequest()
        {
            //GET LIST
            List<horizontal_datums> RequestList = this.GETRequest<List<horizontal_datums>>(host + Configuration.horizontaldatumResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            horizontal_datums postObj;
            postObj = this.POSTRequest<horizontal_datums>(host + Configuration.horizontaldatumResource, new horizontal_datums() { datum_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.datum_id.ToString());

            //GET POSTed item
            horizontal_datums RequestObj = this.GETRequest<horizontal_datums>(host + Configuration.horizontaldatumResource + "/" + postObj.datum_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.datum_name = "put-test";
            horizontal_datums putObj = this.PUTRequest<horizontal_datums>(host + Configuration.horizontaldatumResource + "/" + postObj.datum_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<horizontal_datums>(host + Configuration.horizontaldatumResource + "/" + postObj.datum_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HousingTypeRequest()
        {
            //GET LIST
            List<housing_type> RequestList = this.GETRequest<List<housing_type>>(host + Configuration.housingtypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            housing_type postObj;
            postObj = this.POSTRequest<housing_type>(host + Configuration.housingtypeResource, new housing_type() { type_name  = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.housing_type_id.ToString());

            //GET POSTed item
            housing_type RequestObj = this.GETRequest<housing_type>(host + Configuration.housingtypeResource + "/" + postObj.housing_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.type_name = "put-test";
            housing_type putObj = this.PUTRequest<housing_type>(host + Configuration.housingtypeResource + "/" + postObj.housing_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<housing_type>(host + Configuration.housingtypeResource + "/" + postObj.housing_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method

        [TestMethod]
        public void ObjectivePointRequest()
        {
            //GET LIST
            List<objective_point> RequestList = this.GETRequest<List<objective_point>>(host + Configuration.objectivePointResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            objective_point postObj;
            postObj = this.POSTRequest<objective_point>(host + Configuration.objectivePointResource, new objective_point() { 
                name = "test-post", description = "test-descr", op_type_id = 1, date_established = DateTime.Now, site_id = 123 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.objective_point_id.ToString());

            //GET POSTed item
            objective_point RequestObj = this.GETRequest<objective_point>(host + Configuration.objectivePointResource + "/" + postObj.objective_point_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.name = "test-put"; postObj.description = "test-descr2"; postObj.op_type_id = 1; postObj.date_established = DateTime.Now; postObj.site_id = 123;
            objective_point putObj = this.PUTRequest<objective_point>(host + Configuration.objectivePointResource + "/" + postObj.objective_point_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<objective_point>(host + Configuration.objectivePointResource + "/" + postObj.objective_point_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void OPTypeRequest()
        {
            //GET LIST
            List<objective_point_type> RequestList = this.GETRequest<List<objective_point_type>>(host + Configuration.objectivePointTypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            objective_point_type postObj;
            postObj = this.POSTRequest<objective_point_type>(host + Configuration.objectivePointTypeResource, new objective_point_type() { op_type = "test-post" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.objective_point_type_id.ToString());

            //GET POSTed item
            objective_point_type RequestObj = this.GETRequest<objective_point_type>(host + Configuration.objectivePointTypeResource + "/" + postObj.objective_point_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.op_type = "test-put";
            objective_point_type putObj = this.PUTRequest<objective_point_type>(host + Configuration.objectivePointTypeResource + "/" + postObj.objective_point_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<objective_point_type>(host + Configuration.objectivePointTypeResource + "/" + postObj.objective_point_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void OPControlRequest()
        {
            //GET LIST
            List<op_control_identifier> RequestList = this.GETRequest<List<op_control_identifier>>(host + Configuration.opControlResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            op_control_identifier postObj;
            postObj = this.POSTRequest<op_control_identifier>(host + Configuration.opControlResource, new op_control_identifier() { objective_point_id = 1, identifier = "test-post", identifier_type = "PID" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.op_control_identifier_id.ToString());

            //GET POSTed item
            op_control_identifier RequestObj = this.GETRequest<op_control_identifier>(host + Configuration.opControlResource + "/" + postObj.op_control_identifier_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.objective_point_id = 2; postObj.identifier = "test-put"; postObj.identifier_type = "PID";
            op_control_identifier putObj = this.PUTRequest<op_control_identifier>(host + Configuration.opControlResource + "/" + postObj.op_control_identifier_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<op_control_identifier>(host + Configuration.opControlResource + "/" + postObj.op_control_identifier_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void OPMeasurementRequest()
        {
            //GET LIST
            List<op_measurements> RequestList = this.GETRequest<List<op_measurements>>(host + Configuration.opMeasurementsResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            op_measurements postObj;
            postObj = this.POSTRequest<op_measurements>(host + Configuration.opMeasurementsResource, new op_measurements() { objective_point_id = 1, instrument_status_id = 12 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.op_measurements_id.ToString());

            //GET POSTed item
            op_measurements RequestObj = this.GETRequest<op_measurements>(host + Configuration.opMeasurementsResource + "/" + postObj.op_measurements_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.objective_point_id = 2; postObj.instrument_status_id = 22;
            op_measurements putObj = this.PUTRequest<op_measurements>(host + Configuration.opMeasurementsResource + "/" + postObj.op_measurements_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<op_measurements>(host + Configuration.opMeasurementsResource + "/" + postObj.op_measurements_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void OPQualityRequest()
        {
            //GET LIST
            List<op_quality> RequestList = this.GETRequest<List<op_quality>>(host + Configuration.opQualityResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            op_quality postObj;
            postObj = this.POSTRequest<op_quality>(host + Configuration.opQualityResource, new op_quality() { quality = "Post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.op_quality_id.ToString());

            //GET POSTed item
            op_quality RequestObj = this.GETRequest<op_quality>(host + Configuration.opQualityResource + "/" + postObj.op_quality_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.quality = "Put-test";
            op_quality putObj = this.PUTRequest<op_quality>(host + Configuration.opQualityResource + "/" + postObj.op_quality_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<op_quality>(host + Configuration.opQualityResource + "/" + postObj.op_quality_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void PeakSummaryRequest()
        {
            //GET LIST
            List<peak_summary> RequestList = this.GETRequest<List<peak_summary>>(host + Configuration.peakSummaryResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            peak_summary postObj;
            postObj = this.POSTRequest<peak_summary>(host + Configuration.peakSummaryResource, new peak_summary() { member_id = 1, peak_date = DateTime.Now, time_zone = "UTC" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.peak_summary_id.ToString());

            //GET POSTed item
            peak_summary RequestObj = this.GETRequest<peak_summary>(host + Configuration.peakSummaryResource + "/" + postObj.peak_summary_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.member_id = 2; postObj.peak_date = DateTime.Now; postObj.time_zone = "UTC";
            peak_summary putObj = this.PUTRequest<peak_summary>(host + Configuration.peakSummaryResource + "/" + postObj.peak_summary_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<peak_summary>(host + Configuration.peakSummaryResource + "/" + postObj.peak_summary_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void ReportingMetricsRequest()
        {
            //GET LIST
            List<reporting_metrics> RequestList = this.GETRequest<List<reporting_metrics>>(host + Configuration.reportMetricResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            reporting_metrics postObj;
            postObj = this.POSTRequest<reporting_metrics>(host + Configuration.reportMetricResource, new reporting_metrics() { report_date = DateTime.Now, event_id = 1, state = "WI", member_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.reporting_metrics_id.ToString());

            //GET POSTed item
            reporting_metrics RequestObj = this.GETRequest<reporting_metrics>(host + Configuration.reportMetricResource + "/" + postObj.reporting_metrics_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.report_date = DateTime.Now; postObj.event_id = 2; postObj.state = "MN"; postObj.member_id = 1;
            reporting_metrics putObj = this.PUTRequest<reporting_metrics>(host + Configuration.reportMetricResource + "/" + postObj.reporting_metrics_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<reporting_metrics>(host + Configuration.reportMetricResource + "/" + postObj.reporting_metrics_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void RoleRequest()
        {
            //GET LIST
            List<role> RequestList = this.GETRequest<List<role>>(host + Configuration.roleResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            role postObj;
            postObj = this.POSTRequest<role>(host + Configuration.roleResource, new role() { role_name = "post-testName", role_description = "post-testDesc" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.role_id.ToString());

            //GET POSTed item
            role RequestObj = this.GETRequest<role>(host + Configuration.roleResource + "/" + postObj.role_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.role_name = "put-testName"; postObj.role_description = "put-testDesc";
            role putObj = this.PUTRequest<role>(host + Configuration.roleResource + "/" + postObj.role_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<role>(host + Configuration.roleResource + "/" + postObj.role_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void SensorBrandRequest()
        {
            //GET LIST
            List<sensor_brand> RequestList = this.GETRequest<List<sensor_brand>>(host + Configuration.sensorBrandResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            sensor_brand postObj;
            postObj = this.POSTRequest<sensor_brand>(host + Configuration.sensorBrandResource, new sensor_brand() { brand_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.sensor_brand_id.ToString());

            //GET POSTed item
            sensor_brand RequestObj = this.GETRequest<sensor_brand>(host + Configuration.sensorBrandResource + "/" + postObj.sensor_brand_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.brand_name = "put-test";
            sensor_brand putObj = this.PUTRequest<sensor_brand>(host + Configuration.sensorBrandResource + "/" + postObj.sensor_brand_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<sensor_brand>(host + Configuration.sensorBrandResource + "/" + postObj.sensor_brand_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void SensorTypeRequest()
        {
            //GET LIST
            List<sensor_type> RequestList = this.GETRequest<List<sensor_type>>(host + Configuration.sensorTypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            sensor_type postObj;
            postObj = this.POSTRequest<sensor_type>(host + Configuration.sensorTypeResource, new sensor_type() { sensor = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.sensor_type_id.ToString());

            //GET POSTed item
            sensor_type RequestObj = this.GETRequest<sensor_type>(host + Configuration.sensorTypeResource + "/" + postObj.sensor_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.sensor = "put-test";
            sensor_type putObj = this.PUTRequest<sensor_type>(host + Configuration.sensorTypeResource + "/" + postObj.sensor_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<sensor_type>(host + Configuration.sensorTypeResource + "/" + postObj.sensor_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void SiteRequest()
        {
            //GET LIST
            List<site> RequestList = this.GETRequest<List<site>>(host + "/FullSites");//Configuration.siteResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            site postObj;
            postObj = this.POSTRequest<site>(host + Configuration.siteResource, new site() { 
               site_description = "site_post1", latitude_dd = 45.52, longitude_dd = -89.32, hdatum_id = 1,
                hcollect_method_id = 1, state = "WI", county = "Dane County", waterbody = "test2", member_id = 1}, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.site_id.ToString());

            //GET POSTed item
            site RequestObj = this.GETRequest<site>(host + Configuration.siteResource + "/" + postObj.site_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.site_description = "site_put2"; postObj.latitude_dd = 42.3; postObj.longitude_dd = -90.3; postObj.hdatum_id = 2;
            postObj.hcollect_method_id = 2; postObj.state = "WI"; postObj.county = "Dane County"; postObj.waterbody = "test2"; postObj.member_id = 1;
            site putObj = this.PUTRequest<site>(host + Configuration.siteResource + "/" + postObj.site_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<site>(host + Configuration.siteResource + "/" + postObj.site_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void SiteHousingRequest()
        {
            //GET LIST
            List<site_housing> RequestList = this.GETRequest<List<site_housing>>(host + Configuration.siteHousingResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            site_housing postObj;
            postObj = this.POSTRequest<site_housing>(host + Configuration.siteHousingResource, new site_housing() { site_id = 123, housing_type_id = 1, amount = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.site_housing_id.ToString());

            //GET POSTed item
            site_housing RequestObj = this.GETRequest<site_housing>(host + Configuration.siteHousingResource + "/" + postObj.site_housing_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.site_id = 234;
            postObj.housing_type_id = 2;
            postObj.amount = 2;
            site_housing putObj = this.PUTRequest<site_housing>(host + Configuration.siteHousingResource + "/" + postObj.site_housing_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<site_housing>(host + Configuration.siteHousingResource + "/" + postObj.site_housing_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void SourceRequest()
        {
            //GET LIST
            List<source> RequestList = this.GETRequest<List<source>>(host + Configuration.sourceResource, basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            source postObj;
            postObj = this.POSTRequest<source>(host + Configuration.sourceResource, new source() { source_name = "post-test", agency_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.source_id.ToString());

            //GET POSTed item
            source RequestObj = this.GETRequest<source>(host + Configuration.sourceResource + "/" + postObj.source_id, basicAuth);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.source_name = "put-test";
            postObj.agency_id = 2;
            source putObj = this.PUTRequest<source>(host + Configuration.sourceResource + "/" + postObj.source_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            ////Delete POSTed item
            //bool success = this.DELETERequest<state>(host + Configuration.stateResource + "/" + postObj.state_id, basicAuth);
            //Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void StateRequest()
        {
            //GET LIST
            List<state> RequestList = this.GETRequest<List<state>>(host + Configuration.stateResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            state postObj;
            postObj = this.POSTRequest<state>(host + Configuration.stateResource, new state() { state_name = "post-test", state_abbrev = "pt" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.state_id.ToString());

            //GET POSTed item
            state RequestObj = this.GETRequest<state>(host + Configuration.stateResource + "/" + postObj.state_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.state_name = "put-test";
            postObj.state_abbrev = "te";
            state putObj = this.PUTRequest<state>(host + Configuration.stateResource + "/" + postObj.state_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<state>(host + Configuration.stateResource + "/" + postObj.state_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void StatusTypeRequest()
        {
            //GET LIST
            List<status_type> RequestList = this.GETRequest<List<status_type>>(host + Configuration.statusTypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            status_type postObj;
            postObj = this.POSTRequest<status_type>(host + Configuration.statusTypeResource, new status_type() { status = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.status_type_id.ToString());

            //GET POSTed item
            status_type RequestObj = this.GETRequest<status_type>(host + Configuration.statusTypeResource + "/" + postObj.status_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.status = "put-test";
            status_type putObj = this.PUTRequest<status_type>(host + Configuration.statusTypeResource + "/" + postObj.status_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<status_type>(host + Configuration.statusTypeResource + "/" + postObj.status_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void VerticalCollectMethodRequest()
        {
            //GET LIST
            List<vertical_collect_methods> RequestList = this.GETRequest<List<vertical_collect_methods>>(host + Configuration.verticalCollectMethodResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            vertical_collect_methods postObj;
            postObj = this.POSTRequest<vertical_collect_methods>(host + Configuration.verticalCollectMethodResource, new vertical_collect_methods() {  vcollect_method = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.vcollect_method_id.ToString());

            //GET POSTed item
            vertical_collect_methods RequestObj = this.GETRequest<vertical_collect_methods>(host + Configuration.verticalCollectMethodResource + "/" + postObj.vcollect_method_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.vcollect_method = "put-test";
            vertical_collect_methods putObj = this.PUTRequest<vertical_collect_methods>(host + Configuration.verticalCollectMethodResource + "/" + postObj.vcollect_method_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<vertical_collect_methods>(host + Configuration.verticalCollectMethodResource + "/" + postObj.vcollect_method_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void VerticalDatumRequest()
        {
            //GET LIST
            List<vertical_datums> RequestList = this.GETRequest<List<vertical_datums>>(host + Configuration.verticalDatumResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            vertical_datums postObj;
            postObj = this.POSTRequest<vertical_datums>(host + Configuration.verticalDatumResource, new vertical_datums() { datum_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.datum_id.ToString());

            //GET POSTed item
            vertical_datums RequestObj = this.GETRequest<vertical_datums>(host + Configuration.verticalDatumResource + "/" + postObj.datum_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.datum_name = "put-test";
            vertical_datums putObj = this.PUTRequest<vertical_datums>(host + Configuration.verticalDatumResource + "/" + postObj.datum_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<vertical_datums>(host + Configuration.verticalDatumResource + "/" + postObj.datum_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        #endregion
    }
}
