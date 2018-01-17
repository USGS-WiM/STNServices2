//------------------------------------------------------------------------------
//----- CsvDotNetCodec -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2015 WiM - USGS

//    authors:  Jeremy Newson USGS Wisconsin Internet Mapping
//  
//   purpose:   Serialize incomming entity as a csv stream 
//              
//
//discussion:   A Codec is an enCOder/DECoder for a resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Codecs
//
//     

#region Comments
// 01.29.15 - JKN - Created to properly serialize CSV
#endregion


using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using OpenRasta.Web;
using OpenRasta.Codecs;

using ServiceStack.Text;
using System.Reflection;


namespace STNServices2.Codecs.json
{
    [MediaType("text/csv;q=0.5", "csv")]
    public class CsvDotNetCodec1 : IMediaTypeWriter
    {
        public object Configuration { get; set; }
        public void WriteTo(object entity, IHttpEntity response, string[] paramneters)
        {
            try
            {
                if (entity == null)
                    return;

                CsvSerializer.SerializeToStream(OverrideEntity(entity), response.Stream);

            }
            catch (Exception ex) 
            {
                throw;
            }

        }//end writeto

        #region Helper Methods

        /// <summary>
        /// Overrides xmlIgnore Attribute for Reference type objects
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        protected dynamic OverrideEntity(object entity)
        {

            Type entityType = entity.GetType();
            // Check for ListTypes
            if (entityType.IsGenericType && entityType.GetGenericTypeDefinition()
                    == typeof(List<>))
            {
                var result = ((IEnumerable<object>)entity).Select(x => toCsvEntity(x));
                return result;
            }

            return new List<object>() { toCsvEntity(entity) };

        }//end OverrideAttributes

        protected dynamic toCsvEntity(object entity)
        {
            var t = entity.GetType();
            
            IEnumerable<PropertyInfo> properties =
                t.GetProperties()
                    .Where(e => e.Name.Contains("Reference") ||
                        (!e.PropertyType.IsPrimitive && !e.PropertyType.Equals(typeof(string)) && !e.PropertyType.Equals(typeof(DateTime))) && !e.Name.Contains("Links") && !String.Equals(e.PropertyType.Name, "Nullable`1"));
            //String.Equals(e.Name, "EntityKey", StringComparison.OrdinalIgnoreCase));

            foreach (PropertyInfo pInfo in properties)
            {
                if (t.GetProperty(pInfo.Name) != null)
                    pInfo.SetValue(entity, null);
            }//Next

            return entity;
        }

        #endregion

    }//end class
}//end namespace

