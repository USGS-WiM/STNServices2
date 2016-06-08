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
    
    public partial class file
    {
        public int file_id { get; set; }
        public string path { get; set; }
        public string description { get; set; }
        public string photo_direction { get; set; }
        public Nullable<double> latitude_dd { get; set; }
        public Nullable<double> longitude_dd { get; set; }
        public Nullable<System.DateTime> file_date { get; set; }
        public Nullable<int> hwm_id { get; set; }
        public Nullable<int> filetype_id { get; set; }
        public Nullable<int> source_id { get; set; }
        public Nullable<int> data_file_id { get; set; }
        public Nullable<int> instrument_id { get; set; }
        public Nullable<int> objective_point_id { get; set; }
        public Nullable<int> site_id { get; set; }
        public Nullable<System.DateTime> photo_date { get; set; }
        public Nullable<int> is_nwis { get; set; }
        public string name { get; set; }
    
        public virtual hwm hwm { get; set; }
        public virtual site site { get; set; }
        public virtual instrument instrument { get; set; }
        public virtual data_file data_file { get; set; }
        public virtual source source { get; set; }
        public virtual file_type file_type { get; set; }
        public virtual objective_point objective_point { get; set; }
    }
}
