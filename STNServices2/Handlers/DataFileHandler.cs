//------------------------------------------------------------------------------
//----- DataFileHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Data file resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.`````````
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 01.30.13 - JKN - added Get(string boolean) method to return approved or nonapproved datafiles
// 01.28.13 - JKN - Update POST handler to check if table is empty before assigning a key
// 07.09.12 - JKN -Created

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
using System.Runtime.InteropServices;


namespace STNServices2.Handlers
{
    public class DataFileHandler : HandlerBase
    {

        #region Properties
        public override string entityName
        {
            get { return "DATAFILES"; }
        }
        #endregion
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<DATA_FILE> dataFiles = new List<DATA_FILE>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFiles = aSTNE.DATA_FILE.OrderBy(df => df.DATA_FILE_ID).Where(df => df.APPROVAL_ID > 0).ToList();
                    }
                    else
                    {
                        //they are logged in, give them all
                        dataFiles = aSTNE.DATA_FILE.OrderBy(df => df.DATA_FILE_ID).ToList();
                    }

                    if (dataFiles != null)
                        dataFiles.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        /// I don't think this one is used anywhere
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string boolean)
        {
            try
            {
                List<DATA_FILE> dataFiles = new List<DATA_FILE>();
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(boolean, out isApprovedStatus);

                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (isApprovedStatus)
                    {
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable().Where(df => df.APPROVAL_ID > 0).ToList();
                    }
                    else
                    {
                        //return all non approved hwms
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable().Where(df => df.APPROVAL_ID < 0 || !df.APPROVAL_ID.HasValue).ToList();
                    }

                    if (dataFiles != null)
                        dataFiles.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApprovedDataFiles")]
        public OperationResult GetApprovedDataFiles(Int32 ApprovalId)
        {
            List<DATA_FILE> datafileList = new List<DATA_FILE>();

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    datafileList = aSTNE.APPROVALs.FirstOrDefault(a => a.APPROVAL_ID == ApprovalId).DATA_FILE.ToList();

                    if (datafileList != null)
                        datafileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = datafileList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredDataFiles")]
        public OperationResult GetFilteredDataFiles(string approved, [Optional] Int32 eventId, [Optional] Int32 memberId, [Optional] string state)
        {
            try
            {
                List<DATA_FILE> dataFileList; ;
                //set defaults
                //default to false
                bool isApprovedStatus = false;
                Boolean.TryParse(approved, out isApprovedStatus);
                string filterState = GetStateByName(state).ToString();
                Int32 filterMember = (memberId > 0) ? memberId : -1;
                Int32 filterEvent = (eventId > 0) ? eventId : -1;

                using (STNEntities2 aSTNE = GetRDS())
                {
                    //Because 'Where' is producing an IQueryable, 
                    //the execution is deferred until the ToList so you can chain 'Wheres' together.
                    IQueryable<DATA_FILE> query;
                    if (isApprovedStatus)
                        query = aSTNE.DATA_FILE.Where(h => h.APPROVAL_ID > 0);
                    else
                        query = aSTNE.DATA_FILE.Where(h => h.APPROVAL_ID <= 0 || !h.APPROVAL_ID.HasValue);

                    if (filterEvent > 0)
                        query = query.Where(d => d.INSTRUMENT.EVENT_ID == filterEvent);

                    if (filterState != State.UNSPECIFIED.ToString())
                        query = query.Where(d => d.INSTRUMENT.SITE.STATE == filterState);

                    if (filterMember > 0)
                        //query = query.Where(d => d.INSTRUMENT.INSTRUMENT_STATUS.Any(i => i.COLLECTION_TEAM_ID == filterMember));
                        query = query.Where(d => d.PROCESSOR_ID == filterMember);


                    dataFileList = query.ToList();

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFileList = dataFileList.Where(d => d.APPROVAL_ID > 0).ToList();
                    }


                    if (dataFileList != null)
                        dataFileList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


                }//end using

                return new OperationResult.OK { ResponseResource = dataFileList };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }// end HttpMethod.Get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            DATA_FILE aDataFile;

            //Return BadRequest if there is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        aDataFile = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == entityId && df.APPROVAL_ID > 0);
                    }
                    else
                    {
                        //they are logged in, give them all
                        aDataFile = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == entityId);
                    }

                    if (aDataFile != null)
                        aDataFile.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

                return new OperationResult.OK { ResponseResource = aDataFile };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetFileDataFile(Int32 fileId)
        {
            DATA_FILE aDataFile;

            //Return BadRequest if there is no ID
            if (fileId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        aDataFile = aSTNE.FILES.FirstOrDefault(f => f.FILE_ID == fileId).DATA_FILE;
                        aDataFile = aDataFile.APPROVAL_ID > 0 ? aDataFile : null;
                    }
                    else
                    {
                        //they are logged in, give them all
                        aDataFile = aSTNE.FILES.FirstOrDefault(f => f.FILE_ID == fileId).DATA_FILE;
                    }

                }//end using   

                return new OperationResult.OK { ResponseResource = aDataFile };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentDataFiles")]
        public OperationResult GetInstrumentDataFiles(Int32 instrumentId)
        {
            List<DATA_FILE> dataFiles = new List<DATA_FILE>();

            //Return BadRequest if there is no ID
            if (instrumentId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable()
                                .Where(df => df.INSTRUMENT_ID == instrumentId && df.APPROVAL_ID > 0)
                                .OrderBy(df => df.DATA_FILE_ID)
                                .ToList<DATA_FILE>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable()
                                .Where(df => df.INSTRUMENT_ID == instrumentId)
                                .OrderBy(df => df.DATA_FILE_ID)
                                .ToList<DATA_FILE>();
                    }

                    if (dataFiles != null)
                        dataFiles.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = dataFiles };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPeakSummaryDatafiles")]
        public OperationResult GetPeakSummaryDatafiles(Int32 peakSummaryId)
        {
            List<DATA_FILE> dataFiles = new List<DATA_FILE>();

            //Return BadRequest if there is no ID
            if (peakSummaryId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {

                    if (Context.User == null || Context.User.Identity.IsAuthenticated == false)
                    {
                        //they are the general public..only approved ones
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable()
                                .Where(df => df.PEAK_SUMMARY_ID == peakSummaryId && df.APPROVAL_ID > 0)
                                .OrderBy(df => df.DATA_FILE_ID)
                                .ToList<DATA_FILE>();
                    }
                    else
                    {
                        //they are logged in, give them all
                        dataFiles = aSTNE.DATA_FILE.AsEnumerable()
                                .Where(df => df.PEAK_SUMMARY_ID == peakSummaryId)
                                .OrderBy(df => df.DATA_FILE_ID)
                                .ToList<DATA_FILE>();
                    }

                    if (dataFiles != null)
                        dataFiles.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

                return new OperationResult.OK { ResponseResource = dataFiles };
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
        [HttpOperation(HttpMethod.POST)] //, ForUriName = "CreateDataFile")]
        public OperationResult Post(DATA_FILE aDataFile)
        {
            //Return BadRequest if missing required fields
            if ((aDataFile.INSTRUMENT_ID <= 0 || !aDataFile.INSTRUMENT_ID.HasValue)) //.DATA_FILE_ID <= 0))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        if (!Exists(aSTNE.DATA_FILE, ref aDataFile))
                        {
                            aSTNE.DATA_FILE.AddObject(aDataFile);
                            aSTNE.SaveChanges();
                        }//end if

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return new OperationResult.OK { ResponseResource = aDataFile };
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
        public OperationResult Put(Int32 entityId, DATA_FILE aDataFile)
        {
            DATA_FILE DataFileToUpdate = null;
            //Return BadRequest if missing required fields
            if (aDataFile.DATA_FILE_ID <= 0 || entityId <= 0)
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
                        //Grab the instrument row to update
                        DataFileToUpdate = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == entityId);

                        //Update fields
                        DataFileToUpdate.START = aDataFile.START;
                        DataFileToUpdate.END = aDataFile.END;
                        DataFileToUpdate.GOOD_START = aDataFile.GOOD_START;
                        DataFileToUpdate.GOOD_END = aDataFile.GOOD_END;
                        DataFileToUpdate.PROCESSOR_ID = aDataFile.PROCESSOR_ID;
                        DataFileToUpdate.INSTRUMENT_ID = aDataFile.INSTRUMENT_ID;
                        DataFileToUpdate.APPROVAL_ID = aDataFile.APPROVAL_ID;
                        DataFileToUpdate.COLLECT_DATE = aDataFile.COLLECT_DATE;
                        DataFileToUpdate.PEAK_SUMMARY_ID = aDataFile.PEAK_SUMMARY_ID;
                        DataFileToUpdate.ELEVATION_STATUS = aDataFile.ELEVATION_STATUS;

                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aDataFile };
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
        [HttpOperation(HttpMethod.DELETE, ForUriName = "DeleteInstrument")]
        public OperationResult Delete(Int32 dataFileId)
        {
            //Return BadRequest if missing required fields
            if (dataFileId <= 0)
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
                        //fetch the object to be updated (assuming that it exists)
                        DATA_FILE ObjectToBeDeleted = aSTNE.DATA_FILE.SingleOrDefault(df => df.DATA_FILE_ID == dataFileId);
                        //delete it
                        aSTNE.DATA_FILE.DeleteObject(ObjectToBeDeleted);

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
        private bool Exists(ObjectSet<DATA_FILE> entityRDS, ref DATA_FILE anEntity)
        {
            DATA_FILE existingEntity;
            DATA_FILE thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = entityRDS.FirstOrDefault(e => e.INSTRUMENT_ID == thisEntity.INSTRUMENT_ID &&
                                                              (!thisEntity.START.HasValue | DateTime.Equals(e.START.Value, thisEntity.START.Value)) &&
                                                              (!thisEntity.END.HasValue | DateTime.Equals(e.END.Value, thisEntity.END.Value)) &&
                                                              (!thisEntity.GOOD_START.HasValue | DateTime.Equals(e.GOOD_START.Value, thisEntity.GOOD_START.Value)) &&
                                                              (!thisEntity.GOOD_END.HasValue | DateTime.Equals(e.GOOD_END.Value, thisEntity.GOOD_END.Value)) &&
                                                              (!thisEntity.COLLECT_DATE.HasValue | DateTime.Equals(e.COLLECT_DATE.Value, thisEntity.COLLECT_DATE.Value)) &&
                                                              (!thisEntity.PROCESSOR_ID.HasValue | e.PROCESSOR_ID.Value == thisEntity.PROCESSOR_ID.Value || thisEntity.PROCESSOR_ID.Value <= 0) &&
                                                              (!thisEntity.PEAK_SUMMARY_ID.HasValue | e.PEAK_SUMMARY_ID.Value == thisEntity.PEAK_SUMMARY_ID.Value || thisEntity.PEAK_SUMMARY_ID.Value <= 0) &&
                                                              (!thisEntity.APPROVAL_ID.HasValue | e.APPROVAL_ID.Value == thisEntity.APPROVAL_ID.Value || thisEntity.APPROVAL_ID.Value <= 0));


                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }//end class DataFileHandler
}//end namespace