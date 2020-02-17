using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication4
{
    public class User
    {
        public string PersonelNo { get; set; }
      
        public string AdSoyad { get; set; }

        public string Departman { get; set; }

        public List<Visitor> VisitorsList { get; set; }
        public int CalismaTuru { get; internal set; }
        public string CalismaTuruText { get; internal set; }
    }
}
