//------------------------------------------------------------------------------
//----- ContactHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Contact resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.`````````
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 11.20.14 - TR -Created

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
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;


namespace STNServices2.Handlers
{
    public class ContactHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "CONTACT"; }
        }
        #endregion
        #region GetMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<CONTACT> contactList = new List<CONTACT>();
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        contactList = aSTNE.CONTACT.OrderBy(cl => cl.LNAME).ToList();

                        if (contactList != null)
                            contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }

                return new OperationResult.OK { ResponseResource = contactList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            CONTACT aContact;

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
                        aContact = aSTNE.CONTACT.SingleOrDefault(rp => rp.CONTACT_ID == entityId);

                        if (aContact != null)
                            aContact.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }//end using
                }

                return new OperationResult.OK { ResponseResource = aContact };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(ForUriName = "GetReportMetricContactsByType")]
        public OperationResult GetReportMetricContactsByType(Int32 reportMetricsId, Int32 contactTypeId)
        {
            CONTACT aContacts = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0 || contactTypeId <= 0)
                {
                    return new OperationResult.BadRequest();
                }
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //first get the reportmetric_contact with this reportID and this contactTypeId
                        var rcontact = aSTNE.REPORTMETRIC_CONTACT.SingleOrDefault(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId && rmc.CONTACT_TYPE_ID == contactTypeId);
                        aContacts = rcontact != null ? rcontact.CONTACT : null;
                        //aContacts = aSTNE.REPORTMETRIC_CONTACT.SingleOrDefault(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId && rmc.CONTACT_TYPE_ID == contactTypeId).CONTACT;

                        if (aContacts != null)
                            aContacts.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aContacts };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportMetricContacts")]
        public OperationResult GetReportMetricContactsByType(Int32 reportMetricsId)
        {
            List<CONTACT> reportContacts = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0)
                {
                    return new OperationResult.BadRequest();
                }
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        //first get the Contacts that are associated with this report
                        reportContacts = aSTNE.CONTACT.Where(c => c.REPORTMETRIC_CONTACT.Any(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId)).ToList();

                        if (reportContacts != null)
                            reportContacts.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }
                return new OperationResult.OK { ResponseResource = reportContacts };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetContactModelByReport")]
        public OperationResult GetContactModelByReport(Int32 reportMetricsId)
        {
            List<ReportContactModel> reportContacts = null;
            List<CONTACT> contactList = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0)
                {
                    return new OperationResult.BadRequest();
                }
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        //first get the Contacts that are associated with this report
                        contactList = aSTNE.CONTACT.Where(c => c.REPORTMETRIC_CONTACT.Any(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId)).ToList();

                        reportContacts = aSTNE.REPORTMETRIC_CONTACT.Where(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId).Select(c => new ReportContactModel
                        {
                            ContactId = c.CONTACT_ID,
                            FNAME = c.CONTACT.FNAME,
                            LNAME = c.CONTACT.LNAME,
                            PHONE = c.CONTACT.PHONE,
                            ALT_PHONE = c.CONTACT.ALT_PHONE,
                            EMAIL = c.CONTACT.EMAIL,
                            TYPE = c.CONTACT_TYPE.TYPE
                        }).ToList();



                        if (contactList != null)
                            contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end using
                }
                return new OperationResult.OK { ResponseResource = reportContacts };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET


        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(ForUriName = "GetContactTypeContact")]
        public OperationResult GetContactTypeContacts(Int32 contactTypeId)
        {
            List<REPORTMETRIC_CONTACT> repMetContacts = new List<REPORTMETRIC_CONTACT>();
            List<CONTACT> contactList = new List<CONTACT>();

            //Get basic authentication password
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {

                        repMetContacts = aSTNE.CONTACT_TYPE.FirstOrDefault(ct => ct.CONTACT_TYPE_ID == contactTypeId).REPORTMETRIC_CONTACT.ToList();

                        if (repMetContacts != null)
                        {
                            foreach (REPORTMETRIC_CONTACT rmc in repMetContacts)
                            {
                                CONTACT aCont = aSTNE.CONTACT.FirstOrDefault(c => c.CONTACT_ID == rmc.CONTACT_ID);
                                contactList.Add(aCont);
                            }
                        }

                        if (contactList != null)
                            contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                    }//end using
                }

                return new OperationResult.OK { ResponseResource = contactList };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(CONTACT anEntity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Post(anEntity) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddReportContact")]
        public OperationResult AddReportContact(Int32 contactTypeId, Int32 reportId, CONTACT aContact)
        {
            REPORTMETRIC_CONTACT aReportMetricContact = null;
            CONTACT thisContact = null;
            //Return BadRequest if missing required fields
            if (contactTypeId <= 0 || String.IsNullOrEmpty(aContact.FNAME))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid Contact Type
                        if (aSTNE.CONTACT_TYPE.First(s => s.CONTACT_TYPE_ID == contactTypeId) == null)
                            return new OperationResult.BadRequest { Description = "Contact Type does not exists" };

                        if (!Exists(ref aContact))
                        {
                            //set ID

                            aSTNE.CONTACT.AddObject(aContact);
                            aSTNE.SaveChanges();
                        }//end if

                        //add to ReportMetrics_Contact
                        //first check if reportmetric_contact already contains this contact
                        if (aSTNE.REPORTMETRIC_CONTACT.FirstOrDefault(nt => nt.CONTACT_ID == aContact.CONTACT_ID &&
                                                                            nt.REPORTING_METRICS_ID == reportId) == null)
                        {//create one
                            aReportMetricContact = new REPORTMETRIC_CONTACT();
                            aReportMetricContact.REPORTING_METRICS_ID = reportId;
                            aReportMetricContact.CONTACT_ID = aContact.CONTACT_ID;
                            aReportMetricContact.CONTACT_TYPE_ID = contactTypeId;
                            aSTNE.REPORTMETRIC_CONTACT.AddObject(aReportMetricContact);
                            aSTNE.SaveChanges();
                        }//end if

                        //return Contact
                        thisContact = aSTNE.CONTACT.FirstOrDefault(nt => nt.CONTACT_ID == aContact.CONTACT_ID);

                        if (thisContact != null)
                            thisContact.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }//end using
                }//end using
                //return object to verify persistance
                return new OperationResult.OK { ResponseResource = thisContact };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }
        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, CONTACT entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion

        #region Delete
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveReportContact")]
        public OperationResult RemoveReportContact(Int32 reportMetricsId, CONTACT aContact)
        {
            //Return BadRequest if missing required fields
            if (reportMetricsId <= 0 || String.IsNullOrEmpty(aContact.FNAME))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //check if valid site
                        if (aSTNE.REPORTING_METRICS.First(s => s.REPORTING_METRICS_ID == reportMetricsId) == null)
                            return new OperationResult.BadRequest { Description = "Report does not exist" };

                        //remove from Reporting_Contact
                        REPORTMETRIC_CONTACT thisRMC = aSTNE.REPORTMETRIC_CONTACT.FirstOrDefault(nts => nts.CONTACT_ID == aContact.CONTACT_ID &&
                                                                                             nts.REPORTING_METRICS_ID == reportMetricsId);

                        if (thisRMC != null)
                        {
                            //remove it
                            aSTNE.REPORTMETRIC_CONTACT.DeleteObject(thisRMC);
                            aSTNE.SaveChanges();
                        }//end if
                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }
        #endregion Delete

        #region Helper Methods
        private bool Exists(ref CONTACT anEntity)
        {
            CONTACT existingEntity;
            CONTACT thisEntity = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingEntity = aSTNE.CONTACT.FirstOrDefault(c => string.Equals(c.FNAME.ToUpper(), thisEntity.FNAME.ToUpper()) &&
                                                                            (string.Equals(c.LNAME.ToUpper(), thisEntity.LNAME.ToUpper()) || string.IsNullOrEmpty(thisEntity.LNAME)) &&
                                                                            (string.Equals(c.EMAIL.ToUpper(), thisEntity.EMAIL.ToUpper()) || string.IsNullOrEmpty(thisEntity.EMAIL)) &&
                                                                            (c.PHONE == thisEntity.PHONE || string.IsNullOrEmpty(thisEntity.PHONE)) &&
                                                                            (c.ALT_PHONE == thisEntity.ALT_PHONE || string.IsNullOrEmpty(thisEntity.ALT_PHONE)));

                }
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
    }//end ContactHandler
}//end namespace