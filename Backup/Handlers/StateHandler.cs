//------------------------------------------------------------------------------
//----- StateHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles state resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 02.07.13 - JKN - Added query to get Instruments by eventId and siteID
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.03.12 - JKN -Added Role Authorization 
// 06.08.12 - JB - Fixed Instrument Serial Number URI
// 06.04.12 - jkn -Created

#endregion

using STNServices2.Resources;
using STNServices2.Authentication;

using OpenRasta.Web;
using OpenRasta.Security;
using OpenRasta.Diagnostics;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;


namespace STNServices2.Handlers
{

    public class StateHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "STATES"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllStates")]
        public OperationResult Get()
        {
            List<STATES> states = new List<STATES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    states = aSTNE.STATES.ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = states };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteStates")]
        public OperationResult SiteStates()
        {
            List<STATES> states = new List<STATES>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    List<SITE> allSites = aSTNE.SITES.ToList();
                    List<string> uniqueSites = allSites.GroupBy(p => p.STATE).Select(g => g.FirstOrDefault()).Select(x => x.STATE).ToList();

                    states = aSTNE.STATES.Where(st => uniqueSites.Contains(st.STATE_ABBREV)).ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = states };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key
         * 
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        #endregion

        #endregion
        #region Helper Methods

        #endregion
    }//end class StateHandler
}//end namespace