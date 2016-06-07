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
//using OpenRasta.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNServices2.Security;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class SourceHandler : STNHandlerBase
    {
        #region GetMethods
        [STNRequiresRole(new string[] {AdminRole, ManagerRole, FieldRole})]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<source> entities = null;

            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        entities = sa.Select<source>().OrderBy(e => e.source_id).ToList();

                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
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
            source anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<source>().FirstOrDefault(e => e.source_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using
                
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAgencySources")]
        public OperationResult GetAgencySources(Int32 agencyId)
        {
            List<source> entities = null;
            
            try
            {
                if (agencyId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword, true))
                    {
                        entities = sa.Select<agency>().FirstOrDefault(i => i.agency_id == agencyId).sources.ToList();
                        sm(MessageType.info, "Count: " + entities.Count()); 
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileSource")]
        public OperationResult GetFileSource(Int32 fileId)
        {
            source anEntity = null;
            
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<file>().Include(i=>i.source).FirstOrDefault(i => i.file_id == fileId).source;
                        if (anEntity == null) throw new NotFoundRequestException(); 
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
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
        public OperationResult POST(source anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.source_name) || anEntity.agency_id <= 0)
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<source>(anEntity);
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
        public OperationResult Put(Int32 entityId, source anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.source_name) || anEntity.agency_id <= 0)
                    throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<source>(entityId, anEntity);
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
        /// No deleting of sources - shared by other Files 
        ///
        //[RequiresRole(AdminRole)]
        //[HttpOperation(HttpMethod.DELETE)]
        //public OperationResult Delete(Int32 entityId)
        //{
        //    source anEntity = null;
        //    try
        //    {
        //        if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
        //        using (EasySecureString securedPassword = GetSecuredPassword())
        //        {
        //            using (STNAgent sa = new STNAgent(username, securedPassword))
        //            {
        //                anEntity = sa.Select<source>().FirstOrDefault(i => i.source_id == entityId);
        //                if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

        //                sa.Delete<source>(anEntity);
        //                sm(sa.Messages);
        //            }//end using
        //        }//end using
        //        return new OperationResult.OK { Description = this.MessageString };
        //    }
        //    catch (Exception ex)
        //    { return HandleException(ex); }

        //}//end HttpMethod.PUT

        #endregion
    }//end horizontal_datumsHandler
}
