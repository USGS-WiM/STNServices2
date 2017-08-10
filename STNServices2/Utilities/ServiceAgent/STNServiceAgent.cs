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
using WiM.Utilities.ServiceAgent;
using WiM.Resources;
using WiM.Resources.Spatial;
using WiM.Utilities.Storage;
using WiM.Exceptions;

using STNServices2.Resources;
using STNDB;

namespace STNServices2.Utilities.ServiceAgent
{
    public class STNServiceAgent: ExternalProcessServiceAgentBase
    {
        #region Properties    
        private DirectoryInfo workspaceDirectory { get; set; }
        private string pressureDFName { get; set; }
        private string waterDFName { get; set; }
        public Boolean initialized { get; private set; }
        private member scriptMember { get; set; }
        private data_file seaDataFile { get; set; }
        private data_file airDataFile { get; set; }
        private site waterSite { get; set; }
        private site airSite { get; set; }
        private string combinedPath { get; set; }
        STNAgent sa = new STNAgent();

        #endregion Properties

        #region Constructors
        public STNServiceAgent(Int32 pressureSensorDFID, Int32 waterSensorDFID, string username)
            :base(ConfigurationManager.AppSettings["EXEPath"], Path.Combine(new String[] {AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts"}))
        {
           /* if (!System.IO.Directory.Exists(ConfigurationManager.AppSettings["STNRepository"]))
                System.IO.Directory.CreateDirectory(ConfigurationManager.AppSettings["STNRepository"]);
            */
            Init(pressureSensorDFID, waterSensorDFID, username);
        }
        #endregion Constructors

        #region Methods
        public Boolean RunScript(Boolean hz)
        {
            JObject result = null;
            string msg;
           
            try
            {
                result = Execute(getProcessRequest(ConfigurationManager.AppSettings["STN_Script"], getBody(hz))) as JObject;
                    //if (isDynamicError(result, out msg)) throw new Exception("Delineation Error: " + msg);                    
                
                return true;
            }
            catch (Exception ex)
            {
            //    sm("Error delineating " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
        
        private void Init(Int32 presDFID, Int32 watDFID, string username)
        {
            try
            {
                //get water level's siteid and eventid
                seaDataFile = sa.Select<data_file>()
                    .Include(df => df.instrument)
                    .Include("instrument.instr_collection_conditions")
                    .Include("instrument.instrument_status")
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
          //      workspaceDirectory = System.IO.Directory.CreateDirectory(System.IO.Path.Combine(ConfigurationManager.AppSettings["STNRepository"].ToString(), seaDataFile.instrument.site_id.ToString() + "INSTRUMENT" + seaDataFile.instrument_id.ToString() + "DFID" + seaDataFile.data_file_id.ToString() + DateTime.Now.ToLongTimeString()));
                
                //bring over files it needs from s3 (air/sea) and store in temp location
                pressureDFName = moveFile(presDFID);
                waterDFName = moveFile(watDFID);
                // get member
                scriptMember = sa.Select<member>().Include(m=> m.agency).FirstOrDefault(m => m.username == username);
                //get air datafile
                airDataFile = sa.Select<data_file>()
                    .Include(df => df.instrument)
                    .Include("instrument.instr_collection_conditions")
                    .Include("instrument.instrument_status")
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
        #endregion Methods

        #region Helper Methods

        private string moveFile(Int32 fileId) {
            STNAgent sa = new STNAgent();
            try
            {                
                string filename = "";
                file fileentity = sa.Select<file>().FirstOrDefault(f => f.data_file_id == fileId);
                InMemoryFile fileItem = sa.GetFileItem(fileentity);
                filename = fileentity.name;
                //copy data file to our temporary location
                string tempLocation = Path.Combine(new String[] { AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts" });
                using (var fileStream = File.Create(System.IO.Path.Combine(tempLocation,filename)))//System.IO.Path.Combine(combinedPath, filename))) //BROKEN HERE
                {
                    fileItem.OpenStream().Seek(0, SeekOrigin.Begin);
                    fileItem.OpenStream().CopyTo(fileStream);
                    fileItem.Dispose();
                }
                return filename;// System.IO.Path.Combine(combinedPath, filename);
            }
            catch
            {
                return string.Empty;
            }
        }
        private string getBody(Boolean hertz)
        {
            List<string> body = new List<string>();
            try
            {
                body.Add("-sea_fname " + waterDFName);
                body.Add("-air_fname " + pressureDFName);
                body.Add("-out_fname " + pressureDFName + "_output");
                body.Add("-creator_name " + scriptMember.fname + " " + scriptMember.lname);
                body.Add("-creator_email " + scriptMember.email);
                body.Add("-creator_url " + "www.usgs.gov");
                body.Add("-sea_instrument_name " + seaDataFile.instrument.serial_number);
                body.Add("-air_instrument_name " + airDataFile.instrument.serial_number);
                body.Add("-sea_stn_station_number " + seaDataFile.instrument.site.site_id);
                body.Add("-air_stn_station_number " + airDataFile.instrument.site.site_id);
                body.Add("-sea_stn_instrument_id " + seaDataFile.instrument.instrument_id);
                body.Add("-air_stn_instrument_id " + airDataFile.instrument.instrument_id);
                body.Add("-sea_latitude " + seaDataFile.instrument.site.latitude_dd);
                body.Add("-air_latitude " + airDataFile.instrument.site.latitude_dd);
                body.Add("-sea_longitude " + seaDataFile.instrument.site.longitude_dd);
                body.Add("-air_longitude " + airDataFile.instrument.site.longitude_dd);
                body.Add("-tz_info " + "UTC");
                body.Add("-daylight_savings " + "False");
                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).vdatum_id != null)
                    body.Add("-datum " + airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.instrument_status_id == 1).vertical_datums.datum_abbreviation);
                else body.Add("-datum " + "NAVD88");

                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation != null)
                    body.Add("-sea_initial_sensor_orifice_elevation " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation);
                else body.Add("-sea_initial_sensor_orifice_elevation " + 1);
                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation != null)
                    body.Add("-air_initial_sensor_orifice_elevation " + airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).sensor_elevation);
                else body.Add("-air_initial_sensor_orifice_elevation " + 1);

                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation != null)
                    body.Add("-sea_final_sensor_orifice_elevation " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation);
                else body.Add("-sea_final_sensor_orifice_elevation " + 1.3);

                if (airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation != null)
                    body.Add("-air_final_sensor_orifice_elevation " + airDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).sensor_elevation);
                else body.Add("-air_final_sensor_orifice_elevation " + 2);

                body.Add("-salinity " + seaDataFile.instrument.instr_collection_conditions.condition);

                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).gs_elevation != null)
                    body.Add("-initial_land_surface_elevation " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).gs_elevation);
                else body.Add("-initial_land_surface_elevation " + 2.2);

                if (seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).gs_elevation != null)
                    body.Add("-final_land_surface_elevation " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).gs_elevation);
                else body.Add("-final_land_surface_elevation " + 2.3);

                body.Add("-deployment_time " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 1).time_stamp);
                body.Add("-retrieval_time " + seaDataFile.instrument.instrument_status.FirstOrDefault(inst => inst.status_type_id == 2).time_stamp);
                body.Add("-sea_name " + seaDataFile.instrument.site.waterbody);
                body.Add("-sea_good_start_date " + seaDataFile.good_start);
                body.Add("-air_good_start_date " + airDataFile.good_start);
                body.Add("-sea_good_end_date " + seaDataFile.good_end);
                body.Add("-air_good_end_date " + airDataFile.good_end);
                body.Add("-sea_4hz " + hertz.ToString());
                
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