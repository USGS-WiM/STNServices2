using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STNDB;
using WiM.Test;
using WiM.Hypermedia;
using STNServices2;
using STNServices2.Resources;

namespace STNServices2.Test
{
    [TestClass]
    public class EndpointTests:EndpointTestBase
    {
        #region Private Fields
        private string host = "http://localhost/";
        private string basicAuth = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
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
            
            //'GetMemberAgency'
            agency memberAgency = this.GETRequest<agency>(host + Configuration.memberResource + "/1/" + Configuration.agencyResource);
            Assert.IsNotNull(memberAgency);

            //'GetSourceAgency'
            agency sourceAgency = this.GETRequest<agency>(host + Configuration.sourceResource + "/99/" + Configuration.agencyResource);
            Assert.IsNotNull(sourceAgency);

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

            //POST DataFileApproval
            List<data_file> df = this.GETRequest<List<data_file>>(host + Configuration.datafileResource, basicAuth);
            approval postDFA; Int32 dfID = df.Find(x => !x.approval_id.HasValue).data_file_id;
            postDFA = this.POSTRequest<approval>(host + Configuration.datafileResource + "/" + dfID + "Approve", null, basicAuth);
            Assert.IsNotNull(postDFA, "ID: " + postDFA.approval_id.ToString());
            
            //'GetDataFileApproval'            
            approval returnedDFapproval = this.GETRequest<approval>(host + Configuration.datafileResource + "/" + dfID + "/Approval");
            Assert.IsNotNull(returnedDFapproval);           

            //POST HWMApproval
            List<hwm> h = this.GETRequest<List<hwm>>(host + Configuration.hwmResource);
            approval postHWMA; Int32 hwmID = h.Find(x => !x.approval_id.HasValue).hwm_id;
            postHWMA = this.POSTRequest<approval>(host + Configuration.hwmResource + "/" + hwmID + "Approve", null, basicAuth);
            Assert.IsNotNull(postHWMA, "ID: " + postHWMA.approval_id.ToString());
            
            //'GetHWMApproval'
            approval returnedHWMapproval = this.GETRequest<approval>(host + Configuration.hwmResource + "/" + hwmID + "/Approval");
            Assert.IsNotNull(returnedHWMapproval);

            //POST NWISDataFileApproval            
            approval postNWISApp; Int32 nwisDF = df.Find(x => !x.approval_id.HasValue).data_file_id;
            postNWISApp = this.POSTRequest<approval>(host + Configuration.datafileResource + "/" + nwisDF + "NWISApprove", null, basicAuth);
            Assert.IsNotNull(postNWISApp, "ID: " + postNWISApp.approval_id.ToString());
            
            //HWMUnpproval 
            bool hsuccess = this.DELETERequest<agency>(host + Configuration.hwmResource + "/" + hwmID + "/Unapprove", basicAuth);
            Assert.IsTrue(hsuccess);

            //DataFileUnapproval 
            bool dfsuccess = this.DELETERequest<agency>(host + Configuration.datafileResource + "/" + dfID + "/Unapprove", basicAuth);
            Assert.IsTrue(dfsuccess);         

            //do post again to be able to test delete approval
            //POST HWMApproval
            List<hwm> allHWMs = this.GETRequest<List<hwm>>(host + Configuration.hwmResource);
            approval postHWMapproval; Int32 ahwmID = allHWMs.Find(x => !x.approval_id.HasValue).hwm_id;
            postHWMapproval = this.POSTRequest<approval>(host + Configuration.hwmResource + "/" + ahwmID + "Approve", null, basicAuth);
            Assert.IsNotNull(postHWMapproval, "ID: " + postHWMapproval.approval_id.ToString());
            //Delete POSTed item
            bool success = this.DELETERequest<approval>(host + Configuration.approvalResource + "/" + postHWMapproval.approval_id, basicAuth);
            Assert.IsTrue(success);            


        }//end method
        [TestMethod]
        public void ContactRequest()
        {
            //GET LIST
            List<contact> RequestList = this.GETRequest<List<contact>>(host + Configuration.contactResource,basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET 'GetReportMetricContactsByType'
            Int32 rmId = this.GETRequest<List<reporting_metrics>>(host + Configuration.reportMetricResource, basicAuth).Find(x => x.reporting_metrics_id > 0).reporting_metrics_id;
            contact RMContactbyT = this.GETRequest<contact>(host + Configuration.contactResource + "?ReportMetric=" + rmId + "&ContactType=1", basicAuth);
            Assert.IsNotNull(RMContactbyT);

            //Delete ReportContact Relationship
            bool repCsuccess = this.DELETERequest<contact>(host + Configuration.contactResource + "/" + RMContactbyT.contact_id + "/removeReportContact?ReportId=" + rmId, basicAuth);
            Assert.IsTrue(repCsuccess);

            //GET 'GetReportMetricContacts'
            List<contact> RMContact = this.GETRequest<List<contact>>(host + Configuration.contactResource + "?ReportMetric=182", basicAuth);
            Assert.IsNotNull(RMContact, RMContact.Count.ToString());

            //POST
            contact postObj;
            postObj = this.POSTRequest<contact>(host + Configuration.contactResource, new contact() { fname = "post", lname="Test", phone ="123" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.contact_id.ToString());

            //POST reportContact
            //contact postreportContObj;
            //postreportContObj = this.POSTRequest<contact>(host + Configuration.contactResource, new contact() { fname = "post", lname = "Test", phone = "123" }, basicAuth);
            //Assert.IsNotNull(postreportContObj, "ID: " + postreportContObj.contact_id.ToString());

            //GET POSTed item
            contact RequestObj = this.GETRequest<contact>(host + Configuration.contactResource + "/" + postObj.contact_id,basicAuth);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.fname = "put";
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

            //GET 'ContactsContactType
            contact_type contactTypeContact = this.GETRequest<contact_type>(host + Configuration.contactResource + "/142/" + Configuration.contacttypeResource);
            Assert.IsNotNull(contactTypeContact);

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

            //GET StateIdCounties
            List<county> StateCoList = this.GETRequest<List<county>>(host + Configuration.stateResource  + "/1/" + Configuration.countyResource);
            Assert.IsNotNull(StateCoList);

            //GET StateAbbrevCounties
            List<county> StateAbCoList = this.GETRequest<List<county>>(host + Configuration.stateResource + "/" + Configuration.countyResource + "?StateAbbrev=AL");
            Assert.IsNotNull(StateAbCoList);

            //GET SiteStateCounties
            List<county> SiteCoList = this.GETRequest<List<county>>(host + Configuration.siteResource + "/CountiesByState?StateAbbrev=AL");
            Assert.IsNotNull(SiteCoList);

            //POST
            county postObj;
            postObj = this.POSTRequest<county>(host + Configuration.countyResource, new county() { county_name = "post-test County", state_id =1, state_fip=1, county_fip =1},basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.county_id.ToString());

            //GET POSTed item
            county RequestObj = this.GETRequest<county>(host + Configuration.countyResource + "/" + postObj.county_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.county_name = "post-test Parish";
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

            //GET File Datafile
            Int32 fileId = this.GETRequest<List<file>>(host + Configuration.fileResource).Find(f => f.data_file_id > 1).file_id;
            data_file FileDF = this.GETRequest<data_file>(host + Configuration.fileResource + "/" + fileId + "/DataFile", basicAuth);
            Assert.IsNotNull(FileDF);

            //GET Approval Datafile
            List<data_file> appDFList = this.GETRequest<List<data_file>>(host + Configuration.approvalResource + "/" + RequestList.Find(df => df.approval_id > 1).approval_id + "/" + Configuration.datafileResource, basicAuth);
            Assert.IsNotNull(appDFList, appDFList.Count.ToString());

            //GET PeakSummary Datafile
            List<data_file> peakDFList = this.GETRequest<List<data_file>>(host + Configuration.peakSummaryResource + "/" + RequestList.Find(df => df.peak_summary_id > 1).peak_summary_id + "/" + Configuration.datafileResource, basicAuth);
            Assert.IsNotNull(peakDFList, peakDFList.Count.ToString());

            //GET instrument Datafile
            List<data_file> instrDFList = this.GETRequest<List<data_file>>(host + Configuration.instrumentsResource + "/" + RequestList.Find(df => df.instrument_id > 1).instrument_id + "/" + Configuration.datafileResource, basicAuth);
            Assert.IsNotNull(instrDFList, instrDFList.Count.ToString());

            //GET Filtered Datafile
            List<data_file> filteredDFList = this.GETRequest<List<data_file>>(host + Configuration.datafileResource + "/?IsApproved=false&Event=23", basicAuth);//optional props: eventId, memberId, stateAbb
            Assert.IsNotNull(filteredDFList, filteredDFList.Count.ToString());

            //POST
            data_file postObj;
            postObj = this.POSTRequest<data_file>(host + Configuration.datafileResource, new data_file() { good_start = DateTime.Now.Date, good_end = DateTime.Now.Date, collect_date = DateTime.Now.Date,
                instrument_id = 7493, processor_id = 1, time_zone= "UTC" },basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.data_file_id.ToString());

            //GET POSTed item
            data_file RequestObj = this.GETRequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id, basicAuth);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.good_end = DateTime.Now.Date; postObj.instrument_id = 7;
            data_file putObj = this.PUTRequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<data_file>(host + Configuration.datafileResource + "/" + postObj.data_file_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void DeploymentPriorityRequest()
        {
            //GET LIST
            List<deployment_priority> RequestList = this.GETRequest<List<deployment_priority>>(host + Configuration.deploymentpriorityResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetSitePriority
            List<site> sites = this.GETRequest<List<site>>(host + Configuration.siteResource);
            Int32 siteId = sites.Find(s => s.priority_id > 0).site_id;
            deployment_priority SiteDPObj = this.GETRequest<deployment_priority>(host + Configuration.siteResource + "/" + siteId + "/" + Configuration.deploymentpriorityResource);
            Assert.IsNotNull(SiteDPObj);

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

            //getSensorDeploymentTypes
            List<deployment_type> SensDepList = this.GETRequest<List<deployment_type>>(host + Configuration.sensorTypeResource + "/1/" + Configuration.deploymenttypeResource);
            Assert.IsNotNull(SensDepList, SensDepList.Count.ToString());

            //getInstrumentDeploymentTypes
            Int32 instId = this.GETRequest<List<instrument>>(host + Configuration.instrumentsResource).Find(i => i.deployment_type_id > 0).instrument_id;
            deployment_type InsDepList = this.GETRequest<deployment_type>(host + Configuration.instrumentsResource + "/" + instId + "/DeploymentType");
            Assert.IsNotNull(InsDepList);

            //Delete sensorDeployment Relationship (manually add sensor 7 and deployment 10 and relationship first to delete it so dont mess with other relationships
            //bool senDepsuccess = this.DELETERequest<deployment_type>(host + Configuration.sensorTypeResource + "/7/removeDeploymentType?DeploymentTypeId=10", basicAuth);
            //Assert.IsTrue(senDepsuccess);
            
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

            //GET SiteEvents
            List<site> sites = this.GETRequest<List<site>>(host + Configuration.siteResource);
            Int32 siteId = sites.Find(s => s.priority_id > 0).site_id;
            List<events> siteEventList = this.GETRequest<List<events>>(host + Configuration.eventsResource + "?Site="+ siteId);
            Assert.IsNotNull(siteEventList, siteEventList.Count.ToString());

            //GET EventTypeEvents
            List<events> EventTypeEventList = this.GETRequest<List<events>>(host + Configuration.eventtypeResource + "/1/" + Configuration.eventsResource);
            Assert.IsNotNull(EventTypeEventList, EventTypeEventList.Count.ToString());

            //GET EventStatusEvents
            List<events> EventStatusEventList = this.GETRequest<List<events>>(host + Configuration.eventstatusResource + "/1/" + Configuration.eventsResource);
            Assert.IsNotNull(EventStatusEventList, EventStatusEventList.Count.ToString());

            //GET filteredEvents
            List<events> filteredEventList = this.GETRequest<List<events>>(host + Configuration.eventsResource + "/FilteredEvents?Date=08/01/2012&Type=2");
            Assert.IsNotNull(filteredEventList, filteredEventList.Count.ToString());

            //POST
            events postObj;
            postObj = this.POSTRequest<events>(host + Configuration.eventsResource, new events() { event_name = "post-test", event_type_id =1, event_status_id =1, event_coordinator =1}, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.event_id.ToString());

            //GET POSTed item
            events RequestObj = this.GETRequest<events>(host + Configuration.eventsResource + "/" + postObj.event_id);
            Assert.IsNotNull(RequestObj);

            //GET hwmEvent
            Int32 hwmId = this.GETRequest<List<hwm>>(host + Configuration.hwmResource).Find(s => s.hwm_id > 0).hwm_id;
            events hwmEvent = this.GETRequest<events>(host + Configuration.hwmResource + "/" + hwmId + "/Event");
            Assert.IsNotNull(hwmEvent);

            //GET instrumentEvent
            Int32 instId = this.GETRequest<List<instrument>>(host + Configuration.instrumentsResource).Find(s => s.instrument_id > 0).instrument_id;
            events InstEvent = this.GETRequest<events>(host + Configuration.instrumentsResource + "/" + instId + "/Event");
            Assert.IsNotNull(InstEvent);

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

            //GET Event EventStatus
            event_status evEvStatus = this.GETRequest<event_status>(host + Configuration.eventsResource + "/23/Status");
            Assert.IsNotNull(evEvStatus);

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

            //GET Event EventType
            event_type evEvType = this.GETRequest<event_type>(host + Configuration.eventsResource + "/23/Type");
            Assert.IsNotNull(evEvType);

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

            //POST{ site_id = 1, description = "post-test" }
            file postObj;
            postObj = this.POSTRequest<file>(host + Configuration.fileResource, new file() {
                name = "http://waterdata.usgs.gov/nwis/uv?site_no=", path = "<link>", file_date = Convert.ToDateTime("2016-05-24T20:23:41.617Z"), filetype_id = 2,
                site_id = 3089, data_file_id = 3694, instrument_id = 7493, is_nwis = 1, description = "test"}, basicAuth);
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

            //GET file filetype
            Int32 fileId = this.GETRequest<List<file>>(host + Configuration.fileResource).Find(f => f.file_id > 0).file_id;
            file_type fileFiletype = this.GETRequest<file_type>(host + Configuration.fileResource + "/" + fileId + "/Type");
            Assert.IsNotNull(fileFiletype);

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

            //GET hwm horizontal collection method item
            Int32 hwmId = this.GETRequest<List<hwm>>(host + Configuration.hwmResource).Find(h => h.hwm_id > 0).hwm_id;
            horizontal_collect_methods hwmHcollMethod = this.GETRequest<horizontal_collect_methods>(host + Configuration.hwmResource + "/" + hwmId + "/HorizontalMethod");
            Assert.IsNotNull(hwmHcollMethod);

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
            postObj = this.POSTRequest<horizontal_datums>(host + Configuration.horizontaldatumResource, new horizontal_datums() { datum_name = "post-test", datum_abbreviation="test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.datum_id.ToString());

            //GET POSTed item
            horizontal_datums RequestObj = this.GETRequest<horizontal_datums>(host + Configuration.horizontaldatumResource + "/" + postObj.datum_id);
            Assert.IsNotNull(RequestObj);

            //GET site horizontal datum item
            Int32 siteId = this.GETRequest<List<site>>(host + Configuration.siteResource).Find(s => s.site_id > 0).site_id;
            horizontal_datums siteHdatum = this.GETRequest<horizontal_datums>(host + Configuration.siteResource + "/" + siteId + "/hDatum");
            Assert.IsNotNull(siteHdatum);

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

            //GET instrument housing type item
            Int32 instId = this.GETRequest<List<instrument>>(host + Configuration.instrumentsResource).Find(i => i.housing_type_id > 0).instrument_id;
            housing_type instHousingType = this.GETRequest<housing_type>(host + Configuration.instrumentsResource + "/" + instId + "/InstrumentHousingType");
            Assert.IsNotNull(instHousingType);

            //PUT POSTed item
            postObj.type_name = "put-test";
            housing_type putObj = this.PUTRequest<housing_type>(host + Configuration.housingtypeResource + "/" + postObj.housing_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<housing_type>(host + Configuration.housingtypeResource + "/" + postObj.housing_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HWMRequest()
        {
            //GET LIST
            List<hwm> RequestList = this.GETRequest<List<hwm>>(host + Configuration.hwmResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET eventHWMs "/Events/{eventId}/HWMs"
            Int32 eventId = this.GETRequest<List<events>>(host + Configuration.eventsResource).Find(e => e.hwms != null).event_id;
            List<hwm> eventHWMList = this.GETRequest<List<hwm>>(host + Configuration.eventsResource + "/" + eventId + "/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(eventHWMList, eventHWMList.Count.ToString());

            //GET SiteEventHWMs "Sites/{siteId}/EventHWMs?Event={eventId}"
            Int32 siteId = this.GETRequest<List<site>>(host + Configuration.siteResource).Find(s => s.hwms != null).site_id;
            List<hwm> SEventHWMList = this.GETRequest<List<hwm>>(host + Configuration.siteResource + "/" + siteId + "/EventHWMs?Event=35", basicAuth);
            Assert.IsNotNull(SEventHWMList, SEventHWMList.Count.ToString());

            //GET ApprovalHWMs "/HWMs?IsApproved={approved}&Event={eventId}&Member={memberId}&State={state}"
            List<hwm> approvalHWMList = this.GETRequest<List<hwm>>(host + Configuration.hwmResource + "?IsApproved=true&Event=35", basicAuth);
            Assert.IsNotNull(approvalHWMList, approvalHWMList.Count.ToString());

            //GET ApprovEDHWMs "/Approvals/{ApprovalId}/HWMs"           
            List<hwm> approvEDHWMList = this.GETRequest<List<hwm>>(host + Configuration.approvalResource + "/33/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(approvEDHWMList, approvEDHWMList.Count.ToString());

            //GET memberHWMs "Members/{memberId}/HWMs"
            List<hwm> memberHWMList = this.GETRequest<List<hwm>>(host + Configuration.memberResource + "/32/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(memberHWMList, memberHWMList.Count.ToString());

            //GET HWMQualityHWMs "/HWMQualities/{hwmQualityId}/HWMs"
            List<hwm> HWMQualityHWMList = this.GETRequest<List<hwm>>(host + Configuration.hwmqualityResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(HWMQualityHWMList, HWMQualityHWMList.Count.ToString());

            //GET HWMTypeHWMs "/HWMTypes/{hwmTypeId}/HWMs"
            List<hwm> HWMTypeHWMList = this.GETRequest<List<hwm>>(host + Configuration.hwmtypeResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(HWMTypeHWMList, HWMTypeHWMList.Count.ToString());

            //GET HmethodHWMs "/HorizontalMethods/{hmethodId}/HWMs"
            List<hwm> HmethodHWMList = this.GETRequest<List<hwm>>(host + Configuration.horizontalmethodResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(HmethodHWMList, HmethodHWMList.Count.ToString());

            //GET VmethodHWMs "/VerticalMethods/{vmethodId}/HWMs"
            List<hwm> VmethodHWMList = this.GETRequest<List<hwm>>(host + Configuration.verticalDatumResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(VmethodHWMList, VmethodHWMList.Count.ToString());

            //GET siteHWMs "/Sites/{siteId}/HWMs"
            List<hwm> siteHWMList = this.GETRequest<List<hwm>>(host + Configuration.siteResource + "/" + siteId + "/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(siteHWMList, siteHWMList.Count.ToString());

            //GET VDatumHWMs "/VerticalDatums/{vdatumId}/HWMs"
            List<hwm> VDatumHWMList = this.GETRequest<List<hwm>>(host + Configuration.verticalDatumResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(VDatumHWMList, VDatumHWMList.Count.ToString());

            //GET markerHWMs "/Markers/{markerId}/HWMs"
            List<hwm> markerHWMList = this.GETRequest<List<hwm>>(host + Configuration.markerResource + "/1/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(markerHWMList, markerHWMList.Count.ToString());

            //GET peakHWMs "/PeakSummaries/{peakSummaryId}/HWMs"            
            List<hwm> peakHWMList = this.GETRequest<List<hwm>>(host + Configuration.peakSummaryResource + "/1126/" + Configuration.hwmResource, basicAuth);
            Assert.IsNotNull(peakHWMList, peakHWMList.Count.ToString());

            //GET filteredHWMs "/HWMs/FilteredHWMs?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}&States={states}&County={counties}&HWMType={hwmTypeIDs}&HWMQuality={hwmQualIDs}&HWMEnvironment={hwmEnvironment}&SurveyComplete={surveyComplete}&StillWater={stillWater}"
            List<hwm> filteredHWMList = this.GETRequest<List<hwm>>(host + Configuration.hwmResource + "/FilteredHWMs.json?Event=8&EventType=&EventStatus=0&States=FL&County=&HWMType=&HWMQuality=&HWMEnvironment=&SurveyComplete=&StillWater=", basicAuth);
            Assert.IsNotNull(filteredHWMList, filteredHWMList.Count.ToString());  //HWMs/FilteredHWMs.json?Event=8&EventType=&EventStatus=0&States=FL&County=&HWMType=&HWMQuality=&HWMEnvironment=&SurveyComplete=&StillWater=

            //POST
            hwm postObj;
            postObj = this.POSTRequest<hwm>(host + Configuration.hwmResource, new hwm()
            {
                site_id = 1,
                event_id = 1,
                hwm_type_id = 1,
                hwm_quality_id = 1,
                hwm_environment = "post-Test",
                hdatum_id = 1,
                flag_member_id = 1
            }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.hwm_id.ToString());

            //GET POSTed item
            hwm RequestObj = this.GETRequest<hwm>(host + Configuration.hwmResource + "/" + postObj.hwm_id, basicAuth);
            Assert.IsNotNull(RequestObj);

            //GET file hwm item  fileResource+"/{fileId}/HWM"            
            hwm fileHWM = this.GETRequest<hwm>(host + Configuration.fileResource + "/8152/HWM", basicAuth);
            Assert.IsNotNull(fileHWM);

            //PUT POSTed item
            postObj.hwm_environment = "put-test";
            hwm putObj = this.PUTRequest<hwm>(host + Configuration.hwmResource + "/" + postObj.hwm_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            ////Delete POSTed item
            bool success = this.DELETERequest<hwm>(host + Configuration.hwmResource + "/" + postObj.hwm_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HWMQualityRequest()
        {
            //GET LIST
            List<hwm_qualities> RequestList = this.GETRequest<List<hwm_qualities>>(host + Configuration.hwmqualityResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            hwm_qualities postObj;
            postObj = this.POSTRequest<hwm_qualities>(host + Configuration.hwmqualityResource, new hwm_qualities() { hwm_quality = "POST-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.hwm_quality_id.ToString());

            //GET POSTed item
            hwm_qualities RequestObj = this.GETRequest<hwm_qualities>(host + Configuration.hwmqualityResource + "/" + postObj.hwm_quality_id);
            Assert.IsNotNull(RequestObj);

            //GET 'GetHWMQuality'
            hwm_qualities hwmQual = this.GETRequest<hwm_qualities>(host + Configuration.hwmResource + "/13284/Quality");
            Assert.IsNotNull(hwmQual);

            //PUT POSTed item
            postObj.hwm_quality = "put-test";
            hwm_qualities putObj = this.PUTRequest<hwm_qualities>(host + Configuration.hwmqualityResource + "/" + postObj.hwm_quality_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<hwm>(host + Configuration.hwmqualityResource + "/" + postObj.hwm_quality_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void HWMTypeRequest()
        {
            //GET LIST
            List<hwm_types> RequestList = this.GETRequest<List<hwm_types>>(host + Configuration.hwmtypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            hwm_types postObj;
            postObj = this.POSTRequest<hwm_types>(host + Configuration.hwmtypeResource, new hwm_types() { hwm_type = "POST-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.hwm_type_id.ToString());

            //GET POSTed item
            hwm_types RequestObj = this.GETRequest<hwm_types>(host + Configuration.hwmtypeResource + "/" + postObj.hwm_type_id);
            Assert.IsNotNull(RequestObj);

            //GET 'GetHWMType'
            hwm_types hwmType = this.GETRequest<hwm_types>(host + Configuration.hwmResource + "/13284/Type");
            Assert.IsNotNull(hwmType);

            //PUT POSTed item
            postObj.hwm_type = "put-test";
            hwm_types putObj = this.PUTRequest<hwm_types>(host + Configuration.hwmtypeResource + "/" + postObj.hwm_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<hwm_types>(host + Configuration.hwmtypeResource + "/" + postObj.hwm_type_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void InstrumentCollectionContitionsRequest()
        {
            //GET LIST
            List<instr_collection_conditions> RequestList = this.GETRequest<List<instr_collection_conditions>>(host + Configuration.instrcollectionResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            instr_collection_conditions postObj;
            postObj = this.POSTRequest<instr_collection_conditions>(host + Configuration.instrcollectionResource, new instr_collection_conditions() { condition = "POST-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.id.ToString());

            //GET POSTed item
            instr_collection_conditions RequestObj = this.GETRequest<instr_collection_conditions>(host + Configuration.instrcollectionResource + "/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //GET Instrument's collectionCondition item
            instr_collection_conditions instColl = this.GETRequest<instr_collection_conditions>(host + Configuration.instrumentsResource + "/25/CollectCondition");
            Assert.IsNotNull(instColl);

            //PUT POSTed item
            postObj.condition = "put-test";
            instr_collection_conditions putObj = this.PUTRequest<instr_collection_conditions>(host + Configuration.instrcollectionResource + "/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<instr_collection_conditions>(host + Configuration.instrcollectionResource + "/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void InstrumentRequest()
        {
            bool test = this.DELETERequest<instrument>(host + Configuration.instrumentsResource + "/" + 3042, basicAuth);
            Assert.IsTrue(test);

            //GET LIST
            List<instrument> RequestList = this.GETRequest<List<instrument>>(host + Configuration.instrumentsResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GetSiteInstruments "/Sites/{siteId}/Instruments"
            List<instrument> siteInstrList = this.GETRequest<List<instrument>>(host + Configuration.siteResource + "/3148/" + Configuration.instrumentsResource);
            Assert.IsNotNull(siteInstrList, siteInstrList.Count.ToString());

            //GetSensorTypeInstruments "/SensorTypes/{sensorTypeId}/Instruments"
            List<instrument> sensoInstrList = this.GETRequest<List<instrument>>(host + Configuration.sensorTypeResource + "/1/" + Configuration.instrumentsResource);
            Assert.IsNotNull(sensoInstrList, sensoInstrList.Count.ToString());

            //GetSensorBrandInstruments "/SensorBrands/{sensorBrandId}/Instruments"
            List<instrument> sensBrInstrList = this.GETRequest<List<instrument>>(host + Configuration.sensorBrandResource + "/1/" + Configuration.instrumentsResource);
            Assert.IsNotNull(sensBrInstrList, sensBrInstrList.Count.ToString());

            //GetDeploymentTypeInstruments  "/DeploymentTypes/{deploymentTypeId}/Instruments"
            List<instrument> depInstrList = this.GETRequest<List<instrument>>(host + Configuration.deploymenttypeResource + "/1/" + Configuration.instrumentsResource);
            Assert.IsNotNull(depInstrList, depInstrList.Count.ToString());

            //GetEventInstruments "/Events/{eventId}/Instruments"
            List<instrument> eventInstrList = this.GETRequest<List<instrument>>(host + Configuration.eventsResource + "/8/" + Configuration.instrumentsResource);
            Assert.IsNotNull(eventInstrList, eventInstrList.Count.ToString());

            //GetSiteEventInstruments "/Sites/{siteId}/Instruments?Event={eventId}
            List<instrument> siteEvInstrList = this.GETRequest<List<instrument>>(host + Configuration.siteResource + "/3148/" + Configuration.instrumentsResource + "?Event=8");
            Assert.IsNotNull(siteEvInstrList, siteEvInstrList.Count.ToString());

            //GetFilteredInstruments "/Instruments/FilteredInstruments?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}&States={states}&County={counties}&CurrentStatus={statusIDs}&CollectionCondition={collectionConditionIDs}&DeploymentType={deploymentTypeIDs}
            List<instrument> filtInstrList = this.GETRequest<List<instrument>>(host + Configuration.instrumentsResource + "/FilteredInstruments?Event=8&States=FL&CollectionCondition=4&DeploymentType=3");
            Assert.IsNotNull(filtInstrList, filtInstrList.Count.ToString());

            //GetSiteFullInstrumentList "/Sites/{siteId}/SiteFullInstrumentList"
            List<instrument> siteFInstrList = this.GETRequest<List<instrument>>(host + Configuration.siteResource + "/3148/SiteFullInstrumentList");
            Assert.IsNotNull(siteFInstrList, siteFInstrList.Count.ToString());

            //POST
            instrument postObj;
            postObj = this.POSTRequest<instrument>(host + Configuration.instrumentsResource, new instrument() { 
                sensor_type_id = 1, sensor_brand_id = 1, serial_number = "postTest123", event_id = 1, site_id = 1, location_description = "Post-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.instrument_id.ToString());

            //GET POSTed item
            instrument RequestObj = this.GETRequest<instrument>(host + Configuration.instrumentsResource + "/" + postObj.instrument_id);
            Assert.IsNotNull(RequestObj);

            //GET GetFullInstruments "/Instruments/{instrumentId}/FullInstrument"
            FullInstrument fullInst = this.GETRequest<FullInstrument>(host + Configuration.instrumentsResource + "/2412/FullInstrument");
            Assert.IsNotNull(fullInst);

            //GET GetDataFileInstrument "/DataFiles/{dataFileId}/Instrument"
            instrument dfInstr = this.GETRequest<instrument>(host + Configuration.datafileResource + "/92/Instrument");
            Assert.IsNotNull(dfInstr);

            //GET GetInstrumentStatusInstrument "/InstrumentStatus/{instrumentStatusId}/Instrument"
            instrument InstStatInstr = this.GETRequest<instrument>(host + Configuration.instrumentStatusResource + "/106/Instrument");
            Assert.IsNotNull(InstStatInstr);

            //GET GetFileInstrument "/Files/{fileId}/Instrument"
            instrument fileInstr = this.GETRequest<instrument>(host + Configuration.fileResource + "/11254/Instrument");
            Assert.IsNotNull(fileInstr);

            //PUT POSTed item
            postObj.location_description = "put-test"; postObj.serial_number = "putTest123";
            instrument putObj = this.PUTRequest<instrument>(host + Configuration.instrumentsResource + "/" + postObj.instrument_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<instrument>(host + Configuration.instrumentsResource + "/" + postObj.instrument_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void InstrumentStatusRequest()
        {
            //GET LIST
            List<instrument_status> RequestList = this.GETRequest<List<instrument_status>>(host + Configuration.instrumentStatusResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetInstrumentStatusLog
            List<instrument_status> instStatusList = this.GETRequest<List<instrument_status>>(host + Configuration.instrumentsResource + "/25/InstrumentStatusLog");
            Assert.IsNotNull(instStatusList, instStatusList.Count.ToString());

            //POST
            instrument_status postObj;
            postObj = this.POSTRequest<instrument_status>(host + Configuration.instrumentStatusResource, new instrument_status() {
                instrument_id = 1, time_stamp = DateTime.Now, status_type_id = 1, member_id = 1, time_zone = "MST", notes = "Post-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.instrument_status_id.ToString());

            //GET POSTed item
            instrument_status RequestObj = this.GETRequest<instrument_status>(host + Configuration.instrumentStatusResource + "/" + postObj.instrument_status_id);
            Assert.IsNotNull(RequestObj);

            //GET GetInstrumentStatus
            instrument_status instrStatus = this.GETRequest<instrument_status>(host + Configuration.instrumentsResource + "/25/InstrumentStatus");
            Assert.IsNotNull(instrStatus);

            //PUT POSTed item
            postObj.notes = "Put-Test";
            instrument_status putObj = this.PUTRequest<instrument_status>(host + Configuration.instrumentStatusResource + "/" + postObj.instrument_status_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<instrument_status>(host + Configuration.instrumentStatusResource + "/" + postObj.instrument_status_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void LandOwnerContactRequest()
        {
            //GET LIST
            List<landownercontact> RequestList = this.GETRequest<List<landownercontact>>(host + Configuration.landOwnerResource, basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            landownercontact postObj;
            postObj = this.POSTRequest<landownercontact>(host + Configuration.landOwnerResource, new landownercontact() { fname = "postFname-Test", lname = "postLname-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.landownercontactid.ToString());

            //GET POSTed item
            landownercontact RequestObj = this.GETRequest<landownercontact>(host + Configuration.landOwnerResource + "/" + postObj.landownercontactid, basicAuth);
            Assert.IsNotNull(RequestObj);

            //GET GetSiteLandOwner
            landownercontact siteLO = this.GETRequest<landownercontact>(host + Configuration.siteResource + "/3425/LandOwner", basicAuth);
            Assert.IsNotNull(siteLO);

            //PUT POSTed item
            postObj.lname = "putLname-test";
            landownercontact putObj = this.PUTRequest<landownercontact>(host + Configuration.landOwnerResource + "/" + postObj.landownercontactid, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<landownercontact>(host + Configuration.landOwnerResource + "/" + postObj.landownercontactid, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void MarkerRequest()
        {
            //GET LIST
            List<marker> RequestList = this.GETRequest<List<marker>>(host + Configuration.markerResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            marker postObj;
            postObj = this.POSTRequest<marker>(host + Configuration.markerResource, new marker() { marker1 = "Post-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.marker_id.ToString());

            //GET POSTed item
            marker RequestObj = this.GETRequest<marker>(host + Configuration.markerResource + "/" + postObj.marker_id);
            Assert.IsNotNull(RequestObj);

            //GET hwm marker
            marker hwmMarker = this.GETRequest<marker>(host + Configuration.hwmResource + "/3980/Marker");
            Assert.IsNotNull(hwmMarker);

            //PUT POSTed item
            postObj.marker1 = "put-test";
            marker putObj = this.PUTRequest<marker>(host + Configuration.markerResource + "/" + postObj.marker_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<marker>(host + Configuration.markerResource + "/" + postObj.marker_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void MemberRequest()
        {
            //GET LIST
            List<member> RequestList = this.GETRequest<List<member>>(host + Configuration.memberResource, basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetAgencyMembers
            List<member> agMemberList = this.GETRequest<List<member>>(host + Configuration.agencyResource + "/10/" + Configuration.memberResource, basicAuth);
            Assert.IsNotNull(agMemberList, agMemberList.Count.ToString());

            //GET GetRoleMembers
            List<member> roleMemberList = this.GETRequest<List<member>>(host + Configuration.roleResource + "/1/" + Configuration.memberResource, basicAuth);
            Assert.IsNotNull(roleMemberList, roleMemberList.Count.ToString());

            //GET GetEventMembers
            List<member> eventMemberList = this.GETRequest<List<member>>(host + Configuration.eventsResource + "/8/" + Configuration.memberResource, basicAuth);
            Assert.IsNotNull(eventMemberList, eventMemberList.Count.ToString());

            //POST
            member postObj;
            postObj = this.POSTRequest<member>(host + Configuration.memberResource, new member() {
                username = "newUser", fname = "New", lname="user", phone="123-456-7890", email="test@usgs.gov", agency_id = 1,
                password = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("STNDef@u1t"))}, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.member_id.ToString());

            //GET POSTed item
            member RequestObj = this.GETRequest<member>(host + Configuration.memberResource + "/" + postObj.member_id,basicAuth);
            Assert.IsNotNull(RequestObj);

            //GET login
            member loginM = this.GETRequest<member>(host + "/login", basicAuth);
            Assert.IsNotNull(loginM);

            //GET  GetEventCoordinator 
            member eventCoord = this.GETRequest<member>(host + Configuration.eventsResource + "/8/EventCoordinator", basicAuth);
            Assert.IsNotNull(eventCoord);

            //GET GetApprovingOfficial
            member approveOfficial = this.GETRequest<member>(host + Configuration.approvalResource + "/73/ApprovingOfficial", basicAuth);
            Assert.IsNotNull(approveOfficial);

            //GET GetDataFileProcessor
            member dfProcessor = this.GETRequest<member>(host + Configuration.datafileResource + "/97/Processor", basicAuth);
            Assert.IsNotNull(dfProcessor);

            //GET GetPeakSummaryProcessor
            member psProcessor = this.GETRequest<member>(host + Configuration.peakSummaryResource + "/3/Processor", basicAuth);
            Assert.IsNotNull(psProcessor);

            //PUT POSTed item
            postObj.fname = "put-test";
            member putObj = this.PUTRequest<member>(host + Configuration.memberResource + "/" + postObj.member_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<member>(host + Configuration.memberResource + "/" + postObj.member_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void NetworkNameRequest()
        {
            //GET LIST
            List<network_name> RequestList = this.GETRequest<List<network_name>>(host + Configuration.networkNameResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetSiteNetworkNames
            List<network_name> siteNetNameList = this.GETRequest<List<network_name>>(host + Configuration.siteResource + "/7163/NetworkNames");
            Assert.IsNotNull(siteNetNameList, siteNetNameList.Count.ToString());

            //POST
            network_name postObj;
            postObj = this.POSTRequest<network_name>(host + Configuration.networkNameResource, new network_name() { name = "Post-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.network_name_id.ToString());

            //POST addSiteNetworkName
          //  List<network_name> postsNN;
          //  postsNN = this.POSTRequest<List<network_name>>(host + Configuration.siteResource + "7226/addNetworkName?NetworkNameId=2", null, basicAuth);
          //  Assert.IsNotNull(postsNN, "ID: " + postsNN.Count.ToString());

            //GET POSTed item
            network_name RequestObj = this.GETRequest<network_name>(host + Configuration.networkNameResource + "/" + postObj.network_name_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.name = "put-test";
            network_name putObj = this.PUTRequest<network_name>(host + Configuration.networkNameResource + "/" + postObj.network_name_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<network_name>(host + Configuration.networkNameResource + "/" + postObj.network_name_id, basicAuth);
            Assert.IsTrue(success);

            //Delete POSTed item "/Sites/{siteId}/removeNetworkName?NetworkNameId={networkNameId}"
            bool nnssuccess = this.DELETERequest<network_name>(host + Configuration.siteResource + "/7226/removeNetworkName?NetworkNameId=2", basicAuth);
            Assert.IsTrue(nnssuccess);
            
        }//end method
        [TestMethod]
        public void NetworkTypeRequest()
        {
            //GET LIST
            List<network_type> RequestList = this.GETRequest<List<network_type>>(host + Configuration.networkTypeResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetSiteNetworkNames
            List<network_type> siteNetTypeList = this.GETRequest<List<network_type>>(host + Configuration.siteResource + "/3867/NetworkTypes");
            Assert.IsNotNull(siteNetTypeList, siteNetTypeList.Count.ToString());

            //POST
            network_type postObj;
            postObj = this.POSTRequest<network_type>(host + Configuration.networkTypeResource, new network_type() { network_type_name = "Post-Test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.network_type_id.ToString());

            //POST addSiteNetworkType
            //  List<network_type> postsNT;
            //  postsNT = this.POSTRequest<List<network_type>>(host + Configuration.siteResource + "4266/addNetworkType?NetworkTypeId=2", null, basicAuth);
            //  Assert.IsNotNull(postsNT, "ID: " + postsNT.Count.ToString());

            //GET POSTed item
            network_type RequestObj = this.GETRequest<network_type>(host + Configuration.networkTypeResource + "/" + postObj.network_type_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.network_type_name = "put-test";
            network_type putObj = this.PUTRequest<network_type>(host + Configuration.networkTypeResource + "/" + postObj.network_type_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //Delete POSTed item
            bool success = this.DELETERequest<network_type>(host + Configuration.networkTypeResource + "/" + postObj.network_type_id, basicAuth);
            Assert.IsTrue(success);

            //Delete POSTed item "/Sites/{siteId}/removeNetworkType?NetworkTypeId={networkTypeId}"
            bool ntssuccess = this.DELETERequest<network_type>(host + Configuration.siteResource + "/4266/removeNetworkType?NetworkTypeId=2", basicAuth);
            Assert.IsTrue(ntssuccess);
        }//end method
        [TestMethod]
        public void ObjectivePointRequest()
        {
            //GET LIST
            List<objective_point> RequestList = this.GETRequest<List<objective_point>>(host + Configuration.objectivePointResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetVDatumOPs
            List<objective_point> vdOPList = this.GETRequest<List<objective_point>>(host + Configuration.verticalDatumResource + "/1/" + Configuration.objectivePointResource);
            Assert.IsNotNull(vdOPList, vdOPList.Count.ToString());

            //GET GetSiteObjectivePoints
            List<objective_point> sOPList = this.GETRequest<List<objective_point>>(host + Configuration.siteResource + "/3089/" + Configuration.objectivePointResource);
            Assert.IsNotNull(sOPList, sOPList.Count.ToString());

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

            //GET GetObjectivePointOPType
            objective_point_type opOPType = this.GETRequest<objective_point_type>(host + Configuration.objectivePointResource + "/26/OPType");
            Assert.IsNotNull(opOPType);

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

            //GET OPControls
            List<op_control_identifier> OPopControls = this.GETRequest<List<op_control_identifier>>(host + Configuration.objectivePointResource + "/2036/OPControls");
            Assert.IsNotNull(OPopControls, OPopControls.Count.ToString());

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

            //GET GetInstrumentStatOPMeasurements
            List<op_measurements> instStatOPMeas = this.GETRequest<List<op_measurements>>(host + Configuration.instrumentStatusResource + "/4945/" + Configuration.opMeasurementsResource);
            Assert.IsNotNull(instStatOPMeas, instStatOPMeas.Count.ToString());

            //GET GetOPOPMeasurements
            List<op_measurements> opOPMeas = this.GETRequest<List<op_measurements>>(host + Configuration.objectivePointResource + "/1128/" + Configuration.opMeasurementsResource);
            Assert.IsNotNull(opOPMeas, opOPMeas.Count.ToString());

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

            //GET GetObjectivePointQuality
            op_quality opQual = this.GETRequest<op_quality>(host + Configuration.objectivePointResource + "/2503/Quality");
            Assert.IsNotNull(opQual);

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

            //GET GetEventPeakSummaries "/Events/{eventId}/PeakSummaries"
            List<peak_summary> evPeaks = this.GETRequest<List<peak_summary>>(host + Configuration.eventsResource + "/18/" + Configuration.peakSummaryResource);
            Assert.IsNotNull(evPeaks, evPeaks.Count.ToString());

            //GET GetSitePeakSummaries "/Sites/{siteId}/PeakSummaries"
            List<peak_summary> sitePeaks = this.GETRequest<List<peak_summary>>(host + Configuration.siteResource + "/6580/" + Configuration.peakSummaryResource);
            Assert.IsNotNull(sitePeaks, sitePeaks.Count.ToString());

            //GET GetPeakSummaryViewBySite "/Sites/{siteId}/PeakSummaryView"
            List<peak_view> sitePeakView = this.GETRequest<List<peak_view>>(host + Configuration.siteResource + "/6547/PeakSummaryView.json");
            Assert.IsNotNull(sitePeakView, sitePeakView.Count.ToString());

            //GET GetFilteredPeaks "/PeakSummaries/FilteredPeaks?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}&States={states}&County={counties}&StartDate={startDate}&EndDate={endDate}"
            List<peak_summary> filtPeaks = this.GETRequest<List<peak_summary>>(host + Configuration.peakSummaryResource + "/FilteredPeaks.json?Event=18&States=LA&StartDate=08/01/2012");
            Assert.IsNotNull(filtPeaks, filtPeaks.Count.ToString());

            //POST
            peak_summary postObj;
            postObj = this.POSTRequest<peak_summary>(host + Configuration.peakSummaryResource, new peak_summary() { member_id = 1, peak_date = DateTime.Now, time_zone = "UTC" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.peak_summary_id.ToString());

            //GET POSTed item 
            peak_summary RequestObj = this.GETRequest<peak_summary>(host + Configuration.peakSummaryResource + "/" + postObj.peak_summary_id);
            Assert.IsNotNull(RequestObj);

            //GET GetHWMPeakSummary "/HWMs/{hwmId}/PeakSummary"
            peak_summary hwmPeak = this.GETRequest<peak_summary>(host + Configuration.hwmResource + "/5815/PeakSummary", basicAuth);
            Assert.IsNotNull(hwmPeak);

            //GET GetDataFilePeakSummary "/DataFiles/{dataFileId}/PeakSummary"
            peak_summary dfPeak = this.GETRequest<peak_summary>(host + Configuration.datafileResource + "/349/PeakSummary", basicAuth);
            Assert.IsNotNull(dfPeak);

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

            //GET GetMemberReports
            List<reporting_metrics> membReports = this.GETRequest<List<reporting_metrics>>(host + Configuration.memberResource + "/36/Reports");
            Assert.IsNotNull(membReports, membReports.Count.ToString());

            //GET GetEventReports
            List<reporting_metrics> eventReports = this.GETRequest<List<reporting_metrics>>(host + Configuration.eventsResource + "/30/Reports");
            Assert.IsNotNull(eventReports, eventReports.Count.ToString());

            //GET GetReportsByDate
            List<reporting_metrics> dateReports = this.GETRequest<List<reporting_metrics>>(host + Configuration.reportMetricResource + "/ReportsByDate?Date=04/01/2014");
            Assert.IsNotNull(dateReports, dateReports.Count.ToString());

            //GET GetReportsByEventAndState "/ReportingMetrics?Event={eventId}&State={stateName}
            List<reporting_metrics> evStReps = this.GETRequest<List<reporting_metrics>>(host + Configuration.reportMetricResource + "?Event=30&State=SD");
            Assert.IsNotNull(evStReps, evStReps.Count.ToString());

            //GET GetFilteredReports "/ReportingMetrics/FilteredReports?Event={eventId}&States={stateNames}&Date={aDate}"
            List<reporting_metrics> filteredReps = this.GETRequest<List<reporting_metrics>>(host + Configuration.reportMetricResource + "/FilteredReports?Event=7&States=AL,AK&Date=05/20/2016");
            Assert.IsNotNull(filteredReps, filteredReps.Count.ToString());

            //GET GetFilteredReportsModel "/ReportingResource/FilteredReportModel?Event={eventId}&States={stateNames}&Date={aDate}"
            List<ReportResource> filtRepModel = this.GETRequest<List<ReportResource>>(host + "ReportResource/FilteredReportModel.json?Event=30&States=SD&Date=05/18/2015");
            Assert.IsNotNull(filtRepModel, filtRepModel.Count.ToString());

            //POST
            reporting_metrics postObj;
            postObj = this.POSTRequest<reporting_metrics>(host + Configuration.reportMetricResource, new reporting_metrics() { report_date = DateTime.Now, event_id = 1, state = "WI", member_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.reporting_metrics_id.ToString());

            //GET POSTed item
            reporting_metrics RequestObj = this.GETRequest<reporting_metrics>(host + Configuration.reportMetricResource + "/" + postObj.reporting_metrics_id);
            Assert.IsNotNull(RequestObj);

            //GET GetReportModel
            ReportResource repRes = this.GETRequest<ReportResource>(host + "/ReportResource/422.json");
            Assert.IsNotNull(repRes);

            //GET GetDailyReportTotals "/ReportingMetrics/DailyReportTotals?Date={date}&Event={eventId}&State="StateName"
            ReportResource dailyTotsRep = this.GETRequest<ReportResource>(host + Configuration.reportMetricResource + "/DailyReportTotals.json?Date=05/12/2015&Event=31&State=KS");
            Assert.IsNotNull(dailyTotsRep);

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

            //GET GetMemberRole
            role memRole = this.GETRequest<role>(host + Configuration.memberResource + "/36/Role");
            Assert.IsNotNull(memRole);

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

            //GET GetInstrumentSensorBrand
            sensor_brand instrSensBrand = this.GETRequest<sensor_brand>(host + Configuration.instrumentsResource + "/985/SensorBrand");
            Assert.IsNotNull(instrSensBrand);

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

            //GET GetInstrumentSensorType
            sensor_type instrSensorType = this.GETRequest<sensor_type>(host + Configuration.instrumentsResource + "/1915/SensorType");
            Assert.IsNotNull(instrSensorType);

            //GET GetDeploymentSensorType
            sensor_type depSensorType = this.GETRequest<sensor_type>(host + Configuration.deploymenttypeResource + "/1/SensorType");
            Assert.IsNotNull(depSensorType);

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
            List<site> RequestList = this.GETRequest<List<site>>(host + Configuration.siteResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetEventSites
            List<site> eventSites = this.GETRequest<List<site>>(host + Configuration.eventsResource + "/18/" + Configuration.siteResource);
            Assert.IsNotNull(eventSites, eventSites.Count.ToString());

            //GET getNetworkTypeSites
            List<site> ntSites = this.GETRequest<List<site>>(host + Configuration.networkTypeResource + "/1/" + Configuration.siteResource);
            Assert.IsNotNull(ntSites, ntSites.Count.ToString());

            //GET getNetworkNamesSites
            List<site> nnSites = this.GETRequest<List<site>>(host + Configuration.networkNameResource + "/1/" + Configuration.siteResource);
            Assert.IsNotNull(nnSites, ntSites.Count.ToString());

            //GET GetSitesByStateName
            List<site> stateSites = this.GETRequest<List<site>>(host + Configuration.stateResource + "/FL/" + Configuration.siteResource);
            Assert.IsNotNull(stateSites, stateSites.Count.ToString());

            //GET GetSitesByLatLong
            List<site> latLongSites = this.GETRequest<List<site>>(host + Configuration.siteResource + "?Latitude=39.950278&Longitude=-74.198889&Buffer=.0005");
            Assert.IsNotNull(latLongSites, latLongSites.Count.ToString());

            //GET GetHDatumSites
            List<site> hDSites = this.GETRequest<List<site>>(host + Configuration.horizontaldatumResource + "/1/" + Configuration.siteResource);
            Assert.IsNotNull(hDSites, hDSites.Count.ToString());

            //GET GetLandOwnserSites
            List<site> loSites = this.GETRequest<List<site>>(host + Configuration.landOwnerResource + "/18/" + Configuration.siteResource);
            Assert.IsNotNull(loSites, loSites.Count.ToString());

            //GET FilteredSites "/Sites/FilteredSites?Event={eventId}&State={stateNames}&SensorType={sensorTypeId}&NetworkName={networkNameId}&OPDefined={opDefined}&HWMOnly={hwmOnlySites}&SensorOnly={sensorOnlySites}&RDGOnly={rdgOnlySites}"
            List<SiteLocationQuery> filtSites = this.GETRequest<List<SiteLocationQuery>>(host + Configuration.siteResource + "/FilteredSites.json?Event=23&State=NJ&NetworkName=2");
            Assert.IsNotNull(filtSites, filtSites.Count.ToString());

            //POST
            site postObj;
            postObj = this.POSTRequest<site>(host + Configuration.siteResource, new site() { 
                site_description = "site_post1", latitude_dd = 45.52, longitude_dd = -89.32, hdatum_id = 1,
                hcollect_method_id = 1, state = "WI", county = "Dane County", waterbody = "test2", member_id = 1}, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.site_id.ToString());

            //GET POSTed item
            site RequestObj = this.GETRequest<site>(host + Configuration.siteResource + "/" + postObj.site_id);
            Assert.IsNotNull(RequestObj);

            //GET GetSiteBySearch
            site siteBySearch = this.GETRequest<site>(host + Configuration.siteResource + "/Search?bySiteNo=PA 00177");
            Assert.IsNotNull(siteBySearch);

            //GET GetFileSite
            site fileSite = this.GETRequest<site>(host + Configuration.fileResource + "/23750/Site");
            Assert.IsNotNull(fileSite);

            //GET GetOPSite
            site opSite = this.GETRequest<site>(host + Configuration.objectivePointResource + "/26/Site");
            Assert.IsNotNull(opSite);

            //GET getHWMSite
            site hwmSite = this.GETRequest<site>(host + Configuration.hwmResource + "/3980/Site");
            Assert.IsNotNull(hwmSite);

            //GET GetInstrumentSite
            site instSite = this.GETRequest<site>(host + Configuration.instrumentsResource + "/25/Site");
            Assert.IsNotNull(instSite);

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

            //GET GetSiteSiteHousing
            List<site_housing> sSiteHouses = this.GETRequest<List<site_housing>>(host + Configuration.siteResource + "/4267/" + Configuration.siteHousingResource);
            Assert.IsNotNull(sSiteHouses, sSiteHouses.Count.ToString());

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

            //GET GetAgencySources
            List<source> agencySources = this.GETRequest<List<source>>(host + Configuration.agencyResource + "/5/" + Configuration.sourceResource, basicAuth);
            Assert.IsNotNull(agencySources, agencySources.Count.ToString());

            //POST
            source postObj;
            postObj = this.POSTRequest<source>(host + Configuration.sourceResource, new source() { source_name = "post-test", agency_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.source_id.ToString());

            //GET POSTed item
            source RequestObj = this.GETRequest<source>(host + Configuration.sourceResource + "/" + postObj.source_id, basicAuth);
            Assert.IsNotNull(RequestObj);

            //GET GetFileSource
            source fileSource = this.GETRequest<source>(host + Configuration.fileResource + "/111/Source", basicAuth);
            Assert.IsNotNull(fileSource);

            //PUT POSTed item
            postObj.source_name = "put-test";
            postObj.agency_id = 2;
            source putObj = this.PUTRequest<source>(host + Configuration.sourceResource + "/" + postObj.source_id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            // sources are not deletable.
        }//end method
        [TestMethod]
        public void StateRequest()
        {
            //GET LIST
            List<state> RequestList = this.GETRequest<List<state>>(host + Configuration.stateResource);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetSiteStates
            List<state> siteStates = this.GETRequest<List<state>>(host + Configuration.siteResource + "/" + Configuration.stateResource);
            Assert.IsNotNull(siteStates, siteStates.Count.ToString());

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

            //GET GetInstrumentStatusStatus
            status_type instrStatStatus = this.GETRequest<status_type>(host + Configuration.instrumentStatusResource + "/106/Status");
            Assert.IsNotNull(instrStatStatus);

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

            //GET GetHWMVerticalMethod
            vertical_collect_methods hwmVMethod = this.GETRequest<vertical_collect_methods>(host + Configuration.hwmResource + "/3980/VerticalMethod");
            Assert.IsNotNull(hwmVMethod);

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
            postObj = this.POSTRequest<vertical_datums>(host + Configuration.verticalDatumResource, new vertical_datums() { datum_name = "post-test",datum_abbreviation ="tst" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.datum_id.ToString());

            //GET POSTed item
            vertical_datums RequestObj = this.GETRequest<vertical_datums>(host + Configuration.verticalDatumResource + "/" + postObj.datum_id);
            Assert.IsNotNull(RequestObj);

            //GET GetHWMVDatum
            vertical_datums hwmVDatum = this.GETRequest<vertical_datums>(host + Configuration.hwmResource + "/4024/vDatum");
            Assert.IsNotNull(hwmVDatum);

            //GET getOPVDatum
            vertical_datums opVMethod = this.GETRequest<vertical_datums>(host + Configuration.objectivePointResource + "/26/vDatum");
            Assert.IsNotNull(opVMethod);

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
