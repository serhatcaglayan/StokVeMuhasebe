using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace coreAden.Controllers
{
    public class MusteriController : Controller
    {
        private readonly IMusteriService _musteriService;

        public MusteriController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _musteriService = new coreAden.Services.MusteriService(unitOfWork);
        }
        public ActionResult getMembersList(int? page, string q = null)
        {
            int pageSize = 10; //sayfada görünecek kişi sayısı
            int pagenumber = (page ?? 1); //sayfa  numarası null ise 1 kabul edecek

            var members = _musteriService.GetMusteriler(pagenumber, pageSize ,q);
            
            return View(members);
        }

        public ActionResult deleteMember(int id)
        {
            _musteriService.DeleteMusteri(id);
            return RedirectToAction("getMembersList");
        }

        [HttpPost]
        public ActionResult AddMember(Musteriler musteri)
        {
            if (ModelState.IsValid)
            {
                _musteriService.AddMusteri(musteri);
                return RedirectToAction("getMembersList");
            }

            // Eğer validasyon hatası varsa liste sayfasına geri dön
            return RedirectToAction("getMembersList");
        }


    }
}