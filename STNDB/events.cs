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
    
    public partial class events
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public events()
        {
            this.hwms = new HashSet<hwm>();
            this.instruments = new HashSet<instrument>();
        }
    
        public int event_id { get; set; }
        public string event_name { get; set; }
        public Nullable<System.DateTime> event_start_date { get; set; }
        public Nullable<System.DateTime> event_end_date { get; set; }
        public string event_description { get; set; }
        public Nullable<int> event_type_id { get; set; }
        public Nullable<int> event_status_id { get; set; }
        public Nullable<int> event_coordinator { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hwm> hwms { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<instrument> instruments { get; set; }
        public virtual event_status event_status { get; set; }
        public virtual event_type event_type { get; set; }
        public virtual member member { get; set; }
    }
}