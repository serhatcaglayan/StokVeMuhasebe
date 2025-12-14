using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;

namespace coreAden.Core.Interfaces
{
    public interface IKasaService
    {
        PagedList<ViewKasaBilgisi> GetKasalarView(int page, int pageSize);

        
        void KasaBakiyeDus(int kasaId, double tutar);
        void KasaBakiyeEkle(int kasaID, double tutar, string aciklama , int? userID = null);
        Kasa getKasaByID(int id);
        void Transfer(int AlýcýID, int VericiID, double tutar);
    }
}
