using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication4
{
    public class Visitor
    {
        public string KartNo  { get; set; }
        public DateTime IslemZamani { get; set; }

        public string Durum { get; set; }

        public string LokasyonNo { get; set; }
        public string PersonelNo { get; internal set; }
        public string IslemTipi { get; internal set; }
    }
}
