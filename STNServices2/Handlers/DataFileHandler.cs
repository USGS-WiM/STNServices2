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
using WiM.Authentication;



namespace STNServices2.Handlers
{
    public class DataFileHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<data_file> dataFiles = null;

            try
            {
                 using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFiles = sa.Select<data_file>().OrderBy(df => df.data_file_id).Where(df => df.approval_id > 0).ToList();
                    }
                    else
                    {
                        //they are logged in, give them all
                        dataFiles = sa.Select<data_file>().OrderBy(df => df.data_file_id).ToList();
                    }
                }

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedDataFiles")]
        public OperationResult GetApprovedDataFiles(Int32 ApprovalId)
        {
            List<data_file> datafileList = new List<data_file>();

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    datafileList = sa.Select<approval>().FirstOrDefault(a => a.approval_id == ApprovalId).data_file.ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = datafileList, Description = MessageString };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredDataFiles")]
        public OperationResult GetFilteredDataFiles(string approved, [Optional] Int32 eventId, [Optional] Int32 memberId, [Optional] string state)
        {
            try
            {
                List<data_file> dataFileList; ;
                //set defaults
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();
                Int32 filterMember = (memberId > 0) ? memberId : -1;
                Int32 filterEvent = (eventId > 0) ? eventId : -1;

                using (STNAgent sa = new STNAgent())
                {
                    //Because 'Where' is producing an IQueryable, 
                    //the execution is deferred until the ToList so you can chain 'Wheres' together.
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


                    dataFileList = query.ToList();

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFileList = dataFileList.Where(d => d.approval_id > 0).ToList();
                    }
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = dataFileList, Description= MessageString };
            }
            catch( Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            data_file aDataFile;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    //general public..only approved ones
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId && df.approval_id > 0);                    
                    else
                        aDataFile = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId);
                    
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = aDataFile };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetFileDataFile(Int32 fileId)
        {
            data_file aDataFile = null;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //general public..only approved ones
                        aDataFile = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).data_file;
                        aDataFile = aDataFile.approval_id > 0 ? aDataFile : null;
                    }
                    else
                    {
                        //they are logged in, give them all
                        aDataFile = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).data_file;
                    }
                    sm(sa.Messages);

                }//end using   

                return new OperationResult.OK { ResponseResource = aDataFile, Description=MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentDataFiles")]
        public OperationResult GetInstrumentDataFiles(Int32 instrumentId)
        {
            List<data_file> dataFiles =null;
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {//general public..only approved ones
                        dataFiles = sa.Select<data_file>().AsEnumerable()
                                .Where(df => df.instrument_id == instrumentId && df.approval_id > 0)
                                .OrderBy(df => df.data_file_id)
                                .ToList<data_file>();
                    }
                    else
                    {
                        dataFiles = sa.Select<data_file>().AsEnumerable()
                                .Where(df => df.instrument_id == instrumentId)
                                .OrderBy(df => df.data_file_id)
                                .ToList<data_file>();
                    }
                }//end using

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryDatafiles")]
        public OperationResult GetPeakSummaryDatafiles(Int32 peakSummaryId)
        {
            List<data_file> dataFiles = null;
            try
            {
                //Return BadRequest if there is no ID
                if (peakSummaryId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFiles = sa.Select<data_file>().AsEnumerable()
                                .Where(df => df.peak_summary_id == peakSummaryId && df.approval_id > 0)
                                .OrderBy(df => df.data_file_id)
                                .ToList<data_file>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        dataFiles = sa.Select<data_file>().AsEnumerable()
                                .Where(df => df.peak_summary_id == peakSummaryId)
                                .OrderBy(df => df.data_file_id)
                                .ToList<data_file>();
                    }
                }//end using

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)] //, ForUriName = "CreateDataFile")]
        public OperationResult Post(data_file aDataFile)
        {
            try
            {
                //Return BadRequest if missing required fields
                if ((aDataFile.instrument_id <= 0 || !aDataFile.instrument_id.HasValue) || 
                    (aDataFile.processor_id <= 0 || !aDataFile.processor_id.HasValue))throw new BadRequestException("Invalid input parameters");

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username,securedPassword))
                    {
                        sa.Add<data_file>(aDataFile);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aDataFile, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key
         * 
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, data_file aDataFile)
        {
            data_file DataFileToUpdate = null;
            try
            {
                //Return BadRequest if missing required fields
                if (aDataFile.data_file_id <= 0 || entityId <= 0) throw new BadRequestException("Invalid input parameters");
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //Grab the instrument row to update
                        DataFileToUpdate = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId);

                        //Update fields
                        DataFileToUpdate.start = aDataFile.start;
                        DataFileToUpdate.end = aDataFile.end;
                        DataFileToUpdate.good_start = aDataFile.good_start;
                        DataFileToUpdate.good_end = aDataFile.good_end;
                        DataFileToUpdate.processor_id = aDataFile.processor_id;
                        DataFileToUpdate.instrument_id = aDataFile.instrument_id;
                        DataFileToUpdate.approval_id = aDataFile.approval_id;
                        DataFileToUpdate.collect_date = aDataFile.collect_date;
                        DataFileToUpdate.peak_summary_id = aDataFile.peak_summary_id;
                        DataFileToUpdate.elevation_status = aDataFile.elevation_status;

                        sa.Update<data_file>(DataFileToUpdate);
                        sm(sa.Messages);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aDataFile, Description=MessageString };
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
                return new OperationResult.OK { Description = MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

    }//end class DataFileHandler
}//end namespace