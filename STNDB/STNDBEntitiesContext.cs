using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace STNDB
{
    public partial class STNDBEntities : DbContext
    {
        public STNDBEntities(string connectionstring)
            : base(connectionstring)
        {
        }
    }
}
