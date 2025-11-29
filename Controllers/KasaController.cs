using coreAden.Core.Interfaces;
using coreAden.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Controllers
{
    public class KasaController : Controller
    {
        private readonly IKasaService _kasaService;
        private readonly ILogService _logService;
        public KasaController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _kasaService = new coreAden.Services.KasaService(unitOfWork);
            _logService = new Services.LogService(unitOfWork);
        }

        public ActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var kasalar = _kasaService.GetKasalarView(pageNumber, pageSize);
            var total = kasalar.Sum(x => x.Tutar).Value;
            ViewBag.total = total;
            

            return View(kasalar);
        }

        public ActionResult TransferEkranı()
        {
            int pageSize = 20;
            int pageNumber = 1;
            var kasalar = _kasaService.GetKasalarView(pageNumber, pageSize);
            ViewBag.SonTransferler = _logService.GetLogsWithLogType(logType: 4, count: 5);
            return View(kasalar);
        }

        [HttpPost]
        public ActionResult TransferYap(int aliciID, int gondericiID, double tutar)
        {
            var alıcı = _kasaService.getKasaByID(aliciID);
            var gönderici = _kasaService.getKasaByID(gondericiID);

            if(alıcı != null && gönderici != null )
            {
                if(gönderici.Tutar >= tutar && tutar>0)
                {
                    try
                    {
                        _kasaService.Transfer(AlıcıID: aliciID, VericiID: gondericiID, tutar: tutar);
                        string log = $"Transfer Miktari : {tutar} , Gönderen Hesap : {gondericiID} , Alıcı Hesap : {aliciID}";

                        TempData["SuccessMessage"] = "İşlem Başarılı ";
                        // Log 
                        _logService.AddLog(islemTurId: 4, aciklama: log, userId: 1);
                    }
                    catch(Exception ex)
                    {
                        string hata = $"Hatalı Transfer Miktari : {tutar} , Gönderen Hesap : {gondericiID} , Alıcı Hesap : {aliciID} , Hata : {ex}";
                        _logService.AddLog(islemTurId: 14, aciklama: hata, 1);
                        TempData["ErrorMessage"] = "İşlem Başarısız Log Mesaşını kontrol ediniz .";

                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Gönderici Hesabın Bakiyesi Yetersiz";
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Kasa Bulunamadı";
            }

            return RedirectToAction("TransferEkranı" , TempData);
        }

       
    }
}
