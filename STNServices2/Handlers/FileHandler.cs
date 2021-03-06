﻿//------------------------------------------------------------------------------
//----- FileHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles File resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 05.11.16 - JKN - refactored
#endregion

//using STNServices2.Resources;
using STNServices2.Security;
using STNServices2.Utilities;

using OpenRasta.Diagnostics;
using OpenRasta.IO;
using OpenRasta.Web;
using OpenRasta.Security;

using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Configuration;
using System.IO;

using System.IO.Compression; // can use this to create zip file of items
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Ionic.Zip;

using STNDB;
using STNServices2.Utilities.ServiceAgent;
using WiM.Exceptions;
using WiM.Resources;
using WiM.Security;
using System.Drawing;

namespace STNServices2.Handlers
{

    public class FileHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<file> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().OrderBy(f => f.file_id).ToList();
                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            file anEntity;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<file>().SingleOrDefault(f => f.file_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHistoricHWMspreadsheet")]
        public OperationResult GetHistoricHWMSpreadsheet()
        {
            InMemoryFile fileItem;            
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    fileItem = sa.GetHWMSpreadsheetItem();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = fileItem, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetTESTdataFile")]
        public OperationResult GetTESTdataFile()
        {
            InMemoryFile fileItem;
            dynamic files;
            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    fileItem = sa.GetTESTdataItem();
                    using (var fileStream = fileItem.OpenStream())

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string json = reader.ReadToEnd();
                        files = JsonConvert.DeserializeObject(json);
                    }

                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = files, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileItem")]
        public OperationResult GetFileItem(Int32 fileId)
        {
            InMemoryFile fileItem;
            file anEntity = null;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    //use include statements for stnagent's GetFileItem to find the event this file is on
                    anEntity = sa.Select<file>().SingleOrDefault(f => f.file_id == fileId);
                    if (anEntity == null) throw new BadRequestException("No file exists for given parameter");

                    fileItem = sa.GetFileItem(anEntity);
                    
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = fileItem, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilesByDateRange")]
        public OperationResult Get(string fromDate, [Optional] string toDate)
        {
            List<file> entities = null;
            try
            {
                DateTime? FromDate = ValidDate(fromDate);
                DateTime? ToDate = ValidDate(toDate);
                if (!FromDate.HasValue) throw new BadRequestException("Invalid input parameters");
                if (!ToDate.HasValue) ToDate = DateTime.Now;

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Where(f => f.file_date >= FromDate && f.file_date <= ToDate).OrderBy(f => f.file_date).ToList();

                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
                    
                
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMFiles")]
        public OperationResult GetHWMFiles(Int32 hwmId)
        {
            List<file> entities = null;
            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Where(f => f.hwm_id == hwmId).ToList();

                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetObjectivePointFiles")]
        public OperationResult GetObjectivePointFiles(Int32 objectivePointId)
        {
            List<file> entities = null;
            try
            {
                if (objectivePointId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Where(file => file.objective_point_id == objectivePointId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileTypeFiles")]
        public OperationResult GetFileTypeFiles(Int32 fileTypeId)
        {
            List<file> entities = null;

            try
            {
                if (fileTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file_type>().Include(f => f.files).FirstOrDefault(f => f.filetype_id == fileTypeId).files.ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteFiles")]
        public OperationResult GetSiteFiles(Int32 siteId)
        {
            List<file> entities = null;

            try
            {
                if (siteId <= 0 ) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Where(f => f.site_id == siteId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSourceFiles")]
        public OperationResult GetSourceFiles(Int32 sourceId)
        {
            List<file> entities = null;

            try
            {
                if (sourceId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<source>().Include(s => s.files).FirstOrDefault(s => s.source_id == sourceId).files.ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileFiles")]
        public OperationResult GetDataFileFiles(Int32 dataFileId)
        {
            List<file> entities = null;

            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<data_file>().Include(df => df.files).FirstOrDefault(df => df.data_file_id == dataFileId).files.ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                // this will always only contain 1 file file:datafile relationship is 1:1
                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentFiles")]
        public OperationResult GetInstrumentFiles(Int32 instrumentId)
        {
            List<file> entities = null;

            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Where(f => f.instrument_id == instrumentId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventFiles")]
        public OperationResult GetEventFiles(Int32 eventId)
        {
            List<file> entities = null;
            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Include(f => f.hwm).Include(f => f.instrument).Include("data_file.instrument")
                                .Where(f => f.hwm.event_id == eventId || f.instrument.event_id == eventId || f.data_file.instrument.event_id == eventId).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventFileItems")]
        public OperationResult GetEventFileItems(Int32 eventId, [Optional] string stateName, [Optional] string county, [Optional] string fromDate, [Optional] string toDate,
            string filesFor, [Optional] string sensorFiles, [Optional] string hwmFileTypes, [Optional] string sensorFileTypes)
        {
            List<file> entities = null;
            InMemoryFile fileItem = null;
            char[] delimiterChars = { ';', ',', ' ' };
            List<decimal> hwmFTList = !string.IsNullOrEmpty(hwmFileTypes) ? hwmFileTypes.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
            List<decimal> sensFTList = !string.IsNullOrEmpty(sensorFileTypes) ? sensorFileTypes.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToList() : null;
            DateTime? FromDate = ValidDate(fromDate);
            DateTime? ToDate = ValidDate(toDate);

            try
            {
                if (eventId <= 0 || (string.IsNullOrEmpty(filesFor) && (string.IsNullOrEmpty(sensorFiles)))) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username,securedPassword))
                    {
                        //all files for this event
                        IQueryable<file> allEventFilesQuery = sa.Select<file>().Include(f => f.hwm).Include(f => f.instrument).Include("data_file.instrument")
                        .Where(f => f.hwm.event_id == eventId || f.instrument.event_id == eventId || f.data_file.instrument.event_id == eventId);

                        IQueryable<file> hwmFilesQuery = null;
                        IQueryable<file> sensorFilesQuery = null;

                        //date range
                        if (FromDate.HasValue) 
                            allEventFilesQuery = allEventFilesQuery.Where(f => f.file_date >= FromDate);
                        
                        if (ToDate.HasValue)
                            allEventFilesQuery = allEventFilesQuery.Where(f => f.file_date <= ToDate);
                        
                        //state
                        if (!string.IsNullOrEmpty(stateName))
                            allEventFilesQuery = allEventFilesQuery.Where( f => f.hwm.site.state.ToUpper() == stateName.ToUpper() || 
                                        f.instrument.site.state.ToUpper() == stateName.ToUpper() || 
                                        f.data_file.instrument.site.state.ToUpper() == stateName.ToUpper());

                        //county
                        if (!string.IsNullOrEmpty(county))
                            allEventFilesQuery = allEventFilesQuery.Where(f => f.hwm.site.county.ToUpper() == county.ToUpper() ||
                                        f.instrument.site.county.ToUpper() == county.ToUpper() ||
                                        f.data_file.instrument.site.county.ToUpper() == county.ToUpper());
                        //only HWM files only
                        if (filesFor == "HWMs")
                        {
                            //only site files
                            hwmFilesQuery = allEventFilesQuery.Where(f => (f.hwm_id.HasValue && f.hwm_id > 0) && (!f.instrument_id.HasValue || f.instrument_id == 0));  

                            //now see which types they want
                            if (hwmFTList != null && hwmFTList.Count > 0)
                                hwmFilesQuery = hwmFilesQuery.Where(f => hwmFTList.Contains(f.filetype_id.Value));
                        }
                        //only Sensor files only
                        if (filesFor == "Sensors")
                        {
                            //only site files
                            sensorFilesQuery = allEventFilesQuery.Where(f => ((!f.hwm_id.HasValue || f.hwm_id == 0) && (!f.objective_point_id.HasValue || f.objective_point_id == 0)) && f.instrument_id.HasValue);
                            
                            //now see which types they want
                            if (sensFTList != null && sensFTList.Count > 0)
                                sensorFilesQuery = sensorFilesQuery.Where(f => sensFTList.Contains(f.filetype_id.Value));
                        }

                        entities = new List<file>();
                        if (hwmFilesQuery != null) hwmFilesQuery.ToList().ForEach(h => entities.Add(h));
                        if (sensorFilesQuery != null) sensorFilesQuery.ToList().ForEach(s => entities.Add(s));

                        sm(MessageType.info, "FileCount:" + entities.Count);
                        fileItem = sa.GetFileItemZip(entities);

                        sm(MessageType.info, "Count: " + entities.Count());
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = fileItem, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteEventFiles")]
        public OperationResult GetSiteEventFiles(Int32 siteId, Int32 eventId)
        {
            List<file> entities= null;

            try
            {
                if (siteId <= 0 || eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Include(f => f.hwm).Include(f => f.instrument).Include("data_file.instrument")
                                    .Where(f => f.site_id == siteId && (f.hwm.event_id == eventId || 
                                        f.instrument.event_id == eventId || f.data_file.instrument.event_id == eventId)).ToList();

                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilesByStateName")]
        public OperationResult GetFilesByStateName(string stateName)
        {
            List<file> entities = null;

            try
            {
                if (string.IsNullOrEmpty(stateName)) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<file>().Include("hwm.site").Include("instrument.site").Include("data_file.instrument.site").Where(
                                    f => f.hwm.site.state.ToUpper() == stateName.ToUpper() || 
                                        f.instrument.site.state.ToUpper() == stateName.ToUpper() || 
                                        f.data_file.instrument.site.state.ToUpper() == stateName.ToUpper()).ToList();
                    sm(MessageType.info, "Count: " + entities.Count);
                    sm(sa.Messages);
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
        public OperationResult Post(file anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            { 
                if (anEntity.filetype_id <= 0 || !anEntity.file_date.HasValue || anEntity.site_id <= 0)
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

                        anEntity = sa.Add<file>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.Created { ResponseResource = anEntity };
            }
            catch (Exception ex)
            { return HandleException(ex); }


        }//end HttpMethod.POST

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "UploadFile")]
        public OperationResult UploadFile(IEnumerable<IMultipartHttpEntity> entities)
        {
            try
            {
                //TODO: The stream decoding should really be in a custom Codec
                using (var memoryStream = new MemoryStream())
                {
                    String filename = "";
                    XmlSerializer serializer;

                    file uploadFile = null;

                    foreach (var entity in entities)
                    {
                        //Process Stream
                        if (!entity.Headers.ContentDisposition.Disposition.ToLower().Equals("form-data"))
                            return new OperationResult.BadRequest { ResponseResource = "Sent a field that is not declared as form-data, cannot process" };

                        if (entity.Stream != null && entity.ContentType != null)
                        {
                            //Process Stream
                            if (entity.Headers.ContentDisposition.Name.Equals("File"))
                            {
                                entity.Stream.CopyTo(memoryStream);
                                filename = entity.Headers.ContentDisposition.FileName;
                            }
                        }
                        else
                        {
                            //Process Variables
                            if (entity.Headers.ContentDisposition.Name.Equals("FileEntity"))
                            {
                                var mem = new MemoryStream();
                                entity.Stream.CopyTo(mem);
                                mem.Position = 0;
                                try
                                {
                                    serializer = new XmlSerializer(typeof(file));
                                    uploadFile = (file)serializer.Deserialize(mem);
                                }
                                catch
                                {
                                    mem.Position = 0;
                                    JsonSerializer jsonSerializer = new JsonSerializer();
                                    using (StreamReader streamReader = new StreamReader(mem, new UTF8Encoding(false, true)))
                                    {
                                        using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                                        {
                                            uploadFile = (file)jsonSerializer.Deserialize(jsonTextReader, typeof(file));
                                        }
                                    }//end using
                                }
                            }
                        }
                    }//next
                    //Return BadRequest if missing required fields

                    if (uploadFile.filetype_id <= 0 || !uploadFile.file_date.HasValue ||
                        uploadFile.site_id <= 0)
                        throw new BadRequestException("Invalid input parameters");

                    //Get basic authentication password
                    using (EasySecureString securedPassword = GetSecuredPassword())
                    {
                        using (STNAgent sa = new STNAgent(username, securedPassword))
                        {                            
                            //now remove existing fileItem for this file
                            if (uploadFile.file_id > 0)
                                sa.RemoveFileItem(uploadFile);
                         
                            //last updated parts
                            List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                            Int32 loggedInUserId = MemberList.First<member>().member_id;
                            uploadFile.last_updated = DateTime.Now;
                            uploadFile.last_updated_by = loggedInUserId;

                            uploadFile.name = filename;
                            
                            //are they 'reuploading lost fileItem to existing file or posting new fileItem with new file
                            if (uploadFile.file_id > 0)
                                sa.PutFileItem(uploadFile, memoryStream);
                            else
                                sa.AddFile(uploadFile, memoryStream);
                            sm(sa.Messages);
                        }//end using
                    }//end using

                    return new OperationResult.Created { ResponseResource = uploadFile, Description = this.MessageString };
                }//end using
            }//end try    
            catch (Exception ex)
            { return HandleException(ex); }//end catch
        }//end HttpMethod.POST
        
        
        [HttpOperation(HttpMethod.POST, ForUriName = "RunChopperScript")]
        public OperationResult RunChopperScript(IEnumerable<IMultipartHttpEntity> entities)
        {
            dynamic responseJson = null;           
            try
            {
                bool dayLight = false;
//                Boolean.TryParse(daylightSavings, out dayLight);

                using (var memoryStream = new MemoryStream())
                {
                    String filename = "";
                    XmlSerializer serializer;
                    file uploadFile = null;

                    foreach (var entity in entities)
                    {
                        //Process Stream
                        if (!entity.Headers.ContentDisposition.Disposition.ToLower().Equals("form-data"))
                            return new OperationResult.BadRequest { ResponseResource = "Sent a field that is not declared as form-data, cannot process" };

                        if (entity.Stream != null && entity.ContentType != null)
                        {
                            //Process Stream
                            if (entity.Headers.ContentDisposition.Name.Equals("File"))
                            {
                                entity.Stream.CopyTo(memoryStream);
                                filename = entity.Headers.ContentDisposition.FileName;
                            }
                        }
                        else
                        {
                            //Process Variables
                            if (entity.Headers.ContentDisposition.Name.Equals("FileEntity"))
                            {
                                var mem = new MemoryStream();
                                entity.Stream.CopyTo(mem);
                                mem.Position = 0;
                                try
                                {
                                    serializer = new XmlSerializer(typeof(file));
                                    uploadFile = (file)serializer.Deserialize(mem);
                                }
                                catch
                                {
                                    mem.Position = 0;
                                    JsonSerializer jsonSerializer = new JsonSerializer();
                                    using (StreamReader streamReader = new StreamReader(mem, new UTF8Encoding(false, true)))
                                    {
                                        using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                                        {
                                            uploadFile = (file)jsonSerializer.Deserialize(jsonTextReader, typeof(file));
                                        }
                                    }//end using
                                } //end catch
                            } //end if FileEntity
                        }//end else
                    }//next

                    //Return BadRequest if missing required fields
                    if (uploadFile.instrument_id <= 0 || uploadFile.site_id <= 0)
                        throw new BadRequestException("Invalid input parameters");

                    STNServiceAgent stnsa = new STNServiceAgent(uploadFile.instrument_id.Value, memoryStream, filename);
                    if (stnsa.chopperInitialized)
                        responseJson = stnsa.RunChopperScript(Convert.ToBoolean(uploadFile.description)); // uploadFile.description actually contains the true/false of daylightsavings
                    else
                        throw new BadRequestException("Error initializing python script.");
                } //end using memoryStream
                return new OperationResult.OK { ResponseResource = responseJson };
            }//end try
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
        public OperationResult Put(Int32 entityId, file anEntity)
        {
            Int32 loggedInUserId = 0;
            try
            {
                if (anEntity.filetype_id <= 0 || !anEntity.file_date.HasValue || anEntity.site_id <= 0) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        //last updated parts
                        List<member> MemberList = sa.Select<member>().Where(m => m.username.ToUpper() == username.ToUpper()).ToList();
                        loggedInUserId = MemberList.First<member>().member_id;
                        anEntity.last_updated = DateTime.Now;
                        anEntity.last_updated_by = loggedInUserId;

                        anEntity = sa.Update<file>(entityId, anEntity);
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
            if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        file ObjectToBeDeleted = sa.Select<file>().SingleOrDefault(c => c.file_id == entityId);
                        if (ObjectToBeDeleted == null) throw new WiM.Exceptions.NotFoundRequestException();
                        Int32 datafileID = ObjectToBeDeleted.data_file_id.HasValue ? ObjectToBeDeleted.data_file_id.Value : 0;
                        sa.RemoveFileItem(ObjectToBeDeleted);
                        sa.Delete<file>(ObjectToBeDeleted);

                        if (datafileID > 0)
                        {
                            data_file df = sa.Select<data_file>().FirstOrDefault(d => d.data_file_id == datafileID);
                            sa.Delete<data_file>(df);
                        }

                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

        #region helpers
       
        #endregion
    }//end class FileHandler
}//end namespace
