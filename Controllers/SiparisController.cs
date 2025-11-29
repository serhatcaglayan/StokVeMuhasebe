using coreAden.Core.Interfaces;
using coreAden.Models;
using coreAden.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace coreAden.Controllers
{
  
    public class SiparisController : Controller
    {
        private readonly ISiparisService _siparisService;
        private readonly IMusteriService _musteriService;
        private readonly ILogService _logService;
       

       

        public SiparisController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            
            _logService = new coreAden.Services.LogService(unitOfWork);
            _siparisService = new coreAden.Services.SiparisService(unitOfWork);
            _musteriService = new coreAden.Services.MusteriService(unitOfWork);
            

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
            if(siparis.SiparisTeklifDurumu == false)
            {
                siparis.SiparisTeklifDurumu = true;
                _siparisService.UpdateSiparis(siparis);
                _logService.AddLog(10, $"Teklif Onaylandı... Siparis ID :  {siparis.SiparisID} , Onaylayan Yetkili ID : {siparis.GuncellemeYapanUserID}", 1);
            }
            else
            {
                TempData["ErrorMessage"] = "Sipariş Zaten Onaylanmış";
            }

            
            return RedirectToAction("Detay" , new { id });
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