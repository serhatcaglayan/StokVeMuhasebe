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
using System.Web.UI;

namespace coreAden.Controllers
{
  //  [Authorize]
    public class GiderController : Controller
    {
        

        private readonly IGiderService _giderService;
        private readonly IUsers _userService;
        private readonly ILogService _logService;
        private readonly IKasaService _kasaService;
        
        public GiderController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _giderService = new GiderService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _logService = new LogService(unitOfWork);
            _kasaService = new KasaService(unitOfWork);
        }


        // GET: Gider
        public ActionResult giderListesi(int? page, DateTime? baslangic = null, DateTime? bitis = null,
            int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            int pageSize = 10; //sayfada görünecek kişi sayısı
            int pagenumber = (page ?? 1); //sayfa  numarası null ise 1 kabul edecek


            var giderler = _giderService.GetGiderler(pagenumber, pageSize, baslangic, bitis,
                giderTurId, userId, kasayaYansitildiMi, odemeTurId);

            ViewBag.GiderTurler = _giderService.GetGiderTurleri();
            ViewBag.OdemeTurler = _giderService.GetOdemeTurleri();            
            ViewBag.Users = _userService.getUsers(); 
            ViewBag.Kasalar = _kasaService.GetKasalarView(1, 100); // Get first 100 kasa records for dropdown

            // Store selected values for the view
            ViewBag.SelectedGiderTurId = giderTurId;
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedKasaYansitildiMi = kasayaYansitildiMi;
            ViewBag.SelectedOdemeTurId = odemeTurId;
            ViewBag.toplamGider = giderler.Sum(x=>x.Tutar).Value;


            
            return View(giderler);
        }

        [HttpPost]
        public ActionResult Ekle(Giderler gider, int? kasaId = -1)
        {

            if (!ModelState.IsValid)
            {
                TempData["ErrorGider"] = "Form verileri geçersiz.";
                return RedirectToAction("giderListesi");
            }

            try
            {
                _giderService.GiderEkle(
                    tutar: gider.Tutar ?? -1,
                    giderTypeID: gider.GiderTurID ?? 0,
                    OdemeTypeID: gider.OdemeTurID ?? 0,
                    UserID: 1,
                    acıklama: gider.EkAcıklama ?? "",
                    kasayaYansıt: gider.KasayaYansıt,
                    kasaId: kasaId
                );

                TempData["SuccessGider"] = "Gider başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorGider"] = "Gider eklenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("giderListesi");
        }


        [HttpGet]
        public ActionResult ExportGiderCsv(DateTime? baslangic = null, DateTime? bitis = null, 
            int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
           
            var excelData = _giderService.ExportGiderlerToCsv(baslangic, bitis, giderTurId, userId, kasayaYansitildiMi, odemeTurId);
            var fname = $"tum_siparisler_{DateTime.Now.ToShortDateString()}.xlsx";
            return File(excelData,
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       fname);

        }



    }
}