using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class GelirlerService : IGelirServicecs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUsers _userService;

        public GelirlerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userService = new UserService(unitOfWork);
        }

        public byte[] ExportGelirlerToCsv(DateTime? baslangic = null, DateTime? bitis = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            throw new NotImplementedException();
        }

        public IPagedList<View_Gelirler> GetGelir(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null, int? userId = null, bool? kasayaYansitildiMi = null, int? odemeTurId = null)
        {
            var repository = _unitOfWork.Repository<View_Gelirler>();
            var query = repository.GetAll().AsQueryable();

            if (baslangic.HasValue)
                query = query.Where(x => x.Tarih >= baslangic.Value);

            if (bitis.HasValue)
                query = query.Where(x => x.Tarih <= bitis.Value);           

            if (userId.HasValue)
                query = query.Where(x => x.EklemeYapanUserID == userId.Value);

            if (kasayaYansitildiMi.HasValue)
                query = query.Where(x => x.KasayaYansit == kasayaYansitildiMi.Value);

            if (odemeTurId.HasValue)
                query = query.Where(x => x.OdemeTurID == odemeTurId.Value);

            query = query.OrderByDescending(x => x.Tarih);



            return query.ToPagedList(page, pageSize);
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

        public void GelirEkle(double tutar, int OdemeTypeID, int UserID, string acıklama, bool kasayaYansıt, int? kasaId = null)
        {
            throw new NotImplementedException();
        }
    }
}