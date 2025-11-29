using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coreAden.Services
{
    public class GiderExportService
    {
        private readonly adenEntities _db;
        public GiderExportService(adenEntities db)
        {
            _db = db;
        }

        public byte[] ExportGiderlerToCsv(DateTime? baslangic, DateTime? bitis, int? giderTurId, int? userId, bool? kasayaYansitildiMi, int? odemeTurId)
        {
            var query = _db.ViewGiderler.AsQueryable();
            if (baslangic.HasValue)
                query = query.Where(x => x.Tarih >= baslangic.Value);
            if (bitis.HasValue)
                query = query.Where(x => x.Tarih <= bitis.Value);
            if (giderTurId.HasValue && giderTurId.Value > 0)
                query = query.Where(x => x.GiderAd != null && x.GiderAd != "" && x.GiderID == giderTurId.Value);
            if (userId.HasValue && userId.Value > 0)
                query = query.Where(x => x.UserAd != null && x.UserAd != "" && x.UserAd == userId.ToString());
            if (kasayaYansitildiMi.HasValue)
                query = query.Where(x => x.KasayaYansıt == kasayaYansitildiMi.Value);
            if (odemeTurId.HasValue && odemeTurId.Value > 0)
                query = query.Where(x => x.OdemeAdı != null && x.OdemeAdı != "" && x.OdemeAdı == _db.OdemeTur.Where(o => o.OdemeTur1 == odemeTurId.Value).Select(o => o.OdemeAdı).FirstOrDefault());

            var data = query.OrderByDescending(x => x.Tarih).ToList();
            var csv = new StringBuilder();
            csv.AppendLine("Tarih;Gider Türü;Tutar;İşlem Sahibi;Kasaya Yansıtıldı Mı;Ödeme Tipi;Ek Açıklama");
            foreach (var x in data)
            {
                csv.AppendLine(string.Join(";", new string[] {
                    x.Tarih.ToString("dd.MM.yyyy"),
                    x.GiderAd,
                    (x.Tutar ?? 0).ToString(),
                    x.UserAd,
                    x.KasayaYansıt ? "Evet" : "Hayır",
                    x.OdemeAdı,
                    x.EkAcıklama
                }));
            }
            var utf8WithBom = new UTF8Encoding(true);
            return utf8WithBom.GetBytes(csv.ToString());
        }
    }
}
