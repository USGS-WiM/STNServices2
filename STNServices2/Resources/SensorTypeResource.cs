using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using STNDB;

namespace STNServices2.Resources
{
    [XmlInclude(typeof(sensor_type))]
    public class SensorType: sensor_type
    {
        public List<deployment_type> deploymenttypes {get;set;}
    }
}
