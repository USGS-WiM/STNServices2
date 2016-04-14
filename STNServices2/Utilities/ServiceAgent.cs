//------------------------------------------------------------------------------
//----- ServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating the service call, 
//              capturing the data that's returned and forwarding the data back to 
//              the ViewModel.
//
//discussion:   delegated hunting and gathering responsibilities.   
//
//    

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using RestSharp.Deserializers;

using STNServices2.Resources;

using RestSharp;

namespace STNServices2.Utilities
{
    public class AGSServiceAgent : ServiceAgentBase
    {
        public AGSServiceAgent()
            : base(ConfigurationManager.AppSettings["AGSSTNServer"])
        { }

        public Boolean PostFeature(Features feature, string postURL, string featureName)
        {
            try
            {
                return Execute(getRestRequest(postURL, featureName, feature));

            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
    }

    public abstract class ServiceAgentBase
    {
        #region "Events"

        #endregion

        #region Properties & Fields

        readonly string _accountSid;
        readonly string _secretKey;

        private RestClient client = new RestClient();
        #endregion

        #region Constructors
        public ServiceAgentBase(string BaseUrl)
        {

            client.BaseUrl = BaseUrl;
            // TODO: Use user login system for Authentication
            //client.Authenticator = new HttpBasicAuthenticator("lampadmin", "cafOR4_yR");
            //request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request

        }

        public ServiceAgentBase(string accountSid, string secretKey, string baseUrl)
            : this(baseUrl)
        {
            _accountSid = accountSid;
            _secretKey = secretKey;
        }
        #endregion

        #region Methods
        public void ExecuteAsync<T>(RestRequest request, Action<T> CallBackOnSuccess, Action<string> CallBackOnFail) where T : new()
        {
            // request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request

            client.ExecuteAsync<T>(request, (response) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    CallBackOnFail(response.ErrorMessage);
                }
                else
                {
                    CallBackOnSuccess(response.Data);
                }
            });


        }//end ExecuteAsync<T>

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            IRestResponse<T> result = null;
            if (request == null) throw new ArgumentNullException("request");

            AutoResetEvent waitHandle = new AutoResetEvent(false);
            Exception exception = null;

            client.ExecuteAsync<T>(request, (response) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    exception = new Exception(response.ResponseStatus.ToString());
                    //release the Event
                    waitHandle.Set();
                }
                else
                {
                    result = response;
                    //release the Event
                    waitHandle.Set();
                }
            });

            //wait until the thread returns
            waitHandle.WaitOne();

            return result;
        }//end Execute<T>

        public Boolean Execute(IRestRequest request)
        {
            RestResponse responseResult = null;
            if (request == null) throw new ArgumentNullException("request");

            responseResult = client.Execute(request) as RestResponse;
            if (responseResult.StatusCode == HttpStatusCode.OK)
                return true;
            //else
            return false;
        }//endExecute

        protected RestRequest getRestRequest(string URI, string bodyName, object Body)
        {
            //http://localhost:6080/arcgis/rest/services/STNSample/AddHWM/GPServer/AddHWM/execute 
            //http://localhost:6080/arcgis/rest/services/STN/AddSite/GPServer/AddSite/execute
            RestRequest request = new RestRequest(URI);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", bodyName + request.JsonSerializer.Serialize(Body), ParameterType.RequestBody);
            request.Method = Method.POST;

            return request;
        }//end BuildRestRequest

        #endregion

    }//end class ServiceAgentBase


}//end namespace
