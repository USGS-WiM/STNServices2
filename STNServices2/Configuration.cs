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
using STNServices2.Resources;
using STNServices2.Codecs.json;

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
        public static string instrumentsResource = "Instruments";
        public static string instrumentStatusResource = "InstrumentStatus";
        public static string landOwnerResource = "LandOwners";
        public static string locatorTypeResource = "LocatorTypes";
        public static string markerResource = "Markers";
        public static string memberResource = "Members";
        public static string networkNameResource = "NetworkNames";
        public static string networkTypeResource = "NetworkTypes";
        public static string objectivePointResource = "ObjectivePoints";
        public static string objectivePointTypeResource = "OPTypes";
        public static string opControlResource = "OPControlIdentifiers";
        public static string opMeasurementsResource = "OPMeasurements";
        public static string opQualityResource = "ObjectivePointQualities";
        public static string peakSummaryResource = "PeakSummaries";
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
                AddINSTRUMENT_Resources();
                AddINSTRUMENT_STATUS_Resources();
                AddLANDOWNER_CONTACT_Resources();
                AddMARKER_Resources();
                AddMEMBER_Resources();
                AddNETWORK_NAME_Resources();
                AddNETWORK_TYPE_Resources(); 
                AddOBJECTIVE_POINT_Resources();
                AddOBJECTIVE_POINT_TYPE_Resources();
                AddOP_CONTROL_IDENTIFIER_Resources(); 
                AddOP_MEASUREMENTS_Resources();
                AddOP_QUALTITY_Resources();
                AddPEAK_SUMMARY_Resources();
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
                AddGEOCODER_Resources();
            } //End using OpenRastaConfiguration.Manual
        }

        #region Helper methods

        private void AddGEOCODER_Resources()
        {
            //GET census.gov geocode for lat/long    
            ResourceSpace.Has.ResourcesOfType<object>()
            .AtUri("Geocode/location?Latitude={latitude}&Longitude={longitude}").Named("GetReverseGeocode")
            .HandledBy<GeocoderHandler>()
            .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }

        private void AddAGENCY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<agency>>()
            .AtUri(agencyResource)
            .HandledBy<AgencyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<agency>()
            .AtUri(agencyResource+"/{entityId}")
            .And.AtUri(memberResource+"/{memberId}/"+agencyResource).Named("GetMemberAgency")
            .And.AtUri(sourceResource+"/{sourceId}/"+agencyResource).Named("GetSourceAgency")
            .HandledBy<AgencyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAGENCY_Resources (1)
        private void AddAPPROVAL_Resources()
        {
            //ApprovalTable
            ResourceSpace.Has.ResourcesOfType<List<approval>>()
            .AtUri(approvalResource)
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<approval>()
            .AtUri(approvalResource + "/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/Approve").Named("ApproveHWM")
            .And.AtUri(datafileResource+"/{dataFileId}/Approve").Named("ApproveDataFile")
            .And.AtUri(datafileResource+"/{dataFileId}/NWISApprove").Named("ApproveNWISDataFile")
            .And.AtUri(hwmResource+"/{hwmId}/Unapprove").Named("UnApproveHWM")
            .And.AtUri(datafileResource+"/{dataFileId}/Unapprove").Named("UnApproveDataFile")
            .And.AtUri(datafileResource+"/{dataFileId}/Approval").Named("GetDataFileApproval")
            .And.AtUri(hwmResource+"/{hwmId}/Approval").Named("GetHWMApproval")
            .HandledBy<ApprovalHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddAPPROVAL_Resources(2)
        private void AddCONTACT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<contact>>()
            .AtUri(contactResource)
            .And.AtUri(contactResource+"?ReportMetric={reportMetricsId}").Named("GetReportMetricContacts")
            .HandledBy<ContactHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<contact>()
                .AtUri(contactResource + "/{entityId}")
             .And.AtUri(contactResource + "?ReportMetric={reportMetricsId}&ContactType={contactTypeId}").Named("GetReportMetricContactsByType")
             .And.AtUri(contactResource + "/{contactId}/removeReportContact?ReportId={reportMetricsId}").Named("RemoveReportContact")
             //.And.AtUri(contactResource + "/{contactId}/addReportContact?ReportId={reportId}&ContactTypeId={contactTypeId}").Named("AddReportContact")
             .And.AtUri(reportMetricResource + "/{reportId}/addContactType/{contactTypeId}").Named("AddReportContact")
            .HandledBy<ContactHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_Resources    (3) 
        private void AddCONTACT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<contact_type>>()
            .AtUri(contacttypeResource)
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<contact_type>()
            .AtUri(contacttypeResource+"/{entityId}")
            .And.AtUri(contactResource+"/{contactId}/"+contacttypeResource).Named("GetContactsContactType")
            .HandledBy<ContactTypeHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCONTACT_TYPE_Resources (4)
        private void AddCOUNTIES_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<county>>()
            .AtUri(countyResource)
            .And.AtUri(stateResource + "/{stateId}/" + countyResource).Named("GetStateCounties")
            .And.AtUri(stateResource+"/"+countyResource+"?StateAbbrev={stateAbbrev}").Named("GetStateCountiesByAbbrev")
            .And.AtUri(siteResource+"/CountiesByState?StateAbbrev={stateAbbrev}").Named("GetStateSiteCounties")
            .HandledBy<CountyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<county>()
            .AtUri(countyResource+"/{entityId}")
            .HandledBy<CountyHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddCOUNTIES_Resources (5)
        private void AddDATA_FILE_Resources()
        {
            //DATA_FILE Table
            ResourceSpace.Has.ResourcesOfType<List<data_file>>()
           .AtUri(datafileResource)
           .And.AtUri(instrumentsResource+"/{instrumentId}/"+datafileResource).Named("GetInstrumentDataFiles")
           .And.AtUri(peakSummaryResource+"/{peakSummaryId}/" + datafileResource).Named("GetPeakSummaryDatafiles")
           .And.AtUri(approvalResource+"/{ApprovalId}/" + datafileResource).Named("GetApprovedDataFiles")
           .And.AtUri(datafileResource+"?IsApproved={approved}&Event={eventId}&Processor={memberId}&State={state}").Named("GetFilteredDataFiles")
           .HandledBy<DataFileHandler>()
           .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<data_file>()
            .AtUri(datafileResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/DataFile").Named("GetFileDataFile")
            .HandledBy<DataFileHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDATA_FILE_Resources (6)
        private void AddDEPLOYMENT_PRIORITY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<deployment_priority>>()
            .AtUri(deploymentpriorityResource)
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<deployment_priority>()
            .AtUri(deploymentpriorityResource+"/{entityId}")
            .And.AtUri("/Sites/{siteId}/" + deploymentpriorityResource).Named("GetSitePriorities")
            .HandledBy<DeploymentPriorityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_PRIORITY_Resources (7)
        private void AddDEPLOYMENT_TYPE_Resources()
        {           
            ResourceSpace.Has.ResourcesOfType<List<deployment_type>>()
            .AtUri(deploymenttypeResource)
            .And.AtUri(sensorTypeResource+"/{sensorTypeId}/" + deploymenttypeResource).Named("GetSensorDeploymentTypes")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<deployment_type>() 
            .AtUri(deploymenttypeResource+"/{entityId}")
            .And.AtUri(instrumentsResource+"/{instrumentId}/DeploymentType").Named("GetInstrumentDeploymentType")
            .And.AtUri(sensorTypeResource+"/{sensorTypeId}/addDeploymentType?DeploymentTypeId={deploymentTypeId}").Named("AddSensorDeployment")
            .And.AtUri(sensorTypeResource + "/{sensorTypeId}/removeDeploymentType?DeploymentTypeId={deploymentTypeId}").Named("RemoveSensorDeployment")
            .HandledBy<DepolymentTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddDEPLOYMENT_TYPE_Resources (8)
        private void AddEVENT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<events>>()
            .AtUri(eventsResource).Named("GetAllEvents")
            .And.AtUri(eventsResource + "?Site={siteId}").Named("GetSiteEvents")
            .And.AtUri(eventtypeResource+"/{eventTypeId}/"+eventsResource).Named("GetEventTypeEvents")
            .And.AtUri(eventstatusResource + "/{eventStatusId}/" + eventsResource).Named("GetEventStatusEvents")
            .And.AtUri(eventsResource+"/FilteredEvents?Date={date}&Type={eventTypeId}&State={stateName}").Named("GetFilteredEvents")
            .HandledBy<EventsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<events>()
            .AtUri(eventsResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/Event").Named("GetHWMEvent")
            .And.AtUri(instrumentsResource+"/{instrumentId}/Event").Named("GetInstrumentEvent")
            .HandledBy<EventsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_Resources (9)
        private void AddEVENT_STATUS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<event_status>>()
            .AtUri(eventstatusResource)
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<event_status>()
            .AtUri(eventstatusResource+"/{entityId}")
            .And.AtUri(eventsResource+"/{eventId}/Status").Named("GetEventStatus")
            .HandledBy<EventStatusHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEvent_Status_Resources (10)
        private void AddEVENT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<event_type>>()
            .AtUri(eventtypeResource)
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<event_type>()
            .AtUri(eventtypeResource+"/{entityId}")
            .And.AtUri(eventsResource+"/{eventId}/Type").Named("GetEventType")
            .HandledBy<EventTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddEVENT_TYPE_Resources (11)
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
           .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<file>>()
           .AtUri(fileResource)
           .And.AtUri(hwmResource+"/{hwmId}/" + fileResource).Named("GetHWMFiles")
           .And.AtUri(objectivePointResource+"/{objectivePointId}/" + fileResource).Named("GetObjectivePointFiles")
           .And.AtUri(filetypeResource+"/{fileTypeId}/" + fileResource).Named("GetFileTypeFiles")
           .And.AtUri(siteResource+"/{siteId}/" + fileResource).Named("GetSiteFile")
           .And.AtUri(sourceResource+"/{sourceId}/" + fileResource).Named("GetSourceFiles")
           .And.AtUri(datafileResource+"/{dataFileId}/" + fileResource).Named("GetDataFileFiles")
           .And.AtUri(instrumentsResource+"/{instrumentId}/" + fileResource).Named("GetInstrumentFiles")
           .And.AtUri(eventsResource+"/{eventId}/" + fileResource).Named("GetEventFiles")
           .And.AtUri(fileResource+"?FromDate={fromDate}&ToDate={toDate}").Named("GetFilesByDateRange")
           .And.AtUri(fileResource+"?State={stateName}").Named("GetFilesByStateName")
           .And.AtUri(fileResource+"?Site={siteId}&Event={eventId}").Named("GetSiteEventFiles")
           .HandledBy<FileHandler>()
           .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<IFile>()
            .AtUri(fileResource+"/{fileId}/Item").Named("GetFileItem")
            .And.AtUri(eventsResource+"/{eventId}/EventFileItems").Named("GetEventFileItems")
            .HandledBy<FileHandler>();
        }//end AddFILE_Resources (12)
        private void AddFILE_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<file_type>>()
            .AtUri(filetypeResource)
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<file_type>()
            .AtUri(filetypeResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/Type").Named("GetFileType")
            .HandledBy<FileTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddFILE_TYPE_Resources (13)
        private void AddHORIZONTAL_COLLECT_METHODS_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<horizontal_collect_methods>>()
            .AtUri(horizontalmethodResource)
            .HandledBy<HorizontalCollectionMethodHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<horizontal_collect_methods>()
            .AtUri(horizontalmethodResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/HorizontalMethod").Named("GetHWMHorizontalMethods")
            .HandledBy<HorizontalCollectionMethodHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_COLLECT_METHODS_Resources (14)
        private void AddHORIZONTAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<horizontal_datums>>()
            .AtUri(horizontaldatumResource)
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<horizontal_datums>()
            .AtUri(horizontaldatumResource+"/{entityId}")
            .And.AtUri(siteResource+"/{siteId}/hDatum").Named("GetSiteHdatum")
            .HandledBy<HorizontalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHORIZONTAL_DATUM_Resources (15)
        private void AddHOUSING_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<housing_type>>()
            .AtUri(housingtypeResource)
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<housing_type>()
            .AtUri(housingtypeResource+"/{entityId}")
            .And.AtUri(instrumentsResource+"/{instrumentId}/InstrumentHousingType").Named("GetInstrumentHousingType")
            .HandledBy<HousingTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddHOUSING_TYPE_Resources (16)
        private void AddHWM_Resources()
        {           
            ResourceSpace.Has.ResourcesOfType<List<hwm>>()
            .AtUri(hwmResource).Named("GetAllHWMs")
            .And.AtUri(eventsResource+"/{eventId}/"+hwmResource).Named("GetEventHWMs")
            .And.AtUri(siteResource+"/{siteId}/EventHWMs?Event={eventId}").Named("GetSiteEventHWMs")
            .And.AtUri(hwmResource+"?IsApproved={approved}&Event={eventId}&Member={memberId}&State={state}").Named("GetApprovalHWMs")
            .And.AtUri(approvalResource+"/{ApprovalId}/"+hwmResource).Named("GetApprovedHWMs")
            .And.AtUri(memberResource+"/{memberId}/"+hwmResource).Named("GetMemberHWMs")
            .And.AtUri(hwmqualityResource+"/{hwmQualityId}/" + hwmResource).Named("GetHWMQualityHWMs")
            .And.AtUri(hwmtypeResource+"/{hwmTypeId}/" + hwmResource).Named("GetHWMTypeHWMs")
            .And.AtUri(horizontalmethodResource+"/{hmethodId}/" + hwmResource).Named("GetHmethodHWMs")
            .And.AtUri(verticalCollectMethodResource+"/{vmethodId}/" + hwmResource).Named("GetVmethodHWMs")
            .And.AtUri(siteResource+"/{siteId}/" + hwmResource).Named("GetSiteHWMs")
            .And.AtUri(verticalDatumResource+"/{vdatumId}/" + hwmResource).Named("GetVDatumHWMs")
             .And.AtUri(markerResource+"/{markerId}/" + hwmResource).Named("GetMarkerHWMs")
            .And.AtUri(peakSummaryResource+"/{peakSummaryId}/" + hwmResource).Named("GetPeakSummaryHWMs")
            .And.AtUri(@"/HWMs/FilteredHWMs?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
                            &States={states}&County={counties}&HWMType={hwmTypeIDs}&HWMQuality={hwmQualIDs}
                            &HWMEnvironment={hwmEnvironment}&SurveyComplete={surveyComplete}&StillWater={stillWater}").Named("FilteredHWMs")
            .HandledBy<HWMHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

            ResourceSpace.Has.ResourcesOfType<hwm>()
            .AtUri(hwmResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/HWM").Named("GetFileHWM")
            .HandledBy<HWMHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

        }//end AddHWM_Resources (17)
        private void AddHWM_QUALITY_Resources()
         {
             //GET
             ResourceSpace.Has.ResourcesOfType<List<hwm_qualities>>()
             .AtUri(hwmqualityResource)
             .HandledBy<HWMQualityHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

             ResourceSpace.Has.ResourcesOfType<hwm_qualities>()
             .AtUri(hwmqualityResource+"/{entityId}")
             .And.AtUri(hwmResource+"/{hwmId}/Quality").Named("GetHWMQuality")
             .HandledBy<HWMQualityHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

         }//end AddHWM_QUALITY_Resources (18)
        private void AddHWM_TYPE_Resources()
         {
             //GET
             ResourceSpace.Has.ResourcesOfType<List<hwm_types>>()
             .AtUri(hwmtypeResource)
             .HandledBy<HWMTypeHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

             ResourceSpace.Has.ResourcesOfType<hwm_types>()
             .AtUri(hwmtypeResource+"/{entityId}")
             .And.AtUri(hwmResource+"/{hwmId}/Type").Named("GetHWMType")
             .HandledBy<HWMTypeHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
             .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
             .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

         }//end AddHWM_TYPE_Resources (19)
        private void AddINSTR_COLLECTION_CONDITIONS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<instr_collection_conditions>>()
            .AtUri(instrcollectionResource)
            .HandledBy<InstrumentCollectionConditionHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<instr_collection_conditions>()
            .AtUri(instrcollectionResource+"/{entityId}")
            .And.AtUri(instrumentsResource+"/{instrumentId}/CollectCondition").Named("GetInstrumentCondition")
            .HandledBy<InstrumentCollectionConditionHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end INSTR_COLLECTION_CONDITIONS_Resources (20)
        private void AddINSTRUMENT_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<FullInstrument>()
                .AtUri(instrumentsResource + "/{instrumentId}/FullInstrument").Named("GetFullInstruments")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
                .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<FullInstrument>>()
                .AtUri(siteResource + "/{siteId}/SiteFullInstrumentList").Named("GetSiteFullInstrumentList")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
                .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<instrument>()
                .AtUri(instrumentsResource + "/{entityId}")
                .And.AtUri(datafileResource + "/{dataFileId}/Instrument").Named("GetDataFileInstrument")
                .And.AtUri(instrumentStatusResource + "/{instrumentStatusId}/Instrument").Named("GetInstrumentStatusInstrument")
                .And.AtUri(fileResource + "/{fileId}/Instrument").Named("GetFileInstrument")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");


            ResourceSpace.Has.ResourcesOfType<List<instrument>>()
               .AtUri(instrumentsResource).Named("GetAllInstruments")
               .And.AtUri(siteResource + "/{siteId}/" + instrumentsResource).Named("GetSiteInstruments")
               .And.AtUri(sensorBrandResource + "/{sensorBrandId}/" + instrumentsResource).Named("GetSensorBrandInstruments")
               .And.AtUri(sensorTypeResource + "/{sensorTypeId}/" + instrumentsResource).Named("GetSensorTypeInstruments")
               .And.AtUri(deploymenttypeResource + "/{deploymentTypeId}/" + instrumentsResource).Named("GetDeploymentTypeInstruments")
               .And.AtUri(@"/Instruments/FilteredInstruments?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}
                                &States={states}&County={counties}&CurrentStatus={statusIDs}&CollectionCondition={collectionConditionIDs}
                                &SensorType={sensorTypeIDs}&DeploymentType={deploymentTypeIDs}").Named("GetFilteredInstruments")
               .And.AtUri(eventsResource + "/{eventId}/" + instrumentsResource).Named("GetEventInstruments")
               .And.AtUri(siteResource + "/{siteId}/" + instrumentsResource + "?Event={eventId}").Named("GetSiteEventInstruments")
               .HandledBy<InstrumentHandler>()
               .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<sensor_view>()
                .AtUri(@"/SensorViews?ViewType={view}&Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}&States={states}&County={counties}
                        &CurrentStatus={statusIDs}&CollectionCondition={collectionConditionIDs}&SensorType={sensorTypeIDs}&DeploymentType={deploymentTypeIDs}").Named("GetSensorViews")
                .HandledBy<InstrumentHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
           .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

        }//end AddINSTRUMENT_Resources (21)
        private void AddINSTRUMENT_STATUS_Resources()
        {
            //InstrumentStatus
            ResourceSpace.Has.ResourcesOfType<List<instrument_status>>()
                .AtUri(instrumentStatusResource)
                .And.AtUri(instrumentsResource+"/{instrumentId}/InstrumentStatusLog").Named("GetInstrumentStatusLog")
                .HandledBy<InstrumentStatusHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
                .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<instrument_status>()
                .AtUri(instrumentStatusResource+"/{entityId}")
                .And.AtUri(instrumentsResource +"/{instrumentId}/" + instrumentStatusResource).Named("GetInstrumentStatus")
                .HandledBy<InstrumentStatusHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
                .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddINSTRUMENT_STATUS_Resources (22)
        private void AddLANDOWNER_CONTACT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<landownercontact>>()
            .AtUri(landOwnerResource)
            .HandledBy<LandOwnerHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<landownercontact>()
            .AtUri(landOwnerResource +"/{entityId}")
            .And.AtUri(siteResource + "/{siteId}/LandOwner").Named("GetSiteLandOwner")
            .HandledBy<LandOwnerHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddLANDOWNER_CONTACT_Resources (23)
        private void AddMARKER_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<marker>>()
            .AtUri(markerResource)
            .HandledBy<MarkerHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<marker>()
            .AtUri(markerResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/Marker").Named("GetHWMMarker")
            .HandledBy<MarkerHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddMARKER_Resources (24)
        private void AddMEMBER_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<member>>()
            .AtUri(memberResource)
            .And.AtUri(eventsResource+"/{eventId}/" + memberResource).Named("GetEventMembers")
            .And.AtUri(agencyResource+"/{agencyId}/" + memberResource).Named("GetAgencyMembers")
            .And.AtUri(roleResource+"/{roleId}/" + memberResource).Named("GetRoleMembers")
            .HandledBy<MemberHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<member>()
            .AtUri(memberResource + "/{entityId}")            
            .And.AtUri("/login").Named("GetLoggedUser")
            .And.AtUri(eventsResource+"/{eventId}/EventCoordinator").Named("GetEventCoordinator")
            .And.AtUri(approvalResource+"/{ApprovalId}/ApprovingOfficial").Named("GetApprovingOfficial")
            .And.AtUri(datafileResource+"/{dataFileId}/Processor").Named("GetDataFileProcessor")
            .And.AtUri(peakSummaryResource+"/{peakSummaryId}/Processor").Named("GetPeakSummaryProcessor")
            .HandledBy<MemberHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<string>()
            .AtUri(memberResource + "/GetMemberName/{entityId}").Named("GetMemberName")
            .HandledBy<MemberHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddMEMBER_Resources (25)
        private void AddNETWORK_NAME_Resources()
        {
            //InstrumentStatus
            ResourceSpace.Has.ResourcesOfType<List<network_name>>()
                .AtUri(networkNameResource)
                .And.AtUri(siteResource+"/{siteId}/addNetworkName?NetworkNameId={networkNameId}").Named("addSiteNetworkName")
                .And.AtUri(siteResource +"/{siteId}/"+networkNameResource).Named("GetSiteNetworkNames")
                .HandledBy<NetworkNameHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<network_name>()
                .AtUri(networkNameResource +"/{entityId}")
                .And.AtUri(siteResource + "/{siteId}/removeNetworkName?NetworkNameId={networkNameId}").Named("removeSiteNetworkName")
                .HandledBy<NetworkNameHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddNETWORK_TYPE_Resources (26)
        private void AddNETWORK_TYPE_Resources()
        {
            //NetworkType
            ResourceSpace.Has.ResourcesOfType<List<network_type>>()
                .AtUri(networkTypeResource)
                .And.AtUri(siteResource + "/{siteId}/addNetworkType?NetworkTypeId={networkTypeId}").Named("addSiteNetworkType")
                .And.AtUri(siteResource+"/{siteId}/"+networkTypeResource).Named("GetSiteNetworkTypes")
                .HandledBy<NetworkTypeHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<network_type>()
                .AtUri(networkTypeResource+"/{entityId}")
                .And.AtUri(siteResource + "/{siteId}/removeNetworkType?NetworkTypeId={networkTypeId}").Named("removeSiteNetworkType")
                .HandledBy<NetworkTypeHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        }//end AddNETWORK_TYPE_Resources (27)
        private void AddOBJECTIVE_POINT_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<objective_point>>()
            .AtUri(objectivePointResource)
            .And.AtUri(siteResource+"/{siteId}/"+objectivePointResource).Named("GetSiteObjectivePoints")
            .And.AtUri(verticalDatumResource + "/{vdatumId}/"+ objectivePointResource).Named("GetVDatumOPs")
            .HandledBy<ObjectivePointHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");


            ResourceSpace.Has.ResourcesOfType<objective_point>()
            .AtUri(objectivePointResource+"/{entityId}")
            .HandledBy<ObjectivePointHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");


        }//end AddOBJECTIVE_POINT_Resources (28)
        private void AddOBJECTIVE_POINT_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<objective_point_type>>()
            .AtUri(objectivePointTypeResource)
            .HandledBy<ObjectivePointTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<objective_point_type>()
            .AtUri(objectivePointTypeResource+"/{entityId}")
            .And.AtUri(objectivePointResource + "/{objectivePointId}/OPType").Named("GetObjectivePointOPType")
            .HandledBy<ObjectivePointTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddOBJECTIVE_POINT_TYPE_Resources (29)
        private void AddOP_CONTROL_IDENTIFIER_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<op_control_identifier>>()
                .AtUri(opControlResource)
                .And.AtUri(objectivePointResource+"/{objectivePointId}/OPControls").Named("OPControls")
                .HandledBy<OP_ControlIdentifierHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<op_control_identifier>()
                .AtUri(opControlResource+"/{entityId}")
                .HandledBy<OP_ControlIdentifierHandler>()
                .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddOP_CONTROL_IDENTIFIER_Resources() (30)
        private void AddOP_MEASUREMENTS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<op_measurements>>()
            .AtUri(opMeasurementsResource)
            .And.AtUri(instrumentStatusResource+"/{instrumentStatusId}/"+opMeasurementsResource).Named("GetInstrumentStatOPMeasurements")
            .And.AtUri(objectivePointResource+"/{objectivePointId}/"+opMeasurementsResource).Named("GetOPOPMeasurements")
            .HandledBy<OP_MeasurementsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<op_measurements>()
            .AtUri(opMeasurementsResource+"/{entityId}")
            .HandledBy<OP_MeasurementsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddOP_MEASUREMENTS_Resources() (31)
        private void AddOP_QUALTITY_Resources()
        {
            //GET ObjectivePointQualities
            ResourceSpace.Has.ResourcesOfType<List<op_quality>>()
            .AtUri(opQualityResource)
            .HandledBy<ObjectivePointQualityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<op_quality>()
            .AtUri(opQualityResource+"/{entityId}")
            .And.AtUri(objectivePointResource+"/{objectivePointId}/Quality").Named("GetObjectivePointQuality")
            .HandledBy<ObjectivePointQualityHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddOP_QUALITY_Resources (32)
        private void AddPEAK_SUMMARY_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<peak_summary>>()
            .AtUri(peakSummaryResource).Named("GetAllPeaks")
            .And.AtUri(eventsResource+"/{eventId}/" + peakSummaryResource).Named("GetEventPeakSummaries")
            .And.AtUri(siteResource+"/{siteId}/"+peakSummaryResource).Named("GetSitePeakSummaries")
            .And.AtUri(@"/PeakSummaries/FilteredPeaks?Event={eventIds}&EventType={eventTypeIDs}&EventStatus={eventStatusID}&States={states}&County={counties}&StartDate={startDate}&EndDate={endDate}").Named("GetFilteredPeaks")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

            ResourceSpace.Has.ResourcesOfType<peak_summary>()
            .AtUri(peakSummaryResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/PeakSummary").Named("GetHWMPeakSummary")
            .And.AtUri(datafileResource+"/{dataFileId}/PeakSummary").Named("GetDataFilePeakSummary")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<peak_view>()
            .AtUri(siteResource+"/{siteId}/PeakSummaryView").Named("GetPeakSummaryViewBySite")
            .HandledBy<PeakSummaryHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddPEAK_SUMMARY_Resources (33)
        private void AddREPORTING_METRICS_Resources()
        {            
            ResourceSpace.Has.ResourcesOfType<ReportResource>()
            .AtUri("/ReportResource/{entityId}").Named("GetReportModel")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<ReportResource>>()
            .AtUri("/ReportResource/FilteredReportModel?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReportsModel")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<reporting_metrics>()
            .AtUri(reportMetricResource+"/{entityId}")
            .And.AtUri(reportMetricResource+"/DailyReportTotals?Date={date}&Event={eventId}&State={stateName}").Named("GetDailyReportTotals")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<List<reporting_metrics>>()
            .AtUri(reportMetricResource)
            .And.AtUri(memberResource+"/{memberId}/Reports").Named("GetMemberReports")
            .And.AtUri(reportMetricResource+"/ReportsByDate?Date={aDate}").Named("GetReportsByDate")
            .And.AtUri(reportMetricResource+"?Event={eventId}&State={stateName}").Named("GetReportsByEventAndState")
            .And.AtUri(eventsResource+"/{eventId}/Reports").Named("GetEventReports")
            .And.AtUri(reportMetricResource+"/FilteredReports?Event={eventId}&States={stateNames}&Date={aDate}").Named("GetFilteredReports")
            .HandledBy<ReportingMetricsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
            
        } //end AddREPORTING_METRICS_Resources (34)
        private void AddROLE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<role>>()
            .AtUri(roleResource)
            .HandledBy<RoleHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<role>()
            .AtUri(roleResource+"/{entityId}")
            .And.AtUri(memberResource+"/{memberId}/Role").Named("GetMemberRole")
            .HandledBy<RoleHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddROLE_TYPE_Resources (35)
        private void AddSENSOR_BRAND_Resourses()
        {
            ResourceSpace.Has.ResourcesOfType<List<sensor_brand>>()
            .AtUri(sensorBrandResource)
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<sensor_brand>()
            .AtUri(sensorBrandResource+"/{entityId}")
            .And.AtUri(instrumentsResource+"/{instrumentId}/SensorBrand").Named("GetInstrumentSensorBrand")
            .HandledBy<SensorBrandHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");
        } //(36)
        private void AddSENSOR_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<sensor_type>>()
            .AtUri(sensorTypeResource)
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<sensor_type>()
            .AtUri(sensorTypeResource+"/{entityId}")
            .And.AtUri(instrumentsResource+"/{instrumentId}/SensorType").Named("GetInstrumentSensorType")
            .And.AtUri(deploymenttypeResource+"/{deploymentTypeId}/SensorType").Named("GetDeploymentSensorType")
            .HandledBy<SensorTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");


        }//end AddSENSOR_TYPE_Resources (37)
        private void AddSITE_Resources()
        {            
            //GET
            ResourceSpace.Has.ResourcesOfType<List<site>>()
            .AtUri(siteResource).Named("GetAllSites")
            .And.AtUri(eventsResource+"/{eventId}/"+siteResource).Named("GetEventSites")
            .And.AtUri(networkTypeResource+"/{networkTypeId}/" + siteResource).Named("getNetworkTypeSites")
            .And.AtUri(networkNameResource+"/{networkNameId}/" + siteResource).Named("getNetworkNameSites")
            .And.AtUri(stateResource + "/{stateAbbrev}/" + siteResource).Named("GetSitesByStateName") //was "/sites?State={stateName}"
            .And.AtUri(siteResource+"?Latitude={latitude}&Longitude={longitude}&Buffer={buffer}").Named("GetSitesByLatLong")
            .And.AtUri(horizontaldatumResource+"/{hdatumId}/"+siteResource).Named("GetHDatumSites")
            .And.AtUri(landOwnerResource+"/{landOwnerId}/"+siteResource).Named("GetLandOwnserSites")
            .HandledBy<SiteHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

            
            ResourceSpace.Has.ResourcesOfType<site>()
            .AtUri(siteResource+"/{entityId}")
            .And.AtUri(siteResource+"/Search?bySiteNo={siteNo}&bySiteName={siteName}&bySiteId={siteId}").Named("GetSiteBySearch")
            .And.AtUri(fileResource+"/{fileId}/Site").Named("GetFileSite")
            .And.AtUri(objectivePointResource+"/{objectivePointId}/Site").Named("GetOPSite")
            .And.AtUri(hwmResource+"/{hwmId}/Site").Named("getHWMSite")
            .And.AtUri(instrumentsResource+"/{instrumentId}/Site").Named("GetInstrumentSite")
            .HandledBy<SiteHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
            .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");

            ResourceSpace.Has.ResourcesOfType<List<site>>()
           .AtUri(@"/Sites/FilteredSites?Event={eventId}&State={stateNames}&SensorType={sensorTypeId}&NetworkName={networkNameId}&OPDefined={opDefined}&HWMOnly={hwmOnlySites}
                    &SensorOnly={sensorOnlySites}&RDGOnly={rdgOnlySites}").Named("FilteredSites")
           .HandledBy<SiteHandler>()
           .TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
           .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
           .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv")
           .And.TranscodedBy<STNGeoJsonDotNetCodec>().ForMediaType("application/geojson;q=0.5").ForExtension("geojson");
            
           
        }//end AddSITE_Resources                  (38)
        private void AddSITE_HOUSING_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<site_housing>>()
            .AtUri(siteHousingResource)
            .And.AtUri(siteResource+"/{siteId}/"+siteHousingResource).Named("GetSiteSiteHousing")
            .HandledBy<Site_HousingHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<site_housing>()
            .AtUri(siteHousingResource+"/{entityId}")
            .HandledBy<Site_HousingHandler>()
             .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//End AddSITE_HOUSING_Resources() (39)
        private void AddSOURCE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<source>>()
            .AtUri(sourceResource)
            .And.AtUri(agencyResource+"/{agencyId}/"+sourceResource).Named("GetAgencySources")
            .HandledBy<SourceHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<source>()
            .AtUri(sourceResource+"/{entityId}")
            .And.AtUri(fileResource+"/{fileId}/Source").Named("GetFileSource")
            .HandledBy<SourceHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<string>()
            .AtUri(sourceResource + "/GetSourceName/{entityId}").Named("GetSourceName")
            .HandledBy<SourceHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddSOURCE_Resources         (40)
        private void AddSTATE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<state>>()
            .AtUri(stateResource).Named("GetAllStates")
            .And.AtUri(siteResource+"/"+stateResource).Named("GetSiteStates")
            .HandledBy<StateHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<state>()
            .AtUri(stateResource+"/{entityId}")
            .HandledBy<StateHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATE_Resources (41)
        private void AddSTATUS_TYPE_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<status_type>>()
            .AtUri(statusTypeResource)
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<status_type>()
            .AtUri(statusTypeResource+"/{entityId}")
            .And.AtUri(instrumentStatusResource+"/{instrumentStatusId}/Status").Named("GetInstrumentStatusStatus")
            .HandledBy<StatusTypeHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddSTATUS_TYPE_Resources (42)
        private void AddVERTICAL_COLLECTION_METHOD_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<vertical_collect_methods>>()
            .AtUri(verticalCollectMethodResource)
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<vertical_collect_methods>()
            .AtUri(verticalCollectMethodResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/VerticalMethod").Named("GetHWMVerticalMethod")
            .HandledBy<VerticalCollectionMethodsHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_COLLECTION_METHOD_Resources (43)
        private void AddVERTICAL_DATUM_Resources()
        {
            //GET
            ResourceSpace.Has.ResourcesOfType<List<vertical_datums>>()
            .AtUri(verticalDatumResource)
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

            ResourceSpace.Has.ResourcesOfType<vertical_datums>()
            .AtUri(verticalDatumResource+"/{entityId}")
            .And.AtUri(hwmResource+"/{hwmId}/vDatum").Named("GetHWMVDatum")
            .And.AtUri(objectivePointResource+"/{objectivePointId}/vDatum").Named("getOPVDatum")
            .HandledBy<VerticalDatumHandler>()
            .TranscodedBy<UTF8EntityXmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
            .And.TranscodedBy<JsonEntityDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json")
            .And.TranscodedBy<CsvDotNetCodec>(null).ForMediaType("text/csv").ForExtension("csv");

        }//end AddVERTICAL_DATUM_Resources (44)

        #endregion

    }//End class Configuration
}//End namespace





