using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Linq;

namespace coreAden.Services
{
    public class MusteriService : IMusteriService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MusteriService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedList<Musteriler> GetMusteriler(int page, int pageSize, string q = null)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            var query = repository.GetAll();
            if(!string.IsNullOrEmpty(q))
            {
                query = query.Where(x => (x.MusteriAd.ToUpper() + " " + x.MusteriSoyad.ToUpper()).Contains(q.ToUpper())) ;
            }
            query = query.OrderByDescending(x => x.KayıtTarihi)
                .ToPagedList(page, pageSize);
           
            //var musteriler = repository.GetAll()
            //    .OrderByDescending(x => x.KayıtTarihi)
            //    .ToPagedList(page, pageSize);
            
            return (PagedList<Musteriler>)query;
        }

        public Musteriler GetMusteriById(int id)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            return repository.GetById(id);
        }

        public void AddMusteri(Musteriler musteri)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            musteri.KayıtTarihi = DateTime.Now;
            repository.Add(musteri);
            _unitOfWork.SaveChanges();
        }

        public void UpdateMusteri(Musteriler musteri)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            repository.Update(musteri);
            _unitOfWork.SaveChanges();
        }

        public void DeleteMusteri(int id)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            var musteri = repository.GetById(id);
            if (musteri != null)
            {
                repository.Remove(musteri);
                _unitOfWork.SaveChanges();
            }
        }

        public bool MusteriExists(int id)
        {
            var repository = _unitOfWork.Repository<Musteriler>();
            return repository.Any(x => x.MusteriID == id);
        }
    }
}
