using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace coreAden.Core.Interfaces
{
    internal interface IGelirServicecs
    {
        void GelirEkle(double tutar,  int OdemeTypeID, int UserID, string acıklama, bool kasayaYansıt, int? kasaId = null);
        List<SelectListItem> GetOdemeTurleri();

        IPagedList<View_Gelirler> GetGelir(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null,
            int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null);

       

        byte[] ExportGelirlerToCsv(DateTime? baslangic = null, DateTime? bitis = null,
            int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null);
    }
}
