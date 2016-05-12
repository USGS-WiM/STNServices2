//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
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
using System.Data.Entity;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class DepolymentTypeHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<deployment_type> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<deployment_type>().OrderBy(e => e.deployment_type_id).ToList();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            deployment_type anEntity = null;

            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<deployment_type>().FirstOrDefault(e => e.deployment_type_id == entityId);
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
            finally
            {

            }//end try
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetInstrumentDeploymentType")]
        public OperationResult GetInstrumentDeploymentType(Int32 instrumentId)
        {
            deployment_type anEntity = null;
            try
            {
                if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent(true))
                {
                    anEntity = sa.Select<instrument>().FirstOrDefault(i => i.instrument_id == instrumentId).deployment_type;
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET

        [HttpOperation(ForUriName = "GetSensorDeploymentTypes")]
        public OperationResult GetSensorDeploymentTypes(Int32 sensorTypeId)
        {
            List<deployment_type> deployment_typeList = null;
            try
            {
                if (sensorTypeId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent(true))
                {
                    deployment_typeList = sa.Select<sensor_deployment>().Where(sd => sd.sensor_type_id == sensorTypeId).Select(s => s.deployment_type).ToList();
                    sm(MessageType.info, "Count: " + deployment_typeList.Count);
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = deployment_typeList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }//end HttpMethod.GET
        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(deployment_type anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.method))
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<deployment_type>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET

        //post sensor_deployment and return all deployment_types for this sensor_type
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSensorDeployment")]
        public OperationResult AddSensorDeployment(Int32 sensorTypeId, Int32 deploymentTypeId)
        {
            sensor_deployment anEntity = null;
            List<deployment_type> deploymentTypeList = null;
            try
            {
                if (sensorTypeId <= 0 || deploymentTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (sa.Select<sensor_type>().First(s => s.sensor_type_id == sensorTypeId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<deployment_type>().First(n => n.deployment_type_id == deploymentTypeId) == null)
                            throw new NotFoundRequestException();

                        if (sa.Select<sensor_deployment>().FirstOrDefault(sd => sd.sensor_type_id == sensorTypeId && sd.deployment_type_id == deploymentTypeId) == null)
                        {
                            anEntity = new sensor_deployment();
                            anEntity.sensor_type_id = sensorTypeId;
                            anEntity.deployment_type_id = deploymentTypeId;
                            anEntity = sa.Add<sensor_deployment>(anEntity);
                            sm(sa.Messages);
                        }
                        //return list of network types
                        deploymentTypeList = sa.Select<deployment_type>().Include(dt => dt.sensor_deployment).Where(dt => dt.sensor_deployment.Any(sd => sd.sensor_type_id == sensorTypeId)).ToList();

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = deploymentTypeList, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }

        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, deployment_type anEntity)
        {
            try
            {
                if (entityId <=0 || string.IsNullOrEmpty(anEntity.method))
                    throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<deployment_type>(entityId, anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.Modified { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
        #region DeleteMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            deployment_type anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<deployment_type>().FirstOrDefault(i => i.deployment_type_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<deployment_type>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSensorDeployment")]
        public OperationResult RemoveSensorDeployment(Int32 sensorTypeId, Int32 deploymentTypeId)
        {
            try
            {
                if (sensorTypeId <= 0 || deploymentTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        if (sa.Select<sensor_type>().First(s => s.sensor_type_id == sensorTypeId) == null)
                            throw new NotFoundRequestException();

                        sensor_deployment ObjectToBeDeleted = sa.Select<sensor_deployment>().SingleOrDefault(sd => sd.sensor_type_id == sensorTypeId && sd.deployment_type_id == deploymentTypeId);

                        if (ObjectToBeDeleted == null) throw new NotFoundRequestException();
                        sa.Delete<sensor_deployment>(ObjectToBeDeleted);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }
        }
        #endregion
    }//end deployment_typeHandler
}
