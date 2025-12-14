using coreAden.Core.Interfaces;
using coreAden.Models;
using coreAden.Services;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Controllers
{
    // [Authorize]
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ILogService _logService;
        private readonly IMalzemeServices _malzemeServices;
        private readonly IGiderService _giderService;
        private readonly IKasaService _kasaService;

        adenEntities db = new adenEntities();

        public StockController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _stockService = new coreAden.Services.StockService(unitOfWork);
            _logService = new coreAden.Services.LogService(unitOfWork);
            _malzemeServices = new coreAden.Services.MalzemeService(unitOfWork);
            _giderService = new coreAden.Services.GiderService(unitOfWork);
            _kasaService = new coreAden.Services.KasaService(unitOfWork);
        }

        public ActionResult getStockList(int? page, DateTime? baslangic = null, DateTime? bitis = null)
        {
            int pageSize = 10; //sayfada görünecek kişi sayısı
            int pagenumber = (page ?? 1); //sayfa  numarası null ise 1 kabul edecek

            var lst = _stockService.GetMalzemeler(pagenumber, pageSize, baslangic, bitis);
            ViewBag.OdemeTurler = _giderService.GetOdemeTurleri();
            ViewBag.Kasalar = _kasaService.GetKasalarView(1, 100); // Tüm kasalar

            return View(lst);
        }

        [HttpGet]
        public ActionResult ExportStockCsv(DateTime? baslangic = null, DateTime? bitis = null)
        {
            var data = _stockService.ExportMalzemelerToCsv(baslangic, bitis);
            var fname = $"Stoklar_{DateTime.Now.ToShortDateString()}.xlsx";
            return File(data,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       fname);
        }

        [HttpPost]
        public ActionResult Guncelle(Malzemeler m)
        {
            _stockService.UpdateMalzeme(m);
            return RedirectToAction("getStockList");
        }

        [HttpPost]
        public ActionResult AddMaterial(string ad, float fiyat)
        {
            var yeniMalzeme = new Malzemeler
            {
                MalzemeAdı = ad,
                AlısFiyati = fiyat,
                StokMiktari = 0,
                AlısTarihi = DateTime.Now,
                EklemeYapanUserID = null
            };

            _stockService.AddMalzeme(yeniMalzeme);
            return RedirectToAction("getStockList");
        }

        [HttpPost]
        public ActionResult StokEkle(int MalzemeID, double miktar, bool kasayaYansitilsinMi = false, int OdemeTypeID = 1, int KasaID = -1)
        {
            var malzeme = _stockService.GetMalzemeById(MalzemeID);
            bool flag;
            if (miktar > 0 && malzeme != null)
            {
                // Stok ekleme
                if(kasayaYansitilsinMi==false)
                {
                    _stockService.StokEkle(MalzemeID, miktar, kasayaYansitilsinMi);
                    // Stok sayısı değiştiğinde trigger tetiklenir ve log kaydı oluşur. [dbo].[tr_Malzemeler_Stok_Update_Log]
                }

                else
                {
                    var tutar = miktar * (double)malzeme.AlısFiyati;
                    var acıklama = $"Gider Eklendi . {miktar} adet {malzeme.MalzemeAdı} .  Toplam Tutar : {tutar} . Tarih {DateTime.Now.ToString()} ";

                   flag = 
                        _giderService.GiderEkle(
                        tutar: (double)tutar,
                        giderTypeID: 4,
                        OdemeTypeID: OdemeTypeID,
                        UserID: 1,
                        acıklama: acıklama,
                        kasayaYansıt: kasayaYansitilsinMi,
                        KasaID
                    );

                    if( flag ) // gider ekleme işlemi başarılıysa stok ekleme işlemi yapılır.
                        _stockService.StokEkle(MalzemeID, miktar, kasayaYansitilsinMi);
                    // Stok sayısı değiştiğinde trigger tetiklenir ve log kaydı oluşur. [dbo].[tr_Malzemeler_Stok_Update_Log]                   
                }

            }
            return RedirectToAction("getStockList");
        }


        public ActionResult Delete(int id)
        {
            var item = _stockService.GetMalzemeById(id);
            if (item != null && item.StokMiktari == 0)
            {
                _stockService.DeleteMalzeme(id);
            }

            return RedirectToAction("getStockList");
        }


    }
}