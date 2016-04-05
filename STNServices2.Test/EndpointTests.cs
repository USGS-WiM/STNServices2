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
        #endregion
    }
}
