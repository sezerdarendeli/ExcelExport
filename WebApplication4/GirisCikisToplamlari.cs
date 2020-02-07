using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication4
{
    public class GirisCikisToplamlari
    {
       public User Users { get; set; }
    
        public List<GunlukZiyaretSaati> GunlukZiyaretSaatleri { get; set; }

        public decimal HaftalikToplam { get; set; }

        public int GirisCikisYapilanGunSayisi { get; set; }

        public int BesSaatenAzOlanGunSayisi {get; set; }
        public int GecgirisErkenCikis { get; internal set; }
    }

    public class GunlukZiyaretSaati
    {
        public DateTime Tarih { get; set; }

        public DateTime GirisZamani { get; set; }

        public DateTime CikisZamani { get; set; }

        public DateTime OgleYemegiCikisZamani { get; set; }

        public decimal OgleYemegindeKalmaDakika { get; set; }


        public TimeSpan GunlukCalismaSaatiTimeSpan { get; set; }

        public decimal GunlukCalismaDakika { get; set; }
        public DateTime OgleYemegiDonusZamani { get; internal set; }
        public object Yonetici { get; internal set; }
    }

}
