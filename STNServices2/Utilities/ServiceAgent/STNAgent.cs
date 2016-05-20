﻿using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO;

using WiM.Utilities;
using WiM.Utilities.ServiceAgent;
using WiM.Security;
using WiM.Exceptions;

using WiM.Utilities.Storage;
using WiM.Resources;

using STNDB;

using Newtonsoft.Json;
using OpenRasta.IO;
using OpenRasta.Web;

//using STNServices2.Resources;
using Ionic.Zip;


namespace STNServices2.Utilities.ServiceAgent
{
    internal class STNAgent:DBAgentBase
    {
               #region "Events"
        #endregion
        #region "Properties"
        #endregion
        #region "Collections & Dictionaries"
        
        #endregion
        #region "Constructor and IDisposable Support"
        #region Constructors
        internal STNAgent(Boolean include = false)
            : this("fradmin", new EasySecureString("Ij7E9doC"), include)
        {        
        }
        internal STNAgent(string username, EasySecureString password, Boolean include = false)
            : base(ConfigurationManager.ConnectionStrings["STNDBEntities"].ConnectionString)
        {
            this.context = new  STNDBEntities (string.Format(connectionString, username.ToLower(), password.decryptString()));
            this.context.Configuration.ProxyCreationEnabled = include;
        }
        #endregion
        #region IDisposable Support
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // TODO:Dispose managed resources here.
                    //ie component.Dispose();

                }//EndIF

                // TODO:Call the appropriate methods to clean up
                // unmanaged resources here.
                //ComRelease(Extent);

                // Note disposing has been done.
                disposed = true;


            }//EndIf
        }//End Dispose
        #endregion
        #endregion
        #region "Methods"
        internal void DeleteFile(Int32 fileID)
        {
            try
            {
                file ObjectToBeDeleted = this.Select<file>().SingleOrDefault(f => f.file_id == fileID);

                S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"],"","");
                aBucket.DeleteObject(BuildFilePath(ObjectToBeDeleted, ObjectToBeDeleted.path));
                sm(MessageType.info, fileID + ": Deleted from storage");
                this.Delete<file>(ObjectToBeDeleted);
                sm(MessageType.info, fileID + ": Deleted from DB");
            }
            catch (Exception ex)
            {
                sm(MessageType.error, fileID +":"+ ex.Message);
                throw ;
            }
        }
        internal void AddFile(file uploadFile, System.IO.MemoryStream memoryStream)
        {
            S3Bucket aBucket = null;
            try
            {
                this.Add<file>(uploadFile);
                
                //Upload to S3
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], "", "");
                aBucket.PutObject(BuildFilePath(uploadFile, uploadFile.path), memoryStream);
                sm(MessageType.info, uploadFile.path + ": added");
            }
            catch (Exception ex)
            {
                sm(WiM.Resources.MessageType.error, "FileID: " + uploadFile.path +" exception: "+ ex.Message);
                throw;
            }
            finally { 
                if(aBucket != null){ aBucket.Dispose(); aBucket = null;}
            }
        }
        internal InMemoryFile GetFileItem(file afile)
        {
            file aFile = null;
            S3Bucket aBucket = null;
            InMemoryFile fileItem = null;
            try
            {
                    if (aFile == null || aFile.path == null || String.IsNullOrEmpty(aFile.path)) throw new WiM.Exceptions.NotFoundRequestException();

                    string directoryName = string.Empty;
                    aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], "", "");
                    directoryName = BuildFilePath(aFile, aFile.path);


                    fileItem = new InMemoryFile(aBucket.GetObject(directoryName));
                    fileItem.ContentType = GetContentType(aFile.path);
                    return fileItem;
            }
            catch (Exception ex)
            {
                sm(WiM.Resources.MessageType.error, "Failed to include item: "+afile.path+" exception: " + ex.Message);
                throw;
            }
        }
        internal InMemoryFile GetFileItemZip(List<file> files)
        {
            string directoryName = string.Empty;
            S3Bucket aBucket = null;
            InMemoryFile fileItem = null;

            MemoryStream ms = null;
            try
            {
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], "", "");
                using (ZipFile zip = new ZipFile())
                {
                    ms = new MemoryStream();
                    foreach (file f in files)
                    {
                        //Note: if empty folder exists, folder will not be included.
                        zip.AddEntry(f.path, GetFileItem(f).OpenStream() );
                        zip.Comment = "Downloaded: " + f.path;
                    }//end foreach
                    zip.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Flush();
                    fileItem = new InMemoryFile(ms);
                    fileItem.FileName = "EventFileDownload.zip";
                    
                };//end using zipfile
                return fileItem;
            }
            catch (Exception)
            {
                throw;                
            }
        }
        #endregion
        #region "Helper Methods"
        IQueryable<T> getTable<T>(object[] args) where T : class,new()
        {
            try
            {
                string sql = String.Format(getSQLStatement(typeof(T).Name),args);
                return context.Database.SqlQuery<T>(sql).AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string getSQLStatement(string type) 
        {
            string sql = string.Empty;
            switch (type)
            {                                    
                default:
                    throw new Exception("No sql for table " + type);
            }//end switch;
        
        }
        private string BuildFilePath(file uploadFile, string fileName)
        {
            try
            {
                //determine default object name
                // ../SITE/3043/ex.jpg
                List<string> objectName = new List<string>();
                objectName.Add("SITE");
                objectName.Add(uploadFile.site_id.ToString());

                if (uploadFile.hwm_id != null && uploadFile.hwm_id > 0)
                {
                    // ../SITE/3043/HWM/7956/ex.jpg
                    objectName.Add("HWM");
                    objectName.Add(uploadFile.hwm_id.ToString());
                }
                else if (uploadFile.data_file_id != null && uploadFile.data_file_id > 0)
                {
                    // ../SITE/3043/DATA_FILE/7956/ex.txt
                    objectName.Add("DATA_FILE");
                    objectName.Add(uploadFile.data_file_id.ToString());
                }
                else if (uploadFile.instrument_id != null && uploadFile.instrument_id > 0)
                {
                    // ../SITE/3043/INSTRUMENT/7956/ex.jpg
                    objectName.Add("INSTRUMENT");
                    objectName.Add(uploadFile.instrument_id.ToString());
                }
                objectName.Add(fileName);

                return string.Join("/", objectName);
            }
            catch
            {
                return null;
            }
        }
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
        #region "Structures"
        //A structure is a value type. When a structure is created, the variable to which the struct is assigned holds
        //the struct's actual data. When the struct is assigned to a new variable, it is copied. The new variable and
        //the original variable therefore contain two separate copies of the same data. Changes made to one copy do not
        //affect the other copy.

        //In general, classes are used to model more complex behavior, or data that is intended to be modified after a
        //class object is created. Structs are best suited for small data structures that contain primarily data that is
        //not intended to be modified after the struct is created.
        #endregion
        #region "Asynchronous Methods"

        #endregion
        #region "Enumerated Constants"
        #endregion



        
    }//end class
}//end namespace