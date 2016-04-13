﻿//------------------------------------------------------------------------------
//----- Configuration ----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Registration of resources using ResourceSpace.Has, specify specific 
//              representations with corresponding handlers and codecs.
//
//discussion:   https://github.com/openrasta/openrasta/wiki/Configuration
//
//     

#region Comments
// 10.28.13 - TR - Added endpoints for NETWORK_TYPE
// 10.25.13 - TR - Added endpoints for PRIORITY
// 04.14.13 - TR  - Added endpoints for Instr_Collection_Conditions
// 02.14.13 - JKN - added endpoint for fileItem, GetFilteredInstruments
// 02.13.13 - JKN - added endpoint for members of a collect team and house cleaning
// 02.12.13 - JKN - added endpoint to add member to team (/CollectionTeams/{teamId}/AddMember)
// 02.07.13 - JKN - Added query to get files,HWMs,and Instruments by eventId and siteID
// 02.07.13 - JKN - Added query to get entities/Events by siteId
// 02.04.13 - JKN - Added Datafile and HWM from peaksummary ID uris
// 01.31.13 - JKN - Added additional uri's to sites, data_file, Peaksummaries, and Files
// 11.07.12 - JKN - Added Events/{eventId}/HWMs endpoint to HWM_resources
// 08.01.12 - JB - Miscorrected spelling
// 08.01.12 - JB - Fixed Reference Point config
// 06.20.12 - JB - Updated media types for Jeremy's new simple XML codec
// 06.19.12 - JKN - Edited addHWM_Resources method, Removed HWMList --> Replaces with List<HWM>
// 06.18.12 - JKN - Edited addResource Methods to use SimpleUTF8XmlSerializerCodec 
// 06.14.12 - JB - Added URI extension to handle multiple Media Types on SITEs
// 06.08.12 - JB - Renamed XML Codec and fixed Instrument Serial Number URI
// 05.31.12 - JB - Added generic POST and PUT methods to allow updates and additions to lookup tables
// 03.13.12 - JB - Added file and file upload endpoints
// 02.17.12 - JB - Added authentication login endpoint
// 02.10.12 - JB - Replaced Events, Datums, and Landowner endpoints with generic entity endpoint
// 01.30.12 - JB - Added Events, Datums, and Landowner endpoints
// 01.25.12 - JB - Added RSS Feed
// 01.23.12 - JB - Added XML codec that removes BOM
// 01.19.12 - JB - Added endpoints for Sites and HWMs
// 12.28.11 - JB - Created
#endregion

using STNServices2.Authentication;
using STNServices2.Codecs;
using STNServices2.Resources;
using STNServices2.Handlers;
using STNServices2.PipeLineContributors;

using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.IO;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web.UriDecorators;

using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.ServiceModel.Syndication;

namespace STNServices2
{
    public class Configuration : IConfigurationSource
    {

        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                // specify the authentication scheme you want to use, you can register multiple ones
                ResourceSpace.Uses.CustomDependency<IAuthenticationScheme, BasicAuthenticationScheme>(DependencyLifetime.Singleton);

                // register your basic authenticator in the DI resolver
                ResourceSpace.Uses.CustomDependency<IBasicAuthenticator, STNBasicAuthentication>(DependencyLifetime.Transient);

                // Allow codec choice by extension 
                ResourceSpace.Uses.UriDecorator<ContentTypeExtensionUriDecorator>();
                ResourceSpace.Uses.PipelineContributor<ErrorCheckingContributor>();
                ResourceSpace.Uses.PipelineContributor<CrossDomainPipelineContributor>();

                AddAGENCY_Resources();
                AddAPPROVAL_Resources();
                AddAUTHENTICATION_Resources();
                AddCONTACT_Resources();
                AddCONTACT_TYPE_Resources();
                AddCOUNTIES_Resources();
                AddDATA_FILE_Resources();
                AddDEPLOYMENT_PRIORITY_Resources();
                AddDEPLOYMENT_TYPE_Resources();
                AddEVENT_Resources();
                AddEVENT_STATUS_Resources();
                AddEVENT_TYPE_Resources();
                AddFEED_Resources();
                AddFILE_Resources();
                AddFILE_TYPE_Resources();
                AddHORIZONTAL_COLLECT_METHODS_Resources();
                AddHORIZONTAL_DATUM_Resources();
                AddHOUSING_TYPE_Resources();
                AddHWM_QUALITY_Resources();
                AddHWM_Resources();
                AddHWM_TYPE_Resources();
                AddINSTR_COLLECTION_CONDITIONS_Resources();
                AddINSTRUMENT_Resources();
                AddINSTRUMENT_STATUS_Resources();
                AddKEYWORD_Resources();
                AddLANDOWNER_CONTACT_Resources();
                AddLocatorType_Resources();
                AddMARKER_Resources();
                AddMEMBER_Resources();
                AddNETWORK_NAME_Resources();
                AddNETWORK_TYPE_Resources();
                AddPEAK_SUMMARY_Resources();
                AddOBJECTIVE_POINT_Resources();
                AddOBJECTIVE_POINT_TYPE_Resources();
                AddOP_CONTROL_IDENTIFIER_Resources();
                AddOP_QUALTITY_Resources();
                AddREPORTING_METRICS_Resources();
                AddOP_MEASUREMENTS_Resources();
                AddROLE_Resources();
                AddSENSOR_BRAND_Resourses();
                AddSENSOR_DEPLOYMENT_Resources();
                AddSENSOR_TYPE_Resources();
                AddSITE_Resources();
                AddSITE_HOUSING_Resources();
                AddSOURCE_Resources();
                AddSTATE_Resources();
                AddSTATUS_TYPE_Resources();
                AddVERTICAL_COLLECTION_METHOD_Resources();
                AddVERTICAL_DATUM_Resources();

            } //End using OpenRastaConfiguration.Manual
        }

        #region Helper methods

        private void AddAGENCY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<AGENCY>>()
            .AtUri("/Agencies")
            .HandledBy<AgencyHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")          //Display XML
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<AGENCY>()
            .AtUri("/Agencies/{entityId}")
            .And.AtUri("/Members/{memberId}/Agency").Named("GetMemberAgency")
            .And.AtUri("/Sources/{sourceId}/Agency").Named("GetSourceAgency")
            .HandledBy<AgencyHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAGENCY_Resources

        private void AddAPPROVAL_Resources()
        {
            //ApprovalTable
            ResourceSpace.Has.ResourcesOfType<List<APPROVAL>>()
            .AtUri("/Approvals")
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<APPROVAL>()
            .AtUri("/Approvals/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/Approve").Named("ApproveHWM")
            .And.AtUri("/DataFiles/{dataFileId}/Approve").Named("ApproveDataFile")
            .And.AtUri("/HWMs/{hwmId}/Unapprove").Named("UnApproveHWM")
            .And.AtUri("/DataFiles/{dataFileId}/Unapprove").Named("UnApproveDataFile")
            .And.AtUri("/DataFiles/{dataFileId}/Approval").Named("GetDataFileApproval")
            .And.AtUri("/HWMs/{hwmId}/Approval").Named("GetHWMApproval")
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAPPROVAL_Resources

        private void AddAUTHENTICATION_Resources()
        {
            //Authentication
            ResourceSpace.Has.ResourcesOfType<Boolean>()
            .AtUri("/login")
            .HandledBy<LoginHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>();

        }//end AddAUTHENTICATION_Resources

        private void AddCONTACT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<CONTACT>>()
            .AtUri("/Contacts")
            .And.AtUri("/Contacts?ReportMetric={reportMetricsId}").Named("GetReportMetricContacts")
                //.And.AtUri("/Contacts?ContactModel/{reportMetricsId}").Named("GetContactModelByReport")
            .HandledBy<ContactHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            //GET
            ResourceSpace.Has.ResourcesOfType<List<ReportContactModel>>()
            .AtUri("/Contacts?ContactModelByReport={reportMetricsId}").Named("GetContactModelByReport")
            .HandledBy<ContactHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<CONTACT>()
            .AtUri("/Contacts/{entityId}")
             .And.AtUri("/Contacts?ReportMetric={reportMetricsId}&ContactType={contactTypeId}").Named("GetReportMetricContactsByType")
             .And.AtUri("/Contacts/{reportMetricsId}/removeReportContact").Named("RemoveReportContact")
             .And.AtUri("/Contacts/AddReportContact?contactTypeId={ContactTypeId}&reportId={ReportId}").Named("AddReportContact")
            .HandledBy<ContactHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_Resources        

        private void AddCONTACT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<CONTACT_TYPE>>()
            .AtUri("/ContactTypes")
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<CONTACT_TYPE>()
            .AtUri("/ContactTypes/{entityId}")
            .And.AtUri("/Contacts/{contactId}/ContactType").Named("GetContactsContactType")
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_TYPE_Resources

        private void AddCOUNTIES_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<COUNTIES>>()
            .AtUri("/Counties")
            .And.AtUri("/States/{stateId}/Counties").Named("GetStateCountiesById")
            .And.AtUri("/States/Counties?StateAbbrev={stateAbbrev}").Named("GetStateCountiesByAbbrev")
            .And.AtUri("/Sites/CountiesByState?StateAbbrev={stateAbbrev}").Named("GetStateSiteCounties")
            .HandledBy<CountiesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<COUNTIES>()
            .AtUri("/Counties/{entityId}")
            .HandledBy<CountiesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCOUNTIES_Resources

        private void AddDATA_FILE_Resources()
        {
            //DATA_FILE Table
            ResourceSpace.Has.ResourcesOfType<List<DATA_FILE>>()
          .AtUri("/DataFiles")
          .And.AtUri("/Instruments/{instrumentId}/DataFiles").Named("GetInstrumentDataFiles")
          .And.AtUri("/PeakSummaries/{peakSummaryId}/DataFiles").Named("GetPeakSummaryDatafiles")
          .And.AtUri("/DataFiles?IsApproved={boolean}")
          .And.AtUri("Approvals/{ApprovalId}/DataFiles").Named("GetApprovedDataFiles")
          .And.AtUri("/DataFiles?IsApproved={approved}&Event={eventId}&Processor={memberId}&State={state}").Named("GetFilteredDataFiles")
          .HandledBy<DataFileHandler>()
          .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
          .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<DATA_FILE>()
                //.AtUri("/DataFiles").Named("CreateDataFile")
            .AtUri("/DataFiles/{entityId}")
            .And.AtUri("Files/{fileId}/DataFile").Named("GetFileDataFile")
            .HandledBy<DataFileHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDATA_FILE_Resources

        private void AddDEPLOYMENT_PRIORITY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<DEPLOYMENT_PRIORITY>>()
            .AtUri("/DeploymentPriorities")
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<DEPLOYMENT_PRIORITY>()
            .AtUri("/DeploymentPriorities/{entityId}")
            .And.AtUri("/Sites/{siteId}/DeploymentPriorities").Named("GetSitePriorities")
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_PRIORITY_Resources

        private void AddDEPLOYMENT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<DEPLOYMENT_TYPE>>()
            .AtUri("/DeploymentTypes")
            .And.AtUri("/SensorTypes/{sensorTypeId}/DeploymentTypes").Named("GetSensorDeploymentTypes")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<DEPLOYMENT_TYPE>()
            .AtUri("/DeploymentTypes/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/DeploymentType").Named("GetInstrumentDeploymentType")
            .And.AtUri("/SensorTypes/{sensorTypeId}/addDeploymentType").Named("AddSensorDeployment")
            .And.AtUri("/SensorTypes/{sensorTypeId}/removeDeploymentType").Named("RemoveSensorDeployment")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_TYPE_Resources

        private void AddEVENT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<EVENT>>()
            .AtUri("/Events")
            .And.AtUri("/Events?Site={siteId}").Named("GetEventsBySite")
            .And.AtUri("/EventTypes/{eventTypeId}/Events").Named("GetEventTypeEvents")
            .And.AtUri("/EventStatus/{eventStatusId}/Events").Named("GetEventStatusEvents")
            .And.AtUri("/Events/FilteredEvents?Date={date}&Type={eventTypeId}&State={stateName}").Named("GetFilteredEvents")
            .HandledBy<EventsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<EVENT>()
            .AtUri("/Events/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/Event").Named("GetHWMEvent")
            .And.AtUri("/Instruments/{instrumentId}/Event").Named("GetInstrumentEvent")
            .HandledBy<EventsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_Resources

        private void AddEVENT_STATUS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<EVENT_STATUS>>()
            .AtUri("/EventStatus")
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<EVENT_STATUS>()
            .AtUri("/EventStatus/{entityId}")
            .And.AtUri("/Events/{eventId}/Status").Named("GetEventStatus")
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEvent_Status_Resources

        private void AddEVENT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<EVENT_TYPE>>()
            .AtUri("/EventTypes")
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<EVENT_TYPE>()
            .AtUri("/EventTypes/{entityId}")
            .And.AtUri("/Events/{eventId}/Type").Named("GetEventType")
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_TYPE_Resources

        private void AddFEED_Resources()
        {
            //Syndication feeds
            ResourceSpace.Has.ResourcesOfType<SyndicationFeed>()
            .AtUri("/feeds/HWMs")
            .Named("GetProvisionalHWMs")
            .HandledBy<HWMHandler>();
            

        }//end AddFEED_Resources

        private void AddFILE_Resources()
        {
            //******NOTE******
            //The key/value codec prevents using a URL like /HWMs/{hwmId}/FILE
            //For now top level URLs are used for new resources
            //(http://stackoverflow.com/questions/7694839/how-to-handle-multipart-post-using-a-uri-template-of-form-prefix-suffix-in-op)
            //http://stackoverflow.com/questions/1952278/get-image-as-stream-or-byte-array-using-openrasta
            //****************
            ResourceSpace.Has.ResourcesOfType<FILES>()
           .AtUri("/Files/{entityId}")           
           .And.AtUri("/Files/bytes").Named("UploadFile")
           .HandledBy<FileHandler>()
           .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<MultipartFormDataObjectCodec>()
           .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<FILES>>()
           .AtUri("/Files")
           .And.AtUri("/HWMs/{hwmId}/Files").Named("GetHWMFiles")
           .And.AtUri("/ObjectivePoints/{objectivePointId}/Files").Named("GetObjectivePointFiles")
           .And.AtUri("/FileTypes/{fileTypeId}/Files").Named("GetFileTypeFiles")
           .And.AtUri("/Sites/{siteId}/Files").Named("GetSiteFile")
           .And.AtUri("/Sources/{sourceId}/Files").Named("GetSourceFiles")
           .And.AtUri("/DataFiles/{dataFileId}/Files").Named("GetDataFileFiles")
           .And.AtUri("/Instruments/{instrumentId}/Files").Named("GetInstrumentFiles")
           .And.AtUri("/Events/{eventId}/Files").Named("GetEventFiles")
           .And.AtUri("/Files?FromDate={fromDate}&ToDate={toDate}")
           .And.AtUri("/Files?State={stateName}").Named("GetFilesByStateName")
           .And.AtUri("/Files?Site={siteId}&Event={eventId}")
           .HandledBy<FileHandler>()
           .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<IFile>()
            .AtUri("/FILES/{fileId}/Item").Named("GetFileItem")
            .And.AtUri("/Events/{eventId}/EventFileItems").Named("GetEventFileItems")
            .And.AtUri("CitizenFiles/{fileId}/Item").Named("GetFileItem")
            .HandledBy<FileHandler>();
            //.RenderedByAspx(new { index = "~/Views/Files/FileItem.aspx" }).ForMediaType("text/html;q=0.2").ForExtension("phtml");  //Display Pretty HTML

            ResourceSpace.Has.ResourcesOfType<List<CITIZEN_FILE>>()
            .AtUri("CitizenFiles").Named("GetCitizenFiles")
            .HandledBy<FileHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>()
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<CITIZEN_FILE>()
            .AtUri("CitizenFiles/{fileId}").Named("GetCitizenFile")
            .And.AtUri("CitizenFiles/Upload").Named("UploadCitizenFile")
            .And.AtUri("/CitizenFile/{fileId}/validate?UserId={userId}").Named("ValidateCitizenFile")
            .And.AtUri("/CitizenFiles/{fileId}").Named("DeleteCitizenFile")
            .HandledBy<FileHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>()
            .And.TranscodedBy<MultipartFormDataObjectCodec>()
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddFILE_Resources

        private void AddFILE_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<FILE_TYPE>>()
            .AtUri("/FileTypes")
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<FILE_TYPE>()
            .AtUri("/FileTypes/{entityId}")
            .And.AtUri("/Files/{fileId}/Type").Named("GetFileType")
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddFILE_TYPE_Resources

        private void AddHORIZONTAL_COLLECT_METHODS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<HORIZONTAL_COLLECT_METHODS>>()
            .AtUri("/HorizontalMethods")
            .HandledBy<HorizontalCollectMethodsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<HORIZONTAL_COLLECT_METHODS>()
            .AtUri("/HorizontalMethods/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/HorizontalMethod").Named("GetHWMHorizontalMethods")
            .HandledBy<HorizontalCollectMethodsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_COLLECT_METHODS_Resources

        private void AddHORIZONTAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<HORIZONTAL_DATUMS>>()
            .AtUri("/HorizontalDatums")
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<HORIZONTAL_DATUMS>()
            .AtUri("/HorizontalDatums/{entityId}")
            .And.AtUri("/Sites/{siteId}/hDatum").Named("GetSiteHdatum")
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_DATUM_Resources

        private void AddHOUSING_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<HOUSING_TYPE>>()
            .AtUri("/HousingTypes")
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<HOUSING_TYPE>()
            .AtUri("/HousingTypes/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/InstrumentHousingType").Named("InstrumentHousingType")
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHOUSING_TYPE_Resources

        private void AddHWM_QUALITY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<HWM_QUALITIES>>()
            .AtUri("/HWMQualities")
            .HandledBy<HWMQualitiesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<HWM_QUALITIES>()
            .AtUri("/HWMQualities/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/Quality").Named("GetHWMQuality")
            .HandledBy<HWMQualitiesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHWM_QUALITY_Resources

        private void AddHWM_Resources()
        {
            //High water marks
            ResourceSpace.Has.ResourcesOfType<HWMList>()
            .AtUri("/HWMs")
            .And.AtUri("/Events/{eventId}/HWMs").Named("GetEventSimpleHWMs")
            .And.AtUri("/Sites/{siteId}/EventHWMs?Event={eventId}").Named("GetSiteEventHWMs")
            .And.AtUri("/HWMS?IsApproved={approved}&Event={eventId}&Member={memberId}&State={state}").Named("GetApprovalHWMs")
            .HandledBy<HWMHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<HWM>>()
            .AtUri("/HWMs?IsApproved={boolean}").Named("GetHWMByApproval")
            .And.AtUri("/Approvals/{ApprovalId}/HWMs").Named("GetApprovedHWMs")
            .And.AtUri("/Members/{memberId}/HWMs").Named("GetMemberHWMs")
            .And.AtUri("/HWMQualities/{hwmQualityId}/HWMs").Named("GetHWMQualityHWMs")
            .And.AtUri("/HWMTypes/{hwmTypeId}/HWMs").Named("GetHWMTypeHWMs")
            .And.AtUri("/HorizontalMethods/{hmethodId}/HWMs").Named("GetHmethodHWMs")
            .And.AtUri("/VerticalMethods/{vmethodId}/HWMs").Named("GetVmethodHWMs")
            .And.AtUri("/Sites/{siteId}/HWMs").Named("GetSiteHWMs")
            .And.AtUri("/VerticalDatums/{vdatumId}/HWMs").Named("GetVDatumHWMs")
             .And.AtUri("/Markers/{markerId}/HWMs").Named("GetMarkerHWMs")
            .And.AtUri("/PeakSummaries/{peakSummaryId}/HWMs").Named("GetPeakSummaryHWMs")
            .HandledBy<HWMHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<HWMDownloadable>>()
            .AtUri(@"/HWMs/FilteredHWMs?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
                            &States={states}&County={counties}&HWMType={hwmTypeIDs}&HWMQuality={hwmQualIDs}
                            &HWMEnvironment={hwmEnvironment}&SurveyComplete={surveyComplete}&StillWater={stillWater}").Named("FilteredHWMs")
            .HandledBy<HWMHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
           
            ResourceSpace.Has.ResourcesOfType<HWM>()
            .AtUri("/HWMs/{entityId}").Named("GetHWM")
            .And.AtUri("/Files/{fileId}/HWM").Named("GetFileHWM")
                //.And.AtUri("/HWMs/").Named("CreateHWM")
            .HandledBy<HWMHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHWM_Resources

        private void AddHWM_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<HWM_TYPES>>()
            .AtUri("/HWMTypes")
            .HandledBy<HWMTypesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<HWM_TYPES>()
            .AtUri("/HWMTypes/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/Type").Named("GetHWMType")
            .HandledBy<HWMTypesHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHWM_TYPE_Resources

        private void AddINSTR_COLLECTION_CONDITIONS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<INSTR_COLLECTION_CONDITIONS>>()
            .AtUri("/InstrCollectConditions")
            .HandledBy<InstrCollectConditionsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<INSTR_COLLECTION_CONDITIONS>()
            .AtUri("/InstrCollectConditions/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/CollectCondition").Named("GetInstrumentCondition")
            .HandledBy<InstrCollectConditionsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end INSTR_COLLECTION_CONDITIONS_Resources

        private void AddINSTRUMENT_Resources()
        {
            //Instruments
            ResourceSpace.Has.ResourcesOfType<InstrumentSerialNumberList>()
               .AtUri("/Instruments/SerialNumbers").Named("SerialNumbers")  //
               .HandledBy<InstrumentHandler>() //handler. what class is gonna handle this
               .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<INSTRUMENT>>()
               .AtUri("/Instruments")
               .And.AtUri("/Instruments/GetAll").Named("GetAllInstruments")
               .And.AtUri("/Sites/{siteId}/Instruments").Named("GetSiteInstruments")
               .And.AtUri("/SensorBrands/{sensorBrandId}/Instruments").Named("SensorBrandInstruments")
               .And.AtUri("/SensorTypes/{sensorTypeId}/Instruments").Named("SensorTypeInstruments")
               .And.AtUri("/DeploymentTypes/{deploymentTypeId}/Instruments").Named("GetDeploymentTypeInstruments")
               .And.AtUri(@"/Instruments?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
                                &States={states}&County={counties}&CurrentStatus={statusIDs}&CollectionCondition={collectionConditionIDs}
                                &DeploymentType={deploymentTypeIDs}").Named("GetFilteredInstruments")
                //.And.AtUri("/Sites/Instruments?bySiteNo={siteNo}").Named("GetSiteInstrumentsByInternalId")
               .And.AtUri("/Events/{eventId}/Instruments").Named("GetEventInstruments")
                //NOT SEEING IT USED .And.AtUri("/Instruments/ReportInstruments?Date={aDate}&Event={eventId}&State={stateAbbrev}").Named("GetReportInstruments")  //Instruments?Date={aDate}&Event={eventId}&State={stateAbbrev}").Named("GetReportInstruments")
               .And.AtUri("/Sites/{siteId}/Instruments?Event={eventId}").Named("GetSiteEventInstruments")
               .HandledBy<InstrumentHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<INSTRUMENT>()
                .AtUri("/Instruments/{entityId}")
                .And.AtUri("/DataFiles/{dataFileId}/Instrument").Named("GetDataFileInstrument")
                .And.AtUri("/InstrumentStatus/{instrumentStatusId}/Instrument").Named("GetInstrumentStatusInstrument")
                .And.AtUri("/Files/{fileId}/Instrument").Named("GetFileInstrument")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<FullInstrument>()
                .AtUri("/Instruments/{instrumentId}/FullInstrument").Named("GetFullInstruments")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<FullInstrument>>()
                .AtUri("/Sites/{siteId}/FullInstrumentList").Named("GetFullInstrumentList")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SensorViews>()
                .AtUri("/SensorViews/{eventId}").Named("GetSensorViews")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddINSTRUMENT_Resources

        private void AddINSTRUMENT_STATUS_Resources()
        {
            //InstrumentStatus
            ResourceSpace.Has.ResourcesOfType<List<INSTRUMENT_STATUS>>()
                .AtUri("/InstrumentStatus")
                .And.AtUri("/Instruments/{instrumentId}/InstrumentStatusLog").Named("GetInstrumentStatusLog")
                //.And.AtUri("/CollectionTeams/{teamId}/InstrumentStatus").Named("GetCollectionTeamInstrumentStatuses")
                .HandledBy<InstrumentStatusHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<INSTRUMENT_STATUS>()
                .AtUri("/InstrumentStatus/{entityId}")
                .And.AtUri("/Instruments/{instrumentId}/InstrumentStatus").Named("GetInstrumentStatus")
                .HandledBy<InstrumentStatusHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddINSTRUMENT_STATUS_Resources

        private void AddKEYWORD_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<KEYWORD>>()
            .AtUri("/Keywords")
            .HandledBy<KeywordHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<KEYWORD>()
            .AtUri("/Keywords/{keywordId}")
            .HandledBy<KeywordHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddKEYWORD_Resources

        private void AddLANDOWNER_CONTACT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<LANDOWNERCONTACT>>()
            .AtUri("/LandOwners")
            .HandledBy<LandOwnerHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<LANDOWNERCONTACT>()
            .AtUri("/LandOwners/{entityId}")
            .And.AtUri("/Sites/{siteId}/LandOwner").Named("GetSiteLandOwner")
            .HandledBy<LandOwnerHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddLANDOWNER_CONTACT_Resources

        private void AddLocatorType_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<LOCATORTYPE>>()
            .AtUri("/LocatorTypes")
            .HandledBy<LocatorTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<LOCATORTYPE>()
            .AtUri("/LocatorTypes/{locatorTypeId}")
            .HandledBy<LocatorTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddLocatorType_Resources

        private void AddMARKER_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<MARKER>>()
            .AtUri("/Markers")
            .HandledBy<MarkerHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<MARKER>()
            .AtUri("/Markers/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/Marker").Named("GetHWMMarker")
            .HandledBy<MarkerHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddMARKER_Resources

        private void AddMEMBER_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<MEMBER>>()
            .AtUri("/Members")
            .And.AtUri("/Events/{eventId}/Members").Named("GetEventMembers")            
            .And.AtUri("/Agencies/{agencyId}/Members").Named("GetAgencyMembers")
            .And.AtUri("/Roles/{roleId}/Members").Named("GetRoleMembers")
            .HandledBy<MemberHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<MEMBER>()
            .AtUri("/Members/{entityId}")
                //.And.AtUri("/Members/{entityId}/Edit").Named("Put")
            .And.AtUri("Members/{pass}/addMember").Named("AddMember")
            .And.AtUri("Members?username={userName}&newPass={newPassword}").Named("ChangeMembersPassword")
            .And.AtUri("Events/{eventId}/EventCoordinator").Named("GetEventCoordinator")
            .And.AtUri("Approvals/{ApprovalId}/ApprovingOfficial").Named("GetApprovingOfficial")
            .And.AtUri("DataFiles/{dataFileId}/Processor").Named("GetDataFileProcessor")
            .And.AtUri("/Members?username={userName}").Named("GetByUserName")
            .And.AtUri("PeakSummaries/{peakSummaryId}/Processor").Named("GetPeakSummaryProcessor")
            .HandledBy<MemberHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddMEMBER_Resources

        private void AddNETWORK_NAME_Resources()
        {
            //InstrumentStatus
            ResourceSpace.Has.ResourcesOfType<List<NETWORK_NAME>>()
                .AtUri("/NetworkNames")
                .And.AtUri("/sites/{siteId}/AddNetworkName").Named("AddSiteNetworkName")
                .And.AtUri("/sites/{siteId}/networkNames").Named("GetSiteNetworkNames")
                .HandledBy<NetworkNameHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<NETWORK_NAME>()
                .AtUri("/NetworkNames/{entityId}")
                .And.AtUri("/sites/{siteId}/removeNetworkName").Named("RemoveSiteNetworkName")
                .HandledBy<NetworkNameHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddNETWORK_TYPE_Resources

        private void AddNETWORK_TYPE_Resources()
        {
            //NetworkType
            ResourceSpace.Has.ResourcesOfType<List<NETWORK_TYPE>>()
                .AtUri("/NetworkTypes")
                .And.AtUri("/sites/{siteId}/AddNetworkType").Named("AddSiteNetworkType")
                .And.AtUri("/sites/{siteId}/networkTypes").Named("GetSiteNetworkTypes")
                .HandledBy<NetworkTypeHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<NETWORK_TYPE>()
                .AtUri("/NetworkTypes/{entityId}")
                .And.AtUri("/sites/{siteId}/removeNetworkType").Named("RemoveSiteNetworkType")
                .HandledBy<NetworkTypeHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddNETWORK_TYPE_Resources

        private void AddOBJECTIVE_POINT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<OBJECTIVE_POINT>>()
            .AtUri("/ObjectivePoints")
            .And.AtUri("/Sites/{siteId}/ObjectivePoints").Named("GetSiteObjectivePoints")
            .And.AtUri("/VerticalDatums/{vdatumId}/OPs").Named("GetVDatumOPs")
            .HandledBy<ObjectivePointHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_POINT>()
            .AtUri("/ObjectivePoints/{entityId}")
            .HandledBy<ObjectivePointHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddOBJECTIVE_POINT_Resources

        private void AddOBJECTIVE_POINT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<OBJECTIVE_POINT_TYPE>>()
            .AtUri("/OPTypes")
            .HandledBy<ObjectivePointTypeHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_POINT_TYPE>()
            .AtUri("/OPTypes/{entityId}")
            .And.AtUri("/ObjectivePoints/{ObjectivePointId}/OPType").Named("GetOPType")
            .HandledBy<ObjectivePointTypeHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddOBJECTIVE_POINT_TYPE_Resources

        private void AddOP_CONTROL_IDENTIFIER_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<OP_CONTROL_IDENTIFIER>>()
                .AtUri("/OPControlIdentifiers")
                .And.AtUri("ObjectivePoints/{objectivePointId}/OPControls").Named("OPControls")
                .HandledBy<OP_ControlIdentifierHandler>()
                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<OP_CONTROL_IDENTIFIER>()
                .AtUri("/OPControlIdentifiers/{entityId}")
                .And.AtUri("ObjectivePoints/{objectivePointId}/AddOPControls").Named("AddOPControls")
                .HandledBy<OP_ControlIdentifierHandler>()
                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddOP_CONTROL_IDENTIFIER_Resources()

        private void AddOP_MEASUREMENTS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<OP_MEASUREMENTS>>()
                .AtUri("/OPMeasurements")
                .And.AtUri("InstrumentStatus/{instrumentStatusId}/InstrMeasurements").Named("GetInstrumentStatOPMeasurements")
                .And.AtUri("ObjectivePoints/{objectivePointId}/OPMeasurements").Named("GetOPOPMeasurements")
                .HandledBy<OP_MeasurementsHandler>()
                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<OP_MEASUREMENTS>()
                .AtUri("/OPMeasurements/{entityId}")
                .And.AtUri("InstrumentStatus/{instrumentStatusId}/AddInstrMeasurement").Named("AddInstrumentStatOPMeasurements")
                .HandledBy<OP_MeasurementsHandler>()
                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddOP_MEASUREMENTS_Resources()

        private void AddOP_QUALTITY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<OP_QUALITY>>()
            .AtUri("/ObjectivePointQualities")
            .HandledBy<OP_QualityHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<OP_QUALITY>()
            .AtUri("/ObjectivePointQualities/{entityId}")
            .And.AtUri("/ObjectivePoints/{objectivePointId}/Quality").Named("GetObjectivePointQuality")
            .HandledBy<OP_QualityHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddOP_QUALITY_Resources

        private void AddPEAK_SUMMARY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<PEAK_SUMMARY>>()
            .AtUri("/PeakSummaries")
            .And.AtUri("/Events/{eventId}/PeakSummaries").Named("GetEventPeakSummaries")
            .And.AtUri("/Sites/{siteId}/PeakSummaries").Named("GetSitePeakSummaries")
            .And.AtUri(@"/PeakSummaries/FilteredPeaks?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
                                &States={states}&County={counties}&StartDate={startDate}&EndDate={endDate}").Named("GetFilteredPeaks")
                //.And.AtUri("/PeakSummaries?State={stateName}").Named("GetPeakSummariesByStateName")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<PEAK_SUMMARY>()
            .AtUri("/PeakSummaries/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/PeakSummary").Named("GetHWMPeakSummary")
            .And.AtUri("/DataFiles/{dataFileId}/PeakSummary").Named("GetDataFilePeakSummary")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<PEAK_VIEW>()
            .AtUri("/Sites/{siteId}/PeakSummaryView").Named("GetPeakSummaryViewBySite")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddPEAK_SUMMARY_Resources

        private void AddREPORTING_METRICS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<REPORTING_METRICS>>()
            .AtUri("/ReportingMetrics")
            .And.AtUri("/Members/{memberId}/Reports").Named("GetMemberReports")
            .And.AtUri("/ReportingMetrics/ReportsByDate?Date={aDate}").Named("GetReportsByDate")
            .And.AtUri("/ReportingMetrics?Event={eventId}&State={stateName}").Named("GetReportsByEventAndState")
            .And.AtUri("/Events/{eventId}/Reports").Named("GetEventReports")
            .And.AtUri("/ReportingMetrics/FilteredReports?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReports")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<ReportResource>>()
            .AtUri("/ReportResource/FilteredReportModel?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReportsModel")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<REPORTING_METRICS>()
            .AtUri("/ReportingMetrics/{entityId}")
            .And.AtUri("/ReportingMetrics/DailyReportTotals?Date={date}&Event={eventId}&State={stateName}").Named("GetDailyReportTotals")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<ReportResource>()
            .AtUri("/ReportResource/{entityId}").Named("GetReportModel")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        } //end AddREPORTING_METRICS_Resources

        private void AddROLE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<ROLE>>()
            .AtUri("/Roles")
            .HandledBy<RoleHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<ROLE>()
            .AtUri("/Roles/{entityId}")
            .And.AtUri("/Members/{memberId}/Role").Named("GetMemberRole")
            .HandledBy<RoleHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddROLE_TYPE_Resources

        private void AddSENSOR_BRAND_Resourses()
        {
            ResourceSpace.Has.ResourcesOfType<List<SENSOR_BRAND>>()
            .AtUri("/SensorBrands")
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SENSOR_BRAND>()
            .AtUri("/SensorBrands/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/SensorBrand").Named("GetInstrumentSensorBrand")
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }

        private void AddSENSOR_DEPLOYMENT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<SENSOR_DEPLOYMENT>>()
            .AtUri("/SensorDeployments")
            .HandledBy<SensorDeploymentHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }

        private void AddSENSOR_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<SENSOR_TYPE>>()
            .AtUri("/SensorTypes")
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SENSOR_TYPE>()
            .AtUri("/SensorTypes/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/SensorType").Named("GetInstrumentSensorType")
            .And.AtUri("/DeploymentTypes/{deploymentTypeId}/SensorType").Named("GetDeploymentSensorType")
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");


        }//end AddSENSOR_TYPE_Resources

        private void AddSITE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<SiteList>()
            .AtUri("/Sites/points").Named("GetPoints")
            .And.AtUri("/Events/{eventId}/Sites").Named("GetEventSites")
            .And.AtUri("/NetworkTypes/{networkTypeId}/Sites").Named("getNetworkTypeSites")
            .And.AtUri("/NetworkNames/{networkNameId}/Sites").Named("getNetworkNameSites")
            .And.AtUri("/Sites?State={stateName}").Named("GetSitesByStateName")
            .And.AtUri("/Sites?Latitude={latitude}&Longitude={longitude}&Buffer={buffer}").Named("GetSitesByLatLong")
            .And.AtUri("HorizontalDatums/{hdatumId}/Sites").Named("GetHDatumSites")
            .And.AtUri("LandOwners/{landOwnerId}/Sites").Named("GetLandOwnserSites")
            .HandledBy<SiteHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SITE>()
            .AtUri("/Sites/{entityId}")
            //.And.AtUri("/Sites?bySiteNo={siteNo}").Named("GetSiteBySiteNo")
            .And.AtUri("/Sites?bySiteNo={siteNo}&bySiteName={siteName}&bySiteId={siteId}").Named("GetSiteBySiteNo")
            .And.AtUri("/Files/{fileId}/Site").Named("GetFileSite")
            .And.AtUri("/ObjectivePoints/{objectivePointId}/Site").Named("GetOPSite")
            .And.AtUri("/HWMs/{hwmId}/Site").Named("getHWMSite")
            .And.AtUri("/Instruments/{instrumentId}/Site").Named("GetInstrumentSite")
            .HandledBy<SiteHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<SITE>>()
            .AtUri("/FullSites").Named("GetAllSites")
            .HandledBy<SiteHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<SiteLocationQuery>>()
           .AtUri("/Sites?Event={eventId}&State={stateNames}&SensorType={sensorTypeId}&NetworkName={networkNameId}&OPDefined={opDefined}&HWMOnly={hwmOnlySites}&SensorOnly={sensorOnlySites}&RDGOnly={rdgOnlySites}").Named("GetFilteredSites")
                //.AtUri("/Sites/GetSensorLocationSites").Named("GetSensorLocationSites")
                //.And.AtUri("/Sites/GetHWMLocationSites").Named("GetHWMLocationSites")
                //.And.AtUri("/Sites/GetRDGLocationSites").Named("GetRDGLocationSites")
            .HandledBy<SiteHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSITE_Resources                 

        private void AddSITE_HOUSING_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<SITE_HOUSING>>()
            .AtUri("/SiteHousings")
            .And.AtUri("Sites/{siteId}/SiteHousings").Named("GetSiteSiteHousing")
            .HandledBy<Site_HousingHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SITE_HOUSING>()
            .AtUri("/SiteHousings/{entityId}")
            .And.AtUri("Site/{siteId}/AddSiteSiteHousing").Named("AddSiteSiteHousing")
            .HandledBy<Site_HousingHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddOP_MEASUREMENTS_Resources()

        private void AddSOURCE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<SOURCE>>()
            .AtUri("/Sources")
            .And.AtUri("Agencies/{agencyId}/Sources").Named("GetAgencySources")
            .HandledBy<SourceHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<SOURCE>()
            .AtUri("/Sources/{entityId}")
            .And.AtUri("Files/{fileId}/Source").Named("GetFileSource")
            .HandledBy<SourceHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSOURCE_Resources        

        private void AddSTATE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<STATES>>()
            .AtUri("/States").Named("GetAllStates")
            .And.AtUri("/Sites/States").Named("GetSiteStates")
                //NOW BEING HANDLED BY COUNTIES .And.AtUri("/StateCounties/{stateName}").Named("StateCounties")
            .HandledBy<StateHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<STATES>()
            .AtUri("States/{entityId}")
            .HandledBy<StateHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATE_Resources

        private void AddSTATUS_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<STATUS_TYPE>>()
            .AtUri("/StatusTypes")
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<STATUS_TYPE>()
            .AtUri("/StatusTypes/{entityId}")
            .And.AtUri("/InstrumentStatus/{instrumentStatusId}/Status").Named("GetInstrumentStatusStatus")
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATUS_TYPE_Resources

        private void AddVERTICAL_COLLECTION_METHOD_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<VERTICAL_COLLECT_METHODS>>()
            .AtUri("/VerticalMethods")
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<VERTICAL_COLLECT_METHODS>()
            .AtUri("/VerticalMethods/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/VerticalMethod").Named("GetHWMVerticalMethod")
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_COLLECTION_METHOD_Resources

        private void AddVERTICAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<VERTICAL_DATUMS>>()
            .AtUri("/VerticalDatums")
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<VERTICAL_DATUMS>()
            .AtUri("/VerticalDatums/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/vDatum").Named("GetHWMVDatum")
            .And.AtUri("/ObjectivePoints/{objectivePointId}/vDatum").Named("getOPVDatum")
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_DATUM_Resources

        #endregion

    }//End class Configuration
}//End namespace




