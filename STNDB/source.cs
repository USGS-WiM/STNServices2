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
    
    public partial class source
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public source()
        {
            this.files = new HashSet<file>();
        }
    
        public int source_id { get; set; }
        public string source_name { get; set; }
        public Nullable<int> agency_id { get; set; }
        public Nullable<System.DateTime> source_date { get; set; }
    
        public virtual agency agency { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<file> files { get; set; }
    }
}
