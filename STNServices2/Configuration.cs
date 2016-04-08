//------------------------------------------------------------------------------
//----- Configuration ----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

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
// 03.23.2016-JKN - update to .NET 4.5 and POSTGRESQL
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
using System;
using System.Collections.Generic;

using WiM.Codecs.json;
using WiM.Codecs.xml;
using WiM.Codecs.csv;
using WiM.PipeLineContributors;

using STNDB;

using STNServices2;
using STNServices2.Handlers;
using STNServices2.PipeLineContributors;
using STNServices2.Security;

using OpenRasta.Configuration;
using OpenRasta.Web.UriDecorators;
using OpenRasta.Security;
using OpenRasta.Codecs;
using OpenRasta.IO;
using OpenRasta.DI;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Pipeline;

namespace STNServices2
{
    public class Configuration : IConfigurationSource
    {
        #region Private Field Properties
        public static string agencyResource = "Agencies";
        public static string approvalResource = "Approvals";
        public static string contactResource = "Contacts";
        public static string contacttypeResource = "ContactTypes";
        public static string countyResource ="Counties";
        public static string datafileResource = "DataFiles";
        public static string deploymentpriorityResource = "DeploymentPriorities";
        public static string deploymenttypeResource = "DeploymentTypes";
        public static string eventsResource = "Events";
        public static string eventstatusResource = "EventStatus";
        public static string eventtypeResource = "EventTypes";
        public static string fileResource = "Files";
        public static string filetypeResource = "FileTypes";
        public static string horizontalmethodResource = "HorizontalMethods";
        public static string horizontaldatumResource = "HorizontalDatums";
        public static string housingtypeResource = "HousingTypes";
        public static string hwmResource = "HWMs";
        public static string hwmqualityResource = "HWMQualities";
        public static string hwmtypeResource = "HWMTypes";
        public static string instrcollectionResource = "InstrCollectConditions";

        public static string memberResource = "Members";


        public static string reportMetricResource = "ReportingMetrics";
        public static string roleResource = "Roles";
        public static string sensorBrandResource = "SensorBrands";
        public static string sensorTypeResource = "SensorTypes";
        public static string siteResource = "Sites";
        public static string siteHousingResource = "SiteHousings";
        public static string sourceResource = "Sources";
        public static string stateResource = "States";
        public static string statusTypeResource = "StatusTypes";
        public static string verticalCollectMethodResource = "VerticalMethods";
        public static string verticalDatumResource = "VerticalDatums";
        #endregion

        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                // specify the authentication scheme you want to use, you can register multiple ones
                ResourceSpace.Uses.CustomDependency<IAuthenticationProvider, STNAuthenticationProvider>(DependencyLifetime.Singleton);
                ResourceSpace.Uses.PipelineContributor<BasicAuthorizerContributor>();

                // Allow codec choice by extension 
                ResourceSpace.Uses.UriDecorator<ContentTypeExtensionUriDecorator>();
                ResourceSpace.Uses.PipelineContributor<ErrorCheckingContributor>();
                ResourceSpace.Uses.PipelineContributor<CrossDomainPipelineContributor>();
                ResourceSpace.Uses.PipelineContributor<MessagePipelineContributor>();
                ResourceSpace.Uses.PipelineContributor<STNHyperMediaPipelineContributor>();

                AddAGENCY_Resources();
                AddAPPROVAL_Resources();
                //AddAUTHENTICATION_Resources();
                AddCONTACT_Resources();
                AddCONTACT_TYPE_Resources();
                AddCOUNTIES_Resources();
                AddDATA_FILE_Resources();
                AddDEPLOYMENT_PRIORITY_Resources();
                AddDEPLOYMENT_TYPE_Resources();
                AddEVENT_Resources();
                AddEVENT_STATUS_Resources();
                AddEVENT_TYPE_Resources();
                AddFILE_Resources();
                AddFILE_TYPE_Resources();
                AddHORIZONTAL_COLLECT_METHODS_Resources();
                AddHORIZONTAL_DATUM_Resources();
                AddHOUSING_TYPE_Resources();
                AddHWM_Resources();
                AddHWM_QUALITY_Resources();
                AddHWM_TYPE_Resources();
                AddINSTR_COLLECTION_CONDITIONS_Resources();
                //AddINSTRUMENT_Resources();
                //AddINSTRUMENT_STATUS_Resources();
                //AddKEYWORD_Resources();
                //AddLANDOWNER_CONTACT_Resources();
                //AddLocatorType_Resources();
                //AddMARKER_Resources();
                //AddMEMBER_Resources();
                //AddNETWORK_NAME_Resources();
                //AddNETWORK_TYPE_Resources();
                //AddPEAK_SUMMARY_Resources();
                //AddOBJECTIVE_POINT_Resources();
                //AddOBJECTIVE_POINT_TYPE_Resources();
                //AddOP_CONTROL_IDENTIFIER_Resources();
                //AddOP_QUALTITY_Resources();
                //AddOP_MEASUREMENTS_Resources();

                AddREPORTING_METRICS_Resources();
                AddROLE_Resources();
                AddSENSOR_BRAND_Resourses();                
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
            ResourceSpace.Has.ResourcesOfType<List<agency>>()
            .AtUri(agencyResource)
            .HandledBy<AgencyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<agency>()
            .AtUri(agencyResource)
            .And.AtUri(agencyResource+"/{entityId}")
            .And.AtUri(memberResource+"/{memberId}/"+agencyResource).Named("GetMemberAgency")
            .And.AtUri(sourceResource+"/{sourceId}/"+agencyResource).Named("GetSourceAgency")
            .HandledBy<AgencyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAGENCY_Resources
        private void AddAPPROVAL_Resources()
        {
            //ApprovalTable
            ResourceSpace.Has.ResourcesOfType<List<approval>>()
            .AtUri(approvalResource)
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<approval>()
            .AtUri(approvalResource + "/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/Approve").Named("ApproveHWM")
            .And.AtUri(datafileResource+"/{dataFileId}/Approve").Named("ApproveDataFile")
            .And.AtUri(hwmResource+"/{hwmId}/Unapprove").Named("UnApproveHWM")
            .And.AtUri(datafileResource+"/{dataFileId}/Unapprove").Named("UnApproveDataFile")
            .And.AtUri(datafileResource+"/{dataFileId}/Approval").Named("GetDataFileApproval")
            .And.AtUri(hwmResource+"/{hwmId}/Approval").Named("GetHWMApproval")
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAPPROVAL_Resources
//        private void AddAUTHENTICATION_Resources()
//        {
//            //Authentication
//            ResourceSpace.Has.ResourcesOfType<Boolean>()
//            .AtUri("/login")
//            .HandledBy<LoginHandler>()
//            .TranscodedBy<UTF8XmlSerializerCodec>();

//        }//end AddAUTHENTICATION_Resources
        private void AddCONTACT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<contact>>()
            .AtUri(contactResource)
            .And.AtUri(contactResource+"?ReportMetric={reportMetricsId}").Named("GetReportMetricContacts")
                //.And.AtUri("/Contacts?ContactModel/{reportMetricsId}").Named("GetContactModelByReport")
            .HandledBy<ContactHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            //GET
            //ResourceSpace.Has.ResourcesOfType<List<ReportContactModel>>()
            //.AtUri("/Contacts?ContactModelByReport={reportMetricsId}").Named("GetContactModelByReport")
            //.HandledBy<ContactHandler>()
            //.TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            //.And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            //.And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<contact>()
            .AtUri(contactResource+"/{entityId}")
             .And.AtUri(contactResource + "?ReportMetric={reportMetricsId}&ContactType={contactTypeId}").Named("GetReportMetricContactsByType")
             .And.AtUri(contactResource + "/{reportMetricsId}/removeReportContact").Named("RemoveReportContact")
             .And.AtUri(contactResource + "/AddReportContact?contactTypeId={ContactTypeId}&reportId={ReportId}").Named("AddReportContact")
            .HandledBy<ContactHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_Resources     
        private void AddCONTACT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<contact_type>>()
            .AtUri(contacttypeResource)
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<contact_type>()
            .AtUri(contacttypeResource+"/{entityId}")
            .And.AtUri(contactResource+"/{contactId}/"+contacttypeResource).Named("GetContactsContactType")
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_TYPE_Resources
        private void AddCOUNTIES_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<county>>()
            .AtUri(countyResource)
            .And.AtUri(stateResource+"/{stateId}/"+countyResource).Named("GetStateCountiesById")
            .And.AtUri(stateResource+"/"+countyResource+"?StateAbbrev={stateAbbrev}").Named("GetStateCountiesByAbbrev")
            .And.AtUri("/Sites/CountiesByState?StateAbbrev={stateAbbrev}").Named("GetStateSiteCounties")
            .HandledBy<CountyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<county>()
            .AtUri(countyResource+"/{entityId}")
            .HandledBy<CountyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCOUNTIES_Resources
        private void AddDATA_FILE_Resources()
        {
            //DATA_FILE Table
            ResourceSpace.Has.ResourcesOfType<List<data_file>>()
          .AtUri(datafileResource)
          .And.AtUri("/Instruments/{instrumentId}/"+datafileResource).Named("GetInstrumentDataFiles")
          .And.AtUri("/PeakSummaries/{peakSummaryId}/" + datafileResource).Named("GetPeakSummaryDatafiles")
          .And.AtUri(datafileResource+"?IsApproved={boolean}")
          .And.AtUri(approvalResource+"/{ApprovalId}/" + datafileResource).Named("GetApprovedDataFiles")
          .And.AtUri(datafileResource+"/?IsApproved={approved}&Event={eventId}&Processor={memberId}&State={state}").Named("GetFilteredDataFiles")
          .HandledBy<DataFileHandler>()
          .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
          .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<data_file>()
                //.AtUri("/DataFiles").Named("CreateDataFile")
            .AtUri(datafileResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/DataFile").Named("GetFileDataFile")
            .HandledBy<DataFileHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDATA_FILE_Resources
        private void AddDEPLOYMENT_PRIORITY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<deployment_priority>>()
            .AtUri(deploymentpriorityResource)
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<deployment_priority>()
            .AtUri(deploymentpriorityResource+"/{entityId}")
            .And.AtUri("/Sites/{siteId}/" + deploymentpriorityResource).Named("GetSitePriorities")
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_PRIORITY_Resources
        private void AddDEPLOYMENT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<deployment_type>>()
            .AtUri(deploymenttypeResource)
            .And.AtUri("/SensorTypes/{sensorTypeId}/" + deploymenttypeResource).Named("GetSensorDeploymentTypes")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<deployment_type>()
            .AtUri(deploymenttypeResource+"/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/DeploymentType").Named("GetInstrumentDeploymentType")
            .And.AtUri("/SensorTypes/{sensorTypeId}/addDeploymentType").Named("AddSensorDeployment")
            .And.AtUri("/SensorTypes/{sensorTypeId}/removeDeploymentType").Named("RemoveSensorDeployment")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_TYPE_Resources
        private void AddEVENT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<events>>()
            .AtUri(eventsResource)
            .And.AtUri(eventsResource+"?Site={siteId}").Named("GetEventsBySite")
            .And.AtUri(eventtypeResource+"/{eventTypeId}/Events").Named("GetEventTypeEvents")
            .And.AtUri(eventstatusResource+"/{eventStatusId}/"+eventsResource).Named("GetEventStatusEvents")
            .And.AtUri(eventsResource+"/FilteredEvents?Date={date}&Type={eventTypeId}&State={stateName}").Named("GetFilteredEvents")
            .HandledBy<EventsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<events>()
            .AtUri(eventsResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/Event").Named("GetHWMEvent")
            .And.AtUri("/Instruments/{instrumentId}/Event").Named("GetInstrumentEvent")
            .HandledBy<EventsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_Resources
        private void AddEVENT_STATUS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<event_status>>()
            .AtUri(eventstatusResource)
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<event_status>()
            .AtUri(eventstatusResource+"/{entityId}")
            .And.AtUri(eventsResource+"/{eventId}/Status").Named("GetEventStatus")
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEvent_Status_Resources
        private void AddEVENT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<event_type>>()
            .AtUri(eventtypeResource)
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<event_type>()
            .AtUri(eventtypeResource+"/{entityId}")
            .And.AtUri(eventsResource+"/{eventId}/Type").Named("GetEventType")
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_TYPE_Resources
        private void AddFILE_Resources()
        {
            //******NOTE******
            //The key/value codec prevents using a URL like /HWMs/{hwmId}/FILE
            //For now top level URLs are used for new resources
            //(http://stackoverflow.com/questions/7694839/how-to-handle-multipart-post-using-a-uri-template-of-form-prefix-suffix-in-op)
            //http://stackoverflow.com/questions/1952278/get-image-as-stream-or-byte-array-using-openrasta
            //****************
            ResourceSpace.Has.ResourcesOfType<file>()
           .AtUri(fileResource+"/{entityId}")           
           .And.AtUri(fileResource+"/bytes").Named("UploadFile")
           .HandledBy<FileHandler>()
           .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<MultipartFormDataObjectCodec>()
           .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<file>>()
           .AtUri(fileResource)
           .And.AtUri(hwmResource+"/{hwmId}/" + fileResource).Named("GetHWMFiles")
           .And.AtUri("/ObjectivePoints/{objectivePointId}/" + fileResource).Named("GetObjectivePointFiles")
           .And.AtUri(filetypeResource+"/{fileTypeId}/" + fileResource).Named("GetFileTypeFiles")
           .And.AtUri("/Sites/{siteId}/" + fileResource).Named("GetSiteFile")
           .And.AtUri("/Sources/{sourceId}/" + fileResource).Named("GetSourceFiles")
           .And.AtUri(datafileResource+"/{dataFileId}/" + fileResource).Named("GetDataFileFiles")
           .And.AtUri("/Instruments/{instrumentId}/" + fileResource).Named("GetInstrumentFiles")
           .And.AtUri(eventsResource+"/{eventId}/" + fileResource).Named("GetEventFiles")
           .And.AtUri(fileResource+"?FromDate={fromDate}&ToDate={toDate}")
           .And.AtUri(fileResource+"?State={stateName}").Named("GetFilesByStateName")
           .And.AtUri(fileResource+"?Site={siteId}&Event={eventId}")
           .HandledBy<FileHandler>()
           .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<IFile>()
            .AtUri(fileResource+"/{fileId}/Item").Named("GetFileItem")
            .And.AtUri(eventsResource+"/{eventId}/EventFileItems").Named("GetEventFileItems")
            .HandledBy<FileHandler>();
        }//end AddFILE_Resources
        private void AddFILE_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<file_type>>()
            .AtUri(filetypeResource)
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<file_type>()
            .AtUri(filetypeResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/Type").Named("GetFileType")
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddFILE_TYPE_Resources
        private void AddHORIZONTAL_COLLECT_METHODS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<horizontal_collect_methods>>()
            .AtUri(horizontalmethodResource)
            .HandledBy<HorizontalCollectionMethodHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<horizontal_collect_methods>()
            .AtUri(horizontalmethodResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/HorizontalMethod").Named("GetHWMHorizontalMethods")
            .HandledBy<HorizontalCollectionMethodHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_COLLECT_METHODS_Resources
        private void AddHORIZONTAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<horizontal_datums>>()
            .AtUri(horizontaldatumResource)
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<horizontal_datums>()
            .AtUri(horizontaldatumResource+"/{entityId}")
            .And.AtUri("/Sites/{siteId}/hDatum").Named("GetSiteHdatum")
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_DATUM_Resources
        private void AddHOUSING_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<housing_type>>()
            .AtUri(housingtypeResource)
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<housing_type>()
            .AtUri(housingtypeResource+"/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/InstrumentHousingType").Named("GetInstrumentHousingType")
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHOUSING_TYPE_Resources
        private void AddHWM_Resources()
        {
            //High water marks
            //ResourceSpace.Has.ResourcesOfType<HWMList>()
            //.AtUri("/HWMs")
            //.And.AtUri("/Events/{eventId}/HWMs").Named("GetEventSimpleHWMs")
            //.And.AtUri("/Sites/{siteId}/EventHWMs?Event={eventId}").Named("GetSiteEventHWMs")
            //.And.AtUri("/HWMS?IsApproved={approved}&Event={eventId}&Member={memberId}&State={state}").Named("GetApprovalHWMs")
            //.HandledBy<HWMHandler>()
            //.TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            //.And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            //.And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<hwm>>()
            .AtUri(hwmResource+"?IsApproved={boolean}").Named("GetHWMByApproval")
            .And.AtUri(approvalResource+"/{ApprovalId}/"+hwmResource).Named("GetApprovedHWMs")
            .And.AtUri(memberResource+"/{memberId}/"+hwmResource).Named("GetMemberHWMs")
            .And.AtUri(hwmqualityResource+"/{hwmQualityId}/" + hwmResource).Named("GetHWMQualityHWMs")
            .And.AtUri(hwmtypeResource+"/{hwmTypeId}/" + hwmResource).Named("GetHWMTypeHWMs")
            .And.AtUri(horizontalmethodResource+"/{hmethodId}/" + hwmResource).Named("GetHmethodHWMs")
            .And.AtUri("/VerticalMethods/{vmethodId}/" + hwmResource).Named("GetVmethodHWMs")
            .And.AtUri("/Sites/{siteId}/" + hwmResource).Named("GetSiteHWMs")
            .And.AtUri("/VerticalDatums/{vdatumId}/" + hwmResource).Named("GetVDatumHWMs")
             .And.AtUri("/Markers/{markerId}/" + hwmResource).Named("GetMarkerHWMs")
            .And.AtUri("/PeakSummaries/{peakSummaryId}/" + hwmResource).Named("GetPeakSummaryHWMs")
            .HandledBy<HWMHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<List<HWMDownloadable>>()
//            .AtUri(@"/HWMs/FilteredHWMs?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
//                            &States={states}&County={counties}&HWMType={hwmTypeIDs}&HWMQuality={hwmQualIDs}
//                            &HWMEnvironment={hwmEnvironment}&SurveyComplete={surveyComplete}&StillWater={stillWater}").Named("FilteredHWMs")
//            .HandledBy<HWMHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<hwm>()
            .AtUri(hwmResource+"/{entityId}").Named("GetHWM")
            .And.AtUri(fileResource+"/{fileId}/HWM").Named("GetFileHWM")
            .HandledBy<HWMHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHWM_Resources
        private void AddHWM_QUALITY_Resources()
         {
             //GET
             ResourceSpace.Has.ResourcesOfType<List<hwm_qualities>>()
             .AtUri(hwmqualityResource)
             .HandledBy<HWMQualityHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

             ResourceSpace.Has.ResourcesOfType<hwm_qualities>()
             .AtUri(hwmqualityResource+"/{entityId}")
             .And.AtUri(hwmResource+"/{hwmId}/Quality").Named("GetHWMQuality")
             .HandledBy<HWMQualityHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

         }//end AddHWM_QUALITY_Resources
        private void AddHWM_TYPE_Resources()
         {
             //GET
             ResourceSpace.Has.ResourcesOfType<List<hwm_types>>()
             .AtUri(hwmtypeResource)
             .HandledBy<HWMTypeHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

             ResourceSpace.Has.ResourcesOfType<hwm_types>()
             .AtUri(hwmtypeResource+"/{entityId}")
             .And.AtUri(hwmResource+"/{hwmId}/Type").Named("GetHWMType")
             .HandledBy<HWMTypeHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

         }//end AddHWM_TYPE_Resources
        private void AddINSTR_COLLECTION_CONDITIONS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<instr_collection_conditions>>()
            .AtUri(instrcollectionResource)
            .HandledBy<InstrumentCollectionConditionHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<instr_collection_conditions>()
            .AtUri(instrcollectionResource+"/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/CollectCondition").Named("GetInstrumentCondition")
            .HandledBy<InstrumentCollectionConditionHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end INSTR_COLLECTION_CONDITIONS_Resources
//        private void AddINSTRUMENT_Resources()
//        {
//            //Instruments
//            ResourceSpace.Has.ResourcesOfType<InstrumentSerialNumberList>()
//               .AtUri("/Instruments/SerialNumbers").Named("SerialNumbers")  //
//               .HandledBy<InstrumentHandler>() //handler. what class is gonna handle this
//               .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<List<INSTRUMENT>>()
//               .AtUri("/Instruments")
//               .And.AtUri("/Instruments/GetAll").Named("GetAllInstruments")
//               .And.AtUri("/Sites/{siteId}/Instruments").Named("GetSiteInstruments")
//               .And.AtUri("/SensorBrands/{sensorBrandId}/Instruments").Named("SensorBrandInstruments")
//               .And.AtUri("/SensorTypes/{sensorTypeId}/Instruments").Named("SensorTypeInstruments")
//               .And.AtUri("/DeploymentTypes/{deploymentTypeId}/Instruments").Named("GetDeploymentTypeInstruments")
//               .And.AtUri(@"/Instruments?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
//                                &States={states}&County={counties}&CurrentStatus={statusIDs}&CollectionCondition={collectionConditionIDs}
//                                &DeploymentType={deploymentTypeIDs}").Named("GetFilteredInstruments")
//                //.And.AtUri("/Sites/Instruments?bySiteNo={siteNo}").Named("GetSiteInstrumentsByInternalId")
//               .And.AtUri("/Events/{eventId}/Instruments").Named("GetEventInstruments")
//                //NOT SEEING IT USED .And.AtUri("/Instruments/ReportInstruments?Date={aDate}&Event={eventId}&State={stateAbbrev}").Named("GetReportInstruments")  //Instruments?Date={aDate}&Event={eventId}&State={stateAbbrev}").Named("GetReportInstruments")
//               .And.AtUri("/Sites/{siteId}/Instruments?Event={eventId}").Named("GetSiteEventInstruments")
//               .HandledBy<InstrumentHandler>()
//               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<INSTRUMENT>()
//                .AtUri("/Instruments/{entityId}")
//                .And.AtUri("/DataFiles/{dataFileId}/Instrument").Named("GetDataFileInstrument")
//                .And.AtUri("/InstrumentStatus/{instrumentStatusId}/Instrument").Named("GetInstrumentStatusInstrument")
//                .And.AtUri("/Files/{fileId}/Instrument").Named("GetFileInstrument")
//                .HandledBy<InstrumentHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<FullInstrument>()
//                .AtUri("/Instruments/{instrumentId}/FullInstrument").Named("GetFullInstruments")
//                .HandledBy<InstrumentHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<List<FullInstrument>>()
//                .AtUri("/Sites/{siteId}/FullInstrumentList").Named("GetFullInstrumentList")
//                .HandledBy<InstrumentHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<SensorViews>()
//                .AtUri("/SensorViews/{eventId}").Named("GetSensorViews")
//                .HandledBy<InstrumentHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddINSTRUMENT_Resources
//        private void AddINSTRUMENT_STATUS_Resources()
//        {
//            //InstrumentStatus
//            ResourceSpace.Has.ResourcesOfType<List<INSTRUMENT_STATUS>>()
//                .AtUri("/InstrumentStatus")
//                .And.AtUri("/Instruments/{instrumentId}/InstrumentStatusLog").Named("GetInstrumentStatusLog")
//                //.And.AtUri("/CollectionTeams/{teamId}/InstrumentStatus").Named("GetCollectionTeamInstrumentStatuses")
//                .HandledBy<InstrumentStatusHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<INSTRUMENT_STATUS>()
//                .AtUri("/InstrumentStatus/{entityId}")
//                .And.AtUri("/Instruments/{instrumentId}/InstrumentStatus").Named("GetInstrumentStatus")
//                .HandledBy<InstrumentStatusHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddINSTRUMENT_STATUS_Resources
//        private void AddKEYWORD_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<KEYWORD>>()
//            .AtUri("/Keywords")
//            .HandledBy<KeywordHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<KEYWORD>()
//            .AtUri("/Keywords/{keywordId}")
//            .HandledBy<KeywordHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
//        }//end AddKEYWORD_Resources
//        private void AddLANDOWNER_CONTACT_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<LANDOWNERCONTACT>>()
//            .AtUri("/LandOwners")
//            .HandledBy<LandOwnerHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<LANDOWNERCONTACT>()
//            .AtUri("/LandOwners/{entityId}")
//            .And.AtUri("/Sites/{siteId}/LandOwner").Named("GetSiteLandOwner")
//            .HandledBy<LandOwnerHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddLANDOWNER_CONTACT_Resources
//        private void AddLocatorType_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<LOCATORTYPE>>()
//            .AtUri("/LocatorTypes")
//            .HandledBy<LocatorTypeHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<LOCATORTYPE>()
//            .AtUri("/LocatorTypes/{locatorTypeId}")
//            .HandledBy<LocatorTypeHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
//        }//end AddLocatorType_Resources
//        private void AddMARKER_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<MARKER>>()
//            .AtUri("/Markers")
//            .HandledBy<MarkerHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<MARKER>()
//            .AtUri("/Markers/{entityId}")
//            .And.AtUri("/HWMs/{hwmId}/Marker").Named("GetHWMMarker")
//            .HandledBy<MarkerHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddMARKER_Resources
//        private void AddMEMBER_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<MEMBER>>()
//            .AtUri("/Members")
//            .And.AtUri("/Events/{eventId}/Members").Named("GetEventMembers")            
//            .And.AtUri("/Agencies/{agencyId}/Members").Named("GetAgencyMembers")
//            .And.AtUri("/Roles/{roleId}/Members").Named("GetRoleMembers")
//            .HandledBy<MemberHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<MEMBER>()
//            .AtUri("/Members/{entityId}")
//                //.And.AtUri("/Members/{entityId}/Edit").Named("Put")
//            .And.AtUri("Members/{pass}/addMember").Named("AddMember")
//            .And.AtUri("Members?username={userName}&newPass={newPassword}").Named("ChangeMembersPassword")
//            .And.AtUri("Events/{eventId}/EventCoordinator").Named("GetEventCoordinator")
//            .And.AtUri("Approvals/{ApprovalId}/ApprovingOfficial").Named("GetApprovingOfficial")
//            .And.AtUri("DataFiles/{dataFileId}/Processor").Named("GetDataFileProcessor")
//            .And.AtUri("/Members?username={userName}").Named("GetByUserName")
//            .And.AtUri("PeakSummaries/{peakSummaryId}/Processor").Named("GetPeakSummaryProcessor")
//            .HandledBy<MemberHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
//        }//end AddMEMBER_Resources
//        private void AddNETWORK_NAME_Resources()
//        {
//            //InstrumentStatus
//            ResourceSpace.Has.ResourcesOfType<List<NETWORK_NAME>>()
//                .AtUri("/NetworkNames")
//                .And.AtUri("/sites/{siteId}/AddNetworkName").Named("AddSiteNetworkName")
//                .And.AtUri("/sites/{siteId}/networkNames").Named("GetSiteNetworkNames")
//                .HandledBy<NetworkNameHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<NETWORK_NAME>()
//                .AtUri("/NetworkNames/{entityId}")
//                .And.AtUri("/sites/{siteId}/removeNetworkName").Named("RemoveSiteNetworkName")
//                .HandledBy<NetworkNameHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
//        }//end AddNETWORK_TYPE_Resources
//        private void AddNETWORK_TYPE_Resources()
//        {
//            //NetworkType
//            ResourceSpace.Has.ResourcesOfType<List<NETWORK_TYPE>>()
//                .AtUri("/NetworkTypes")
//                .And.AtUri("/sites/{siteId}/AddNetworkType").Named("AddSiteNetworkType")
//                .And.AtUri("/sites/{siteId}/networkTypes").Named("GetSiteNetworkTypes")
//                .HandledBy<NetworkTypeHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<NETWORK_TYPE>()
//                .AtUri("/NetworkTypes/{entityId}")
//                .And.AtUri("/sites/{siteId}/removeNetworkType").Named("RemoveSiteNetworkType")
//                .HandledBy<NetworkTypeHandler>()
//                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
//        }//end AddNETWORK_TYPE_Resources
//        private void AddOBJECTIVE_POINT_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<OBJECTIVE_POINT>>()
//            .AtUri("/ObjectivePoints")
//            .And.AtUri("/Sites/{siteId}/ObjectivePoints").Named("GetSiteObjectivePoints")
//            .And.AtUri("/VerticalDatums/{vdatumId}/OPs").Named("GetVDatumOPs")
//            .HandledBy<ObjectivePointHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_POINT>()
//            .AtUri("/ObjectivePoints/{entityId}")
//            .HandledBy<ObjectivePointHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddOBJECTIVE_POINT_Resources
//        private void AddOBJECTIVE_POINT_TYPE_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<OBJECTIVE_POINT_TYPE>>()
//            .AtUri("/OPTypes")
//            .HandledBy<ObjectivePointTypeHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_POINT_TYPE>()
//            .AtUri("/OPTypes/{entityId}")
//            .And.AtUri("/ObjectivePoints/{ObjectivePointId}/OPType").Named("GetOPType")
//            .HandledBy<ObjectivePointTypeHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddOBJECTIVE_POINT_TYPE_Resources
//        private void AddOP_CONTROL_IDENTIFIER_Resources()
//        {
//            ResourceSpace.Has.ResourcesOfType<List<OP_CONTROL_IDENTIFIER>>()
//                .AtUri("/OPControlIdentifiers")
//                .And.AtUri("ObjectivePoints/{objectivePointId}/OPControls").Named("OPControls")
//                .HandledBy<OP_ControlIdentifierHandler>()
//                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<OP_CONTROL_IDENTIFIER>()
//                .AtUri("/OPControlIdentifiers/{entityId}")
//                .And.AtUri("ObjectivePoints/{objectivePointId}/AddOPControls").Named("AddOPControls")
//                .HandledBy<OP_ControlIdentifierHandler>()
//                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//End AddOP_CONTROL_IDENTIFIER_Resources()
//        private void AddOP_MEASUREMENTS_Resources()
//        {
//            ResourceSpace.Has.ResourcesOfType<List<OP_MEASUREMENTS>>()
//                .AtUri("/OPMeasurements")
//                .And.AtUri("InstrumentStatus/{instrumentStatusId}/InstrMeasurements").Named("GetInstrumentStatOPMeasurements")
//                .And.AtUri("ObjectivePoints/{objectivePointId}/OPMeasurements").Named("GetOPOPMeasurements")
//                .HandledBy<OP_MeasurementsHandler>()
//                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<OP_MEASUREMENTS>()
//                .AtUri("/OPMeasurements/{entityId}")
//                .And.AtUri("InstrumentStatus/{instrumentStatusId}/AddInstrMeasurement").Named("AddInstrumentStatOPMeasurements")
//                .HandledBy<OP_MeasurementsHandler>()
//                .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//End AddOP_MEASUREMENTS_Resources()
//        private void AddOP_QUALTITY_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<OP_QUALITY>>()
//            .AtUri("/ObjectivePointQualities")
//            .HandledBy<OP_QualityHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<OP_QUALITY>()
//            .AtUri("/ObjectivePointQualities/{entityId}")
//            .And.AtUri("/ObjectivePoints/{objectivePointId}/Quality").Named("GetObjectivePointQuality")
//            .HandledBy<OP_QualityHandler>()
//            .TranscodedBy<STNXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddOP_QUALITY_Resources
//        private void AddPEAK_SUMMARY_Resources()
//        {
//            //GET
//            ResourceSpace.Has.ResourcesOfType<List<PEAK_SUMMARY>>()
//            .AtUri("/PeakSummaries")
//            .And.AtUri("/Events/{eventId}/PeakSummaries").Named("GetEventPeakSummaries")
//            .And.AtUri("/Sites/{siteId}/PeakSummaries").Named("GetSitePeakSummaries")
//            .And.AtUri(@"/PeakSummaries/FilteredPeaks?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
//                                &States={states}&County={counties}&StartDate={startDate}&EndDate={endDate}").Named("GetFilteredPeaks")
//                //.And.AtUri("/PeakSummaries?State={stateName}").Named("GetPeakSummariesByStateName")
//            .HandledBy<PeakSummaryHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<PEAK_SUMMARY>()
//            .AtUri("/PeakSummaries/{entityId}")
//            .And.AtUri("/HWMs/{hwmId}/PeakSummary").Named("GetHWMPeakSummary")
//            .And.AtUri("/DataFiles/{dataFileId}/PeakSummary").Named("GetDataFilePeakSummary")
//            .HandledBy<PeakSummaryHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//            ResourceSpace.Has.ResourcesOfType<PEAK_VIEW>()
//            .AtUri("/Sites/{siteId}/PeakSummaryView").Named("GetPeakSummaryViewBySite")
//            .HandledBy<PeakSummaryHandler>()
//            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
//            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
//            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

//        }//end AddPEAK_SUMMARY_Resources
        private void AddREPORTING_METRICS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<reporting_metrics>>()
            .AtUri(reportMetricResource)
            .And.AtUri("/Members/{memberId}/Reports").Named("GetMemberReports")
            .And.AtUri(reportMetricResource+"/ReportsByDate?Date={aDate}").Named("GetReportsByDate")
            .And.AtUri(reportMetricResource+"?Event={eventId}&State={stateName}").Named("GetReportsByEventAndState")
            .And.AtUri("/Events/{eventId}/Reports").Named("GetEventReports")
            .And.AtUri(reportMetricResource+"/FilteredReports?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReports")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<reporting_metrics>()
            .AtUri(reportMetricResource+"/{entityId}")
            .And.AtUri(reportMetricResource+"/DailyReportTotals?Date={date}&Event={eventId}&State={stateName}").Named("GetDailyReportTotals")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
            
            //ResourceSpace.Has.ResourcesOfType<List<ReportResource>>()
            //.AtUri("/ReportResource/FilteredReportModel?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReportsModel")
            //.HandledBy<ReportingMetricsHandler>()
            //.TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            //.And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            //.And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            //ResourceSpace.Has.ResourcesOfType<ReportResource>()
            //.AtUri("/ReportResource/{entityId}").Named("GetReportModel")
            //.HandledBy<ReportingMetricsHandler>()
            //.TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            //.And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            //.And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        } //end AddREPORTING_METRICS_Resources
        private void AddROLE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<role>>()
            .AtUri(roleResource)
            .HandledBy<RoleHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<role>()
            .AtUri(roleResource+"/{entityId}")
            .And.AtUri("/Members/{memberId}/Role").Named("GetMemberRole")
            .HandledBy<RoleHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddROLE_TYPE_Resources
        private void AddSENSOR_BRAND_Resourses()
        {
            ResourceSpace.Has.ResourcesOfType<List<sensor_brand>>()
            .AtUri(sensorBrandResource)
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<sensor_brand>()
            .AtUri(sensorBrandResource+"/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/SensorBrand").Named("GetInstrumentSensorBrand")
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }
        private void AddSENSOR_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<sensor_type>>()
            .AtUri(sensorTypeResource)
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<sensor_type>()
            .AtUri(sensorTypeResource+"/{entityId}")
            .And.AtUri("/Instruments/{instrumentId}/SensorType").Named("GetInstrumentSensorType")
            .And.AtUri("/DeploymentTypes/{deploymentTypeId}/SensorType").Named("GetDeploymentSensorType")
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");


        }//end AddSENSOR_TYPE_Resources
        private void AddSITE_Resources()
        {
            //GET
            //ResourceSpace.Has.ResourcesOfType<SiteList>()
            //.AtUri("/Sites/points").Named("GetPoints")
            //.And.AtUri("/Events/{eventId}/Sites").Named("GetEventSites")
            //.And.AtUri("/NetworkTypes/{networkTypeId}/Sites").Named("getNetworkTypeSites")
            //.And.AtUri("/NetworkNames/{networkNameId}/Sites").Named("getNetworkNameSites")
            //.And.AtUri("/Sites?State={stateName}").Named("GetSitesByStateName")
            //.And.AtUri("/Sites?Latitude={latitude}&Longitude={longitude}&Buffer={buffer}").Named("GetSitesByLatLong")
            //.And.AtUri("HorizontalDatums/{hdatumId}/Sites").Named("GetHDatumSites")
            //.And.AtUri("LandOwners/{landOwnerId}/Sites").Named("GetLandOwnserSites")
            //.HandledBy<SiteHandler>()
            //.TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            //.And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            //.And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<site>()
            .AtUri(siteResource+"/{entityId}")
            .And.AtUri(siteResource+"?bySiteNo={siteNo}&bySiteName={siteName}&bySiteId={siteId}").Named("GetSiteBySiteNo")
            .And.AtUri("/Files/{fileId}/Site").Named("GetFileSite")
            .And.AtUri("/ObjectivePoints/{objectivePointId}/Site").Named("GetOPSite")
            .And.AtUri("/HWMs/{hwmId}/Site").Named("getHWMSite")
            .And.AtUri("/Instruments/{instrumentId}/Site").Named("GetInstrumentSite")
            .HandledBy<SiteHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<site>>()
            .AtUri("/FullSites").Named("GetAllSites")
            .HandledBy<SiteHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

           // ResourceSpace.Has.ResourcesOfType<List<SiteLocationQuery>>()
           //.AtUri("/Sites?Event={eventId}&State={stateNames}&SensorType={sensorTypeId}&NetworkName={networkNameId}&OPDefined={opDefined}&HWMOnly={hwmOnlySites}&SensorOnly={sensorOnlySites}&RDGOnly={rdgOnlySites}").Named("GetFilteredSites")
           // .HandledBy<SiteHandler>()
           // .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           // .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           // .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSITE_Resources                 
        private void AddSITE_HOUSING_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<site_housing>>()
            .AtUri(siteHousingResource)
            .And.AtUri("Sites/{siteId}/"+siteHousingResource).Named("GetSiteSiteHousing")
            .HandledBy<Site_HousingHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<site_housing>()
            .AtUri(siteHousingResource+"/{entityId}")
            //.And.AtUri("Site/{siteId}/AddSiteSiteHousing").Named("AddSiteSiteHousing") -- just use regular post and apply siteId before sending object
            .HandledBy<Site_HousingHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddSITE_HOUSING_Resources()
        private void AddSOURCE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<source>>()
            .AtUri(sourceResource)
            .And.AtUri("Agencies/{agencyId}/"+sourceResource).Named("GetAgencySources")
            .HandledBy<SourceHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<source>()
            .AtUri(sourceResource+"/{entityId}")
            .And.AtUri("Files/{fileId}/Source").Named("GetFileSource")
            .HandledBy<SourceHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSOURCE_Resources        
        private void AddSTATE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<state>>()
            .AtUri(stateResource).Named("GetAllStates")
            .And.AtUri("/Sites/"+stateResource).Named("GetSiteStates")
            .HandledBy<StateHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<state>()
            .AtUri(stateResource+"/{entityId}")
            .HandledBy<StateHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATE_Resources
        private void AddSTATUS_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<status_type>>()
            .AtUri(statusTypeResource)
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<status_type>()
            .AtUri(statusTypeResource+"/{entityId}")
            .And.AtUri("/InstrumentStatus/{instrumentStatusId}/Status").Named("GetInstrumentStatusStatus")
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATUS_TYPE_Resources
        private void AddVERTICAL_COLLECTION_METHOD_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<vertical_collect_methods>>()
            .AtUri(verticalCollectMethodResource)
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<vertical_collect_methods>()
            .AtUri(verticalCollectMethodResource+"/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/VerticalMethod").Named("GetHWMVerticalMethod")
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_COLLECTION_METHOD_Resources
        private void AddVERTICAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<vertical_datums>>()
            .AtUri(verticalDatumResource)
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<vertical_datums>()
            .AtUri(verticalDatumResource+"/{entityId}")
            .And.AtUri("/HWMs/{hwmId}/vDatum").Named("GetHWMVDatum")
            .And.AtUri("/ObjectivePoints/{objectivePointId}/vDatum").Named("getOPVDatum")
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_DATUM_Resources

        #endregion

    }//End class Configuration
}//End namespace





