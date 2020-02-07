using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4
{
    public class ReportResponse
    {
        public List<GirisCikisToplamlariReport> RaporList { get; set; }

        public List<Manager> ManagerList { get; set; }
    }
}
