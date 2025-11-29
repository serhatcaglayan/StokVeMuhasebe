using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;

namespace coreAden.Core.Interfaces
{
    public interface IStockService
    {
        PagedList<Malzemeler> GetMalzemeler(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null);
        Malzemeler GetMalzemeById(int id);
        void AddMalzeme(Malzemeler malzeme);
        void UpdateMalzeme(Malzemeler malzeme);
        void DeleteMalzeme(int id);
        void StokEkle(int malzemeId, double miktar, bool kasayaYansitilsinMi = false);
        byte[] ExportMalzemelerToCsv(DateTime? baslangic = null, DateTime? bitis = null);
    }
}
