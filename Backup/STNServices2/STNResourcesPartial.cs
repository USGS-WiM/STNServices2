//------------------------------------------------------------------------------
//----- EntityObjectResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   EntityObject resources through the HTTP uniform interface.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//              EntityObjectResource contain the remaining entity object partial classes 
//              that where generated from the entity object model generator.
//              The partial classes were created in order to extend resource method to the 
//              generated objects
//
//              A partial class can implement more than one interface, 
//              but it cannot inherit from more than one base class. 
//              Therefore, all partial classes Inherits statements must specify the same base class.
//              http://msdn.microsoft.com/en-us/library/wa80x488.aspx
//              

#region Comments
// 08.02.12 - jkn - added method to add links to specified related objects
// 06.29.12 - jkn - added additional entities, changed base from EntityObject to ResourceBase to HypermediaEntity
// 06.28.12 - jkn - Created
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace STNServices2
{

    //  POST  /entities/{entityName}

    //  PUT   /entities/{entityName}/{entityId} 
    //  GET   /entities/{entityName}
    //        /entities/{entityName}/{entityId}

    public partial class AGENCY : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Agencies/{entityId}
                    uriString = string.Format("Agencies/{0}", this.AGENCY_ID);
                    break;

                case refType.POST:
                    uriString = "Agencies";
                    break;
                case refType.DELETE:
                    //Agencies/{entityId}
                    uriString = string.Format("Agencies/{0}", this.AGENCY_ID);
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Agencies/{entityId}/Sources
            this.LINKS.Add(GetLinkResource(baseURI, "Sources", refType.GET, "/Sources"));
            //Agencies/{entityId}/Members
            this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));
        }

        #endregion

    }//end Class AGENCY

    public partial class APPROVAL : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //Approvals/{ApprovalId}
                    uriString = string.Format("Approvals/{0}", this._APPROVAL_ID);
                    break;

                case refType.POST:
                    uriString = "Approvals";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Approvals/{ApprovalId}/ApprovingOfficial
            if (this.MEMBER_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Approving Official", refType.GET, "/ApprovingOfficial"));
            //Approvals/{ApprovalId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
            //Approvals/{ApprovalId}/DataFiles
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/DataFiles"));
        }

        #endregion

    }//end Class APPROVAL
    
    public partial class CONTACT : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Contacts/{entityId}
                    uriString = string.Format("Contacts/{0}", this.CONTACT_ID);
                    break;

                case refType.POST:
                    uriString = "Contacts";
                    break;
                case refType.DELETE:
                    //Contacts/{entityId}
                    uriString = string.Format("Contacts/{0}", this.CONTACT_ID);
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Contacts/{contactId}/ContactType
            this.LINKS.Add(GetLinkResource(baseURI, "Contact Type", refType.GET, "/ContactType"));

            ///Contacts?ReportMetric={reportMetricsId}
            //this.LINKS.Add(GetLinkResource(baseURI, "Report Metric Contacts", refType.GET, "/Sources"));

            ///Contacts?ReportMetric={reportMetricsId}&ContactType={contactTypeId}
            //this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));

            ////Contacts/{reportMetricsId}/removeReportContact
            //this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));

            ////Contacts/AddReportContact?contactTypeId={ContactTypeId}&reportId={ReportId}
            //this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));


        }

        #endregion

    }//end Class CONTACTS

    public partial class CONTACT_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //ContactTypes/{entityId}
                    uriString = string.Format("ContactTypes/{0}", this.CONTACT_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "ContactTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            ////ContactTypes/{entityId}/ContactType
            //this.LINKS.Add(GetLinkResource(baseURI, "Contact", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class CONTACT_TYPES

    public partial class DATA_FILE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //DataFiles/{dataFileId}
                    uriString = string.Format("DataFiles/{0}", this.DATA_FILE_ID);
                    break;

                case refType.POST:
                    uriString = "DataFiles";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //DataFiles/{dataFileId}/PeakSummary
            if (this.PEAK_SUMMARY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "PeakSummary", refType.GET, "/PeakSummary"));
            //DataFiles/{dataFileId}/Approval
            if (this.APPROVAL_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Approval", refType.GET, "/Approval"));
            //DataFiles/{dataFileId}/Processor
            if (this.PROCESSOR_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Processor", refType.GET, "/Processor"));
            //DataFiles/{dataFileId}/Instrument
            if (this.INSTRUMENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Instrument", refType.GET, "/Instrument"));
            //DataFiles/{dataFileId}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));

            //DataFiles/{dataFileId}/Approve
            //this.LINKS.Add(GetLinkResource(baseURI, "Approve Data File", refType.POST, "/Approve"));
            //DataFiles/{dataFileId}/Unapprove
            //this.LINKS.Add(GetLinkResource(baseURI, "Unapprove Data File", refType.POST, "/UnApprove"));
            ///DataFiles?IsApproved={boolean}
            ////DataFiles?IsApproved={approved}&eventId={eventId}&processorId={memberId}&state={state}
        }

        #endregion

    }//end Class DATA_FILE

    public partial class DEPLOYMENT_PRIORITY : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //StatusTypes/{statusTypeId}
                    uriString = string.Format("DeploymentPriorities/{0}", this.PRIORITY_ID);
                    break;

                case refType.POST:
                    uriString = "DeploymentPriorities";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
        }

        #endregion

    }//end Class DEPLOYMENT_PRIORITY

    public partial class DEPLOYMENT_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //DeploymentTypes/{entityId}
                    uriString = string.Format("DeploymentTypes/{0}", this.DEPLOYMENT_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "DeploymentTypes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //DeploymentTypes/{entityId}/Instruments
            this.LINKS.Add(GetLinkResource(baseURI, "Instrumemts", refType.GET, "/Instruments"));
        }

        #endregion

    }//end Class DEPLOYMENT_TYPE

    public partial class EVENT : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Events/{entityId}
                    uriString = string.Format("Events/{0}", this.EVENT_ID);
                    break;

                case refType.POST:
                    uriString = "Events";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Events/{eventId}/Status
            if (this.EVENT_STATUS_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Status", refType.GET, "/Status"));
            //Events/{eventId}/Type
            if (this.EVENT_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Type", refType.GET, "/Type"));
            //Events/{eventId}/EventCoordinator
            if (this.EVENT_COORDINATOR > 0) this.LINKS.Add(GetLinkResource(baseURI, "Event Coordinator", refType.GET, "/EventCoordinator"));
            //Events/{eventId}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
            //Events/{eventId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
            //Events/{eventId}/Instruments
            this.LINKS.Add(GetLinkResource(baseURI, "Instruments", refType.GET, "/Instruments"));
            //Events/{eventId}/Members
            this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));
            //Events/{eventId}/PeakSummaries
            this.LINKS.Add(GetLinkResource(baseURI, "Peam Summaries", refType.GET, "/PeakSummaries"));
            //Events/{eventId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
            //Events/{eventId}/Teams
            this.LINKS.Add(GetLinkResource(baseURI, "Instruments", refType.GET, "/Teams"));

        }

        #endregion

    }//end Class EVENT

    public partial class EVENT_STATUS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //entities/EVENT_STATUS{entityId}
                    uriString = string.Format("EventStatus/{0}", this.EVENT_STATUS_ID);
                    break;

                case refType.POST:
                    uriString = "EventStatus";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //EventStatus{entityId}/Events
            this.LINKS.Add(GetLinkResource(baseURI, "Events", refType.GET, "/Events"));
        }

        #endregion

    }//end Class EVENT_STATUS

    public partial class EVENT_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //EventTypes/{entityId}
                    uriString = string.Format("EventTypes/{0}", this.EVENT_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "EventTypes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //EventTypes/{entityId}/Events
            this.LINKS.Add(GetLinkResource(baseURI, "Events", refType.GET, "/Events"));
        }

        #endregion

    }//end Class EVENT_TYPE

    public partial class HORIZONTAL_DATUMS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HorizontalDatums/{entityId}
                    uriString = string.Format("HorizontalDatums/{0}", this.DATUM_ID);
                    break;

                case refType.POST:
                    uriString = "HorizontalDatums";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HorizontalDatums/{entityId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
        }

        #endregion

    }//end Class HORIZONTAL_DATUMS

    public partial class HWM_QUALITIES : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HWMQualities{entityId}
                    uriString = string.Format("HWMQualities/{0}", this.HWM_QUALITY_ID);
                    break;

                case refType.POST:
                    uriString = "HWMQualities";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HWMQualities{entityId}/hwms
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class HWM_QUALITIES

    public partial class HWM_TYPES : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HWMTypes/{entityId}
                    uriString = string.Format("HWMTypes/{0}", this.HWM_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "HWMTypes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HWMTypes/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class HWM_TYPES

    public partial class FILES : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Files/{entityId}
                    uriString = string.Format("Files/{0}", this.FILE_ID);
                    break;

                case refType.POST:
                    uriString = "Files";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Files/{fileId}/Source
            if (this.SOURCE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Source", refType.GET, "/Source"));
            //Files/{fileId}/HWM
            if (this.HWM_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "HWM", refType.GET, "/HWM"));
            //Files/{fileId}/Type
            if (this.FILETYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Type", refType.GET, "/Type"));
            //Files/{fileId}/Site
            if (this.SITE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Site", refType.GET, "/Site"));
            //Files/{fileId}/DataFile
            if (this.DATA_FILE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Data File", refType.GET, "/DataFile"));
            //Files/{fileId}/Instrument
            if (this.INSTRUMENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Instrument", refType.GET, "/Instrument"));
            //Files/{fileId}/Item
            this.LINKS.Add(GetLinkResource(baseURI, "Download Item", refType.GET, "/Item"));
        }

        #endregion

    }//end Class FILE

    public partial class FILE_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //entities/FILE_TYPE/{entityId}
                    uriString = string.Format("FileTypes/{0}", this.FILETYPE_ID);
                    break;

                case refType.POST:
                    uriString = "FileTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //FileTypes/{entityId}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
        }

        #endregion

    }//end Class FILE_TYPE

    public partial class HORIZONTAL_COLLECT_METHODS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HorizontalMethods/{entityId}
                    uriString = string.Format("/HorizontalMethods/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/HorizontalMethods";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            ///HorizontalMethods/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class HORIZONTAL_COLLECT_METHODS

    public partial class HORIZONTAL_DATUM : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HorizontalDatums/{entityId}
                    uriString = string.Format("/HorizontalDatums/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/HorizontalDatums";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            ///HorizontalDatums/{entityId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
        }

        #endregion

    }//end Class HORIZONTAL_DATUM

    public partial class HOUSING_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HousingTypes/{entityId}
                    uriString = string.Format("/HousingTypes/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/HousingTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class HOUSING_TYPES

    public partial class HWM_QUALITY : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HWMQualities/{entityId}
                    uriString = string.Format("/HWMQualities/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/HWMQualities";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HWMQualities/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class HWM_QUALITY

    public partial class HWM : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //HWMs/{entityId}
                    uriString = string.Format("HWMs/{0}", this.HWM_ID);
                    break;

                case refType.POST:
                    uriString = "HWMs";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HWMs/{hwmId}/Type
            if (this.HWM_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Type", refType.GET, "/Type"));
            //HWMs/{hwmId}/Marker
            if (this.MARKER_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Marker", refType.GET, "/Marker"));
            //HWMs/{hwmId}/PeakSummary
            if (this.PEAK_SUMMARY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "PeakSummary", refType.GET, "/PeakSummary"));
            //HWMs/{hwmId}/Site
            if (this.SITE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Site", refType.GET, "/Site"));
            //HWMs/{hwmId}/VerticalMethod
            if (this.VCOLLECT_METHOD_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Vertical Collection Method", refType.GET, "/VerticalMethod"));
            //HWMs/{hwmId}/vDatum
            if (this.VDATUM_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Vertical Datum", refType.GET, "/vDatum"));
            //HWMs/{hwmId}/Approval
            if (this.APPROVAL_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Approved", refType.GET, "/Approval"));
            //HWMs/{hwmId}/FlagTeam
            if (this.FLAG_MEMBER_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Flag Member", refType.GET, "/FlagMember"));
            //HWMs/{hwmId}/SurveyTeam
            if (this.SURVEY_MEMBER_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Survey Member", refType.GET, "/SurveyMember"));
            //HWMs/{hwmId}/Event
            if (this.EVENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Event", refType.GET, "/Event"));
            //HWMs/{hwmId}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
            //HWMs/{hwmId}/HorizontalMethod
            if (this.HCOLLECT_METHOD_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Horizontal Collection Method", refType.GET, "/HorizontalMethod"));
            //HWMs/{hwmId}/Quality
            if (this.HWM_QUALITY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Quality", refType.GET, "/Quality"));

        }

        #endregion

    }//end Class HWM 

    public partial class HWM_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //HWMTypes/{entityId}
                    uriString = string.Format("/HWMTypes/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/HWMTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //HWMTypes/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class HWM_TYPE

    public partial class INSTR_COLLECTION_CONDITIONS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //InstrCollectConditions/{entityId}
                    uriString = string.Format("/InstrCollectConditions/{0}", this);
                    break;

                case refType.POST:
                    uriString = "/InstrCollectConditions";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class INSTR_COLLECTION_CONDITIONS

    public partial class INSTRUMENT : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //Instruments/{entityId}
                    uriString = string.Format("Instruments/{0}", this.INSTRUMENT_ID);
                    break;

                case refType.POST:
                    uriString = "Instruments";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Instruments/{instrumentID}/DataFiles
            this.LINKS.Add(GetLinkResource(baseURI, "DataFiles", refType.GET, "/DataFiles"));
            //Instruments/{instrumentId}/DeploymentType
            if (this.DEPLOYMENT_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Deployment Type", refType.GET, "/DeploymentType"));
            //Instruments/{instrumentId}/Event
            if (this.EVENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Event", refType.GET, "/Event"));
            //Instruments/{instrumentID}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
            //Instruments/{instrumentID}/InstrumentHousingType
            this.LINKS.Add(GetLinkResource(baseURI, "Housing Type", refType.GET, "/InstrumentHousingType"));
            //Instruments/{instrumentID}/CollectCondition
            this.LINKS.Add(GetLinkResource(baseURI, "Collection Condition", refType.GET, "/CollectCondition"));
            //Instruments/{instrumentId}/InstrumentStatusLog"
            this.LINKS.Add(GetLinkResource(baseURI, "InstrumentStatusLog", refType.GET, "/InstrumentStatusLog"));
            //Instruments/{instrumentId}/InstrumentStatus"
            this.LINKS.Add(GetLinkResource(baseURI, "InstrumentStatus", refType.GET, "/InstrumentStatus"));
            //Instruments/{instrumentId}/SensorBrand
            if (this.SENSOR_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Sensor Brand", refType.GET, "/SensorBrand"));
            //Instruments/{instrumentId}/SensorType
            if (this.SENSOR_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Sensor Type", refType.GET, "/SensorType"));
            //Instruments/{instrumentId}/Site
            if (this.SITE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Site", refType.GET, "/Site"));

        }

        #endregion

    }//end Class INSTRUMENT

    public partial class INSTRUMENT_STATUS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //InstrumentStatus/{entityId}
                    uriString = string.Format("InstrumentStatus/{0}", this.INSTRUMENT_STATUS_ID);
                    break;

                case refType.POST:
                    uriString = "InstrumentStatus";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //InstrumentStatus/{instrumentStatusId}/InstrMeasurements
            if (this.INSTRUMENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Instrument Status OP Measurements", refType.GET, "/InstrMeasurements"));
            //InstrumentStatus/{instrumentStatusId}/Instrument
            if (this.INSTRUMENT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Instrument", refType.GET, "/Instrument"));            
            //InstrumentStatus/{instrumentStatusId}/Status
            if (this.STATUS_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Status", refType.GET, "/Status"));
        }

        #endregion

    }//end Class INSTRUMENT_STATUS

    public partial class LANDOWNERCONTACT : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //LandOwners/{entityId}
                    uriString = string.Format("LandOwners/{0}", this.LANDOWNERCONTACTID);
                    break;

                case refType.POST:
                    uriString = "LandOwners";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //LandOwners/{entityId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
        }

        #endregion

    }//end Class LANDOWNERCONTACT

    public partial class MARKER : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Markers/{entityId}
                    uriString = string.Format("Markers/{0}", this.MARKER_ID);
                    break;

                case refType.POST:
                    uriString = "Markers";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Markers/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class MARKER

    public partial class MEMBER : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    uriString = string.Format("Members/{0}", this.MEMBER_ID);
                    break;

                case refType.POST:
                    uriString = "Members";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Members/{memberId}/Role
            if (this.ROLE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Role", refType.GET, "/Role"));
            //Members/{memberId}/Teams
            this.LINKS.Add(GetLinkResource(baseURI, "Teams", refType.GET, "/Teams"));
            //Members/{memberId}/Agency
            if (this.AGENCY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Agency", refType.GET, "/Agency"));

        }

        #endregion

    }//end Class MEMBER

    public partial class NETWORK_NAME : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //NetworkNames/{entityId}
                    uriString = string.Format("NetworkNames/{0}", this._NETWORK_NAME_ID);
                    break;

                case refType.POST:
                    uriString = "NetworkNames";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //NetworkNames/{entityId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
        }

        #endregion

    }//end Class NETWORK_NAME

    public partial class NETWORK_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //NetworkTypes/{entityId}
                    uriString = string.Format("NetworkTypes/{0}", this._NETWORK_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "NetworkTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //NetworkTypes/{entityId}/Sites
            this.LINKS.Add(GetLinkResource(baseURI, "Sites", refType.GET, "/Sites"));
        }

        #endregion

    }//end Class NETWORK_TYPE

    public partial class OBJECTIVE_POINT : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //ObjectivePoints/{objectivePointId}
                    uriString = string.Format("ObjectivePoints/{0}", this._OBJECTIVE_POINT_ID);
                    break;

                case refType.POST:
                    uriString = "ObjectivePoints";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //ObjectivePoints/{objectivePointId}/OPType
            if (this.OP_TYPE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "OP Type", refType.GET, "/OPType"));
            //ObjectivePoints/{objectivePointId}/OPControls
            this.LINKS.Add(GetLinkResource(baseURI, "OP Controls", refType.GET, "/OPControls"));
            //ObjectivePoints/{objectivePointId}/vDatum
            if (this.VDATUM_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Vertical Datum", refType.GET, "/vDatum"));
            //ObjectivePoints/{referencePointId}/Site
            if (this.SITE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Site", refType.GET, "/Site"));
            //ObjectivePoints/{referencePointId}/Files
            if (this.SITE_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
        }

        #endregion

    }//end Class OBJECTIVE_POINT

    public partial class OP_CONTROL_IDENTIFIER : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //OPControlIdentifiers/{entityId}
                    uriString = string.Format("OPControlIdentifiers/{0}", this._OP_CONTROL_IDENTIFIER_ID);
                    break;

                case refType.POST:
                    uriString = "OPControlIdentifiers";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class OP_CONTROL_IDENTIFIER

    public partial class OP_QUALITY : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //ObjectivePointQualities/{entityId}
                    uriString = string.Format("ObjectivePointQualities/{0}", this._OP_QUALITY_ID);
                    break;

                case refType.POST:
                    uriString = "ObjectivePointQualities";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class OP_QUALITY

    public partial class PEAK_SUMMARY : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //PeakSummaries/{entityId}
                    uriString = string.Format("PeakSummaries/{0}", this.PEAK_SUMMARY_ID);
                    break;

                case refType.POST:
                    uriString = "PeakSummaries";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //PeakSummaries/{peakSummaryId}/Processor
            if (this.MEMBER_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Processor", refType.GET, "/Processor"));
            //PeakSummaries/{peakSummaryId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
            //PeakSummaries/{peakSummaryId}/DataFiles
            this.LINKS.Add(GetLinkResource(baseURI, "data files", refType.GET, "/DataFiles"));
        }

        #endregion

    }//end Class PEAK_SUMMARY

    public partial class REPORTING_METRICS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //ReportingMetrics/{entityId}
                    uriString = string.Format("ReportingMetrics/{0}", this.REPORTING_METRICS_ID);
                    break;

                case refType.POST:
                    uriString = "ReportingMetrics";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class REPORTING_METRICS

    public partial class ROLE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    uriString = string.Format("Roles/{0}", this.ROLE_ID);
                    break;

                case refType.POST:
                    uriString = "Roles";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Roles/{roleId}/Members
            this.LINKS.Add(GetLinkResource(baseURI, "Members", refType.GET, "/Members"));
        }

        #endregion

    }//end Class ROLE

    public partial class OP_MEASUREMENTS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    uriString = string.Format("OPMeasurements/{0}", this.OP_MEASUREMENTS_ID);
                    break;

                case refType.POST:
                    uriString = "OPMeasurements";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {

        }

        #endregion

    }//end Class OP_MEASURES

    public partial class SENSOR_BRAND : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //SensorBrands/{entityId}
                    uriString = string.Format("SensorBrands/{0}", this.SENSOR_BRAND_ID);
                    break;

                case refType.POST:
                    uriString = "SensorBrands";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //SensorBrands/{entityId}/Instruments
            this.LINKS.Add(GetLinkResource(baseURI, "Instrumemts", refType.GET, "/Instruments"));
        }

        #endregion

    }//end Class SENSOR_BRAND

    public partial class SENSOR_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //SensorTypes/{entityId}
                    uriString = string.Format("SensorTypes/{0}", this.SENSOR_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "SensorTypes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //SensorTypes/{entityId}/Instruments
            this.LINKS.Add(GetLinkResource(baseURI, "Instrumemts", refType.GET, "/Instruments"));
        }

        #endregion

    }//end Class SENSOR_TYPE

    public partial class SITE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //Sites/{siteId}
                    uriString = string.Format("Sites/{0}", this.SITE_ID);
                    break;

                case refType.POST:
                    uriString = "Sites";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Sites/{siteId}/DeploymentPriorities
            if (this.PRIORITY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "DeploymentPriorities", refType.GET, "/DeploymentPriorities"));
            //Sites/{siteID}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
            //Sites/{siteId}/hDatum
            if (this.HDATUM_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Horizontal Datum", refType.GET, "/hDatum"));
            //Sites/{siteID}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
            //Sites/{siteID}/Instruments
            this.LINKS.Add(GetLinkResource(baseURI, "Instruments", refType.GET, "/Instruments"));
            //Sites/{siteId}/LandOwner
            if (this.LANDOWNERCONTACT_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "Land Owner", refType.GET, "/LandOwner"));
            //sites/{siteId}/networkNames"
            this.LINKS.Add(GetLinkResource(baseURI, "NetworkName", refType.GET, "/networkNames"));
            //sites/{siteId}/PeakSummaries"
            this.LINKS.Add(GetLinkResource(baseURI, "PeakSummaries", refType.GET, "/PeakSummaries"));
            //sites/{siteId}/networkTypes"
            this.LINKS.Add(GetLinkResource(baseURI, "networkTypes", refType.GET, "/networkTypes"));
            //Sites/{siteID}/ObjectivePoints
            this.LINKS.Add(GetLinkResource(baseURI, "ObjectivePoints", refType.GET, "/ObjectivePoints"));
            //sites/{siteId}/SiteHousings"
            this.LINKS.Add(GetLinkResource(baseURI, "SiteHousings", refType.GET, "/SiteHousings"));
        }

        #endregion

    }//end Class SITE

    public partial class SOURCE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //Sources/{entityId}
                    uriString = string.Format("Sources/{0}", this.SOURCE_ID);
                    break;

                case refType.POST:
                    uriString = "Sources";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //Sources/{entityId}/Agency
            if (this.AGENCY_ID > 0) this.LINKS.Add(GetLinkResource(baseURI, "agency", refType.GET, "/Agency"));
            //Sources/{entityId}/Files
            this.LINKS.Add(GetLinkResource(baseURI, "Files", refType.GET, "/Files"));
        }

        #endregion

    }//end Class SOURCE

    public partial class STATUS_TYPE : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //StatusTypes/{statusTypeId}
                    uriString = string.Format("StatusTypes/{0}", this.STATUS_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "StatusTypes";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
        }

        #endregion

    }//end Class STATUS_TYPE

    public partial class VERTICAL_COLLECT_METHODS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //CollectionMethods/{collectionMethodId}
                    uriString = string.Format("VerticalMethods/{0}", this.VCOLLECT_METHOD_ID);
                    break;

                case refType.POST:
                    uriString = "VerticalMethods";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //CollectionMethods/{collectionMethodId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "HWMs", refType.GET, "/HWMs"));
        }

        #endregion

    }//end Class COLLECT_METHODS

    public partial class VERTICAL_DATUMS : HypermediaEntity
    {
        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    //VerticalDatums{entityId}
                    uriString = string.Format("VerticalDatums/{0}", this.DATUM_ID);
                    break;

                case refType.POST:
                    //VerticalDatums
                    uriString = "VerticalDatums";
                    break;
            }
            return uriString;
        }
        protected override void addRelatedLinks(string baseURI)
        {
            //VerticalDatums/{entityId}/HWMs
            this.LINKS.Add(GetLinkResource(baseURI, "hwms", refType.GET, "/HWMs"));
            //VerticalDatums/{entityId}/ObjPoints
            this.LINKS.Add(GetLinkResource(baseURI, "Objective Points", refType.GET, "/OPs"));
        }

        #endregion

    }//end Class VERTICAL_DATUMS

}//end namespace