//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 03.23.16 - JKN - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using STNServices2.Security;
using WiM.Authentication;

namespace STNServices2.Handlers
{
    public class ContactHandler : STNHandlerBase
    {
        #region GetMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<contact> contactList = new List<contact>();
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        contactList = sa.Select<contact>().OrderBy(cl => cl.lname).ToList();
                        sm(sa.Messages);
                    }//end using
                }
                return new OperationResult.OK { ResponseResource = contactList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            contact aContact;            
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        aContact = sa.Select<contact>().SingleOrDefault(rp => rp.contact_id == entityId);
                        sm(sa.Messages);
                    }//end using
                }

                return new OperationResult.OK { ResponseResource = aContact, Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(ForUriName = "GetReportMetricContactsByType")]
        public OperationResult GetReportMetricContactsByType(Int32 reportMetricsId, Int32 contactTypeId)
        {
            contact aContacts = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0 || contactTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        var rcontact = sa.Select<reportmetric_contact>().FirstOrDefault(rmc => rmc.reporting_metrics_id == reportMetricsId && rmc.contact_id == contactTypeId);
                        aContacts = rcontact != null ? rcontact.contact : null;
                     
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
            List<contact> reportContacts = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0) throw new BadRequestException("Invalid input parameters");
                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //first get the Contacts that are associated with this report
                        reportContacts = sa.Select<contact>().Where(c => c.reportmetric_contact.Any(rmc => rmc.reporting_metrics_id == reportMetricsId)).ToList();
                        sm(sa.Messages);
                    }//end using
                }
                return new OperationResult.OK { ResponseResource = reportContacts, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetContactModelByReport")]
        public OperationResult GetContactModelByReport(Int32 reportMetricsId)
        {
            //List<ReportContactModel> reportContacts = null;
            //List<contact> contactList = null;
            //try
            //{
            //    //Return BadRequest if there is no ID
            //    if (reportMetricsId <= 0)
            //    {
            //        return new OperationResult.BadRequest();
            //    }
            //    //Get basic authentication password
            //    using (EasySecureString securedPassword = GetSecuredPassword())
            //    {
            //        using (STNEntities2 aSTNE = GetRDS(securedPassword))
            //        {

            //            //first get the Contacts that are associated with this report
            //            contactList = aSTNE.CONTACT.Where(c => c.REPORTMETRIC_CONTACT.Any(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId)).ToList();

            //            reportContacts = aSTNE.REPORTMETRIC_CONTACT.Where(rmc => rmc.REPORTING_METRICS_ID == reportMetricsId).Select(c => new ReportContactModel
            //            {
            //                ContactId = c.CONTACT_ID,
            //                FNAME = c.CONTACT.FNAME,
            //                LNAME = c.CONTACT.LNAME,
            //                PHONE = c.CONTACT.PHONE,
            //                ALT_PHONE = c.CONTACT.ALT_PHONE,
            //                EMAIL = c.CONTACT.EMAIL,
            //                TYPE = c.CONTACT_TYPE.TYPE
            //            }).ToList();



            //            if (contactList != null)
            //                contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            //        }//end using
            //    }
            //    return new OperationResult.OK { ResponseResource = reportContacts };
            //}
            //catch (Exception)
            //{
            //    return new OperationResult.BadRequest();
            //}
            return new OperationResult.SeeOther { Description= "Not implemented yet." };
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetContactTypeContact")]
        public OperationResult GetContactTypeContacts(Int32 contactTypeId)
        {
            List<contact> contactList = new List<contact>();

            //Get basic authentication password
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        contactList = sa.Select<contact_type>().FirstOrDefault(ct => ct.contact_type_id == contactTypeId)
                                                                .reportmetric_contact.Select(c=>c.contact).ToList();
                        sm(sa.Messages);
                    }//end using
                }

                return new OperationResult.OK { ResponseResource = contactList, Description = this.MessageString};
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET
        #endregion
        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(contact anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.fname) || string.IsNullOrEmpty(anEntity.lname) || string.IsNullOrEmpty(anEntity.phone))
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<contact>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using

                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddReportContact")]
        public OperationResult AddReportContact(Int32 contactTypeId, Int32 reportId, contact aContact)
        {
            contact thisContact = null;
            try
            {
                if (contactTypeId <= 0 || reportId <= 0 || String.IsNullOrEmpty(aContact.fname)) { throw new BadRequestException("Invalid input parameters"); }
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //check if valid Contact Type
                        if (sa.Select<contact_type>().First(s => s.contact_type_id == contactTypeId) == null) throw new BadRequestException("Contact Type does not exists" );

                        thisContact = sa.Add<contact>(aContact);
                       
                        //add ReportMetrics_Contact if not already there.
                        if(sa.Select<reportmetric_contact>().FirstOrDefault(nt => nt.contact_id == aContact.contact_id &&
                                                                            nt.reporting_metrics_id == reportId) == null)
                        {
                            sa.Add<reportmetric_contact>(new reportmetric_contact() { reporting_metrics_id = reportId, 
                                                                                      contact_id = aContact.contact_id,
                                                                                      contact_type_id = contactTypeId });
                        }//end if
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = thisContact, Description=MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, contact anEntity)
        {
            try
            {
                if (entityId <=0 || string.IsNullOrEmpty(anEntity.fname) || string.IsNullOrEmpty(anEntity.lname) || 
                    string.IsNullOrEmpty(anEntity.phone))
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<contact>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Modified { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.PUT

        #endregion
        #region Delete

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            contact anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<contact>().FirstOrDefault(i => i.contact_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<contact>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveReportContact")]
        public OperationResult RemoveReportContact(Int32 reportMetricsId, contact aContact)
        {
            reportmetric_contact thisRMC = null;
            try
            {
                if (reportMetricsId <= 0 || String.IsNullOrEmpty(aContact.fname)) throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //check if valid site
                        if (sa.Select<reporting_metrics>().FirstOrDefault(s => s.reporting_metrics_id == reportMetricsId) == null) throw new BadRequestException("Report does not exist");

                        //remove from Reporting_Contact
                        thisRMC = sa.Select<reportmetric_contact>().FirstOrDefault(nts => nts.contact_id == aContact.contact_id &&
                                                                                             nts.reporting_metrics_id == reportMetricsId);
                        if (thisRMC != null)
                        {
                            //remove it
                            sa.Delete<reportmetric_contact>(thisRMC);
                        }//end if
                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }
        #endregion Delete

    }//end ContactHandler
}//end namespace