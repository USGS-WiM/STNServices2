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
    
    public partial class objective_point
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public objective_point()
        {
            this.op_measurements = new HashSet<op_measurements>();
            this.op_control_identifier = new HashSet<op_control_identifier>();
            this.files = new HashSet<file>();
        }
    
        public int objective_point_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Nullable<double> elev_ft { get; set; }
        public Nullable<System.DateTime> date_established { get; set; }
        public Nullable<double> op_is_destroyed { get; set; }
        public string op_notes { get; set; }
        public Nullable<int> site_id { get; set; }
        public Nullable<int> vdatum_id { get; set; }
        public Nullable<double> latitude_dd { get; set; }
        public Nullable<double> longitude_dd { get; set; }
        public Nullable<int> hdatum_id { get; set; }
        public Nullable<int> hcollect_method_id { get; set; }
        public Nullable<int> vcollect_method_id { get; set; }
        public int op_type_id { get; set; }
        public Nullable<System.DateTime> date_recovered { get; set; }
        public Nullable<double> uncertainty { get; set; }
        public string unquantified { get; set; }
        public Nullable<int> op_quality_id { get; set; }
        public Nullable<System.DateTime> last_updated { get; set; }
        public Nullable<int> last_updated_by { get; set; }
    
        public virtual objective_point_type objective_point_type { get; set; }
        public virtual op_quality op_quality { get; set; }
        public virtual site site { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<op_measurements> op_measurements { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<op_control_identifier> op_control_identifier { get; set; }
        public virtual vertical_datums vertical_datums { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<file> files { get; set; }
    }
}
