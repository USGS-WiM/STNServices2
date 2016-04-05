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

using WiM.Authentication;

namespace STNServices2.Handlers
{
    public class AgencyHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<agency> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<agency>().OrderBy(e => e.agency_id).ToList();
              
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
            agency entity = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entity = sa.Select<agency>().FirstOrDefault(e => e.agency_id == entityId);
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET
        [HttpOperation(ForUriName = "GetMemberAgency")]
        public OperationResult GetMemberAgency(Int32 memberId)
        {
            agency mAgency = null;

            //Return BadRequest if there is no ID
            if (memberId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    mAgency = sa.Select<member>().FirstOrDefault(i => i.member_id == memberId).agency;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = mAgency, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSourceAgency")]
        public OperationResult GetSourceAgency(Int32 sourceId)
        {
            agency mAgency = null;

            //Return BadRequest if there is no ID
            if (sourceId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    mAgency = sa.Select<source>().FirstOrDefault(i => i.source_id == sourceId).agency;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = mAgency };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(agency anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<agency>(anEntity);
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
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, agency anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<agency>(anEntity);
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
            agency anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<agency>().FirstOrDefault(i => i.agency_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<agency>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end AgencyHandler
}
