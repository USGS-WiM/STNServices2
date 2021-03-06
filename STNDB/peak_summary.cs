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
    
    public partial class peak_summary
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public peak_summary()
        {
            this.data_file = new HashSet<data_file>();
            this.hwms = new HashSet<hwm>();
        }
    
        public int peak_summary_id { get; set; }
        public Nullable<int> member_id { get; set; }
        public Nullable<System.DateTime> peak_date { get; set; }
        public int is_peak_estimated { get; set; }
        public int is_peak_time_estimated { get; set; }
        public Nullable<double> peak_stage { get; set; }
        public int is_peak_stage_estimated { get; set; }
        public Nullable<double> peak_discharge { get; set; }
        public int is_peak_discharge_estimated { get; set; }
        public Nullable<int> vdatum_id { get; set; }
        public Nullable<double> height_above_gnd { get; set; }
        public Nullable<int> is_hag_estimated { get; set; }
        public string time_zone { get; set; }
        public Nullable<double> aep { get; set; }
        public Nullable<double> aep_lowci { get; set; }
        public Nullable<double> aep_upperci { get; set; }
        public Nullable<double> aep_range { get; set; }
        public string calc_notes { get; set; }
        public Nullable<System.DateTime> last_updated { get; set; }
        public Nullable<int> last_updated_by { get; set; }
    
        public virtual member member { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<data_file> data_file { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hwm> hwms { get; set; }
        public virtual vertical_datums vertical_datums { get; set; }
    }
}
