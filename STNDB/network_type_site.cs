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
    
    public partial class network_type_site
    {
        public int networktype_site_id { get; set; }
        public int network_type_id { get; set; }
        public int site_id { get; set; }
    
        public virtual network_type network_type { get; set; }
        public virtual site site { get; set; }
    }
}
