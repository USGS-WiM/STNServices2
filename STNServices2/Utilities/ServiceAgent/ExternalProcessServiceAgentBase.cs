//------------------------------------------------------------------------------
//----- ExternalProcessServiceAgentBase ----------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating to a stand alone 
//              process using a process shell request, capturing the data that's 
//              returned and forwarding the data back to the requestor.
//
//discussion:   delegated hunting and gathering responsibilities.   
//
//  
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STNServices2.Utilities.ServiceAgent
{
    public abstract class ExternalProcessServiceAgentBase
    {
        #region "Events"

        #endregion

        #region Properties & Fields
        public string BaseEXE { get; private set; }
        public string BasePath { get; private set; }
        #endregion

        #region Constructors
        public ExternalProcessServiceAgentBase(string baseExe, string basePath)
        {
            this.BaseEXE = baseExe;
            this.BasePath = basePath;
        }

        #endregion

        #region Methods
        public void ExecuteAsync<T>(ProcessStartInfo psi, Action<T> CallBackOnSuccess, Action<string> CallBackOnFail) where T : new()
        {
            String response = null;
            Process task = null;
            try
            {

                if (psi == null) throw new ArgumentNullException("processInfo");
                task = new Process();
                task = Process.Start(psi);
                response = task.StandardOutput.ReadToEnd();

                if (!String.IsNullOrEmpty(response))
                {
                    CallBackOnSuccess(JsonConvert.DeserializeObject<T>(response));
                }//else
                else
                {
                    CallBackOnFail(psi.FileName + " response is null");
                }

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                //local clean up
                if (task != null) { task.Dispose(); task = null; }
            }


        }//end ExecuteAsync<T>

        public void Execute(ProcessStartInfo psi)
        {            
            Process task = null;
           // string results = null;
            try
            {
                if (psi == null) throw new ArgumentNullException("processInfo");
                task = new Process();
                task = Process.Start(psi);
                task.WaitForExit();

            }
            catch (Exception ex)
            {
                throw new Exception("Error executing " + psi.Arguments + " & " + psi.FileName + " dir " + psi.WorkingDirectory + " error1 msg: " + ex.Message);
            }
            finally
            {
                //local clean up
                if (task != null) { task.Close(); task.Dispose(); task = null; }
            }
        }//endExecute

        #endregion
        #region Helper Methods
        protected ProcessStartInfo getProcessRequest(string filename, string args)
        {

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = this.BaseEXE;
            psi.Arguments = string.Format("{0} {1}", filename, args);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = this.BasePath;
            psi.WindowStyle = ProcessWindowStyle.Normal;

            return psi;
        }//end BuildRestRequest
        #endregion

    }
}