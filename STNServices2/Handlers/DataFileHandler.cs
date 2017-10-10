//------------------------------------------------------------------------------
//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

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
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    else
                    {
                        anEntity = sa.Select<data_file>().SingleOrDefault(df => df.data_file_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
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
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
                        sm(sa.Messages);
                    }
                    else
                    {
                        //they are logged in, give them all
                        anEntity = sa.Select<file>().FirstOrDefault(f => f.file_id == fileId).data_file;
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();
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
        public OperationResult GetFilteredDataFiles(string approved, [Optional] string eventId, [Optional] string state, [Optional] string counties)
        {
            List<data_file> entities;
            List<data_file> refreshedVersion;
            
            try
            {
                if (string.IsNullOrEmpty(approved)) throw new BadRequestException("Invalid input parameters");
               
                //set defaults
                char[] countydelimiterChars = { ';', ',' };
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();

                List<String> countyList = !string.IsNullOrEmpty(counties) ? counties.ToUpper().Split(countydelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                Int32 filterEvent = (!string.IsNullOrEmpty(eventId)) ? Convert.ToInt32(eventId) : -1;

                using (STNAgent sa = new STNAgent(true))
                {
                    IQueryable<data_file> query;
                    if (isApprovedStatus)
                        query = sa.Select<data_file>().Include(d => d.instrument).Where(d => d.approval_id > 0);
                    else
                        query = sa.Select<data_file>().Include(d => d.instrument).Where(d => d.approval_id <= 0 || !d.approval_id.HasValue);

                    if (filterEvent > 0)
                        query = query.Where(d => d.instrument.event_id.Value == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(d => d.instrument.site.state == filterState);

                    if (countyList != null && countyList.Count > 0)
                        query = query.Where(d => countyList.Contains(d.instrument.site.county.ToUpper()));                    

                    entities = query.ToList();
                   
                    // do this for serialization for csv
                    refreshedVersion = entities.Select(d => new data_file()
                    {
                        approval_id = d.approval_id,
                        collect_date = d.collect_date,
                        data_file_id = d.data_file_id,
                        elevation_status = d.elevation_status,
                        end = d.end,
                        good_end = d.good_end,
                        good_start = d.good_start,
                        instrument_id = d.instrument_id,
                        peak_summary_id = d.peak_summary_id,
                        processor_id = d.processor_id,
                        start = d.start,
                        time_zone = d.time_zone
                    }).ToList();

                    sm(MessageType.info, "Count: " + refreshedVersion.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = refreshedVersion, Description = this.MessageString };
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

        [HttpOperation(HttpMethod.GET, ForUriName = "RunStormDataFileScript")]
        public OperationResult RunStormScript(Int32 seaDataFileId, Int32 airDataFileId, string hertz, string username)
        {            
            try
            {
                //Return BadRequest if there is no ID
                bool isHertz = false;
                Boolean.TryParse(hertz, out isHertz);
                if (airDataFileId <= 0 || seaDataFileId <= 0 || string.IsNullOrEmpty(username))
                    throw new BadRequestException("Invalid input parameters");
                
                STNServiceAgent stnsa = new STNServiceAgent(airDataFileId, seaDataFileId, username);
                if (stnsa.initialized)
                    stnsa.RunStormScript(isHertz);
                else
                    throw new BadRequestException("Error initializing python script.");
                
                return new OperationResult.OK {  };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "RunAirDataFileScript")]
        public OperationResult RunAirDataFileScript(Int32 airDataFileId, string username)
        {
            try
            {
                if (airDataFileId <= 0 || string.IsNullOrEmpty(username))
                    throw new BadRequestException("Invalid input parameters");

                STNServiceAgent stnsa = new STNServiceAgent(airDataFileId, username);
                if (stnsa.pressureInitialized)
                    stnsa.RunAirScript();
                else
                    throw new BadRequestException("Error initializing python script.");
                
                return new OperationResult.OK { };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventDataFileView")]
        public OperationResult GetEventDataFileView(Int32 eventId)
        {
            List<dataFile_view> entities;

            //Return BadRequest if there is no ID
            if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.getTable<dataFile_view>(new Object[1] { null }).Where(e => e.event_id == eventId).ToList();
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
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(data_file anEntity)
        {
            try
            {
                if (!anEntity.good_start.HasValue || !anEntity.good_end.HasValue || !anEntity.collect_date.HasValue || 
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
                if (entityId <= 0 || !anEntity.good_start.HasValue || !anEntity.good_end.HasValue || !anEntity.collect_date.HasValue ||
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