using coreAden.Core.Interfaces;
using coreAden.Models;
using coreAden.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Windows.Media.Animation;

namespace coreAden.Controllers
{
  
    public class SiparisController : Controller
    {
        private readonly ISiparisService _siparisService;
        private readonly IMusteriService _musteriService;
        private readonly ILogService _logService;
        private readonly IKasaService _kasaService;
        private readonly ISiparisLogService _siparisLogService;
        private readonly IGiderService _giderService;
        private readonly IGelirServicecs _gelirService;


        public SiparisController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            
            _logService = new coreAden.Services.LogService(unitOfWork);
            _siparisService = new coreAden.Services.SiparisService(unitOfWork);
            _musteriService = new coreAden.Services.MusteriService(unitOfWork);
            _kasaService = new coreAden.Services.KasaService(unitOfWork);
            _siparisLogService = new coreAden.Services.SiparisLogService(unitOfWork);
            _giderService = new coreAden.Services.GiderService(unitOfWork);
            _gelirService = new coreAden.Services.GelirlerService(unitOfWork);

        }

        public ActionResult TümSiparisListesi(int? page, string q = null , bool? SprsTklf = null, DateTime? baslangic = null, DateTime? bitis = null)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var list = _siparisService.GetSiparisler(pageNumber, pageSize, q, SprsTklf, baslangic, bitis);

            ViewBag.Kategoriler = _siparisService.GetKategoriler();
            ViewBag.users = _siparisService.GetUsers();
            ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();

            return View(list);
        }



        [HttpGet]
        public ActionResult ExportTumSiparisToExcel(DateTime? baslangic = null, DateTime? bitis = null, string q = null, bool? SprsTklf = null)
        {
            //var csvData = _siparisService.ExportSiparislerToCsv(baslangic, bitis,q,SprsTklf);
            //return File(csvData, "text/csv", "tum_siparisler.csv");
            var excelData = _siparisService.ExportSiparislerToExcel(baslangic, bitis, q, SprsTklf);
            return File(excelData,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "tum_siparisler.xlsx");
        }






        public ActionResult Detay(int id)
        {
            var siparis = _siparisService.GetSiparisDetay(id);
            if (siparis == null)
                return HttpNotFound();

            return View(siparis);
        }

        public ActionResult MusteriSiparisListesi(int id , int? page , bool? siparisTeklifDurumu = null , bool? SiparisAktifMi = null)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            
            var list = _siparisService.GetMusteriSiparisleri(id, pageNumber, pageSize,siparisTeklifDurumu,SiparisAktifMi);            
            return View(list);
        }

        public ActionResult Guncelle(int id)
        {
            
            var siparis = _siparisService.GetSiparisById(id);
            if (siparis == null)
                return HttpNotFound();

            ViewBag.Kategoriler = _siparisService.GetKategoriler();
            ViewBag.users = _siparisService.GetUsers();
            ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();

            return View(siparis);
        }


        [HttpPost]
        public ActionResult Guncelle(Siparisler model)
        {
            if (ModelState.IsValid && model.SiparisAktifMi ==true )
            {
                _siparisService.UpdateSiparis(model);
                return RedirectToAction("Detay", new { id = model.SiparisID });
            }

            ViewBag.Kategoriler = _siparisService.GetKategoriler();
            ViewBag.users = _siparisService.GetUsers();
            ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();

            // Log işlemi Veritabanında Trigger olacak. [trg_Siparis_Guncellendiginde_Log_Ekle]

            return View(model);
        }

        // GET: Siparis/Create
        public ActionResult Create()
        {
            try
            {
                ViewBag.Kategoriler = _siparisService.GetKategoriler();
                ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();
                return View();
            }
            catch (Exception ex)
            {
                _logService.AddLog(9, $"Sipariş oluşturma sayfası yüklenirken hata: {ex.Message}");
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction("TümSiparisListesi");
            }
        }

        [HttpPost]
      
        public ActionResult Create(Siparisler model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Kategoriler = _siparisService.GetKategoriler();
                    ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();
                    return View(model);
                }

                if (model.MusteriID <= 0)
                {
                    ModelState.AddModelError("MusteriID", "Lütfen müşteri seçiniz");
                    ViewBag.Kategoriler = _siparisService.GetKategoriler();
                    ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();
                    return View(model);
                }

                model.SiparisTutari = model.Birim * model.BirimFiyat;
                model.SiparisTarihi = DateTime.Now;
                model.SiparisTeklifDurumu = false;
                model.SiparisAktifMi = true;
                model.GuncellemeYapanUserID = 1; // Kullanıcı ID'sini alın
                model.GuncellemeTarihi = DateTime.Now;

                _siparisService.CreateSiparis(model);

                TempData["SuccessMessage"] = "Sipariş başarıyla oluşturuldu.";

                //Log işlemi veritabanında : [trg_Teklıf_Olustugunda_Log_Ekle]

                return RedirectToAction("TümSiparisListesi");
            }
            catch (Exception ex)
            {
                _logService.AddLog(9, $"Sipariş oluşturulurken hata: Sipariş Detayları : {model.ToString()} \n error :  {ex.Message}");
                ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu.");
                ViewBag.Kategoriler = _siparisService.GetKategoriler();
                ViewBag.OdemeTurler = _siparisService.GetOdemeTurleri();
                return View(model);
            }
        }


        [HttpGet]
        public JsonResult SearchCustomers(string term)
        {
            var customers = _musteriService.GetMusteriler(1, 10)
                .Where(m => m.MusteriAd.ToLower().Contains(term.ToLower()) | m.MusteriSoyad.ToLower().Contains(term.ToLower()))
                .Select(m => new { id = m.MusteriID, text = m.MusteriAd + " " + m.MusteriSoyad + " [ " + m.Telefon + " ]" })
                .ToList();
            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        

        public ActionResult SiparisOnayla (int id)
        {
            var siparis = _siparisService.GetSiparisById(id);
           
            if (siparis == null)
            {
                TempData["ErrorMessage"] = "Sipariş bulunamadı";
                return RedirectToAction("Index");
            }

            // Teklifi Siparişe Dönüştür.
            if (siparis.SiparisTeklifDurumu == false)
            {
                siparis.SiparisTeklifDurumu = true;
                _siparisService.UpdateSiparis(siparis);
                _logService.AddLog(10, $"Teklif Onaylandı... Siparis ID :  {siparis.SiparisID} , Onaylayan Yetkili ID : {siparis.GuncellemeYapanUserID}", 1);
            }
            else
            {
                TempData["ErrorMessage"] = "Sipariş Zaten Onaylanmış";
                return RedirectToAction("Detay", new { id });
            }

            // Sipariş Gelirini kasaya kaydet . Nakit olarak gönder. //Log EKle
            double gelir = (double)(siparis.Birim * siparis.BirimFiyat);
            double malzemeTutari = _siparisService.toplamSiparisMalzemeTutari(siparisId:id);
            var malzemeListesi = _siparisService.SiparisMalzemeListesi(siparisId: id);
            var musteri_bilgisi = _musteriService.GetMusteriById(siparis.MusteriID);
            var text = "Sipariş Geliri Kaydedildi. Müşteri : " +  musteri_bilgisi.MusteriAd + " "+musteri_bilgisi.MusteriSoyad + "  ID : " + musteri_bilgisi.MusteriID ;
            var logtext = $"Kasaya Sipariş Geliri Eklendi. SİparisID : {id}";
            _kasaService.KasaBakiyeEkle(kasaID:1 ,tutar:gelir , aciklama:logtext , userID:1);
            _siparisLogService.AddSiparisLog(islemTurId:3 ,userId:1,kasaID:1,siparisID:id,malzemeTutari:malzemeTutari,siparisTutari:gelir ,aciklama:text);

            // Gelir tablosuna ekleme
            
            

            return RedirectToAction("Detay", new { id });
        }

        public ActionResult SiparisKapat (int id)
        {
            var siparis = _siparisService.GetSiparisById(id);
            
            if(siparis.SiparisAktifMi == true && siparis.SiparisTeklifDurumu == true)
            {
                siparis.SiparisAktifMi = false;
                _siparisService.UpdateSiparis(siparis);
                _logService.AddLog(11, $"Siparis Kapatıldı... Siparis ID :  {siparis.SiparisID} , Onaylayan Yetkili ID : {siparis.GuncellemeYapanUserID}", 1);
            }
            else
            {
                TempData["ErrorMessage"] = "Sipariş Zaten Kapalı veya Sipariş Onaylanmamış";
            }
            return RedirectToAction("Detay", new { id });

        }

        public ActionResult TeklifSil(int id)
        {
            var siparis = _siparisService.GetSiparisById(id);

            if(siparis.SiparisTeklifDurumu==true)
            {
                TempData["ErrorMessage"] = "Onaylanmış Sipariş Silinemez";
                return RedirectToAction("Detay", new { id });

            }
            else if (siparis.SiparisTeklifDurumu == false)
            {
                try
                {
                    _siparisService.DeleteSiparis(id);
                    TempData["ErrorMessage"] = " Teklif Silindi";
                    _logService.AddLog(12, $"Siparis Silindi... Siparis ID :  {siparis.SiparisID} , Onaylayan Yetkili ID : {siparis.GuncellemeYapanUserID} , detay : {siparis.ToString()}", 1);
                    return RedirectToAction("TümSiparisListesi");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = " Teklif Silmede Hata Oluştu";
                    _logService.AddLog(13, $"Siparis Silmede Hata oluştu... Siparis ID :  {siparis.SiparisID} , Onaylayan Yetkili ID : {siparis.GuncellemeYapanUserID} \n error : {ex.Message}", 1);
                    return RedirectToAction("Detay", new { id });
                }

            }
            else
            {
                TempData["ErrorMessage"] = "İşlem Başarısız";
                return RedirectToAction("Detay", new { id });
            }
        }



    }
}