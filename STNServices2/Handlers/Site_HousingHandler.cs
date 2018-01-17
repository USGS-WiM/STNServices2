//------------------------------------------------------------------------------

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
// 03.28.16 - JKN - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class Site_HousingHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<site_housing> entities = null;

            try
            {               
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<site_housing>().OrderBy(e => e.site_housing_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            site_housing anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");              
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<site_housing>().FirstOrDefault(e => e.site_housing_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using            
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }            
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteSiteHousing")]
        public OperationResult SiteHousings(Int32 siteId)
        {
            List<site_housing> entities = null;
                        
            try
            {
                if (siteId <= 0) throw new BadRequestException("Invalid input parameters");        
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<site_housing>().Where(m => m.site_id == siteId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(site_housing anEntity)
        {
            //this is changed from previous version.. passed in siteId and site_housing object , then before adding, put the site_id prop on it.
            try
            {
                if (anEntity.site_id <= 0 || anEntity.housing_type_id <= 0  || anEntity.amount <= 0) 
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Add<site_housing>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, site_housing anEntity)
        {
            try
            {
                if (anEntity.site_id <= 0 || anEntity.housing_type_id <= 0 || anEntity.amount <= 0)
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        // last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        Int32 loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<site_housing>(entityId, anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Modified { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
        #region DeleteMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            site_housing anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<site_housing>().FirstOrDefault(i => i.site_housing_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<site_housing>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end horizontal_datumsHandler
}
