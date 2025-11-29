using coreAden.Core.Interfaces;
using coreAden.Models;
using coreAden.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Controllers
{
    public class GelirController : Controller
    {
        private readonly IGelirServicecs _gelirService;
        private readonly IUsers _userService;
        private readonly ILogService _logService;
        private readonly IKasaService _kasaService;

        public GelirController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _gelirService = new GelirlerService(unitOfWork);
            _userService = new UserService(unitOfWork);
            _logService = new LogService(unitOfWork);
            _kasaService = new KasaService(unitOfWork);
        }
        public ActionResult gelirListesi(int? page, DateTime? baslangic = null, DateTime? bitis = null,
              int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            int pageSize = 10; //sayfada görünecek kişi sayısı
            int pagenumber = (page ?? 1); //sayfa  numarası null ise 1 kabul edecek


            var gelirler = _gelirService.GetGelir(pagenumber, pageSize, baslangic, bitis,
                 userId, kasayaYansitildiMi, odemeTurId);

            
            ViewBag.OdemeTurler = _gelirService.GetOdemeTurleri();
            ViewBag.Users = _userService.getUsers();
            ViewBag.Kasalar = _kasaService.GetKasalarView(1, 100); // Get first 100 kasa records for dropdown

            // Store selected values for the view
            
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedKasaYansitildiMi = kasayaYansitildiMi;
            ViewBag.SelectedOdemeTurId = odemeTurId;
            ViewBag.toplamGelir = gelirler.Sum(x => x.Tutar).Value;



            return View(gelirler);
        }



    }
}