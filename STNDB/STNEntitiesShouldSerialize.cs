using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STNDB
{
    public partial class member
    {
        //public bool ShouldSerializepassword() { return false; }
        public bool ShouldSerializesalt() { return false; }
    }
}
