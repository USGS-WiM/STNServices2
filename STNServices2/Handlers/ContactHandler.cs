﻿//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
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
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using STNServices2.Security;
using STNServices2.Resources;
using WiM.Security;

namespace STNServices2.Handlers
{
    public class ContactHandler : STNHandlerBase
    {
        #region GetMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<contact> entities = new List<contact>();
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        entities = sa.Select<contact>().OrderBy(cl => cl.lname).ToList();
                        sm(sa.Messages);
                    }//end using
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            contact anEntity;            
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<contact>().SingleOrDefault(rp => rp.contact_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sm(sa.Messages);
                    }//end using
                }

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportMetricContactsByType")]
        public OperationResult GetReportMetricContactsByType(Int32 reportMetricsId, Int32 contactTypeId)
        {
            contact anEntity = null;
            try
            {
                //Return BadRequest if there is no ID
                if (reportMetricsId <= 0 || contactTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        var rcontact = sa.Select<reportmetric_contact>().Include(rmc=> rmc.contact).FirstOrDefault(rmc => rmc.reporting_metrics_id == reportMetricsId && rmc.contact_type_id == contactTypeId);
                        anEntity = rcontact != null ? rcontact.contact : null;
                        if (anEntity == null) throw new NotFoundRequestException(); 
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetReportMetricContacts")]
        public OperationResult GetReportMetricContacts(Int32 reportMetricsId)
        {
            List<contact> entities = null;
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
                        entities = sa.Select<reportmetric_contact>().Include(b => b.contact).Include(c => c.contact_type).Where(rmc => rmc.reporting_metrics_id == reportMetricsId)
                        .Select(c => new Contact()
                        {
                            contact_id = c.contact_id,
                            fname = c.contact.fname,
                            lname = c.contact.lname,
                            phone = c.contact.phone,
                            alt_phone = c.contact.alt_phone,
                            email = c.contact.email,
                            contactType = c.contact_type.type
                        }).ToList<contact>();

                        sm(sa.Messages);
                    }//end using
                }
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddReportContact")]
        public OperationResult AddReportContact(contact anEntity, Int32 contactTypeId, Int32 reportId)
        {
            reportmetric_contact newRepContact = null;
            contact thisContact = null;
            Int32 loggedInUserId = 0;
            try
            {
                if (contactTypeId <= 0 || reportId <= 0 || string.IsNullOrEmpty(anEntity.fname) || 
                    string.IsNullOrEmpty(anEntity.lname) || string.IsNullOrEmpty(anEntity.phone)) 
                { throw new BadRequestException("Invalid input parameters"); }
                
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        
                        //check if valid Contact Type
                        if (sa.Select<contact_type>().First(s => s.contact_type_id == contactTypeId) == null) throw new NotFoundRequestException();                        
                        if (sa.Select<reporting_metrics>().First(rm => rm.reporting_metrics_id == reportId) == null) throw new NotFoundRequestException();
                        // last_updated parts
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;
                        //save the contact (if already exists, will return it
                        thisContact = sa.Add<contact>(anEntity);

                        //add ReportMetrics_Contact if not already there.
                        if (sa.Select<reportmetric_contact>().FirstOrDefault(rt =>
                            rt.contact_id == thisContact.contact_id && rt.reporting_metrics_id == reportId && rt.contact_type_id == contactTypeId) == null)
                        {
                            newRepContact = new reportmetric_contact();
                            newRepContact.reporting_metrics_id = reportId;
                            newRepContact.contact_type_id = contactTypeId;
                            newRepContact.contact_id = thisContact.contact_id;
                            // last_updated parts
                            newRepContact.last_updated = DateTime.Now;
                            newRepContact.last_updated_by = loggedInUserId;
                            newRepContact = sa.Add<reportmetric_contact>(newRepContact);
                            sm(sa.Messages);
                        }//end if
                        
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = thisContact, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        #endregion
     
        #region PutMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, contact anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            {
                if (entityId <=0 || string.IsNullOrEmpty(anEntity.fname) || string.IsNullOrEmpty(anEntity.lname) || string.IsNullOrEmpty(anEntity.phone))
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last_updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<contact>(entityId, anEntity);
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
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveReportContact")]
        public OperationResult RemoveReportContact(Int32 contactId, Int32 reportMetricsId)
        {            
            try
            {
                if (contactId <= 0 || reportMetricsId <= 0) throw new BadRequestException("Invalid input parameters");
               
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //check if valid site
                        if (sa.Select<reporting_metrics>().First(r => r.reporting_metrics_id == reportMetricsId) == null) throw new NotFoundRequestException();

                        //remove from Reporting_Contact
                        reportmetric_contact ObjectToBeDeleted = sa.Select<reportmetric_contact>().SingleOrDefault(rmc => rmc.contact_id == contactId && rmc.reporting_metrics_id == reportMetricsId);
                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException();
                        sa.Delete<reportmetric_contact>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        #endregion Delete

    }//end ContactHandler
}//end namespace