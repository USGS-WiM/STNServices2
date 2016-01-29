//------------------------------------------------------------------------------
//----- Keyword Handler --------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Peak keyword resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 12.17.13 - JKN -Created

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
    public class KeywordHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "KEYWORDS"; }
        }
        #endregion
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.GetList<KEYWORD>() };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 keywordId)
        {
            KEYWORD keyword = null;

            //Return BadRequest if there is no ID
            if (keywordId <= 0)
                return new OperationResult.BadRequest();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    keyword = aSTNE.KEYWORDS.FirstOrDefault(i => i.KEYWORD_ID == keywordId);

                    if (keyword != null)
                        keyword.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = keyword };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET
        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(KEYWORD anEntity)
        {
            try
            {
                if (!Exists(ref anEntity))
                {
                    anEntity = base.Post(anEntity) as KEYWORD;
                }

                return new OperationResult.OK { ResponseResource = anEntity };

            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, KEYWORD entity)
        {
            try
            {
                return new OperationResult.OK { ResponseResource = base.Put(entity, entityId) };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.PUT

        #endregion
        #region Helper Methods
        private bool Exists(ref KEYWORD anEntity)
        {
            KEYWORD existingKeyword;
            KEYWORD thisKeyword = anEntity;
            //check if it exists
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    existingKeyword = aSTNE.KEYWORDS.FirstOrDefault(l => string.Equals(l.WORD.ToUpper(), thisKeyword.WORD.ToUpper()));

                }//end using
                if (existingKeyword == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingKeyword;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }//end class
}//end namespace