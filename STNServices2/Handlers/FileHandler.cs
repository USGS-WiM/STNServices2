//------------------------------------------------------------------------------
//----- FileHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
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
// 02.14.13 - JKN - added GetFileItem and created BuildFilePath method for building file paths
// 02.07.13 - JKN - Added query to get files by eventId and siteID
// 01.31.13 - JKN - Added GetFilesByStateName and Get(string fromDate, [Optional] string toDate methods
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 12.12.12 - TR - Added FILE_URL to be updated
// 07.03.12 - JKN - Role authorization, and moved context to base class
// 05.29.12 - JKN - added connection string and delete method
// 03.14.12 - JB - Added File Put (Update) and upload replacement
// 03.13.12 - JB - Added File Post and S3 upload
// 02.03.12 - JB - Created
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

namespace STNServices2.Handlers
{

    public class FileHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<file> files = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().OrderBy(f => f.file_id)
                                    .ToList();
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            file aFile;

            //Return BadRequest if there is no ID
            if (entityId <= 0) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    aFile = sa.Select<file>().SingleOrDefault(
                                f => f.file_id == entityId);

                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = aFile, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileItem")]
        public OperationResult GetFileItem(Int32 fileId)
        {
            InMemoryFile fileItem;
            file aFile = null;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    aFile = sa.Select<file>().SingleOrDefault(f => f.file_id == fileId);
                    if (aFile == null) throw new BadRequestException("No file exists for given parameter");

                    fileItem = sa.GetFileItem(aFile);                    

                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = fileItem, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

        //get event file items back in zip file
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventFileItems")]
        public OperationResult GetEventFileItems(Int32 eventId)
        {
            List<file> files = null;
            InMemoryFile fileItem = null;

            try
            {

                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {

                    files = sa.Select<file>().Where(
                                    f => f.hwm.event_id == eventId || f.instrument.event_id == eventId ||
                                        f.data_file.instrument.event_id == eventId)
                                    .ToList();
                    sm(MessageType.info, "FileCount:" + files.Count);
                    fileItem = sa.GetFileItemZip(files);

                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = fileItem, Description =MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string fromDate, [Optional] string toDate)
        {
            List<file> files = null;
            try
            {
                DateTime? FromDate = ValidDate(fromDate);
                DateTime? ToDate = ValidDate(toDate);
                if (!FromDate.HasValue) throw new BadRequestException("Invalid input parameters");
                if (!ToDate.HasValue) ToDate = DateTime.Now;

                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(f => f.file_date >= FromDate && f.file_date <= ToDate).OrderBy(f => f.file_date)
                                    .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
                    
                
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMFiles")]
        public OperationResult GetHWMFiles(Int32 hwmId)
        {
            List<file> files = null;

            try
            {
                if (hwmId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                        f => f.hwm_id == hwmId)
                                        .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetObjectivePointFiles")]
        public OperationResult GetObjectivePointFiles(Int32 objectivePointId)
        {
            List<file> files =null;
            try
            {
                if (objectivePointId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                        file => file.objective_point_id == objectivePointId)
                                        .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileTypeFiles")]
        public OperationResult GetFileTypeFiles(Int32 fileTypeId)
        {
            List<file> files = null;

            try
            {
                if (fileTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file_type>().FirstOrDefault(
                                        f => f.filetype_id == fileTypeId).files
                                        .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteFiles")]
        public OperationResult GetSiteFiles(Int32 siteId)
        {
            List<file> files = null;

            try
            {
                if (siteId <= 0 ) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                    f => f.site_id == siteId)
                                    .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSourceFiles")]
        public OperationResult GetSourceFiles(Int32 sourceId)
        {
            List<file> files = null;

            try
            {
                if (sourceId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {

                    files = sa.Select<source>().FirstOrDefault(
                                    s => s.source_id == sourceId).files
                                    .ToList();
                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataFileFiles")]
        public OperationResult GetDataFileFiles(Int32 dataFileId)
        {
            List<file> files = null;

            try
            {
                if (dataFileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<data_file>().FirstOrDefault(
                                    df => df.data_file_id == dataFileId).files
                                    .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentFiles")]
        public OperationResult GetInstrumentFiles(Int32 instrumentId)
        {
            List<file> files = null;

            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                    f => f.instrument_id == instrumentId)
                                    .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventFiles")]
        public OperationResult GetEventFiles(Int32 eventId)
        {
            List<file> files = null;
            try
            {
                if (eventId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                    f => f.hwm.event_id == eventId || f.instrument.event_id == eventId ||
                                        f.data_file.instrument.event_id == eventId)
                                    .ToList();
                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSite")]
        public OperationResult GetSite(Int32 siteId, Int32 eventId)
        {
            List<file> files= null;

            try
            {
                if (siteId <= 0 || eventId <= 0) throw new BadRequestException("Invalid input parameters");

                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                    f => f.site_id == siteId && (f.hwm.event_id == eventId || 
                                        f.instrument.event_id == eventId ||
                                        f.data_file.instrument.event_id == eventId))
                                    .ToList();

                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
            }
            catch(Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilesByStateName")]
        public OperationResult GetFilesByStateName(string stateName)
        {
            List<file> files = new List<file>();

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    files = sa.Select<file>().Where(
                                    f => string.Equals(f.hwm.site.state.ToUpper(), stateName.ToUpper()) ||
                                        string.Equals(f.instrument.site.state.ToUpper(), stateName.ToUpper()) ||
                                        string.Equals(f.data_file.instrument.site.state.ToUpper(), stateName.ToUpper()))
                                    .ToList();
                    sm(MessageType.info, "Count: " + files.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = files, Description = MessageString };
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
            //Return BadRequest if missing required fields
            if (anEntity.filetype_id <= 0 || anEntity.file_date.HasValue ||
               anEntity.site_id <= 0)
                throw new BadRequestException("Invalid input parameters");
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
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

                    if (uploadFile.filetype_id <= 0 || uploadFile.file_date.HasValue ||
                        uploadFile.site_id <= 0)
                        throw new BadRequestException("Invalid input parameters");

                    //Get basic authentication password
                    using (EasySecureString securedPassword = GetSecuredPassword())
                    {
                        using (STNAgent sa = new STNAgent(username, securedPassword))
                        {
                            //Update path
                            uploadFile.path = filename;
                            sa.AddFile(uploadFile, memoryStream);
                            sm(sa.Messages);
                        }//end using
                    }//end using

                    return new OperationResult.Created { ResponseResource = uploadFile, Description=MessageString };
                }//end using
            }//end try    
            catch (Exception ex)
            { return HandleException(ex); }//end catch
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, file aFile)
        {
            if ((aFile.file_id < 0) && (aFile.site_id <= 0)) throw new BadRequestException("Invalid input parameters");

            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        aFile = sa.Update<file>(entityId, aFile);
                        sm(sa.Messages);
                    }//end using
                }//end using

                return new OperationResult.Modified { ResponseResource = aFile,Description= MessageString };
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
                        sa.DeleteFile(entityId);
                        sm(sa.Messages);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { Description = MessageString};
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HTTP.DELETE
        #endregion

    }//end class FileHandler
}//end namespace
