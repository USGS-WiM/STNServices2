//------------------------------------------------------------------------------
//----- Site_HousingHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site Housings resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 05.25.14 - TR - Created

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
    public class Site_HousingHandler : STNHandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "SITE_HOUSING"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<SITE_HOUSING> SiteHousingList = new List<SITE_HOUSING>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    SiteHousingList = aSTNE.SITE_HOUSING.ToList();

                    if (SiteHousingList != null)
                        SiteHousingList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = SiteHousingList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            SITE_HOUSING aSiteHousing;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    aSiteHousing = aSTNE.SITE_HOUSING.SingleOrDefault(
                                m => m.SITE_HOUSING_ID == entityId);

                    if (aSiteHousing != null)
                        aSiteHousing.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aSiteHousing };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "SiteHousing")]
        public OperationResult SiteHousings(Int32 siteId)
        {
            List<SITE_HOUSING> siteHousing;

            //Return BadRequest if there is no ID
            if (siteId <= 0)
            { return new OperationResult.BadRequest(); }
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    siteHousing = aSTNE.SITE_HOUSING.Where(m => m.SITE_ID == siteId).ToList();

                    if (siteHousing != null)
                        siteHousing.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = siteHousing };
            }
            catch (Exception)
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSiteSiteHousing")]
        public OperationResult AddSiteSiteHousing(Int32 siteId, SITE_HOUSING aSiteHousing)
        {
            //Return BadRequest if there is no ID
            if (siteId <= 0 || aSiteHousing.HOUSING_TYPE_ID <= 0)
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
                        aSiteHousing.SITE_ID = siteId;
                        if (!Exists(aSTNE.SITE_HOUSING, ref aSiteHousing))
                        {
                            aSTNE.SITE_HOUSING.AddObject(aSiteHousing);
                            aSTNE.SaveChanges();
                        }//end if
                    }
                }
                return new OperationResult.OK();
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

        #endregion

        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT, ForUriName = "Put")]
        public OperationResult Put(Int32 entityId, SITE_HOUSING aSiteHouse)
        {
            //Return BadRequest if missing required fields
            if (aSiteHouse.SITE_ID <= 0 || aSiteHouse.HOUSING_TYPE_ID <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        SITE_HOUSING ObjectToBeUpdated = aSTNE.SITE_HOUSING.Single(m => m.SITE_HOUSING_ID == entityId);

                        ObjectToBeUpdated.LENGTH = aSiteHouse.LENGTH != ObjectToBeUpdated.LENGTH ? aSiteHouse.LENGTH : ObjectToBeUpdated.LENGTH;
                        ObjectToBeUpdated.MATERIAL = aSiteHouse.MATERIAL != ObjectToBeUpdated.MATERIAL ? aSiteHouse.MATERIAL : ObjectToBeUpdated.MATERIAL;
                        ObjectToBeUpdated.NOTES = aSiteHouse.NOTES != ObjectToBeUpdated.NOTES ? aSiteHouse.NOTES : ObjectToBeUpdated.NOTES;
                        ObjectToBeUpdated.AMOUNT = aSiteHouse.AMOUNT != ObjectToBeUpdated.AMOUNT ? aSiteHouse.AMOUNT : ObjectToBeUpdated.AMOUNT;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }

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
            SITE_HOUSING ObjectToBeDeleted = null;

            //Return BadRequest if missing required fields
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (STNEntities2 aSTNE = GetRDS(securedPassword))
                {

                    // create user profile using db stored proceedure
                    try
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = aSTNE.SITE_HOUSING.SingleOrDefault(
                                                m => m.SITE_HOUSING_ID == entityId);

                        //delete it
                        aSTNE.SITE_HOUSING.DeleteObject(ObjectToBeDeleted);
                        aSTNE.SaveChanges();
                        //Return object to verify persisitance

                        return new OperationResult.OK { };

                    }
                    catch (Exception)
                    {
                        return new OperationResult.BadRequest();
                    }

                }// end using
            } //end using
        }//end HTTP.DELETE
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<SITE_HOUSING> entityRDS, ref SITE_HOUSING anEntity)
        {
            SITE_HOUSING existingSH;
            SITE_HOUSING thisEntity = anEntity as SITE_HOUSING;
            //check if it exists
            try
            {

                existingSH = entityRDS.FirstOrDefault(e => e.SITE_ID == thisEntity.SITE_ID &&
                                                               e.HOUSING_TYPE_ID == thisEntity.HOUSING_TYPE_ID &&
                                                               e.LENGTH == thisEntity.LENGTH &&
                                                               e.AMOUNT == thisEntity.AMOUNT &&
                                                               (string.Equals(e.MATERIAL.ToUpper(), thisEntity.MATERIAL.ToUpper())) &&
                                                               (string.Equals(e.NOTES.ToUpper(), thisEntity.NOTES.ToUpper())));


                if (existingSH == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingSH;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


    }//end class PeakSummaryHandler
}//end namespace