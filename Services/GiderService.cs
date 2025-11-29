using ClosedXML.Excel;
using coreAden.Core.Interfaces;
using coreAden.Data.Repositories;
using coreAden.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Text;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class GiderService : IGiderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUsers _userService;
        private readonly IKasaService _kasaService;
        private readonly ILogService _logService;

        public GiderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userService = new UserService(unitOfWork);
            _kasaService = new KasaService(unitOfWork);
            _logService = new LogService(unitOfWork);
        }



        public bool GiderEkle(double tutar, int giderTypeID, int odemeTypeID, int userID, string aciklama, bool kasayaYansit, int? kasaId = null)
        {
            var msg = "";
            var repository = _unitOfWork.Repository<Giderler>();

            Giderler gider = new Giderler
            {
                EkAcıklama = aciklama,
                EklemeYapanUserID = userID,
                GiderTurID = giderTypeID,
                KasayaYansıt = kasayaYansit,
                OdemeTurID = odemeTypeID,
                Tarih = DateTime.Now,
                Tutar = tutar
            };

            try
            {
                // 🔹 Transaction başlat
                _unitOfWork.BeginTransaction();

                // 🔹 Gider ekle
                repository.Add(gider);
                _unitOfWork.SaveChanges();

                _logService.AddLog(islemTurId: 5, $"Gider eklendi: {gider.ToString()}", userId: userID);

                // 🔹 Kasaya yansıt işlemi (isteğe bağlı)
                if (kasayaYansit)
                {
                    if (kasaId == null || kasaId <= 0)
                        throw new Exception("Kasa ID geçersiz.");

                    var kasa = _kasaService.getKasaByID(kasaId.Value);
                    if (kasa == null)
                        throw new Exception("Kasa bulunamadı.");

                    if (kasa.Tutar < tutar)
                        throw new Exception("Kasa bakiyesi yetersiz.");

                    _kasaService.KasaBakiyeDus(kasa.KasaID, tutar);
                    msg = $"Kasadan (ID: {kasa.KasaID}) {tutar} TL düşüldü. Gider: {gider.ToString()}";
                    _logService.AddLog(islemTurId: 15, msg, userID);
                }

                // 🔹 Tüm işlemler başarılı → Commit
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // 🔹 Hata varsa Rollback
                _unitOfWork.Rollback();

                // 🔹 Log kaydı
                _logService.AddLog(
                    islemTurId: 17,
                    aciklama: $"Gider ekleme hatası: {ex.Message} | Inner: {ex.InnerException?.Message} | Gider: {gider} ",
                    userId: userID
                );
                return false;
              
            }
        }




        public List<SelectListItem> GetOdemeTurleri()
        {
            var repository = _unitOfWork.Repository<OdemeTur>();
            return repository.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.OdemeAdı,
                    Value = x.OdemeTur1.ToString()
                }).ToList();
        }

        public List<SelectListItem> GetGiderTurleri()
        {
            var repository = _unitOfWork.Repository<GiderTur>();

            return repository.GetAll().ToList()
                .Select(x => new SelectListItem
                {
                    Text = x.GiderAd,
                    Value = x.GiderTurID.ToString()
                }).ToList();

        }


        public IPagedList<ViewGiderler> GetGiderler(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null,
            int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            var repository = _unitOfWork.Repository<ViewGiderler>();
            var query = repository.GetAll().AsQueryable();

            if (baslangic.HasValue)
                query = query.Where(x => x.Tarih >= baslangic.Value);

            if (bitis.HasValue)
                query = query.Where(x => x.Tarih <= bitis.Value);

            if (giderTurId.HasValue)
                query = query.Where(x => x.GiderTurID == giderTurId.Value);

            if (userId.HasValue)
                query = query.Where(x => x.EklemeYapanUserID == userId.Value);

            if (kasayaYansitildiMi.HasValue)
                query = query.Where(x => x.KasayaYansıt == kasayaYansitildiMi.Value);

            if (odemeTurId.HasValue)
                query = query.Where(x => x.OdemeTurID == odemeTurId.Value);

            return query.OrderByDescending(x => x.Tarih).ToPagedList(page, pageSize);
        }

        public byte[] ExportGiderlerToCsv(DateTime? baslangic = null, DateTime? bitis = null, int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            var repo = _unitOfWork.Repository<ViewGiderler>();
            var query = repo.GetAll().AsQueryable();
            
            if (baslangic.HasValue)
                query = query.Where(x => x.Tarih >= baslangic.Value);

            if (bitis.HasValue)
                query = query.Where(x => x.Tarih <= bitis.Value);

            if (giderTurId.HasValue)
                query = query.Where(x => x.GiderTurID == giderTurId);
         
            if (kasayaYansitildiMi.HasValue)
                query = query.Where(x => x.KasayaYansıt == kasayaYansitildiMi);

            if (odemeTurId.HasValue)
                query = query.Where(x => x.OdemeTurID == odemeTurId);

            if (userId.HasValue)
            {
                var usr = _userService.getUserByID(userId.Value);

                if (usr != null && !string.IsNullOrEmpty(usr.UserAd))
                {
                    var userAd = usr.UserAd.ToLower();
                    query = query.Where(x => x.UserAd.ToLower().Contains(userAd));
                }
            }


            var data = query.OrderByDescending(x=>x.Tarih).ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Giderler");
                
                // Başlık satırı
                string[] headers = { "Gider_ID" , "Gider_Adı" , "Tutar" , "Açıklama" , "Tarih" , "User_Ad" , "Kasaya_Yansıtıldı_Mı" , "Ödeme_Adı"  };

                for(int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                    ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Data satırları
                int row = 2;
                foreach (var x in data)
                {
                    

                    ws.Cell(row, 1).Value = x.GiderID;
                    ws.Cell(row, 2).Value = x.GiderAd;
                    ws.Cell(row, 3).Value = x.Tutar;
                    ws.Cell(row, 4).Value = x.EkAcıklama;
                    ws.Cell(row, 5).Value = x.Tarih;
                    ws.Cell(row, 6).Value = x.UserAd;
                    ws.Cell(row, 7).Value = x.KasayaYansıt;
                    ws.Cell(row, 8).Value = x.OdemeAdı;                 
                                    

                    row++;
                }
                ws.Cell(row +1 , 8).Style.Font.Bold = true;
                ws.Cell(row + 1, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;
                ws.Cell(row + 1, 9).Style.Font.Bold = true;
                ws.Cell(row + 1, 9).Style.Fill.BackgroundColor = XLColor.LightGreen;
                ws.Cell(row + 1, 10).Style.Font.Bold = true;
                ws.Cell(row + 1, 10).Style.Fill.BackgroundColor = XLColor.LightGreen;



                ws.Cell(row + 1, 8).Value = "Toplam Tutar";
                ws.Cell(row + 1, 9).Value = "=";
                ws.Cell(row + 1, 10).Value = data.Sum(x => x.Tutar);

                ws.Columns().AdjustToContents(); // Otomatik kolon genişliği

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return ms.ToArray();
                }
            }


        }
    }
}