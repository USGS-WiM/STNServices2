//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace STNDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class sensor_deployment
    {
        public int sensor_deployment_id { get; set; }
        public Nullable<int> sensor_type_id { get; set; }
        public Nullable<int> deployment_type_id { get; set; }
    
        public virtual sensor_type sensor_type { get; set; }
        public virtual deployment_type deployment_type { get; set; }
    }
}
