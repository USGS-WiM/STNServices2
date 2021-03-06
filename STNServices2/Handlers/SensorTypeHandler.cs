﻿//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles Sensor Type resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 04.07.16 - TR - Created
#endregion
using OpenRasta.Web;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using STNServices2.Utilities.ServiceAgent;
using STNDB;
using WiM.Exceptions;
using WiM.Resources;
using STNServices2.Resources;

using WiM.Security;

namespace STNServices2.Handlers
{
    public class SensorTypeHandler : STNHandlerBase
    {
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<sensor_type> entities = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    entities = sa.Select<sensor_type>().Include("sensor_deployment.deployment_type")
                        .Select(st => new SensorType() { sensor_type_id = st.sensor_type_id,
                                                         sensor = st.sensor,
                                                         deploymenttypes = st.sensor_deployment.Select(x=>x.deployment_type).ToList()
                                                       }).ToList<sensor_type>();

                    sm(MessageType.info, "Count: " + entities.Count());
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = entities, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            sensor_type anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (STNAgent sa = new STNAgent())
                {
                    anEntity = sa.Select<sensor_type>().FirstOrDefault(e => e.sensor_type_id == entityId);
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);

                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetInstrumentSensorType")]
        public OperationResult GetInstrumentSensorType(Int32 instrumentId)
        {
            sensor_type anEntity = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                     if (instrumentId <= 0) throw new BadRequestException("Invalid input parameters");
                 
                     anEntity = sa.Select<instrument>().Include(i=>i.sensor_type).FirstOrDefault(i => i.instrument_id == instrumentId).sensor_type;
                     if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDeploymentSensorType")]
        public OperationResult GetDeploymentSensorType(Int32 deploymentTypeId)
        {
            sensor_type anEntity = null;

            try
            {
                using (STNAgent sa = new STNAgent())
                {
                    if (deploymentTypeId <= 0) throw new BadRequestException("Invalid input parameters");

                    //need the sensor type that this deployment type is connected to.
                    anEntity = sa.Select<sensor_deployment>().Include(i=>i.sensor_type).FirstOrDefault(i => i.deployment_type_id == deploymentTypeId).sensor_type;
                    if (anEntity == null) throw new NotFoundRequestException(); 
                    sm(sa.Messages);
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion
        #region PostMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(sensor_type anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.sensor)) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Add<sensor_type>(anEntity);
                        sm(sa.Messages);

                    }//end using
                }//end using
                return new OperationResult.Created { ResponseResource = anEntity, Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.GET
        #endregion
        #region PutMethods

        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, sensor_type anEntity)
        {
            try
            {
                if (string.IsNullOrEmpty(anEntity.sensor)) throw new BadRequestException("Invalid input parameters");

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Update<sensor_type>(entityId, anEntity);
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
            sensor_type anEntity = null;
            try
            {
                if (entityId <= 0) throw new BadRequestException("Invalid input parameters");
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNAgent sa = new STNAgent(username, securedPassword))
                    {
                        anEntity = sa.Select<sensor_type>().FirstOrDefault(i => i.sensor_type_id == entityId);
                        if (anEntity == null) throw new WiM.Exceptions.NotFoundRequestException();

                        sa.Delete<sensor_type>(anEntity);
                        sm(sa.Messages);
                    }//end using
                }//end using
                return new OperationResult.OK { Description = this.MessageString };
            }
            catch (Exception ex)
            { return HandleException(ex); }

        }//end HttpMethod.PUT

        #endregion
    }//end horizontal_datumsHandler
}
