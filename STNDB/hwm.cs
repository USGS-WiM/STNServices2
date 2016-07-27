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
    
    public partial class hwm
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hwm()
        {
            this.files = new HashSet<file>();
        }
    
        public int hwm_id { get; set; }
        public string waterbody { get; set; }
        public Nullable<int> site_id { get; set; }
        public Nullable<int> event_id { get; set; }
        public int hwm_type_id { get; set; }
        public int hwm_quality_id { get; set; }
        public string hwm_locationdescription { get; set; }
        public Nullable<double> latitude_dd { get; set; }
        public Nullable<double> longitude_dd { get; set; }
        public Nullable<System.DateTime> survey_date { get; set; }
        public Nullable<double> elev_ft { get; set; }
        public Nullable<int> vdatum_id { get; set; }
        public Nullable<int> vcollect_method_id { get; set; }
        public string bank { get; set; }
        public Nullable<int> approval_id { get; set; }
        public Nullable<int> marker_id { get; set; }
        public Nullable<double> height_above_gnd { get; set; }
        public Nullable<int> hcollect_method_id { get; set; }
        public Nullable<int> peak_summary_id { get; set; }
        public string hwm_notes { get; set; }
        public string hwm_environment { get; set; }
        public Nullable<System.DateTime> flag_date { get; set; }
        public Nullable<double> stillwater { get; set; }
        public Nullable<int> hdatum_id { get; set; }
        public Nullable<int> flag_member_id { get; set; }
        public Nullable<int> survey_member_id { get; set; }
        public Nullable<double> uncertainty { get; set; }
        public Nullable<double> hwm_uncertainty { get; set; }
    
        public virtual hwm_types hwm_types { get; set; }
        public virtual hwm_qualities hwm_qualities { get; set; }
        public virtual marker marker { get; set; }
        public virtual vertical_datums vertical_datums { get; set; }
        public virtual approval approval { get; set; }
        public virtual events @event { get; set; }
        public virtual site site { get; set; }
        public virtual peak_summary peak_summary { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<file> files { get; set; }
        public virtual horizontal_datums horizontal_datums { get; set; }
        public virtual horizontal_collect_methods horizontal_collect_methods { get; set; }
        public virtual vertical_collect_methods vertical_collect_methods { get; set; }
        public virtual member survey_member { get; set; }
        public virtual member flag_member { get; set; }
    }
}
