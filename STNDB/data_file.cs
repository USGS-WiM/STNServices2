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
    
    public partial class data_file
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public data_file()
        {
            this.files = new HashSet<file>();
        }
    
        public int data_file_id { get; set; }
        public Nullable<System.DateTime> start { get; set; }
        public Nullable<System.DateTime> end { get; set; }
        public Nullable<System.DateTime> good_start { get; set; }
        public Nullable<System.DateTime> good_end { get; set; }
        public Nullable<int> processor_id { get; set; }
        public Nullable<int> instrument_id { get; set; }
        public Nullable<int> approval_id { get; set; }
        public Nullable<System.DateTime> collect_date { get; set; }
        public Nullable<int> peak_summary_id { get; set; }
        public string elevation_status { get; set; }
        public string time_zone { get; set; }
    
        public virtual instrument instrument { get; set; }
        public virtual approval approval { get; set; }
        public virtual peak_summary peak_summary { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<file> files { get; set; }
        public virtual member member { get; set; }
    }
}
