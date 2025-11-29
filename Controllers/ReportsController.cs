using coreAden.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace coreAden.Controllers
{
    //[Authorize]
    public class ReportsController : Controller
    {
        private readonly adenEntities _db = new adenEntities();

    
        public ActionResult MusteriSiparisOzeti(DateTime? baslangic = null, DateTime? bitis = null)
        {
            // Basit özet: Müşteriye göre toplam sipariş tutarı
            var vs = _db.ViewSiparis.AsQueryable();
            if (baslangic.HasValue)
            {
                vs = vs.Where(x => x.SiparisTarihi >= baslangic.Value);
            }
            if (bitis.HasValue)
            {
                vs = vs.Where(x => x.SiparisTarihi <= bitis.Value);
            }
            // Sadece Onaylanmış Siparişler ve Aktif Siparişler
            vs = vs.Where(x => x.SiparisAktifMi == true && x.SiparisTeklifDurumu == true);
            var ozet = vs
                .GroupBy(x => new { x.MusteriID, x.MusteriAdSoyad })
                .Select(g => new MusteriSiparisOzetVM
                {
                    MusteriID = g.Key.MusteriID,
                    MusteriAdSoyad = g.Key.MusteriAdSoyad,
                    ToplamTutar = g.Sum(s => (double?)(s.SiparisTutari) ?? 0)
                })
                .OrderByDescending(x => x.ToplamTutar)
                .ToList();
            return View(ozet);
        }

        [HttpGet]
        public ActionResult ExportMusteriSiparisOzetiCsv(DateTime? baslangic = null, DateTime? bitis = null)
        {
            var vs = _db.ViewSiparis.AsQueryable();
            if (baslangic.HasValue) vs = vs.Where(x => x.SiparisTarihi >= baslangic.Value);
            if (bitis.HasValue) vs = vs.Where(x => x.SiparisTarihi <= bitis.Value);
            // Sadece Onaylanmış Siparişler ve Aktif Siparişler
            vs = vs.Where(x => x.SiparisAktifMi == true && x.SiparisTeklifDurumu == true);
            var ozet = vs
                .GroupBy(x => new { x.MusteriID, x.MusteriAdSoyad })
                .Select(g => new {
                    g.Key.MusteriID,
                    g.Key.MusteriAdSoyad,
                    ToplamTutar = g.Sum(s => (double?)(s.SiparisTutari) ?? 0)
                })
                .OrderByDescending(x => x.ToplamTutar)
                .ToList();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("MusteriID;Musteri;ToplamTutar");
            foreach (var x in ozet)
            {
                csv.AppendLine(string.Join(";", new string[] {
                    x.MusteriID.ToString(),
                    x.MusteriAdSoyad,
                    x.ToplamTutar.ToString()
                }));
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "musteri_siparis_ozet.csv");
        }

        public ActionResult MusteriTeklifOzeti(DateTime? baslangic = null, DateTime? bitis = null)
        {
            // Basit özet: Müşteriye göre toplam sipariş tutarı
            var vs = _db.ViewSiparis.AsQueryable();
            if (baslangic.HasValue)
            {
                vs = vs.Where(x => x.SiparisTarihi >= baslangic.Value);
            }
            if (bitis.HasValue)
            {
                vs = vs.Where(x => x.SiparisTarihi <= bitis.Value);
            }
            // Sadece Teklifleri Göster
            vs = vs.Where(x => x.SiparisTeklifDurumu == false);
            var ozet = vs
                .GroupBy(x => new { x.MusteriID, x.MusteriAdSoyad })
                .Select(g => new MusteriSiparisOzetVM
                {
                    MusteriID = g.Key.MusteriID,
                    MusteriAdSoyad = g.Key.MusteriAdSoyad,
                    ToplamTutar = g.Sum(s => (double?)(s.SiparisTutari) ?? 0)
                })
                .OrderByDescending(x => x.ToplamTutar)
                .ToList();
            return View(ozet);
        }
        [HttpGet]
        public ActionResult ExportMusteriTeklifOzetiCsv(DateTime? baslangic = null, DateTime? bitis = null)
        {
            var vs = _db.ViewSiparis.AsQueryable();
            if (baslangic.HasValue) vs = vs.Where(x => x.SiparisTarihi >= baslangic.Value);
            if (bitis.HasValue) vs = vs.Where(x => x.SiparisTarihi <= bitis.Value);
            // Sadece Teklifleri Göster
            vs = vs.Where(x => x.SiparisTeklifDurumu == false);
            var ozet = vs
                .GroupBy(x => new { x.MusteriID, x.MusteriAdSoyad })
                .Select(g => new {
                    g.Key.MusteriID,
                    g.Key.MusteriAdSoyad,
                    ToplamTutar = g.Sum(s => (double?)(s.SiparisTutari) ?? 0)
                })
                .OrderByDescending(x => x.ToplamTutar)
                .ToList();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("MusteriID;Musteri;ToplamTutar");
            foreach (var x in ozet)
            {
                csv.AppendLine(string.Join(";", new string[] {
                    x.MusteriID.ToString(),
                    x.MusteriAdSoyad,
                    x.ToplamTutar.ToString()
                }));
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "musteri_siparis_ozet.csv");
        }




    }

    public class MusteriSiparisOzetVM
    {
        public int MusteriID { get; set; }
        public string MusteriAdSoyad { get; set; }
        public double ToplamTutar { get; set; }
    }
}


