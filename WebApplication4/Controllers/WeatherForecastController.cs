using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebApplication4.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IMemoryCache _memoryCache;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("post")]
        public dynamic UploadJustFile(IFormCollection form)
        {
            try
            {
                foreach (var file in form.Files)
                {
                    UploadFile(file);
                }

                string cacheKey = $"personelListKey";
                var response = new ReportResponse();


                var cacheExpirationOptions =
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                        Priority = CacheItemPriority.Normal
                    };

                var valueList = GetValuesList();
                _memoryCache.Set(cacheKey, valueList, cacheExpirationOptions);


                return new { Success = true };
            }
            catch (Exception ex)
            {
                return new { Success = false, ex.Message };
            }
        }




        public void UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty!");
            byte[] fileArray;
            using (var stream = file.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                fileArray = memoryStream.ToArray();
            }


            var filePath = @"content\\";

            var result = ByteArrayToFile(filePath, fileArray);
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {


                string targetFile = Path.Combine(fileName, "test.xls");
                if (System.IO.File.Exists(targetFile)) System.IO.File.Delete(targetFile);
                System.IO.File.WriteAllBytes(targetFile, byteArray);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        [HttpGet]
        public ReportResponse Get(string ortalama = null, string manager = null, int workingType = 0)
        {
            string cacheKey = $"personelListKey";

            var response = new ReportResponse();
            if (!_memoryCache.TryGetValue(cacheKey, out List<GirisCikisToplamlari> valueList))
            {

                var cacheExpirationOptions =
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                        Priority = CacheItemPriority.Normal
                    };

                valueList = GetValuesList();
                _memoryCache.Set(cacheKey, valueList, cacheExpirationOptions);
            }


            var rng = new Random();
            var non_filterresponse = new List<GirisCikisToplamlariReport>();
            response.ManagerList = new List<Manager>();

            foreach (var item in valueList)
            {
                string tarihler = "";
                var reportItem = new GirisCikisToplamlariReport();
                reportItem.UserId = item.Users.PersonelNo;
                reportItem.Users = item.Users;
                var ilkTarih = item.GunlukZiyaretSaatleri.OrderBy(e => e.Tarih).FirstOrDefault().Tarih;
                var sonTarih = item.GunlukZiyaretSaatleri.OrderByDescending(e => e.Tarih).FirstOrDefault().Tarih;
                reportItem.ToplamCalismaGunSayisi = item.GunlukZiyaretSaatleri.GroupBy(e => e.Tarih).Count();
                reportItem.TarihAraligi = ilkTarih.ToShortDateString() + " - " + sonTarih.ToShortDateString();
                reportItem.ToplamCalismaDakika = Convert.ToInt32(item.GunlukZiyaretSaatleri.GroupBy(x => new { x.Tarih, value = x.GunlukCalismaDakika }).Sum(e => e.Key.value));
                reportItem.ToplamCalismaSaati = Convert.ToInt32(Math.Round(Convert.ToDouble(reportItem.ToplamCalismaDakika / 60)));
                reportItem.OrtalamaCalisilmasiGerekenSaat = reportItem.ToplamCalismaGunSayisi * 8;
                reportItem.OrtalamaCalisilmasiGerekenSaat = reportItem.ToplamCalismaGunSayisi * 8;
                reportItem.OrtalamaCalisilmasiGerekenDakika = reportItem.OrtalamaCalisilmasiGerekenSaat * 60;
                reportItem.CalismaTuruText = item.Users.CalismaTuruText;
                if (reportItem.ToplamCalismaDakika < reportItem.OrtalamaCalisilmasiGerekenDakika)
                {
                    reportItem.OrtalamaninAltinda = true;
                }
                else
                {
                    reportItem.OrtalamaninAltinda = false;
                }
                if (!response.ManagerList.Any(e => e.ManagerName == item.Users.Departman))
                    response.ManagerList.Add(new Manager { Key = item.Users.Departman, ManagerName = item.Users.Departman });

                reportItem.GirisYapilmayanTarihler = "";

                DateTime StartDate = new DateTime(2020, 1, 2, 0, 0, 0);
                DateTime EndDate = new DateTime(2020, 2, 16, 0, 0, 0);
                int count = 0;
                int maxGunSayisi = 0;
                foreach (DateTime day in EachDay(StartDate, EndDate))
                {
              

                    if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                    {
                        if (day.DayOfWeek == DayOfWeek.Saturday)
                        {
                            maxGunSayisi++;
                        }
                        continue;
                    }
                    else
                    {
                        maxGunSayisi++;
                    }

                    if (day.Date.ToShortDateString().Equals("20.01.2020"))
                        continue;
                    if (!item.GunlukZiyaretSaatleri.Any(e => e.Tarih.Equals(day.Date)))
                    {
                        tarihler += day.Date.ToShortDateString() + Environment.NewLine;
                        count = count + 1;
                    }

                }
                reportItem.ToplamCalismaGunSayisi = reportItem.ToplamCalismaGunSayisi - count;
                reportItem.GirisYapilmayanTarihler = tarihler;
                reportItem.MaxGunSayisi = maxGunSayisi;
                non_filterresponse.Add(reportItem);






            }

            response.RaporList = new List<GirisCikisToplamlariReport>();
            if (string.IsNullOrEmpty(ortalama) || ortalama.Equals("hepsi"))
            {
                response.RaporList = non_filterresponse;
            }
            else if (ortalama.Equals("alt"))
            {
                response.RaporList = non_filterresponse.Where(e => e.OrtalamaninAltinda == true).ToList();
            }
            else if (ortalama.Equals("ust"))
            {
                response.RaporList = non_filterresponse.Where(e => e.OrtalamaninAltinda == false).ToList();
            }

            if (!string.IsNullOrEmpty(manager) && !manager.Contains("Yönetici Seçiniz"))
            {
                response.RaporList = response.RaporList.Where(e => e.Users.Departman == manager).ToList();
            }

            if (workingType != 0)
            {
                response.RaporList = response.RaporList.Where(e => e.Users.CalismaTuru == workingType).ToList();
            }

            response.MaxCalismaSayisi = response.RaporList.Max(e => e.ToplamCalismaGunSayisi);


            return response;
        }

        [HttpPost]
        public void PostData(IFormCollection file)
        {
            string cacheKey = $"personelListKey";

            var response = new ReportResponse();
            if (!_memoryCache.TryGetValue(cacheKey, out List<GirisCikisToplamlari> valueList))
            {

                var cacheExpirationOptions =
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddMinutes(30),
                        Priority = CacheItemPriority.Normal
                    };

                valueList = GetValuesList();
                _memoryCache.Set(cacheKey, valueList, cacheExpirationOptions);
            }
        }


        public List<GirisCikisToplamlari> GetValuesList()
        {
            var list = Import(Directory.GetCurrentDirectory() + @"\content\test.xls");
            Console.WriteLine("Ziyaret logları okundu. Veriler inceleniyor...");
            var userCalismaSaatiToplamlari = new List<GirisCikisToplamlari>();
            foreach (var item in list)
            {
                var user = item;
                GirisCikisToplamlari girisCikisToplamlari = new GirisCikisToplamlari();
                girisCikisToplamlari.Users = user;
                if (item.PersonelNo.Equals(""))
                    continue;

                girisCikisToplamlari.GunlukZiyaretSaatleri = new List<GunlukZiyaretSaati>();
                foreach (var visitorItem in item.VisitorsList.Where(e => e.PersonelNo == item.PersonelNo))
                {
                    var gunlukZiyaretSaatleri = new GunlukZiyaretSaati();
                    var date = visitorItem.IslemZamani.ToShortDateString();

                    if (girisCikisToplamlari.GunlukZiyaretSaatleri.Any(e => e.Tarih.Equals(date)))
                        continue;


                    var itemZiyaretLog = item.VisitorsList.Where(e => e.PersonelNo == item.PersonelNo && e.IslemZamani.ToShortDateString().Equals(date));

                    // Giriş Çıkış Hesaplatma
                    var girisVar = false;

                    DateTime ilkGirisZamani = new DateTime();
                    DateTime ilkGirisZamaniYazilacak = new DateTime();
                    DateTime ilkCikisZamani = new DateTime();
                    decimal calismaZamaniToplami = 0;
                    bool ilkDegerAtanmasi = false;
                    DateTime ogleYemegiCikisZamani = new DateTime();
                    DateTime ogleYemegiDonusZamani = new DateTime();
                    bool ogleYemegineCikmis = false;
                    foreach (var ziyaretLog in item.VisitorsList.Where(e => e.PersonelNo == item.PersonelNo && e.IslemZamani.ToShortDateString().Equals(date)).OrderBy(e => e.IslemZamani))
                    {
                        if (ziyaretLog.IslemTipi == "Giris" && !girisVar)
                        {
                            girisVar = true;
                            ilkGirisZamani = ziyaretLog.IslemZamani;
                            if (!ilkDegerAtanmasi)
                            {
                                ilkGirisZamaniYazilacak = ziyaretLog.IslemZamani;
                                ilkDegerAtanmasi = true;
                            }

                            TimeSpan start = new TimeSpan(11, 00, 0);
                            TimeSpan end = new TimeSpan(15, 00, 0);
                            var result = TimeBetween(ilkGirisZamani, start, end);

                            if (result == true && ogleYemegineCikmis)
                            {
                                if (ogleYemegiCikisZamani < ilkGirisZamani)
                                {
                                    ogleYemegiDonusZamani = ilkGirisZamani;
                                }
                            }

                        }


                        if (ziyaretLog.IslemTipi == "Cikis" && girisVar)
                        {

                            girisVar = false;
                            DateTime sonCikisSaati = new DateTime();
                            foreach (var sonCikisItem in item.VisitorsList.Where(e => e.PersonelNo == item.PersonelNo &&
                                                                        e.IslemZamani.ToShortDateString().Equals(date) &&
                                                                        e.IslemZamani >= ziyaretLog.IslemZamani).
                                                                        OrderBy(e => e.IslemZamani))

                            {

                                if (sonCikisItem.IslemTipi == "Giris")
                                {
                                    break;
                                }
                                else
                                {
                                    sonCikisSaati = sonCikisItem.IslemZamani;
                                }
                            }
                            TimeSpan start = new TimeSpan(11, 30, 0);
                            TimeSpan end = new TimeSpan(15, 00, 0);
                            var result = TimeBetween(sonCikisSaati, start, end);

                            if (result == true && ogleYemegineCikmis == false)
                            {
                                ogleYemegiCikisZamani = sonCikisSaati;
                                ogleYemegineCikmis = true;
                            }


                            ilkCikisZamani = sonCikisSaati;
                            TimeSpan calismaZamanifark = ilkCikisZamani - ilkGirisZamani;
                            calismaZamaniToplami += Convert.ToDecimal(calismaZamanifark.TotalMinutes);
                            continue;
                        }

                    }
                    gunlukZiyaretSaatleri.GunlukCalismaDakika = calismaZamaniToplami;

                    gunlukZiyaretSaatleri.GirisZamani = ilkGirisZamaniYazilacak;
                    if (ilkCikisZamani.Year == 1)
                    {
                        ilkCikisZamani = gunlukZiyaretSaatleri.GirisZamani;
                    }

                    gunlukZiyaretSaatleri.CikisZamani = ilkCikisZamani;
                    gunlukZiyaretSaatleri.Tarih = Convert.ToDateTime(date);
                    if (ogleYemegineCikmis)
                    {
                        TimeSpan fark = new TimeSpan(0);
                        gunlukZiyaretSaatleri.OgleYemegiCikisZamani = ogleYemegiCikisZamani;
                        gunlukZiyaretSaatleri.OgleYemegiDonusZamani = ogleYemegiDonusZamani;
                        if (gunlukZiyaretSaatleri.OgleYemegiCikisZamani.Year != 0001 && gunlukZiyaretSaatleri.OgleYemegiDonusZamani.Year != 0001)
                        {
                            fark = ogleYemegiDonusZamani - ogleYemegiCikisZamani;
                        }

                        gunlukZiyaretSaatleri.OgleYemegindeKalmaDakika = Convert.ToDecimal(fark.TotalMinutes);
                    }
                    girisCikisToplamlari.GunlukZiyaretSaatleri.Add(gunlukZiyaretSaatleri);
                }
                userCalismaSaatiToplamlari.Add(girisCikisToplamlari);
            }
            return userCalismaSaatiToplamlari;
        }

        public static List<User> Import(String path)
        {
            var listGirisLokasyonId = new List<string>();
            listGirisLokasyonId.Add("1");
            listGirisLokasyonId.Add("4");
            listGirisLokasyonId.Add("5");

            var listCikisLokasyonId = new List<string>();
            listCikisLokasyonId.Add("2");
            listCikisLokasyonId.Add("3");
            listCikisLokasyonId.Add("6");


            object rowIndex = 2;
            List<User> userList = new List<User>();
            List<Visitor> visitorList = new List<Visitor>();
            var calismaTurleriList = GetSheet2(path);
            GetSheet1(path, listGirisLokasyonId, listCikisLokasyonId, userList, visitorList, calismaTurleriList);

            foreach (var item in userList)
            {
                foreach (var visitorItem in visitorList.Where(e => e.PersonelNo == item.PersonelNo))
                {
                    if (item.VisitorsList == null)
                        item.VisitorsList = new List<Visitor>();
                    item.VisitorsList.Add(visitorItem);
                }

            }

            //app.Workbooks.Close();
            return userList;
        }

        private static List<CalismaTipleri> GetSheet2(string path)
        {
            var response = new List<CalismaTipleri>();

            string con = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + @path.Replace("\\", @"\") + ";" +
            @"Extended Properties='Excel 8.0;HDR=Yes;'";
            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [Sheet2$]", connection);




                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var calismaTipleriClass = new CalismaTipleri();
                        calismaTipleriClass.PersonelNo = Convert.ToString(dr[1]);
                        calismaTipleriClass.CalismaTuru = Convert.ToInt32(dr[2]);

                        response.Add(calismaTipleriClass);



                    }
                }
            }

            return response;

        }

        private static void GetSheet1(string path, List<string> listGirisLokasyonId, List<string> listCikisLokasyonId, List<User> userList, List<Visitor> visitorList, List<CalismaTipleri> calismaTurleriList)
        {
            string con = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + @path.Replace("\\", @"\") + ";" +
            @"Extended Properties='Excel 8.0;HDR=Yes;'";
            using (OleDbConnection connection = new OleDbConnection(con))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);



                using (OleDbDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {



                        var kartNo = Convert.ToString(dr[9]); //Convert.ToString(((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, 10]).Value2);
                        var personelNo = Convert.ToString(dr[2]); //Convert.ToString(((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, 3]).Value2);

                        if (calismaTurleriList.Any(e => e.PersonelNo == personelNo && e.CalismaTuru == 3))
                            continue;

                        var adSoyad = Convert.ToString(dr[1]); //Convert.ToString(((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, 2]).Value2);
                        if (adSoyad.Contains("ZÝYARETÇ"))
                        {
                            continue;
                        }


                        var departman = Convert.ToString(dr[0]); //Convert.ToString(((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, 1]).Value2);
                        if (string.IsNullOrWhiteSpace(kartNo))
                        {
                            kartNo = "";
                        }
                        if (departman.Equals("Euroko"))
                            continue;


                        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(adSoyad);
                        byte[] utf8BytesDepartman = System.Text.Encoding.UTF8.GetBytes(departman);
                        string s_unicode2 = System.Text.Encoding.UTF8.GetString(utf8Bytes).Replace("Þ", "Ş").Replace("Ý", "İ").Replace("Ð", "Ğ").Replace("þ", "ş").ToUpperInvariant();
                        string s_unicode2Departman = System.Text.Encoding.UTF8.GetString(utf8BytesDepartman).Replace("Þ", "Ş").Replace("Ý", "İ").Replace("Ð", "Ğ").Replace("þ", "ş").ToUpperInvariant();

                        if (!userList.Any(e => e.PersonelNo == personelNo))
                        {
                            var calismaTuru = 1;
                            var calismaTuruPersonel = calismaTurleriList.FirstOrDefault(e => e.PersonelNo == personelNo);
                            if (calismaTuruPersonel != null)
                                calismaTuru = calismaTuruPersonel.CalismaTuru;

                            userList.Add(new User
                            {
                                AdSoyad = s_unicode2,
                                PersonelNo = personelNo,
                                Departman = s_unicode2Departman,
                                CalismaTuru = calismaTuru,
                                CalismaTuruText = calismaTuru == 1 ? "Tam Zamanlı" : "Out Source"
                            }); ;
                        }
                        if (string.IsNullOrEmpty(dr[3].ToString()))
                            continue;
                        ;

                        Console.WriteLine(dr[3]);
                        var newVisitor = new Visitor();
                        newVisitor.Durum = Convert.ToString(dr[4]);// Convert.ToString(((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, 5]).Value2);
                        newVisitor.IslemZamani = Convert.ToDateTime(dr[3]);
                        newVisitor.KartNo = kartNo;
                        newVisitor.PersonelNo = personelNo;
                        newVisitor.LokasyonNo = Convert.ToString(dr[5]);
                        if (listGirisLokasyonId.Any(e => e.Equals(newVisitor.LokasyonNo)))
                        {
                            newVisitor.IslemTipi = "Giris";
                        }
                        else if (listCikisLokasyonId.Any(e => e.Equals(newVisitor.LokasyonNo)))
                        {
                            newVisitor.IslemTipi = "Cikis";
                        }


                        visitorList.Add(newVisitor);



                    }
                }
            }
        }

        public static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }



    }
}
