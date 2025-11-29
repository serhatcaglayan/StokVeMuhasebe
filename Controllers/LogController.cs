using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogService _logService;

        public LogController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _logService = new coreAden.Services.LogService(unitOfWork);
        }

        public ActionResult Index(int? page, int? logTurId, int? userId, DateTime? baslangic, DateTime? bitis)
        {
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var loglar = _logService.GetLoglar(pageNumber, pageSize, logTurId, userId, baslangic, bitis);

            ViewBag.LogIslemTurleri = _logService.GetLogIslemTurleri();
            ViewBag.Users = _logService.GetUsers();
            ViewBag.SelectedLogTurId = logTurId;
            ViewBag.SelectedUserId = userId;
            ViewBag.Baslangic = baslangic;
            ViewBag.Bitis = bitis;

            return View(loglar);
        }

        [HttpPost]
        public ActionResult Filtrele(int? logTurId, int? userId, DateTime? baslangic, DateTime? bitis)
        {
            return RedirectToAction("Index", new { logTurId = logTurId, userId = userId, baslangic = baslangic, bitis = bitis });
        }

        public ActionResult ExportExcel(int? logTurId, int? userId, DateTime? baslangic, DateTime? bitis)
        {
            try
            {
                var csvData = _logService.ExportLoglarToExcel(logTurId, userId, baslangic, bitis);
                var fileName = $"Loglar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "CSV export sırasında hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
