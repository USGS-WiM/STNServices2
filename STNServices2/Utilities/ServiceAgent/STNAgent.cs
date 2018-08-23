using System;
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
            : this(ConfigurationManager.AppSettings["Username"], new EasySecureString(ConfigurationManager.AppSettings["Password"]), include)     
        {        
        }
        internal STNAgent(string username, EasySecureString password, Boolean include = false)
            : base(ConfigurationManager.AppSettings["connectionString"])
        {
            this.context = new STNDBEntities(string.Format(connectionString, ConfigurationManager.AppSettings["Username"], new EasySecureString(ConfigurationManager.AppSettings["Password"]).decryptString()));
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
        internal void RemoveFileItem(file fileToRemove)
        {
            try
            {
                
                S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                            ConfigurationManager.AppSettings["AWSSecretKey"]);
                string directoryName = string.Empty;
                directoryName = fileToRemove.path + "/" + fileToRemove.name;// 
                aBucket.DeleteObject(directoryName);
                sm(MessageType.info, fileToRemove.file_id + ": Deleted from storage");
            }
            catch (Exception ex)
            {
                sm(MessageType.error, fileToRemove.file_id + ":" + ex.Message);
                throw ;
            }
        }
        internal void PutFileItem(file uploadFile, System.IO.MemoryStream memoryStream)
        {
            S3Bucket aBucket = null;
            try
            {

                int eventId = 0;
                if (uploadFile.hwm_id.HasValue && uploadFile.hwm_id > 0)
                {
                    eventId = this.Select<hwm>().FirstOrDefault(h => h.hwm_id == uploadFile.hwm_id).event_id.Value;                    
                }
                else if (uploadFile.instrument_id.HasValue && uploadFile.instrument_id > 0)
                {
                    eventId = this.Select<instrument>().FirstOrDefault(h => h.instrument_id == uploadFile.instrument_id).event_id.Value;                    
                }
                //Upload to S3
                uploadFile.path = BuildNewpath(uploadFile, eventId);
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                            ConfigurationManager.AppSettings["AWSSecretKey"]);

                aBucket.PutObject(uploadFile.path + "/" + uploadFile.name, memoryStream);
                this.Update<file>(uploadFile);
                sm(MessageType.info, uploadFile.path + ": added");
            }
            catch (Exception ex)
            {
                sm(WiM.Resources.MessageType.error, "FileID: " + uploadFile.path + " exception: " + ex.Message);
                throw;
            }
            finally
            {
                if (aBucket != null) { aBucket.Dispose(); aBucket = null; }
            }
        }
        internal void AddFile(file uploadFile, System.IO.MemoryStream memoryStream)
        {
            S3Bucket aBucket = null;
            try
            {          
                
                int eventId = 0;
                if (uploadFile.hwm_id.HasValue && uploadFile.hwm_id > 0)
                {
                    eventId = this.Select<hwm>().FirstOrDefault(h => h.hwm_id == uploadFile.hwm_id).event_id.Value;
                    //eventId = uploadFile.hwm.@event.event_id;
                }
                else if (uploadFile.instrument_id.HasValue && uploadFile.instrument_id > 0)
                {
                    eventId = this.Select<instrument>().FirstOrDefault(h => h.instrument_id == uploadFile.instrument_id).event_id.Value;
                    //eventId = uploadFile.instrument.@event.event_id;
                }
                //Upload to S3
                uploadFile.path = BuildNewpath(uploadFile, eventId);
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                            ConfigurationManager.AppSettings["AWSSecretKey"]);

                aBucket.PutObject(uploadFile.path + "/" + uploadFile.name, memoryStream);
                this.Add<file>(uploadFile);
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
        internal InMemoryFile GetHWMSpreadsheetItem()
        {
            S3Bucket aBucket = null;
            InMemoryFile fileItem = null;
            try
            {
                string directoryName = string.Empty;
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                        ConfigurationManager.AppSettings["AWSSecretKey"]);
                directoryName = "cleanHistoricHWMUploadSpreadsheet.xlsx";
                var fileStream = aBucket.GetObject(directoryName);

                fileItem = new InMemoryFile(fileStream);
                fileItem.ContentType = GetContentType(".XLSX");
               // fileItem.Length = fileStream != null ? fileStream.Length : 0;
                fileItem.FileName = "cleanHistoricHWMUploadSpreadsheet.xlsx";
                return fileItem;

            }
            catch (Exception ex)
            {
                sm(WiM.Resources.MessageType.error, "Failed to include item: cleanHistoricHWMUploadSpreadsheet.xlsx exception: " + ex.Message);
                throw;
            }
        }
        internal InMemoryFile GetTESTdataItem()
        {
            S3Bucket aBucket = null;
            InMemoryFile fileItem = null;
            try
            {
                string directoryName = string.Empty;
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                        ConfigurationManager.AppSettings["AWSSecretKey"]);
                directoryName = "data.json";
                var fileStream = aBucket.GetObject(directoryName);

                fileItem = new InMemoryFile(fileStream);
                fileItem.ContentType = GetContentType(".json");
                // fileItem.Length = fileStream != null ? fileStream.Length : 0;
                fileItem.FileName = "data.json";
                return fileItem;

            }
            catch (Exception ex)
            {
                sm(WiM.Resources.MessageType.error, "Failed to include item: data.json exception: " + ex.Message);
                throw;
            }
        }
        internal InMemoryFile GetFileItem(file afile)
        {
            //file aFile = null;
            S3Bucket aBucket = null;
            InMemoryFile fileItem = null;
            try
            {
                if (afile == null || afile.path == null || String.IsNullOrEmpty(afile.path)) throw new WiM.Exceptions.NotFoundRequestException();
                    
                string directoryName = string.Empty;
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"], 
                                        ConfigurationManager.AppSettings["AWSSecretKey"]);
                directoryName = afile.path + "/" + afile.name;
                var fileStream = aBucket.GetObject(directoryName);
                
                fileItem = new InMemoryFile(fileStream);
                fileItem.ContentType = GetContentType(afile.name);
                fileItem.Length = fileStream != null ? fileStream.Length : 0;
                fileItem.FileName = afile.name;                
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
                aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"], ConfigurationManager.AppSettings["AWSAccessKey"],
                                            ConfigurationManager.AppSettings["AWSSecretKey"]);
                using (ZipFile zip = new ZipFile())
                {
                    ms = new MemoryStream();
                    
                    foreach (file f in files)
                    {
                        //Note: if empty folder exists, folder will not be included.
                        if (!zip.ContainsEntry(f.path + "/" + f.name))
                        {
                            zip.AddEntry(f.path + "/" + f.name, GetFileItem(f).OpenStream());
                            zip.Comment = "Downloaded: " + f.path;
                        }
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
        //public dynamic GetGeocodeLocation(decimal latitudeV, decimal longitudeV)
        //{
        //    string uri = ConfigurationManager.AppSettings["GeocoderServer"];
        //    RestSharp.RestRequest request = getRestRequest(String.Format(uri), wucode, token), getBody(workspaceID, startyear, endyear, "pjson"));
        //    request.AddHeader("Referer", "Referer: http://streamstats.ags.cr.usgs.gov/streamstats");

        //    return Execute(request);
        //}
        #endregion
        #region "Helper Methods"
        public IQueryable<T> getTable<T>(object[] args) where T : class,new()
        {
            try
            {
                string sql = string.Empty;
                if (args[0] != null)
                {
                    if (args[0].ToString() == "baro_view" || args[0].ToString() == "met_view" || args[0].ToString() == "rdg_view" || args[0].ToString() == "stormtide_view" || args[0].ToString() == "waveheight_view" ||
                         args[0].ToString() == "pressuretemp_view" || args[0].ToString() == "therm_view" || args[0].ToString() == "webcam_view" || args[0].ToString() == "raingage_view")
                        sql = String.Format(getSQLStatement(args[0].ToString()));
                }
                else
                    sql = String.Format(getSQLStatement(typeof(T).Name), args);

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
                case "peak_view":
                    return @"SELECT DISTINCT pk.peak_summary_id, pk.peak_stage, pk.peak_date, vd.datum_name, 
                        CASE
                            WHEN NOT hwm_v.peak_summary_id IS NULL THEN hwm_v.latitude_dd
                            WHEN NOT df_v.peak_summary_id IS NULL THEN df_v.latitude_dd
                            ELSE NULL::double precision
                            END AS latitude,
                        CASE
                            WHEN NOT hwm_v.peak_summary_id IS NULL THEN hwm_v.site_id
                            WHEN NOT df_v.peak_summary_id IS NULL THEN df_v.site_id
                            ELSE NULL::integer
                            END AS site_id,
                        CASE
                            WHEN NOT hwm_v.peak_summary_id IS NULL THEN hwm_v.longitude_dd
                            WHEN NOT df_v.peak_summary_id IS NULL THEN df_v.longitude_dd
                            ELSE NULL::double precision
                            END AS longitude,
                        CASE
                            WHEN NOT hwm_v.peak_summary_id IS NULL THEN hwm_v.event_name
                            WHEN NOT df_v.peak_summary_id IS NULL THEN df_v.event_name
                            ELSE NULL::character varying
                            END AS event_name
                    FROM peak_summary pk
                    LEFT JOIN vertical_datums vd ON pk.vdatum_id = vd.datum_id
                    LEFT JOIN ( SELECT df.peak_summary_id, sv.latitude_dd, sv.longitude_dd, sv.site_id, sv.event_name
                                FROM sensor_view sv, data_file df
                                WHERE df.instrument_id = sv.instrument_id AND NOT df.peak_summary_id IS NULL) df_v ON pk.peak_summary_id = df_v.peak_summary_id
                    LEFT JOIN ( SELECT hwm.peak_summary_id, hwm.latitude_dd, hwm.longitude_dd, hwm.site_id, e.event_name
                                FROM hwm hwm, events e
                                WHERE hwm.event_id = e.event_id AND NOT hwm.peak_summary_id IS NULL) hwm_v ON pk.peak_summary_id = hwm_v.peak_summary_id;";                    
                case "baro_view":
                    return @"SELECT * FROM barometric_view;";
                case "met_view":
                    return @"SELECT * FROM meteorological_view;";
                case "rdg_view":
                    return @"SELECT * FROM rapid_deployment_view;";
                case "stormtide_view":
                    return @"SELECT * FROM storm_tide_view;";
                case "waveheight_view":
                    return @"SELECT * FROM wave_height_view;";
                case "pressuretemp_view":
                    return @"SELECT * FROM pressuretemp_view;";
                case "therm_view":
                    return @"SELECT * FROM therm_view;";
                case "webcam_view":
                    return @"SELECT * FROM webcam_view;";
                case "raingage_view":
                    return @"SELECT * FROM raingage_view;";
                case "dataFile_view":
                    return @"SELECT s.site_id, s.site_no, s.latitude_dd, s.longitude_dd, s.county, s.state, s.waterbody, i.instrument_id, i.event_id, i.sensor_type_id, st.sensor, i.deployment_type_id, dt.method,
                                i.location_description, i.sensor_brand_id, sb.brand_name, f.file_id, f.name, f.file_date, f.script_parent, f.data_file_id, df.good_start, df.good_end, df.collect_date
                            FROM sites s
                            LEFT JOIN instrument i ON s.site_id = i.site_id
                            LEFT JOIN files f ON i.instrument_id = f.instrument_id
                            LEFT JOIN sensor_type st ON i.sensor_type_id = st.sensor_type_id
                            LEFT JOIN deployment_type dt ON i.deployment_type_id = dt.deployment_type_id
                            LEFT JOIN sensor_brand sb ON i.sensor_brand_id = sb.sensor_brand_id
                            LEFT JOIN data_file df ON f.data_file_id = df.data_file_id
                            WHERE i.sensor_type_id = 1 AND i.deployment_type_id = 3 AND f.filetype_id = 2;";
                default:
                    throw new Exception("No sql for table " + type);
            }//end switch;
        
        }
        private static string BuildNewpath(file FileItem, decimal eventId)
        {
            //SITES
            //  -SITE_123
            //      *contains all site 123 specific files, such as site sketch etc, images etc. 
            //EVENTS
            //  -EVENT_123
            //      -SITE_456
            //          * contains all event 123 specific files for site 456, such as hwm, data, and image files etc.
            try
            {
                List<string> objectName = new List<string>();
                if (eventId > 0)
                {
                    // ../SITE/3043/HWM/7956/ex.jpg
                    objectName.Add("EVENTS");
                    objectName.Add("EVENT_" + eventId);
                    objectName.Add("SITE_" + FileItem.site_id);
                }
                else
                {
                    objectName.Add("SITES");
                    objectName.Add("SITE_" + FileItem.site_id);
                }

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
                case ".PNG":
                    return new MediaType("image/png");
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
                case ".PDF":
                    return new MediaType("application/pdf");
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
        #region Geocoder Helper
        private string getBody(decimal latitudeVal, decimal longitudeVal)
        {
            List<string> body = new List<string>();
            try
            {
                body.Add("vintage=4");
                body.Add("format=json");
                body.Add("x=" + longitudeVal);
                body.Add("y=" + latitudeVal);
                return string.Join("&", body);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion Geocoder helper
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
