//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

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
// 03.28.16 - JKN - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RestSharp;
using WiM.Exceptions;
using WiM.Resources;
using System.Net;
using STNServices2.Resources;
using System.Configuration;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WiM.Security;
using WiM.Utilities.ServiceAgent;
using RestSharp.Authenticators;
using OpenRasta.IO;
using STNDB;
using STNServices2.Utilities.ServiceAgent;
using System.IO;

namespace STNServices2.Handlers
{
    public class GeocoderHandler : STNHandlerBase
    {
        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName = "GetReverseGeocode")]
        public OperationResult GetReverseGeocode(decimal latitude, decimal longitude)
        {
            //GeocoderResource geocode = null; : ServiceAgentBase
            try
            {
                if (latitude <= 0 || longitude >= 0) throw new BadRequestException("Invalid input parameters");
               
                var client = new RestClient("https://geocoding.geo.census.gov");                
                string url = string.Format("geocoder/geographies/coordinates?x={0}&y={1}&benchmark=4&vintage=4&format=json", longitude, latitude );
                var request = new RestRequest(url, Method.GET);
                
                //without this, get ssl/tsl error
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };      
               
                IRestResponse response = client.Execute(request);

                return new OperationResult.OK { ResponseResource = JsonConvert.DeserializeObject(response.Content) };
            }
            catch (Exception ex)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetNewsFeed")]
        public OperationResult GetSTNNewsFeed()
        {
            //GeocoderResource geocode = null; : ServiceAgentBase
            try
            {
                var client = new RestClient("https://my.usgs.gov");
                string url = string.Format("confluence/rest/api/content/{0}?expand=body.storage", ConfigurationManager.AppSettings["ConfluenceID"]);
                var request = new RestRequest(url, Method.GET);

                //without this, get ssl/tsl error
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                IRestResponse response = client.Execute(request);

                return new OperationResult.OK { ResponseResource = JsonConvert.DeserializeObject(response.Content) };
            }
            catch (Exception ex)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFileItemasJson")]
        public OperationResult GetFileItemAsJson(Int32 fileId)
        {
            InMemoryFile fileItem;
            dynamic files;
            file anEntity = null;
            try
            {
                if (fileId <= 0) throw new BadRequestException("Invalid input parameters");

                using (Utilities.ServiceAgent.STNAgent sa = new STNAgent())
                {
                    //use include statements for stnagent's GetFileItem to find the event this file is on
                    anEntity = sa.Select<file>().SingleOrDefault(f => f.file_id == fileId);
                    if (anEntity == null) throw new BadRequestException("No file exists for given parameter");

                    fileItem = sa.GetFileItem(anEntity);
                    using (var fileStream = fileItem.OpenStream())

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string json = reader.ReadToEnd();
                        files = JsonConvert.DeserializeObject(json);
                    }

                    sm(sa.Messages);
                }//end using
                return new OperationResult.OK { ResponseResource = files, Description = this.MessageString };
                //                return new OperationResult.OK { ResponseResource = fileItem, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        
        #endregion


    }//end GeocoderHandler
}
