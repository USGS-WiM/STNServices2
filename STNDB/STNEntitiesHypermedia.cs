using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiM.Hypermedia;

namespace STNDB
{

    public partial class agency : IHypermedia
    {
        public List<Link> Links { get; set; }    
    }
    public partial class approval : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class contact : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class contact_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class county : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class data_file : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class deployment_priority : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class deployment_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }    
    public partial class event_status : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class event_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class events : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class file : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class file_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class horizontal_collect_methods : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class horizontal_datums : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class housing_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class hwm : IHypermedia
    {
        public List<Link> Links { get; set; }
    }

    public partial class objective_point : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class objective_point_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class op_control_identifier : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class op_measurements : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class op_quality : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class peak_summary : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class reporting_metrics : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class role : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class sensor_brand : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class sensor_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class site : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class site_housing : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class source : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class state : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class status_type : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class vertical_collect_methods : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class vertical_datums : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class hwm_qualities : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class hwm_types : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    public partial class instr_collection_conditions : IHypermedia
    {
        public List<Link> Links { get; set; }
    }
    
}
