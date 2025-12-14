using coreAden.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace coreAden.Core.Interfaces
{
    internal interface ISiparisMalzemeleri
    {
        IEnumerable<ViewSiparisMalzemeleri> GetSiparisMalzemeleriView(int siparisId , string q = null, bool? stokFlag = null);       
        List<SelectListItem> GetMalzemeler();
        void AddSiparisMalzeme(SiparisMalzemeler siparisMalzeme);
        void DeleteSiparisMalzeme(int id);
        Malzemeler getMalzemeByID(int id);
        SiparisMalzemeler GetSiparisMalzemeler(int id);
        void StokDus(int malzemeID ,double miktar);
        void StoktanEkle(int malzemeID, double miktar);
        byte[] ExportMalzemelerToCsv(int siparisId, string q = null, bool? stokFlag = null);        
       


    }
}
