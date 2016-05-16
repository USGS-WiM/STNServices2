//------------------------------------------------------------------------------
//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

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
using WiM.Security;



namespace STNServices2.Handlers
{
    public class DataFileHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<data_file> entities = null;

            try
            {
                 using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        entities = sa.Select<data_file>().OrderBy(df => df.data_file_id).Where(df => df.approval_id > 0).ToList();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                    else
                    {
                        //they are logged in, give them all
                        entities = sa.Select<data_file>().OrderBy(df => df.data_file_id).ToList();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                }
                 return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get
 
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            data_file anEntity;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    //general public..only approved ones
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        anEntity = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId && df.approval_id > 0);
                        if (anEntity == null) throw new NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    else
                    {
                        anEntity = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId);
                        if (anEntity == null) throw new NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileDataFile")]
        public OperationResult GetFileDataFile(Int32 fileId)
        {
            data_file anEntity = null;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent(true))
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //general public..only approved ones
                        anEntity = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).data_file;
                        anEntity = anEntity.approval_id > 0 ? anEntity : null;
                        if (anEntity == null) throw new NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    else
                    {
                        //they are logged in, give them all
                        anEntity = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).data_file;
                        if (anEntity == null) throw new NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    sm(sa.Messages);

                }//end using   

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedDataFiles")]
        public OperationResult GetApprovedDataFiles(Int32 ApprovalId)
        {
            List<data_file> entities = null;

            try
            {
                if (ApprovalId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<data_file>().Where(d => d.approval_id == ApprovalId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                //this will always be one because each datafile is approved individually
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredDataFiles")]
        public OperationResult GetFilteredDataFiles(string approved, [Optional] Int32 eventId, [Optional] Int32 memberId, [Optional] string state)
        {
            List<data_file> entities;
            try
            {
                if (string.IsNullOrEmpty(approved)) throw new BadRequestException("Invalid input parameters");
               
                //set defaults
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();
                Int32 filterMember = (memberId > 0) ? memberId : -1;
                Int32 filterEvent = (eventId > 0) ? eventId : -1;

                using (STNAgent sa = new STNAgent(true))
                {
                    IQueryable<data_file> query;
                    if (isApprovedStatus)
                        query = sa.Select<data_file>().Where(h => h.approval_id > 0);
                    else
                        query = sa.Select<data_file>().Where(h => h.approval_id <= 0 || !h.approval_id.HasValue);

                    if (filterEvent > 0)
                        query = query.Where(d => d.instrument.event_id == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(d => d.instrument.site.state == filterState);

                    if (filterMember > 0)
                        query = query.Where(d => d.processor_id == filterMember);


                    entities = query.ToList();

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        entities = entities.Where(d => d.approval_id > 0).ToList();
                    }
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch( Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentDataFiles")]
        public OperationResult GetInstrumentDataFiles(Int32 instrumentId)
        {
            List<data_file> entities = null;
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //general public..only approved ones
                        entities = sa.Select<data_file>().AsEnumerable().Where(df => df.instrument_id == instrumentId && df.approval_id > 0).OrderBy(df => df.data_file_id).ToList<data_file>();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                    else
                    {
                        entities = sa.Select<data_file>().AsEnumerable().Where(df => df.instrument_id == instrumentId).OrderBy(df => df.data_file_id).ToList<data_file>();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryDatafiles")]
        public OperationResult GetPeakSummaryDatafiles(Int32 peakSummaryId)
        {
            List<data_file> entities = null;
            try
            {
                //Return BadRequest if there is no ID
                if (peakSummaryId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        entities = sa.Select<data_file>().AsEnumerable().Where(df => df.peak_summary_id == peakSummaryId && df.approval_id > 0).OrderBy(df => df.data_file_id).ToList<data_file>();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                    else
                    {
                        //they are logged in, give them all
                        entities = sa.Select<data_file>().AsEnumerable().Where(df => df.peak_summary_id == peakSummaryId).OrderBy(df => df.data_file_id).ToList<data_file>();
                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(data_file anEntity)
        {
            try
            {
                if (!anEntity.good_start.HasValue || !anEntity.good_end.HasValue || anEntity.collect_date.HasValue || 
                    (!anEntity.instrument_id.HasValue || anEntity.instrument_id <= 0) || (!anEntity.processor_id.HasValue || anEntity.processor_id <= 0))
                    throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username,securedPassword))
                    {
                        sa.Add<data_file>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key         
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, data_file anEntity)
        {            
            try
            {
                if (entityId <= 0 || !anEntity.good_start.HasValue || !anEntity.good_end.HasValue || anEntity.collect_date.HasValue ||
                    (!anEntity.instrument_id.HasValue || anEntity.instrument_id <= 0) || (!anEntity.processor_id.HasValue || anEntity.processor_id <= 0)) 
                    throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<data_file>(entityId, anEntity);
                        sm(sa.Messages);                       

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex);}
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
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        data_file ObjectToBeDeleted = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId);
                        //delete it
                        sa.Delete(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

    }//end class DataFileHandler
}//end namespace