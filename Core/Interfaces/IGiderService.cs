using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using coreAden.Models;

namespace coreAden.Core.Interfaces
{
    public interface IGiderService
    {
        bool GiderEkle(double tutar, int giderTypeID, int OdemeTypeID, int UserID, string acıklama, bool kasayaYansıt, int? kasaId = null);
        List<SelectListItem> GetOdemeTurleri();
        
        IPagedList<ViewGiderler> GetGiderler(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null, 
            int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null);

        List<SelectListItem> GetGiderTurleri();

        byte[] ExportGiderlerToCsv(DateTime? baslangic = null, DateTime? bitis = null,
            int? giderTurId = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null);
    }
}
