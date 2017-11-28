using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

using OpenRasta.IO;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using WiM.Utilities.ServiceAgent;
using WiM.Resources;
using WiM.Resources.Spatial;
using WiM.Utilities.Storage;
using WiM.Exceptions;

using STNServices2.Resources;
using STNDB;
using System.Text;
using OpenRasta.Web;

namespace STNServices2.Utilities.ServiceAgent
{
    public class STNServiceAgent:  ExternalProcessServiceAgentBase
    {
        #region Properties
        private DirectoryInfo workspaceDirectory { get; set; }
        private string combinedPath { get; set; }
        
        // storm parts
        private string airDFName { get; set; }
        private string waterDFName { get; set; }
        public Boolean initialized { get; private set; }
        private member scriptMember { get; set; }
        private data_file seaDataFile { get; set; }
        private data_file airDataFile { get; set; }
        private site waterSite { get; set; }
        
        // air parts
        private string pressureDFName { get; set; }
        public Boolean pressureInitialized { get; private set; }
        private member pressureScriptMember { get; set; }
        private data_file pressureDataFile { get; set; }

        // choper parts
        private instrument chopperInstrument { get; set; }
        public Boolean chopperInitialized { get; private set; }
        private string chopperDFName { get; set; }
        private string chopperPressureType { get; set; }

        STNAgent sa = new STNAgent();
        private int value;
        private MemoryStream memoryStream;
        private string v;

        #endregion Properties

        #region Constructors
        // for storm script
        public STNServiceAgent(Int32 pressureSensorDFID, Int32 waterSensorDFID, string username)
            :base(ConfigurationManager.AppSettings["EXEPath"], Path.Combine(new String[] {AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts"}))
        {
            StormInit(pressureSensorDFID, waterSensorDFID, username);
        }
        // for air only script
        public STNServiceAgent(Int32 pressureSensorDFID, string username)
            :base(ConfigurationManager.AppSettings["EXEPath"], Path.Combine(new String[] {AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts"}))
        {
            AirInit(pressureSensorDFID, username);
        }
        // for chopper script
        public STNServiceAgent(Int32 instrument_id, MemoryStream ms, string fileName) //uploadFile.instrument_id, memoryStream
            : base(ConfigurationManager.AppSettings["EXEPath"], Path.Combine(new String[] { AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts" }))
        {
            ChopperInit(instrument_id, ms, fileName);
        }
       
        #endregion Constructors

        #region Methods
        // air and sea together
        public Boolean RunStormScript(bool hz)
        {                     
            try
            {
                // execute the stn_script
                Execute(getProcessRequest(ConfigurationManager.AppSettings["STN_Script"], getBody(hz)));
             
                // remove temp air/sea files
                File.Delete(airDFName);
                File.Delete(waterDFName);
                
                // put everything in s3 under air file
                DirectoryInfo di = new DirectoryInfo(combinedPath);

                foreach(var f in di.GetFiles())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //create new file from air and update the fields
                        file newFile = new file();                         
                        newFile.file_date = DateTime.Now;
                        newFile.site_id = seaDataFile.files.FirstOrDefault().site_id;
                        newFile.name = f.Name;
                        newFile.instrument_id = seaDataFile.instrument_id;
                        newFile.last_updated = DateTime.Now;
                        newFile.last_updated_by = scriptMember.member_id;
                        newFile.script_parent = seaDataFile.files.FirstOrDefault().file_id.ToString() + "_" + airDataFile.files.FirstOrDefault().file_id.ToString();
                        // if photo
                        if (f.Extension == ".png")
                        {
                            newFile.filetype_id = 1;
                            newFile.photo_date = DateTime.Now;
                            newFile.description = "Photo File generated from Storm Script processing using water data file: " + seaDataFile.files.FirstOrDefault().name + " and air data file: " + airDataFile.files.FirstOrDefault().name;
                            //create source
                            source createdSource = new source();
                            createdSource.source_name = scriptMember.fname + " " + scriptMember.lname;
                            createdSource.agency_id = scriptMember.agency_id;
                            createdSource.last_updated = DateTime.Now;
                            createdSource.last_updated_by = scriptMember.member_id;
                            createdSource = sa.Add<source>(createdSource);
                            newFile.source_id = createdSource.source_id;                                
                        }
                        else if (f.Extension == ".csv")
                        {
                            //this is the output showing how the script ran (successful or not) save as 'Other'                           
                            newFile.filetype_id = 7;
                            newFile.description = "Output csv file generated from Storm Script processing using water data file: " + seaDataFile.files.FirstOrDefault().name + " and air data file: " + airDataFile.files.FirstOrDefault().name + " This file shows success/errors from the processing.";
                            //create source
                            source createdSource = new source();
                            createdSource.source_name = scriptMember.fname + " " + scriptMember.lname;
                            createdSource.agency_id = scriptMember.agency_id;
                            createdSource.last_updated = DateTime.Now;
                            createdSource.last_updated_by = scriptMember.member_id;
                            createdSource = sa.Add<source>(createdSource);
                            newFile.source_id = createdSource.source_id;
                        }
                        else
                        {
                            //it's a data file
                            newFile.filetype_id = 2;                            
                            newFile.description = "Data File generated from Storm Script processing using water data file: " + seaDataFile.files.FirstOrDefault().name + " and air data file: " + airDataFile.files.FirstOrDefault().name;
                            //create new data_file and new file
                            data_file newDF = new data_file();
                            // assign this datafile's script_parent to be the waterDFID_airDFID used to make it
                            newDF.good_start = seaDataFile.good_start;
                            newDF.good_end = seaDataFile.good_end;
                            newDF.processor_id = scriptMember.member_id;
                            newDF.instrument_id = seaDataFile.instrument_id;
                            newDF.collect_date = DateTime.Now;
                            newDF.time_zone = "UTC";
                            newDF.last_updated = DateTime.Now;
                            newDF.last_updated_by = scriptMember.member_id;
                            newDF = sa.Add<data_file>(newDF);
                            newFile.data_file_id = newDF.data_file_id;
                        }
                                     
                        // create new memory stream, copy the f to it, and then save it in s3
                        f.OpenRead().CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        sa.AddFile(newFile, memoryStream);
                    } // end using MemoryStream
                }// end foreach file
                                
                // update that this is the script_parent (what if it failed?? Don't want this to be flagged as parent, if nothing was processed)
                // get the 'fileName_output.csv' file and read each row, look for 'complete with no errors', if present, update script_parent

                file parentFile = seaDataFile.files.FirstOrDefault();
                string airFileID = airDataFile.files.FirstOrDefault().file_id.ToString();
                string scriptParentIDs = parentFile.file_id.ToString() + "_" + airFileID;
                file outputFileCSV = sa.Select<file>().FirstOrDefault(f => f.script_parent == scriptParentIDs && f.filetype_id == 7);
                InMemoryFile fileItem = sa.GetFileItem(outputFileCSV);
                Boolean isValidOutput = false;
                using (var fileStream = fileItem.OpenStream())
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Contains("complete with no errors"))
                            isValidOutput = true;
                    }
                }
                if (isValidOutput)
                {
                    parentFile.last_updated = DateTime.Now;
                    parentFile.last_updated_by = scriptMember.member_id;
                    parentFile.script_parent = "true";
                    sa.Update<file>(parentFile.file_id, parentFile);
                }

                //delete directory
                Directory.Delete(combinedPath, true);

                return true;
            }
            catch (Exception ex)
            {
                Directory.Delete(combinedPath, true); // delete the directory either way since output.csv should specify errors and client needs to know when complete
                throw new Exception(ex.Message);
            }
        }
        
        //air only
        public Boolean RunAirScript()
        {
            try
            {
                // execute the stn_script
                Execute(getProcessRequest(ConfigurationManager.AppSettings["STN_Air_Script"], getAirBody()));

                // remove temp air/sea files
                File.Delete(pressureDFName);

                // put everything in s3 under air file
                DirectoryInfo di = new DirectoryInfo(combinedPath);

                foreach (var f in di.GetFiles())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //create new file from air and update the fields
                        file newFile = new file();
                        newFile.file_date = DateTime.Now;
                        newFile.site_id = pressureDataFile.files.FirstOrDefault().site_id;
                        newFile.name = f.Name;
                        newFile.instrument_id = pressureDataFile.instrument_id;
                        newFile.last_updated = DateTime.Now;
                        newFile.last_updated_by = pressureScriptMember.member_id;
                        newFile.script_parent = pressureDataFile.files.FirstOrDefault().file_id.ToString();
                        // if photo
                        if (f.Extension == ".png")
                        {
                            newFile.filetype_id = 1;
                            newFile.photo_date = DateTime.Now;
                            newFile.description = "Photo File generated from Air Script processing.";
                            //create source
                            source createdSource = new source();
                            createdSource.source_name = pressureScriptMember.fname + " " + pressureScriptMember.lname;
                            createdSource.agency_id = pressureScriptMember.agency_id;
                            createdSource.last_updated = DateTime.Now;
                            createdSource.last_updated_by = pressureScriptMember.member_id;
                            createdSource = sa.Add<source>(createdSource);
                            newFile.source_id = createdSource.source_id;
                        }
                        else if (f.Extension == ".csv")
                        {
                            //this is the output showing how the script ran (successful or not) save as 'Other'                           
                            newFile.filetype_id = 7;
                            newFile.description = "Output csv file generated from Air Script processing. Shows success/errors from processing.";
                            //create source
                            source createdSource = new source();
                            createdSource.source_name = pressureScriptMember.fname + " " + pressureScriptMember.lname;
                            createdSource.agency_id = pressureScriptMember.agency_id;
                            createdSource.last_updated = DateTime.Now;
                            createdSource.last_updated_by = pressureScriptMember.member_id;
                            createdSource = sa.Add<source>(createdSource);
                            newFile.source_id = createdSource.source_id;
                        }
                        else
                        {
                            //it's a data file
                            newFile.filetype_id = 2;
                            newFile.description = "Data File generated from Air Script processing.";
                            //create new data_file and new file
                            data_file newDF = new data_file();
                            newDF.good_start = pressureDataFile.good_start;
                            newDF.good_end = pressureDataFile.good_end;
                            newDF.processor_id = pressureScriptMember.member_id;
                            newDF.instrument_id = pressureDataFile.instrument_id;
                            newDF.collect_date = DateTime.Now;
                            newDF.time_zone = "UTC";
                            newDF.last_updated = DateTime.Now;
                            newDF.last_updated_by = pressureScriptMember.member_id;
                            newDF = sa.Add<data_file>(newDF);
                            newFile.data_file_id = newDF.data_file_id;
                        }

                        // create new memory stream, copy the f to it, and then save it in s3
                        f.OpenRead().CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        sa.AddFile(newFile, memoryStream);
                    } // end using MemoryStream
                }// end foreach file

                // update that this is the script_parent (what if it failed?? Don't want this to be flagged as parent, if nothing was processed)
                // get the 'fileName_output.csv' file and read each row, look for 'complete with no errors', if present, update script_parent
                
                file parentFile = pressureDataFile.files.FirstOrDefault();
                file outputFileCSV = sa.Select<file>().FirstOrDefault(f => f.script_parent == parentFile.file_id.ToString() && f.filetype_id == 7);
                InMemoryFile fileItem = sa.GetFileItem(outputFileCSV);
                Boolean isValidOutput = false;
                using (var fileStream = fileItem.OpenStream())
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Contains("complete with no errors"))
                            isValidOutput = true;
                    }
                }
                if (isValidOutput)
                {
                    parentFile.script_parent = "true";
                    parentFile.last_updated = DateTime.Now;
                    parentFile.last_updated_by = pressureScriptMember.member_id;
                    sa.Update<file>(parentFile.file_id, parentFile);
                }

                //delete directory
                Directory.Delete(combinedPath, true);

                return true;
            }
            catch (Exception ex)
            {
                Directory.Delete(combinedPath, true); // delete the directory either way since output.csv should specify errors and client needs to know when complete
                throw new Exception(ex.Message);
            }
        }
        
        // chopper
        public dynamic RunChopperScript()
        {
            dynamic jsonfile = null;
            Boolean isValidOutput = false;
            string error = "";
            try
            {
                // execute the stn_script
                Execute(getProcessRequest(ConfigurationManager.AppSettings["STN_Chopper_Script"], getChopperBody()));
                File.Delete(chopperDFName);

                DirectoryInfo di = new DirectoryInfo(combinedPath);
                foreach (var f in di.GetFiles().Where(fil => fil.Extension == ".json" || fil.Extension == ".csv"))
                {
                    using (StreamReader r = new StreamReader(System.IO.Path.Combine(combinedPath, f.Name)))
                    {       
                        if (f.Extension == ".json")
                        {                        
                            //write the file ... 
                            string j = r.ReadToEnd();
                            jsonfile = JsonConvert.DeserializeObject<dynamic>(j);
                        }//end if "json"
                    
                        if (f.Extension == ".csv")
                        {
                            String line;                                                        
                            while ((line = r.ReadLine()) != null)
                            {
                                if (line.Split(',').ToList<string>()[6] == "failed")
                                {
                                    error = line.Split(',').ToList<string>()[1];
                                    isValidOutput = false;
                                }
                                else
                                {
                                    isValidOutput = true;
                                }
                            }
                        }//end "csv"
                    }//end streamreader
                } //end foreach

                // if the output is valid return the jsonfile else return invalid json result for handler to handle
                if (!isValidOutput)
                {
                    error = "{'Error': '" + error + "'}";
                    jsonfile = JsonConvert.DeserializeObject<dynamic>(error);
                }
                return jsonfile;
            }
            catch (Exception ex)
            {
                Directory.Delete(combinedPath, true);
                throw new Exception(ex.Message);
            }
            finally
            {
                //delete directory
                Directory.Delete(combinedPath, true);
            }
        }

        private dynamic deserializeFromStream(MemoryStream memoryStream)
        {
            dynamic jsonfile = null;
            using (var reader = new StreamReader(memoryStream))
            {
                string json = reader.ReadToEnd();
                jsonfile = JsonConvert.DeserializeObject(json);

                /*  reader.Close();
                  reader.Dispose();*/
            } // end streamreader
            return jsonfile;
        }

        //init air and see script
        private void StormInit(Int32 presDFID, Int32 watDFID, string username)
        {
            try
            {
                combinedPath = "";
                //get water level's siteid and eventid
                seaDataFile = sa.Select<data_file>()
                    .Include(df => df.files)
                    .Include(df => df.instrument)
                    .Include("instrument.instr_collection_conditions")
                    .Include("instrument.instrument_status")
                    .Include("instrument.sensor_brand")
                    .Include("instrument.instrument_status.vertical_datums")
                    .Include("instrument.site")
                    .FirstOrDefault(df => df.data_file_id == watDFID);

                // create directory;
                string path1 = ConfigurationManager.AppSettings["STNRepository"].ToString();
                string path2 = seaDataFile.instrument.site_id.ToString();
                string path3 = "INSTRUMENT" + seaDataFile.instrument_id.ToString();
                string path4 = "DFID" + seaDataFile.data_file_id.ToString() + DateTime.Now.ToString("MMddyyyyHHmmss");
                combinedPath = System.IO.Path.Combine(path1, path2 + path3 + path4);
                workspaceDirectory = System.IO.Directory.CreateDirectory(combinedPath);
                
                //bring over files it needs from s3 (air/sea) and store in temp location
                airDFName = moveFile(presDFID);
                waterDFName = moveFile(watDFID);
                // get member
                scriptMember = sa.Select<member>().Include(m=> m.agency).FirstOrDefault(m => m.username == username);
                //get air datafile
                airDataFile = sa.Select<data_file>()
                    .Include(df => df.files)
                    .Include(df => df.instrument)
                    .Include("instrument.instr_collection_conditions")
                    .Include("instrument.instrument_status")
                    .Include("instrument.sensor_brand")
                    .Include("instrument.instrument_status.vertical_datums")
                    .Include("instrument.site")
                    .FirstOrDefault(df => df.data_file_id == presDFID);
                

                initialized = true;
            }
            catch
            {
                initialized = false;
            }            
        }

        //init air only
        private void AirInit(Int32 presDFID, string username)
        {
            try
            {
                combinedPath = "";
                //get air datafile
                pressureDataFile = sa.Select<data_file>()
                    .Include(df => df.files)
                    .Include(df => df.instrument)
                    .Include("instrument.instr_collection_conditions")
                    .Include("instrument.instrument_status")
                    .Include("instrument.sensor_brand")
                    .Include("instrument.instrument_status.vertical_datums")
                    .Include("instrument.site")
                    .FirstOrDefault(df => df.data_file_id == presDFID);

                // create directory;
                string path1 = ConfigurationManager.AppSettings["STNRepository"].ToString();
                string path2 = pressureDataFile.instrument.site_id.ToString();
                string path3 = "INSTRUMENT" + pressureDataFile.instrument_id.ToString();
                string path4 = "DFID" + pressureDataFile.data_file_id.ToString() + DateTime.Now.ToString("MMddyyyyHHmmss");
                combinedPath = System.IO.Path.Combine(path1, path2 + path3 + path4);
                workspaceDirectory = System.IO.Directory.CreateDirectory(combinedPath);

                //bring over files it needs from s3 (air/sea) and store in temp location
                pressureDFName = moveFile(presDFID);
                // get member
                pressureScriptMember = sa.Select<member>().Include(m => m.agency).FirstOrDefault(m => m.username == username);
                pressureInitialized = true;
            }
            catch (Exception ex)
            {
                pressureInitialized = false;
            }
        }

        //init air only
        private void ChopperInit(Int32 instrumentId, MemoryStream memoryStream, string fileName)
        {
            try
            {
                combinedPath = "";
                //get the instrument to get access to required properties
                chopperInstrument = sa.Select<instrument>()                    
                    .Include(i => i.instr_collection_conditions)
                    .Include(i => i.instrument_status)
                    .Include(i => i.sensor_brand)
                    .Include("instrument_status.vertical_datums")
                    .Include(i => i.site)
                    .FirstOrDefault(inst => inst.instrument_id == instrumentId);

                // create directory;
                string path1 = ConfigurationManager.AppSettings["STNRepository"].ToString();
                string path2 = "ChopperScript";
                string path3 = "INST_" + chopperInstrument.instrument_id.ToString();
                string path4 = "DATE_" + DateTime.Now.ToString("MMddyyyyHHmmss");
                combinedPath = System.IO.Path.Combine(path1, path2 + path3 + path4);
                workspaceDirectory = System.IO.Directory.CreateDirectory(combinedPath);
                                
                if (chopperInstrument.sensor_type_id == 1 && chopperInstrument.deployment_type_id == 3)
                    chopperPressureType = "Air Pressure";
                if (chopperInstrument.sensor_type_id == 1 && (chopperInstrument.deployment_type_id == 1 || chopperInstrument.deployment_type_id == 2))
                    chopperPressureType = "Sea Pressure";

                //store in temp location                
               chopperDFName = storeFile(memoryStream, fileName);
                
                chopperInitialized = true;
            }
            catch (Exception ex)
            {
                pressureInitialized = false;
            }
        }
        #endregion Methods

        #region Helper Methods

        // get from s3 and move to temp location for script access
        private string moveFile(Int32 fileId) {
            STNAgent sa = new STNAgent();
            try
            {
                string filename = "";
                file fileentity = sa.Select<file>().FirstOrDefault(f => f.data_file_id == fileId);
                InMemoryFile fileItem = sa.GetFileItem(fileentity);
                filename = System.IO.Path.Combine(combinedPath, fileentity.name);
                //copy data file to our temporary location
                string tempLocation = Path.Combine(new String[] { AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts" });// Path.Combine(ConfigurationManager.AppSettings["STNRepository"], filename);
                using (var fileStream = File.Create(System.IO.Path.Combine(tempLocation, filename)))
                {
                    fileItem.OpenStream().Seek(0, SeekOrigin.Begin);
                    fileItem.OpenStream().CopyTo(fileStream);
                    fileItem.Dispose();
                }
                return filename;
            }
            catch (Exception ex)
            { return string.Empty; }
        }
        
        private string storeFile(MemoryStream memoryStream, string fn)
        {
            try
            {
                string filename = "";
                filename = System.IO.Path.Combine(combinedPath, fn);
                //copy data file to our temporary location
                string tempLocation = Path.Combine(ConfigurationManager.AppSettings["STNRepository"], filename);
                memoryStream.Position = 0;
                System.IO.File.WriteAllBytes(tempLocation, memoryStream.ToArray());
                
                return filename;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        // for air and see script (stn_script.py)
        private string getBody(Boolean hertz)
        {
            List<string> body = new List<string>();
            try
            {
                // -sea_fname
                body.Add('"' + waterDFName + '"');
                // -air_fname
                body.Add('"' + airDFName + '"');
                // -out_fname
                body.Add('"' + Path.ChangeExtension(waterDFName, null) + "_output" + '"');
                // -creator_name
                body.Add('"' + scriptMember.fname + " " + scriptMember.lname + '"');
                // -creator_email
                body.Add(scriptMember.email);
                // -creator_url
                body.Add("www.usgs.gov");
                // -sea_instrument_name
                body.Add('"' + seaDataFile.instrument.sensor_brand.brand_name + '"'); // only "Measurement Specialties", "Level Troll", or "Hobo"
                // -air_instrument_name
                body.Add('"' + airDataFile.instrument.sensor_brand.brand_name + '"'); // only "Measurement Specialties", "Level Troll", or "Hobo"
                // -sea_stn_station_number
                body.Add(seaDataFile.instrument.site.site_name.ToString());
                // -air_stn_station_number
                body.Add(airDataFile.instrument.site.site_name.ToString());
                // -sea_stn_instrument_id
                body.Add(seaDataFile.instrument.instrument_id.ToString());
                // -air_stn_instrument_id 
                body.Add(airDataFile.instrument.instrument_id.ToString());
                // -sea_latitude
                body.Add(seaDataFile.instrument.site.latitude_dd.ToString());
                // -air_latitude 
                body.Add(airDataFile.instrument.site.latitude_dd.ToString());
                // -sea_longitude
                body.Add(seaDataFile.instrument.site.longitude_dd.ToString());
                // -air_longitude
                body.Add(airDataFile.instrument.site.longitude_dd.ToString());
                // -tz_info
                body.Add("UTC");
                // -daylight_savings
                body.Add("False");

                // -datum
                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vdatum_id != null)
                    body.Add('"' + airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vertical_datums.datum_abbreviation + '"');
                else body.Add("NAVD88");

                // -sea_initial_sensor_orifice_elevation
                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation != null)
                    body.Add(seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation.ToString());
                else body.Add("0");

                // -air_initial_sensor_orifice_elevation 
                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation != null)
                    body.Add(airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation.ToString());
                else body.Add("0");

                // -sea_final_sensor_orifice_elevation 
                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation != null)
                    body.Add(seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation.ToString());
                else body.Add("0");

                // -air_final_sensor_orifice_elevation
                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation != null)
                    body.Add(airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation.ToString());
                else body.Add("0");

                // -salinity 
                body.Add('"' + seaDataFile.instrument.instr_collection_conditions.condition + '"');
                
                // -initial_land_surface_elevation
                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).gs_elevation != null)
                    body.Add(seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).gs_elevation.ToString());
                else body.Add("0");

                // -final_land_surface_elevation
                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).gs_elevation != null)
                    body.Add(seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).gs_elevation.ToString());
                else body.Add("0");

                string depTime = seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).time_stamp.Value.ToString("yyyyMMdd HHmm");                
                // -deployment_time
                body.Add('"' + depTime + '"');
                string retTime = seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).time_stamp.Value.ToString("yyyyMMdd HHmm");
                // -retrieval_time
                body.Add('"' + retTime + '"');
                // -sea_name 
                body.Add('"' + seaDataFile.instrument.site.waterbody + '"');

                string seaGoodStart = seaDataFile.good_start.Value.ToString("yyyyMMdd HHmm");
                // -sea_good_start_date         
                body.Add('"' + seaGoodStart + '"');

                string airGoodStart = airDataFile.good_start.Value.ToString("yyyyMMdd HHmm");
                // -air_good_start_date 
                body.Add('"' + airGoodStart + '"');

                string seaGoodEnd = seaDataFile.good_end.Value.ToString("yyyyMMdd HHmm");
                // -sea_good_end_date
                body.Add('"' + seaGoodEnd + '"');

                string airGoodEnd = airDataFile.good_end.Value.ToString("yyyyMMdd HHmm");
                // -air_good_end_date
                body.Add('"' + airGoodEnd + '"');

                // -sea_4hz 
                body.Add(hertz.ToString());

                var joinedBody = "\"" + string.Join("\", \"", body) + "\"";
                return string.Join(" ", body);
            }
            catch (Exception)
            {
                throw;
            }
        }//end getParameterList   
        
        // for air only (stn_script_air.py)
        private string getAirBody()
        {
            List<string> body = new List<string>();
            try
            {
                // -air_fname
                body.Add('"' + pressureDFName + '"');
                // -out_fname
                body.Add('"' + Path.ChangeExtension(pressureDFName, null) + "_output" + '"');
                // -creator_name
                body.Add('"' + pressureScriptMember.fname + " " + pressureScriptMember.lname + '"');
                // -creator_email
                body.Add(pressureScriptMember.email);
                // -creator_url
                body.Add("www.usgs.gov");
                // -air_instrument_name
                body.Add('"' + pressureDataFile.instrument.sensor_brand.brand_name + '"'); // only "Measurement Specialties", "Level Troll", or "Hobo"
                // -air_stn_instrument_id 
                body.Add(pressureDataFile.instrument.instrument_id.ToString());
                // -air_stn_station_number
                body.Add(pressureDataFile.instrument.site.site_name.ToString());
                // -air_latitude 
                body.Add(pressureDataFile.instrument.site.latitude_dd.ToString());
                // -air_longitude
                body.Add(pressureDataFile.instrument.site.longitude_dd.ToString());
                // -tz_info
                body.Add("UTC");
                // -daylight_savings
                body.Add("False");

                // -datum
                if (pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vdatum_id != null)
                    body.Add('"' + pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vertical_datums.datum_abbreviation + '"');
                else body.Add("NAVD88");

                // -air_initial_sensor_orifice_elevation 
                if (pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation != null)
                    body.Add(pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation.ToString());
                else body.Add("0");

                // -air_final_sensor_orifice_elevation
                if (pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation != null)
                    body.Add(pressureDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation.ToString());
                else body.Add("0");

                string airGoodStart = pressureDataFile.good_start.Value.ToString("yyyyMMdd HHmm");
                // -air_good_start_date 
                body.Add('"' + airGoodStart + '"');

                string airGoodEnd = pressureDataFile.good_end.Value.ToString("yyyyMMdd HHmm");
                // -air_good_end_date
                body.Add('"' + airGoodEnd + '"');
                
                var joinedBody = "\"" + string.Join("\", \"", body) + "\"";
                return string.Join(" ", body);
            }
            catch (Exception)
            {
                throw;
            }
        }//end getParameterList   

        private string getChopperBody()
        {
            List<string> body = new List<string>();
            try
            {
                // -in_fname
                body.Add('"' + chopperDFName + '"');
                // -out_fname
                body.Add('"' + Path.ChangeExtension(chopperDFName, null) + "_output" + '"');
                // -pressure_type
                body.Add('"' + chopperPressureType + '"');
                // -daylight_savings
                body.Add("False");
                // -tz_info
                body.Add("UTC");                                
                // -datum
                if (chopperInstrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vdatum_id != null)
                    body.Add('"' + chopperInstrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vertical_datums.datum_abbreviation + '"');
                else body.Add("NAVD88");
                // -instrument_name
                body.Add('"' + chopperInstrument.sensor_brand.brand_name + '"'); // only "Measurement Specialties", "Level Troll", or "Hobo"

                var joinedBody = "\"" + string.Join("\", \"", body) + "\"";
                return string.Join(" ", body);
            }
            catch (Exception)
            {
                throw;
            }
        }//end getParameterList   
        #endregion Helper Methods

    }
}