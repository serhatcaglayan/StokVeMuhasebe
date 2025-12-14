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
    public class SiparisLogController : Controller
    {
        private readonly ISiparisLogService _siparisLogService;

        public SiparisLogController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());

            _siparisLogService = new coreAden.Services.SiparisLogService(unitOfWork);           

        }
        public ActionResult SiparisLogListesi(int? page ,int? logTurID=null ,int? userID = null ,DateTime? baslangic = null , DateTime? bitis = null , int? siparisID=null)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var list = _siparisLogService.GetSiparisLogs(pageNumber, pageSize, logTurID, userID, baslangic, bitis);
            return View(list);
        }
    }
}