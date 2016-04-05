using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STNServices2.Resources
{
    public class RSSFeed
    {
        public RSSChannel channel { get; set; }

        public RSSItem[] items { get; set; }
    }
}