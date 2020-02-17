using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication4
{
    public class GirisCikisToplamlariReport
    {
        internal string CalismaTuruText;

        public User Users { get; set; }
    
       public string TarihAraligi { get; set; }
        public string UserId { get; internal set; }
        public int ToplamCalismaGunSayisi { get; internal set; }

        public int ToplamCalismaSaati { get; internal set; }

        public int ToplamCalismaDakika { get; internal set; }
        public int OrtalamaCalisilmasiGerekenSaat { get; internal set; }
        public int OrtalamaCalisilmasiGerekenDakika { get; internal set; }
        public bool OrtalamaninAltinda { get; internal set; }
        public string GirisYapilmayanTarihler { get; internal set; }
        public int MaxGunSayisi { get; internal set; }
    }

}
