﻿//------------------------------------------------------------------------------

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

using WiM.Authentication;

namespace STNServices2.Handlers
{
    public class CountyHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<county> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<county>().OrderBy(e => e.county_id).ToList();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            county anentity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (STNAgent sa = new STNAgent())
                {
                    anentity = sa.Select<county>().SingleOrDefault(rp => rp.county_id == entityId);
                    sm(sa.Messages);
                }//end using


                return new OperationResult.OK { ResponseResource = anentity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetStateCountiesById")]
        public OperationResult GetStateCountiesById(Int32 stateId)
        {
            List<county> stateCountyList = null;
            try
            {
                if (stateId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    stateCountyList = sa.Select<state>().FirstOrDefault(i => i.state_id == stateId).counties.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = stateCountyList, Description=MessageString };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetStateCountiesByAbbrev")]
        public OperationResult GetStateCountiesByAbbrev(string stateAbbrev)
        {
            List<county> stateCountyList = null;
            try
            {
                //Return BadRequest if there is no ID
                if (string.IsNullOrEmpty(stateAbbrev)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    stateCountyList = sa.Select<state>().FirstOrDefault(i => i.state_abbrev == stateAbbrev).counties.ToList();
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = stateCountyList, Description=MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetStateSiteCounties")]
        public OperationResult GetStateSiteCounties(string stateAbbrev)
        {
            List<county> SiteCounties;
            state thisState;
            try
            {
                if (stateAbbrev == null) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    thisState = sa.Select<state>().FirstOrDefault(st => st.state_abbrev == stateAbbrev);
                    List<site> allSitesInThisState = sa.Select<site>().Where(p => p.state == stateAbbrev).ToList();
                    List<string> uniqueCoList = allSitesInThisState.GroupBy(p => p.county).Select(g => g.FirstOrDefault()).Select(y => y.county).ToList();

                    SiteCounties = sa.Select<county>().Where(co => uniqueCoList.Contains(co.county_name) && co.state_id == thisState.state_id).ToList();
                }//end using            
                sm(MessageType.info, "Count:" + SiteCounties.Count);
                return new OperationResult.OK { ResponseResource = SiteCounties, Description=MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(county anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<county>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.POST
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, county anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<county>(anEntity);
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
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            county anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<county>().FirstOrDefault(i => i.county_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<county>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.Delete

        #endregion
    }//end countyHandler
}