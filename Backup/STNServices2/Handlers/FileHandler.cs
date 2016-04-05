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

using STNServices2.Resources;
using STNServices2.Authentication;
using STNServices2.Utilities;

using OpenRasta.Diagnostics;
using OpenRasta.IO;
using OpenRasta.Web;
using OpenRasta.Security;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
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
namespace STNServices2.Handlers
{

    public class FileHandler : HandlerBase
    {
        #region Properties

        public ILogger Log { get; set; }

        public override string entityName
        {
            get { return "FILES"; }
        }
        #endregion

        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<FILES> FileList = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILES.OrderBy(f => f.FILE_ID)
                                    .ToList();

                    if (FileList != null)
                        FileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetCitizenFiles")]
        public OperationResult GetCitizenFiles()
        {
            List<CITIZEN_FILE> FileList = null;

            try
            {
                //Get basic authentication password
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILES.Where(
                              f => f.SOURCE_ID == 1639).AsEnumerable().Select(
                               sf => new CITIZEN_FILE
                               {
                                   FILE_ID = sf.FILE_ID,
                                   DESCRIPTION = sf.DESCRIPTION,
                                   LATITUDE_DD = sf.LATITUDE_DD,
                                   LONGITUDE_DD = sf.LONGITUDE_DD,
                                   FILE_DATE = sf.FILE_DATE,
                                   VALIDATED = sf.VALIDATED,
                                   DATE_VALIDATED = sf.DATE_VALIDATED,
                                   VALIDATOR_USERID = sf.VALIDATOR_USERID,
                                   LOCATOR_TYPE_ID = sf.LOCATOR_TYPE_ID,
                                   KEYWORDS = getFileKeywords(aSTNE.FILE_KEYWORD, (Int32)sf.FILE_ID)
                               }).ToList();
                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            FILES aFile;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aFile = aSTNE.FILES.SingleOrDefault(
                                f => f.FILE_ID == entityId);

                    if (aFile != null)
                        aFile.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aFile };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetCitizenFile")]
        public OperationResult GetCitizenFile(Int32 fileId)
        {
            CITIZEN_FILE aFile;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aFile = aSTNE.FILES.Where(
                              f => f.FILE_ID == fileId && f.SOURCE_ID == 1639).AsEnumerable().Select(
                               sf => new CITIZEN_FILE
                               {
                                   FILE_ID = sf.FILE_ID,
                                   DESCRIPTION = sf.DESCRIPTION,
                                   LATITUDE_DD = sf.LATITUDE_DD,
                                   LONGITUDE_DD = sf.LONGITUDE_DD,
                                   FILE_DATE = sf.FILE_DATE,
                                   VALIDATED = sf.VALIDATED,
                                   DATE_VALIDATED = sf.DATE_VALIDATED,
                                   VALIDATOR_USERID = sf.VALIDATOR_USERID,
                                   LOCATOR_TYPE_ID = sf.LOCATOR_TYPE_ID,
                                   KEYWORDS = getFileKeywords(aSTNE.FILE_KEYWORD, fileId)
                               }).FirstOrDefault();

                    //if (aFile != null)
                    //  aFile.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aFile };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileItem")]
        public OperationResult GetFileItem(Int32 fileId)
        {
            FILES aFile;
            InMemoryFile fileItem;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aFile = aSTNE.FILES.SingleOrDefault(f => f.FILE_ID == fileId);
                    if (aFile == null || aFile.PATH == null || String.IsNullOrEmpty(aFile.PATH))
                        return new OperationResult.BadRequest { Description = "No items exist for specified file" };

                    string directoryName = string.Empty;
                    S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                    //Storage aStorage = new Storage(AppDomain.CurrentDomain.BaseDirectory);

                    if (aFile.SOURCE_ID.HasValue)
                    {
                        switch ((Int32)aFile.SOURCE_ID.Value)
                        {
                            case 1639: // ASFPM source
                                directoryName = "CITIZEN_FILE/" + aFile.PATH;
                                break;
                            default:
                                directoryName = BuildFilePath(aFile, aFile.PATH);
                                break;
                        }//end switch
                    }
                    else
                    {
                        directoryName = BuildFilePath(aFile, aFile.PATH);
                    }

                    fileItem = new InMemoryFile(aBucket.GetObject(directoryName));

                    fileItem.ContentType = GetContentType(aFile.PATH);

                    //in case the image isn't there (all the testing files)
                    if (fileItem == null)
                    {
                        fileItem = new InMemoryFile(new MemoryStream());
                    }

                    return new OperationResult.OK { ResponseResource = fileItem };
                }//end using
            }
            catch
            { return new OperationResult.BadRequest(); }
        }

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string fromDate, [Optional] string toDate)
        {
            List<FILES> FileList = new List<FILES>();
            DateTime? FromDate = ValidDate(fromDate);
            DateTime? ToDate = ValidDate(toDate);
            if (!ToDate.HasValue) ToDate = DateTime.Now;

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILES.Where(f => f.FILE_DATE >= FromDate && f.FILE_DATE <= ToDate).OrderBy(f => f.FILE_ID)
                                    .ToList();

                    if (FileList != null)
                        FileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetHWMFiles")]
        public OperationResult GetHWMFiles(Int32 hwmId)
        {
            List<FILES> FileList = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILES.Where(
                                        file => file.HWM_ID == hwmId)
                                        .ToList<FILES>();

                    if (FileList != null)
                        FileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetObjectivePointFiles")]
        public OperationResult GetObjectivePointFiles(Int32 objectivePointId)
        {
            List<FILES> FileList = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILES.Where(
                                        file => file.OBJECTIVE_POINT_ID == objectivePointId)
                                        .ToList<FILES>();

                    if (FileList != null)
                        FileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileTypeFiles")]
        public OperationResult GetFileTypeFiles(Int32 fileTypeId)
        {
            List<FILES> FileList = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    FileList = aSTNE.FILE_TYPE.FirstOrDefault(
                                        f => f.FILETYPE_ID == fileTypeId).FILEs
                                        .ToList();

                    if (FileList != null)
                        FileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = FileList };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSiteFiles")]
        public OperationResult GetSiteFiles(Int32 siteId)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.FILES.Where(
                                    file => file.SITE_ID == siteId)
                                    .ToList<FILES>();
                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSourceFiles")]
        public OperationResult GetSourceFiles(Int32 sourceId)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {

                    files = aSTNE.SOURCES.FirstOrDefault(
                                    s => s.SOURCE_ID == sourceId).FILEs
                                    .ToList<FILES>();

                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetDataFileFiles")]
        public OperationResult GetDataFileFiles(Int32 dataFileId)
        {
            if (dataFileId <= 0)
                return new OperationResult.BadRequest();

            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.DATA_FILE.FirstOrDefault(
                                    df => df.DATA_FILE_ID == dataFileId).FILEs
                                    .ToList<FILES>();
                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentFiles")]
        public OperationResult GetInstrumentFiles(Int32 instrumentId)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.FILES.Where(
                                    file => file.INSTRUMENT_ID == instrumentId)
                                    .ToList<FILES>();

                }//end using            

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        //get event file items back in zip file
        [HttpOperation(ForUriName = "GetEventFileItems")]
        public OperationResult GetEventFileItems(Int32 eventId)
        {
            //add additional parameters (don't include 'landowner form' filetypes, allow state filter, file type filter (data, photo
            List<FILES> files = new List<FILES>();
            InMemoryFile fileZip = null;
            MemoryStream ms = new MemoryStream();

            try
            {
                String conn = @"metadata=res://*/STNModel.csdl|res://*/STNModel.ssdl|res://*/STNModel.msl;provider=Oracle.DataAccess.Client;provider connection string=""DATA SOURCE=STNRDS;USER ID={0};PASSWORD={1}""";

                ////string.Format(conn, "FRPUBLIC", "STN_Pub1ic"))
                using (STNEntities2 aSTNE = new STNEntities2(string.Format(conn, "FRPUBLIC", "STN_Pub1ic")))
                {
                    //get all the files
                    files = aSTNE.FILES.Where(
                                    file => file.HWM.EVENT_ID == eventId || file.INSTRUMENT.EVENT_ID == eventId ||
                                        file.DATA_FILE.INSTRUMENT.EVENT_ID == eventId)
                                    .ToList<FILES>();

                    //get each item for these files
                    string directoryName = string.Empty;
                    S3Bucket aBucket = new S3Bucket("STNStorage");
                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (FILES f in files)
                        {
                            directoryName = BuildFilePath(f, f.PATH);

                            //Note: if empty folder exists, folder will not be included.
                            zip.AddEntry(f.PATH, aBucket.GetObject(directoryName));
                            zip.Comment = "Downloaded: " + f.PATH;

                        }//end foreach
                        zip.Save(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Flush();
                        //fileItem = new InMemoryFile(sAgent.GetFileItem((Int32)fType));
                        fileZip = new InMemoryFile(ms);
                        fileZip.FileName = "EventFileDownload.zip";
                    };//end using zipfile

                }//end using

                return new OperationResult.OK { ResponseResource = fileZip };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetEventFiles")]
        public OperationResult GetEventFiles(Int32 eventId)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.FILES.Where(
                                    file => file.HWM.EVENT_ID == eventId || file.INSTRUMENT.EVENT_ID == eventId ||
                                        file.DATA_FILE.INSTRUMENT.EVENT_ID == eventId)
                                    .ToList<FILES>();
                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetSite(Int32 siteId, Int32 eventId)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.FILES.Where(
                                    file => file.SITE_ID == siteId && (file.HWM.EVENT_ID == eventId || file.INSTRUMENT.EVENT_ID == eventId ||
                                        file.DATA_FILE.INSTRUMENT.EVENT_ID == eventId))
                                    .ToList<FILES>();
                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetFilesByStateName")]
        public OperationResult GetFilesByStateName(string stateName)
        {
            List<FILES> files = new List<FILES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    files = aSTNE.FILES.Where(
                                    f => string.Equals(f.HWM.SITE.STATE.ToUpper(), stateName.ToUpper()) ||
                                        string.Equals(f.INSTRUMENT.SITE.STATE.ToUpper(), stateName.ToUpper()) ||
                                        string.Equals(f.DATA_FILE.INSTRUMENT.SITE.STATE.ToUpper(), stateName.ToUpper()))
                                    .ToList<FILES>();
                }//end using

                return new OperationResult.OK { ResponseResource = files };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET
        #endregion

        #region PostMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(FILES aFile)
        {
            //Return BadRequest if missing required fields
            if (aFile.SITE_ID <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        if (!Exists(aSTNE.FILES, ref aFile))
                        {
                            aSTNE.FILES.AddObject(aFile);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aFile };
            }
            catch
            { return new OperationResult.BadRequest(); }


        }//end HttpMethod.POST

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "UploadFile")]
        public OperationResult Post(IEnumerable<IMultipartHttpEntity> entities)
        {
            try
            {
                //TODO: The stream decoding should really be in a custom Codec
                using (var memoryStream = new MemoryStream())
                {
                    String filename = "";
                    XmlSerializer serializer;

                    FILES uploadFile = null;

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

                                    serializer = new XmlSerializer(typeof(FILES));
                                    uploadFile = (FILES)serializer.Deserialize(mem);
                                }
                                catch
                                {
                                    mem.Position = 0;
                                    JsonSerializer jsonSerializer = new JsonSerializer();
                                    using (StreamReader streamReader = new StreamReader(mem, new UTF8Encoding(false, true)))
                                    {
                                        using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                                        {
                                            uploadFile = (FILES)jsonSerializer.Deserialize(jsonTextReader, typeof(FILES));
                                        }
                                    }//end using

                                }
                            }

                        }
                    }//next
                    //Return BadRequest if missing required fields

                    if (uploadFile == null ||
                        (uploadFile.SITE_ID == null || uploadFile.SITE_ID <= 0))
                    {
                        return new OperationResult.BadRequest { ResponseResource = "Bad, missing, or partial file entity" };
                    }
                    //Setup S3 Bucket
                    S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                    //Storage aStorage = new Storage(AppDomain.CurrentDomain.BaseDirectory);

                    //Get basic authentication password
                    using (EasySecureString securedPassword = GetSecuredPassword())
                    {
                        using (STNEntities2 aSTNE = GetRDS(securedPassword))
                        {
                            //Update path
                            uploadFile.PATH = filename;

                            if (!Exists(aSTNE.FILES, ref uploadFile))
                            {
                                aSTNE.FILES.AddObject(uploadFile);
                                aSTNE.SaveChanges();
                            }//end if

                            //Upload to S3
                            aBucket.PutObject(BuildFilePath(uploadFile, filename), memoryStream);
                            //aStorage.PutObject(BuildFilePath(uploadFile,filename), memoryStream);


                        }//end using
                    }//end using

                    return new OperationResult.OK { ResponseResource = uploadFile };
                }//end using
            }//end try
            catch (IOException ioe)
            {
                Log.WriteError("IO Error: " + ioe.Message + " \r\n Inner Exception: " + ioe.InnerException);
                return new OperationResult.BadRequest { ResponseResource = "Cannot process the request" };
            }//end catch
        }//end HttpMethod.POST

        [HttpOperation(HttpMethod.POST, ForUriName = "UploadCitizenFile")]
        public OperationResult UploadCitizenFile(IEnumerable<IMultipartHttpEntity> entities)
        {
            try
            {
                //TODO: The stream decoding should really be in a custom Codec
                using (var memoryStream = new MemoryStream())
                {
                    String filename = "";
                    XmlSerializer serializer;
                    CITIZEN_FILE uploadFile = null;

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
                            }//end if
                        }
                        else
                        {
                            //Process Variables
                            if (entity.Headers.ContentDisposition.Name.Equals("FileEntity"))
                            {
                                var mem = new MemoryStream();
                                entity.Stream.CopyTo(mem);
                                mem.Position = 0;

                                serializer = new XmlSerializer(typeof(CITIZEN_FILE));
                                uploadFile = (CITIZEN_FILE)serializer.Deserialize(mem);
                            }//end if
                        }//end if
                    }//next
                    //Return BadRequest if missing required fields

                    if (uploadFile == null || uploadFile.LATITUDE_DD == null ||
                        uploadFile.LONGITUDE_DD == null)
                    {
                        return new OperationResult.BadRequest { ResponseResource = "Bad, missing, or partial file entity" };
                    }
                    //Setup S3 Bucket
                    S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                    //Storage aStorage = new Storage(AppDomain.CurrentDomain.BaseDirectory);

                    //Get basic authentication password
                    using (STNEntities2 aSTNE = GetRDS())
                    {

                        //Upload to S3
                        aBucket.PutObject("CITIZEN_FILE/" + filename, memoryStream);
                        //aStorage.PutObject("CITIZEN_FILE/"+filename, memoryStream);

                        FILES newFile = new FILES();
                        newFile.PATH = filename;
                        newFile.LATITUDE_DD = uploadFile.LATITUDE_DD.HasValue ? uploadFile.LATITUDE_DD : null;
                        newFile.LONGITUDE_DD = uploadFile.LONGITUDE_DD.HasValue ? uploadFile.LONGITUDE_DD : null;
                        newFile.DESCRIPTION = uploadFile.DESCRIPTION != string.Empty ? uploadFile.DESCRIPTION : string.Empty;
                        newFile.FILE_DATE = uploadFile.FILE_DATE.HasValue ? uploadFile.FILE_DATE : null;
                        newFile.LOCATOR_TYPE_ID = uploadFile.LOCATOR_TYPE_ID.HasValue ? uploadFile.LOCATOR_TYPE_ID : null;
                        newFile.FILETYPE_ID = 1;  // photo-type
                        newFile.SOURCE_ID = 1639; // ASFPM source
                        newFile.PATH = filename;
                        newFile.VALIDATED = 0;    //set validated to false


                        if (!Exists(aSTNE.FILES, ref newFile))
                        {
                            aSTNE.FILES.AddObject(newFile);
                            aSTNE.SaveChanges();
                        }//end if

                        if (uploadFile.KEYWORDS != null && uploadFile.KEYWORDS.Count > 0)
                        {
                            addKeywords(aSTNE.FILE_KEYWORD, newFile.FILE_ID, uploadFile.KEYWORDS);
                            aSTNE.SaveChanges();
                        }//endif

                        //refresh uploaded file
                        uploadFile = new CITIZEN_FILE
                        {
                            FILE_ID = newFile.FILE_ID,
                            DESCRIPTION = newFile.DESCRIPTION,
                            LATITUDE_DD = newFile.LATITUDE_DD,
                            LONGITUDE_DD = newFile.LONGITUDE_DD,
                            FILE_DATE = newFile.FILE_DATE,
                            VALIDATED = newFile.VALIDATED,
                            DATE_VALIDATED = newFile.DATE_VALIDATED,
                            VALIDATOR_USERID = newFile.VALIDATOR_USERID,
                            LOCATOR_TYPE_ID = newFile.LOCATOR_TYPE_ID,
                            KEYWORDS = getFileKeywords(aSTNE.FILE_KEYWORD, (Int32)newFile.FILE_ID)
                        };

                    }//end using

                    return new OperationResult.OK { ResponseResource = uploadFile };
                }//end using
            }//end try
            catch (IOException ioe)
            {
                Log.WriteError("IO Error: " + ioe.Message + " \r\n Inner Exception: " + ioe.InnerException);
                return new OperationResult.BadRequest { ResponseResource = "Cannot process the request" };
            }//end catch
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, FILES aFile)
        {
            FILES updatedFile;

            //Return BadRequest if missing required fields
            if ((aFile.FILE_ID < 0) && (aFile.SITE_ID <= 0) && (aFile.HWM_ID <= 0))
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //Grab the file row to update
                        updatedFile = aSTNE.FILES.SingleOrDefault(
                                           file => file.FILE_ID == entityId);
                        //Update fields
                        updatedFile.DESCRIPTION = aFile.DESCRIPTION;
                        updatedFile.FILE_DATE = aFile.FILE_DATE;
                        updatedFile.FILE_TYPE = aFile.FILE_TYPE;
                        updatedFile.FILETYPE_ID = aFile.FILETYPE_ID;
                        updatedFile.HWM_ID = aFile.HWM_ID;
                        updatedFile.LATITUDE_DD = aFile.LATITUDE_DD;
                        updatedFile.LONGITUDE_DD = aFile.LONGITUDE_DD;
                        updatedFile.PATH = aFile.PATH;
                        updatedFile.PHOTO_DIRECTION = aFile.PHOTO_DIRECTION;
                        updatedFile.SITE_ID = aFile.SITE_ID;
                        updatedFile.SOURCE_ID = aFile.SOURCE_ID;
                        updatedFile.FILE_URL = aFile.FILE_URL;
                        updatedFile.DATA_FILE_ID = aFile.DATA_FILE_ID;
                        updatedFile.INSTRUMENT_ID = aFile.INSTRUMENT_ID;
                        //Save changes
                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aFile };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.PUT

        [STNRequiresRole(new string[] { AdminRole, CitizenManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult ValidateCitizenFile(Int32 fileId, string userID, [Optional]CITIZEN_FILE aFile)
        {
            FILES updatedFile;

            //Return BadRequest if missing required fields
            if ((fileId < 0) && (string.IsNullOrEmpty(userID)))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //Grab the file row to update
                        updatedFile = aSTNE.FILES.SingleOrDefault(
                                           file => file.FILE_ID == fileId && file.SOURCE_ID == 1639);

                        if (aFile != null)
                        {
                            //Update fields
                            updatedFile.DESCRIPTION = aFile.DESCRIPTION;
                            updatedFile.LATITUDE_DD = aFile.LATITUDE_DD;
                            updatedFile.LONGITUDE_DD = aFile.LONGITUDE_DD;
                            updatedFile.LOCATOR_TYPE_ID = aFile.LOCATOR_TYPE_ID;
                            updatedFile.FILE_DATE = aFile.FILE_DATE;
                            updateFileKeywords(aSTNE.FILE_KEYWORD, updatedFile.FILE_ID, aFile.KEYWORDS);
                        }//end if

                        updatedFile.VALIDATED = 1;
                        updatedFile.VALIDATOR_USERID = userID;
                        updatedFile.DATE_VALIDATED = DateTime.Now;
                        //Save changes
                        aSTNE.SaveChanges();
                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aFile };
            }
            catch
            { return new OperationResult.BadRequest(); }
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
            //Return BadRequest if missing required fields
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        FILES ObjectToBeDeleted = aSTNE.FILES.SingleOrDefault(f => f.FILE_ID == entityId);
                        
                        S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                        aBucket.DeleteObject(BuildFilePath(ObjectToBeDeleted,ObjectToBeDeleted.PATH));

                        //see if a datafile needs to be deleted
                        if (ObjectToBeDeleted.DATA_FILE_ID.HasValue)
                        {
                            DATA_FILE df = aSTNE.DATA_FILE.Where(x => x.DATA_FILE_ID == ObjectToBeDeleted.DATA_FILE_ID).FirstOrDefault();
                            aSTNE.DeleteObject(df);
                            aSTNE.SaveChanges();
                        }
                        //delete it
                        aSTNE.FILES.DeleteObject(ObjectToBeDeleted);

                        //remove any keywords
                        removeKeywords(aSTNE.FILE_KEYWORD, entityId, ObjectToBeDeleted.FILE_KEYWORD.Select(k => k.KEYWORD).ToList());

                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HTTP.DELETE

        [STNRequiresRole(new string[] { AdminRole, CitizenManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteCitizenFile")]
        public OperationResult DeleteCitizenFile(Int32 fileId)
        {
            //Return BadRequest if missing required fields
            if (fileId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        FILES ObjectToBeDeleted = aSTNE.FILES.SingleOrDefault(f => f.FILE_ID == fileId && f.SOURCE_ID == 1639);

                        //delete it
                        if (ObjectToBeDeleted == null) return new OperationResult.NotFound { };

                        //remove item
                        aSTNE.FILES.DeleteObject(ObjectToBeDeleted);

                        //remove associated keywords
                        removeKeywords(aSTNE.FILE_KEYWORD, fileId, ObjectToBeDeleted.FILE_KEYWORD.Select(k => k.KEYWORD).ToList());

                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HTTP.DELETE

        #endregion

        #endregion

        #region Helper Methods
        private List<KEYWORD> getFileKeywords(ObjectSet<FILE_KEYWORD> objectSet, int fileId)
        {
            List<KEYWORD> keywordList = null;
            try
            {

                keywordList = objectSet.Where(fkw => fkw.FILE_ID == fileId)
                                .Select(k => k.KEYWORD).OrderBy(k => k.KEYWORD_ID).ToList();

                return keywordList;
            }
            catch (Exception)
            {
                return null;
            }
        }//end HttpMethod.GET
        
        private string BuildFilePath(FILES uploadFile, string fileName)
        {
            try
            {
                //determine default object name
                // ../SITE/3043/ex.jpg
                List<string> objectName = new List<string>();
                objectName.Add("SITE");
                objectName.Add(uploadFile.SITE_ID.ToString());

                if (uploadFile.HWM_ID != null && uploadFile.HWM_ID > 0)
                {
                    // ../SITE/3043/HWM/7956/ex.jpg
                    objectName.Add("HWM");
                    objectName.Add(uploadFile.HWM_ID.ToString());
                }
                else if (uploadFile.DATA_FILE_ID != null && uploadFile.DATA_FILE_ID > 0)
                {
                    // ../SITE/3043/DATA_FILE/7956/ex.txt
                    objectName.Add("DATA_FILE");
                    objectName.Add(uploadFile.DATA_FILE_ID.ToString());
                }
                else if (uploadFile.INSTRUMENT_ID != null && uploadFile.INSTRUMENT_ID > 0)
                {
                    // ../SITE/3043/INSTRUMENT/7956/ex.jpg
                    objectName.Add("INSTRUMENT");
                    objectName.Add(uploadFile.INSTRUMENT_ID.ToString());
                }
                objectName.Add(fileName);

                return string.Join("/", objectName);
            }
            catch
            {
                return null;
            }
        }
        private bool Exists(ObjectSet<FILES> entityRDS, ref FILES anEntity)
        {
            FILES existingEntity;
            FILES thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => (e.HWM_ID == thisEntity.HWM_ID || thisEntity.HWM_ID <= 0 || thisEntity.HWM_ID == null) &&
                                                               (e.SITE_ID == thisEntity.SITE_ID || thisEntity.SITE_ID <= 0 || thisEntity.SITE_ID == null) &&
                                                               (e.INSTRUMENT_ID == thisEntity.INSTRUMENT_ID || thisEntity.INSTRUMENT_ID <= 0 || thisEntity.INSTRUMENT_ID == null) &&
                                                               (e.DATA_FILE_ID == thisEntity.DATA_FILE_ID || thisEntity.DATA_FILE_ID <= 0 || thisEntity.DATA_FILE_ID == null) &&
                                                               (e.FILETYPE_ID == thisEntity.FILETYPE_ID || thisEntity.FILETYPE_ID <= 0 || thisEntity.FILETYPE_ID == null) &&
                                                               (e.SOURCE_ID == thisEntity.SOURCE_ID || thisEntity.SOURCE_ID <= 0 || thisEntity.SOURCE_ID == null) &&
                                                               (string.Equals(e.PATH.ToUpper(), thisEntity.PATH.ToUpper()) || string.IsNullOrEmpty(thisEntity.PATH)) &&
                                                               (string.Equals(e.FILE_URL.ToUpper(), thisEntity.FILE_URL.ToUpper()) || string.IsNullOrEmpty(thisEntity.FILE_URL)) &&
                                                               (string.Equals(e.PHOTO_DIRECTION.ToUpper(), thisEntity.PHOTO_DIRECTION.ToUpper()) || string.IsNullOrEmpty(thisEntity.PHOTO_DIRECTION)) &&
                                                               (e.LATITUDE_DD == thisEntity.LATITUDE_DD || thisEntity.LATITUDE_DD <= 0 || thisEntity.LATITUDE_DD == null) &&
                                                               (e.LONGITUDE_DD == thisEntity.LONGITUDE_DD || thisEntity.LONGITUDE_DD <= 0 || thisEntity.LONGITUDE_DD == null) &&
                                                               (DateTime.Equals(e.FILE_DATE.Value, thisEntity.FILE_DATE.Value) || !thisEntity.FILE_DATE.HasValue));


                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;

            }
            catch
            {
                return false;
            }
        }
        private bool fileKeywordExists(ObjectSet<FILE_KEYWORD> entityRDS, ref FILE_KEYWORD anEntity)
        {
            FILE_KEYWORD existingEntity;
            FILE_KEYWORD thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => (e.KEYWORD_ID == thisEntity.KEYWORD_ID) &&
                                                               (e.FILE_ID == thisEntity.FILE_ID));

                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        private void updateFileKeywords(ObjectSet<FILE_KEYWORD> objectSet, decimal fileId, List<KEYWORD> keywordList)
        {
            // Keyword list difference

            List<KEYWORD> currentList = objectSet.Where(k => k.FILE_ID == fileId)
                                                 .Select(k => k.KEYWORD).ToList();
            //listA.Except(listB) will give you all of the items in listA that are not in listB
            removeKeywords(objectSet, fileId, currentList.Except(keywordList).ToList());
            addKeywords(objectSet, fileId, keywordList.Except(currentList).ToList());
        }//end updateFileKeywords
        private void addKeywords(ObjectSet<FILE_KEYWORD> objectSet, decimal fileId, List<KEYWORD> keywordList)
        {
            foreach (KEYWORD item in keywordList)
            {
                FILE_KEYWORD filekwrd = new FILE_KEYWORD()
                {
                    FILE_ID = fileId,
                    KEYWORD_ID = item.KEYWORD_ID
                };

                if (!fileKeywordExists(objectSet, ref filekwrd))
                    objectSet.AddObject(filekwrd);

            }//next keyword
        }//end addKeywords
        private void removeKeywords(ObjectSet<FILE_KEYWORD> objectSet, decimal fileId, List<KEYWORD> ListDiff)
        {
            foreach (KEYWORD item in ListDiff)
            {
                FILE_KEYWORD obj = objectSet.FirstOrDefault(fk => fk.KEYWORD_ID == item.KEYWORD_ID && fk.FILE_ID == fileId);
                if (obj != null)
                    objectSet.DeleteObject(obj);
            }//next
        }//end removeKeywords
        private MediaType GetContentType(string path)
        {
            string theExtension = System.IO.Path.GetExtension(path);
            switch (theExtension.ToUpper())
            {
                case ".JPG":
                    return new MediaType("image/jpeg");
                case ".GIF":
                    return new MediaType("image/gif");
                case ".CSV":
                    return new MediaType("text/csv");
                case ".TXT":
                    return new MediaType("text/plain");
                case ".DOC":
                    return new MediaType("application/msword");
                case ".DOCX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                case ".DOTX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.wordprocessingml.template");
                case ".POTX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.presentationml.template");
                case ".PPS":
                    return new MediaType("application/vnd.ms-powerpoint");
                case ".PPSX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.presentationml.slideshow");
                case ".PPT":
                    return new MediaType("application/vnd.ms-powerpoint");
                case ".PPTX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.presentationml.presentation");
                case ".SLDX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.presentationml.slide");
                case ".XLA":
                    return new MediaType("application/vnd.ms-excel");
                case ".XLAM":
                    return new MediaType("application/vnd.ms-excel.addin.macroEnabled.12");
                case ".XLS":
                    return new MediaType("application/vnd.ms-excel");
                case ".XLSB":
                    return new MediaType("application/vnd.ms-excel.sheet.binary.macroEnabled.12");
                case ".XLSX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                case ".XLT":
                    return new MediaType("application/vnd.ms-excel");
                case ".XLTX":
                    return new MediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.template");
                case ".XML":
                    return new MediaType("application/xml");
                default:
                    return null;

            }
        }//end HttpMethod.GET

        #endregion

    }//end class FileHandler
}//end namespace
