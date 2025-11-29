using coreAden.Core.Interfaces;
using coreAden.Models;
using coreAden.Services;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace coreAden.Controllers
{
    public class SiparisMalzemeleriController : Controller
    {

        private readonly ISiparisMalzemeleri _siparisMalzemeleri;     
        private static int SiparisID ;

        public SiparisMalzemeleriController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _siparisMalzemeleri = new SiparisMalzemeleriService(unitOfWork);    
           
           
        }

        
        public ActionResult Index(int id , string q = null, bool? stokFlag = null)
        {

            var list = _siparisMalzemeleri.GetSiparisMalzemeleriView(id ,q,stokFlag);

            SiparisID = id;

            ViewBag.SiparisID = id;
            ViewBag.Malzemeler = _siparisMalzemeleri.GetMalzemeler();
            ViewBag.ToplamTutar = list.Sum(x => (x.AlısFiyati ?? 0) * (x.Birim ?? 0));

            

            return View(list);
        }

        [HttpPost]
        public ActionResult Ekle(int id, SiparisMalzemeler siparisMalzeme)
        {
            var malzemeAdı = "";
            if (siparisMalzeme == null)
            {
                TempData["ErrorMessage"] = "Geçersiz istek.";
                return RedirectToAction("Index", new { id = id });
            }

            if (siparisMalzeme.Birim == null || siparisMalzeme.Birim <= 0)
            {
                TempData["ErrorMessage"] = "Kullanılacak birim 0'dan büyük olmalıdır.";
                return RedirectToAction("Index", new { id = id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                      try
                        {
                            // SiparisID'yi Index metodundaki id parametresinden al
                            siparisMalzeme.SiparisID = id;

                            // Stok kontrolü (stoktan düşülecekse)
                            if (siparisMalzeme.StoktanDus)
                            {
                               var malzeme = _siparisMalzemeleri.getMalzemeByID(siparisMalzeme.MalzemeID);                               
                                malzemeAdı = malzeme.MalzemeAdı;
                                var mevcutStok = malzeme?.StokMiktari ?? 0;
                                if (malzeme == null)
                                {
                                    TempData["ErrorMessage"] = "Malzeme bulunamadı.";
                                  
                                    return RedirectToAction("Index", new { id = id });
                                }
                                if (mevcutStok < (siparisMalzeme.Birim ?? 0))
                                {
                                    TempData["ErrorMessage"] = $"Yetersiz stok. Mevcut: {mevcutStok}, İstenen: {siparisMalzeme.Birim}";
                                  
                                    return RedirectToAction("Index", new { id = id });
                                }
                            }

                            _siparisMalzemeleri.AddSiparisMalzeme(siparisMalzeme);                       

                            // Eğer stoktan düşülecekse
                            if (siparisMalzeme.StoktanDus)
                            {
                              //  StoktanDus(siparisMalzeme.MalzemeID, siparisMalzeme.Birim ?? 0);
                            }                           
                            TempData["SuccessMessage"] = "Malzeme eklendi.";

                            // Log
                          
                            return RedirectToAction("Index", new { id = id });
                        }
                        catch (Exception)
                        {
                           
                            TempData["ErrorMessage"] = "İşlem sırasında hata oluştu.";
                            return RedirectToAction("Index", new { id = id });
                        }
                    }
                
                catch (System.Exception ex)
                {
                    TempData["ErrorMessage"] = "Hata oluştu: " + ex.Message;
                    return RedirectToAction("Index", new { id = id });
                }
            }

            // Model validasyon hatası durumunda
            TempData["ErrorMessage"] = "Geçersiz veri girişi!";
            return RedirectToAction("Index", new { id = id });
        }

        // Stoktan düşme metodu
        private void StoktanDus(int malzemeID, double miktar)
        {
            try
            {
                var malzeme = _siparisMalzemeleri.getMalzemeByID(malzemeID);
                
                if (malzeme != null)
                {
                    // Stok miktarını kontrol et
                    if ((malzeme.StokMiktari ?? 0) >= miktar)
                    {
                        _siparisMalzemeleri.StokDus(malzemeID, miktar);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Yetersiz stok! Mevcut: {malzeme.StokMiktari}, İstenen: {miktar}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Malzeme bulunamadı! ID: {malzemeID}");
                }
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                throw new Exception($"Stoktan düşme hatası: {ex.Message}");
            }
        }

         public ActionResult Sil(int? siparisMalzemeID, bool? stoktanEkle)
        {
            if (!siparisMalzemeID.HasValue)
            {
                TempData["ErrorMessage"] = "Geçersiz malzeme ID'si.";
                return RedirectToAction("TümSiparisListesi", "Siparis");
            }
            var malzemeAdı = "";
            var id = 0;
            try
            {
              
                    try
                    {
                    var silinecekMalzeme = _siparisMalzemeleri.GetSiparisMalzemeler(siparisMalzemeID.Value);
                    malzemeAdı = _siparisMalzemeleri.getMalzemeByID(silinecekMalzeme.MalzemeID).MalzemeAdı;
                     
                        if (silinecekMalzeme != null)
                        {
                            int siparisID = silinecekMalzeme.SiparisID; // Yönlendirme için SiparisID'yi al
                            id = siparisID;
                            // Eğer malzeme stoktan düşülmüşse ve kullanıcı stoğa eklemek istiyorsa
                            if (silinecekMalzeme.StoktanDus && stoktanEkle == true)
                            {
                                StoktanEkle(silinecekMalzeme.MalzemeID, silinecekMalzeme.Birim ?? 0);
                                TempData["SuccessMessage"] = "Malzeme silindi ve stok geri eklendi.";
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Malzeme silindi.";
                            }

                        _siparisMalzemeleri.DeleteSiparisMalzeme(silinecekMalzeme.SiparisMalzemeID);
                            
                                                   

                            // Log                         


                            return RedirectToAction("Index", new { id = siparisID });
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Silinecek malzeme bulunamadı.";
                        return RedirectToAction("Index", new { id = id });
                    }
                    }
                    catch (Exception e )
                    {
                       
                        TempData["ErrorMessage"] = "Silme sırasında hata oluştu."  + e.Message + e.Source;
                    return RedirectToAction("Index", new { id = id });
                }
                
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Silme işlemi sırasında hata oluştu: " + ex.Message;
             return RedirectToAction("Index", new { id = id });
            }
        }


        // Stoktan ekleme metodu (silme işleminde kullanılır)
        private void StoktanEkle(int malzemeID, double miktar)
        {
            try
            {
                var malzeme = _siparisMalzemeleri.getMalzemeByID(malzemeID);
               
                if (malzeme != null)
                {
                    _siparisMalzemeleri.StoktanEkle(malzemeID, miktar);
                    
                }
                else
                {
                    throw new InvalidOperationException($"Malzeme bulunamadı! ID: {malzemeID}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Stoktan ekleme hatası: {ex.Message}");
            }
        }

        //  Export Excel

        [HttpGet]
        public ActionResult ExportTumSiparisCsv(string q = null, bool? stokFlag = null)
        {
            var csvData = _siparisMalzemeleri.ExportMalzemelerToCsv(SiparisID,q,stokFlag);
            return File(csvData, "text/csv", "siparisMalzemeListesi.csv");
        }

    }
}