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
