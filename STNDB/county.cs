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
    
    public partial class county
    {
        public int county_id { get; set; }
        public string county_name { get; set; }
        public int state_id { get; set; }
        public Nullable<int> state_fip { get; set; }
        public Nullable<int> county_fip { get; set; }
    
        public virtual state state { get; set; }
    }
}
