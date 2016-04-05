//------------------------------------------------------------------------------
//----- PeakSummaryHandler -----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Peak summary resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 08.01.12 - JB - Reference Point by site id should return list instead of single RP
// 07.31.12 - JKN -Created

#endregion

using STNServices2.Resources;
using STNServices2.Authentication;
using STNServices2.Utilities;
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
using System.Configuration;

namespace STNServices2.Handlers
{
    public class ObjectivePointHandler : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "OBJECTIVEPOINTS"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<OBJECTIVE_POINT> objectivePointList = new List<OBJECTIVE_POINT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    objectivePointList = aSTNE.OBJECTIVE_POINT.OrderBy(rp => rp.OBJECTIVE_POINT_ID).ToList();

                    if (objectivePointList != null)
                        objectivePointList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = objectivePointList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            OBJECTIVE_POINT anObjPoint;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    anObjPoint = aSTNE.OBJECTIVE_POINT.SingleOrDefault(rp => rp.OBJECTIVE_POINT_ID == entityId);

                    if (anObjPoint != null)
                        anObjPoint.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using

                return new OperationResult.OK { ResponseResource = anObjPoint };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetVDatumOPs")]
        public OperationResult GetVDatumOPs(Int32 vdatumId)
        {
            List<OBJECTIVE_POINT> ops = new List<OBJECTIVE_POINT>();

            //Return BadRequest if there is no ID
            if (vdatumId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    ops = aSTNE.VERTICAL_DATUMS.FirstOrDefault(vd => vd.DATUM_ID == vdatumId).OBJECTIVE_POINT.ToList();

                    if (ops != null)
                        ops.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = ops };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSiteObjectivePoints")]
        public OperationResult GetSiteObjectivePoints(Int32 siteId)
        {
            List<OBJECTIVE_POINT> objectivePointList = new List<OBJECTIVE_POINT>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {

                    objectivePointList = aSTNE.OBJECTIVE_POINT
                                .Where(rp => rp.SITE.SITE_ID == siteId)
                                .OrderBy(rp => rp.OBJECTIVE_POINT_ID)
                                .ToList();

                    if (objectivePointList != null)
                        objectivePointList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = objectivePointList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "PostObjectivePoint")]
        public OperationResult Post(OBJECTIVE_POINT anObjectivePoint)
        {
            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(anObjectivePoint.NAME) || string.IsNullOrEmpty(anObjectivePoint.DESCRIPTION)))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        if (!Exists(aSTNE.OBJECTIVE_POINT, ref anObjectivePoint))
                        {
                            aSTNE.OBJECTIVE_POINT.AddObject(anObjectivePoint);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = anObjectivePoint };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST

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
        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, OBJECTIVE_POINT anObjectivePoint)
        {

            //Return BadRequest if missing required fields
            if (anObjectivePoint.OBJECTIVE_POINT_ID <= 0 || entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }
            try
            {
                OBJECTIVE_POINT OPToUpdate = null;
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        //Grab the instrument row to update
                        OPToUpdate = aSTNE.OBJECTIVE_POINT.SingleOrDefault(
                                           rp => rp.OBJECTIVE_POINT_ID == entityId);
                        //Update fields
                        OPToUpdate.NAME = anObjectivePoint.NAME;
                        OPToUpdate.DESCRIPTION = anObjectivePoint.DESCRIPTION;
                        OPToUpdate.ELEV_FT = anObjectivePoint.ELEV_FT;
                        OPToUpdate.DATE_ESTABLISHED = anObjectivePoint.DATE_ESTABLISHED;
                        OPToUpdate.DATE_RECOVERED = anObjectivePoint.DATE_RECOVERED;
                        OPToUpdate.OP_IS_DESTROYED = anObjectivePoint.OP_IS_DESTROYED;
                        OPToUpdate.OP_NOTES = anObjectivePoint.OP_NOTES;
                        OPToUpdate.SITE_ID = anObjectivePoint.SITE_ID;
                        OPToUpdate.VDATUM_ID = anObjectivePoint.VDATUM_ID;
                        OPToUpdate.LATITUDE_DD = anObjectivePoint.LATITUDE_DD;
                        OPToUpdate.LONGITUDE_DD = anObjectivePoint.LONGITUDE_DD;
                        OPToUpdate.HDATUM_ID = anObjectivePoint.HDATUM_ID;
                        OPToUpdate.HCOLLECT_METHOD_ID = anObjectivePoint.HCOLLECT_METHOD_ID;
                        OPToUpdate.VCOLLECT_METHOD_ID = anObjectivePoint.VCOLLECT_METHOD_ID;
                        OPToUpdate.OP_TYPE_ID = anObjectivePoint.OP_TYPE_ID;
                        OPToUpdate.UNCERTAINTY = anObjectivePoint.UNCERTAINTY;
                        OPToUpdate.UNQUANTIFIED = anObjectivePoint.UNQUANTIFIED;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anObjectivePoint };
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
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteObjectivePoint")]
        public OperationResult Delete(Int32 entityId)
        {
            //Return BadRequest if missing required fields
            if (entityId <= 0)
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
                        //delete files associated with this op
                        List<FILES> opFiles = aSTNE.FILES.Where(x => x.OBJECTIVE_POINT_ID == entityId).ToList();
                        if (opFiles.Count >= 1)
                        {
                            foreach (FILES f in opFiles)
                            {

                                //delete the file item from s3
                                S3Bucket aBucket = new S3Bucket(ConfigurationManager.AppSettings["AWSBucket"]);
                                aBucket.DeleteObject(BuildFilePath(f, f.PATH));

                                //delete the file
                                aSTNE.FILES.DeleteObject(f);
                                aSTNE.SaveChanges();
                            }
                        }
                        //delete op_control_identifiers for this op
                        List<OP_CONTROL_IDENTIFIER> opci = aSTNE.OP_CONTROL_IDENTIFIER.Where(x => x.OBJECTIVE_POINT_ID == entityId).ToList();
                        if (opci.Count >= 1)
                        {
                            foreach (OP_CONTROL_IDENTIFIER o in opci)
                            {
                                aSTNE.OP_CONTROL_IDENTIFIER.DeleteObject(o);
                                aSTNE.SaveChanges();
                            }
                        }

                        //fetch the object to be updated (assuming that it exists)
                        OBJECTIVE_POINT ObjectToBeDeleted = aSTNE.OBJECTIVE_POINT.SingleOrDefault(rp => rp.OBJECTIVE_POINT_ID == entityId);
                        //delete it
                        aSTNE.OBJECTIVE_POINT.DeleteObject(ObjectToBeDeleted);

                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HTTP.DELETE
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(ObjectSet<OBJECTIVE_POINT> entityRDS, ref OBJECTIVE_POINT anEntity)
        {
            OBJECTIVE_POINT existingOP;
            OBJECTIVE_POINT thisOP = anEntity as OBJECTIVE_POINT;
            //check if it exists
            try
            {

                existingOP = entityRDS.FirstOrDefault(r => string.Equals(r.NAME.ToUpper(), thisOP.NAME.ToUpper()) &&
                                                                        (string.Equals(r.DESCRIPTION.ToUpper(), thisOP.DESCRIPTION.ToUpper()) || string.IsNullOrEmpty(thisOP.DESCRIPTION)) &&
                                                                        (r.ELEV_FT == thisOP.ELEV_FT) && (DateTime.Equals(r.DATE_ESTABLISHED, thisOP.DATE_ESTABLISHED) || !thisOP.DATE_ESTABLISHED.HasValue) &&
                                                                        (DateTime.Equals(r.DATE_RECOVERED, thisOP.DATE_RECOVERED) || !thisOP.DATE_RECOVERED.HasValue) &&
                                                                        (r.OP_IS_DESTROYED == thisOP.OP_IS_DESTROYED) &&
                                                                        (string.Equals(r.OP_NOTES.ToUpper(), thisOP.OP_NOTES.ToUpper()) || string.IsNullOrEmpty(thisOP.OP_NOTES)) &&
                                                                        (r.SITE_ID == thisOP.SITE_ID || thisOP.SITE_ID <= 0 || thisOP.SITE_ID == null) &&
                                                                        (r.LATITUDE_DD == thisOP.LATITUDE_DD || thisOP.LATITUDE_DD <= 0 || thisOP.LATITUDE_DD == null) &&
                                                                        (r.LONGITUDE_DD == thisOP.LONGITUDE_DD || thisOP.LONGITUDE_DD <= 0 || thisOP.LONGITUDE_DD == null) &&
                                                                        (r.VDATUM_ID == thisOP.VDATUM_ID || thisOP.VDATUM_ID <= 0 || thisOP.VDATUM_ID == null) &&
                                                                        (r.HDATUM_ID == thisOP.HDATUM_ID || thisOP.HDATUM_ID <= 0 || thisOP.HDATUM_ID == null) &&
                                                                        (r.VCOLLECT_METHOD_ID == thisOP.VCOLLECT_METHOD_ID || thisOP.VCOLLECT_METHOD_ID <= 0 || thisOP.VCOLLECT_METHOD_ID == null) &&
                                                                        (r.HCOLLECT_METHOD_ID == thisOP.HCOLLECT_METHOD_ID || thisOP.HCOLLECT_METHOD_ID <= 0 || thisOP.HCOLLECT_METHOD_ID == null) &&
                                                                        (r.UNCERTAINTY == thisOP.UNCERTAINTY || thisOP.UNCERTAINTY <= 0 || thisOP.UNCERTAINTY == null) &&
                                                                        (string.Equals(r.UNQUANTIFIED.ToUpper(), thisOP.UNQUANTIFIED.ToUpper()) || string.IsNullOrEmpty(thisOP.UNQUANTIFIED)));


                if (existingOP == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingOP;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        private string BuildFilePath(FILES uploadFile, string fileName)
        {
            try
            {
                //determine default object name
                // ../SITE/3043/ex.jpg
                List<string> objectName = new List<string>();
                objectName.Add("SITE");
                objectName.Add(uploadFile.SITE_ID.ToString());

                if (uploadFile.HWM_ID != null && uploadFile.HWM_ID > 0)
                {
                    // ../SITE/3043/HWM/7956/ex.jpg
                    objectName.Add("HWM");
                    objectName.Add(uploadFile.HWM_ID.ToString());
                }
                else if (uploadFile.DATA_FILE_ID != null && uploadFile.DATA_FILE_ID > 0)
                {
                    // ../SITE/3043/DATA_FILE/7956/ex.txt
                    objectName.Add("DATA_FILE");
                    objectName.Add(uploadFile.DATA_FILE_ID.ToString());
                }
                else if (uploadFile.INSTRUMENT_ID != null && uploadFile.INSTRUMENT_ID > 0)
                {
                    // ../SITE/3043/INSTRUMENT/7956/ex.jpg
                    objectName.Add("INSTRUMENT");
                    objectName.Add(uploadFile.INSTRUMENT_ID.ToString());
                }
                objectName.Add(fileName);

                return string.Join("/", objectName);
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
    }//end class ObjectivePointHandler
}//end namespace