using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace coreAden.Core.Interfaces
{
    public interface ISiparisService
    {
        PagedList<ViewSiparis> GetSiparisler(int page, int pageSize,string q =null , bool? SprsTklf =null, DateTime? baslangic = null, DateTime? bitis = null);
        PagedList<Siparisler> GetMusteriSiparisleri(int musteriId, int page, int pageSize , bool? SiparisAktifMi = null , bool? siparisTeklifDurumu = null);
        ViewSiparis GetSiparisDetay(int id);
        Siparisler GetSiparisById(int id);
        void CreateSiparis(Siparisler siparis);
        void UpdateSiparis(Siparisler siparis);
        void DeleteSiparis(int id);
        List<SelectListItem> GetKategoriler();
        List<SelectListItem> GetUsers();
        List<SelectListItem> GetOdemeTurleri();
        byte[] ExportSiparislerToExcel(DateTime? baslangic = null, DateTime? bitis = null, string q = null, bool? SprsTklf = null);
        double toplamSiparisMalzemeTutari(int siparisId);
        List<ViewSiparisMalzemeleri> SiparisMalzemeListesi(int siparisId);
    }
}
