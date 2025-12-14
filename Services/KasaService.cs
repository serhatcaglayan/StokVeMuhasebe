using coreAden.Core.Interfaces;
using coreAden.Models;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coreAden.Services
{
    public class KasaService : IKasaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public KasaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logService = new LogService(_unitOfWork);
        }

        public PagedList<ViewKasaBilgisi> GetKasalarView(int page, int pageSize)
        {
            var repository = _unitOfWork.Repository<ViewKasaBilgisi>();
            var kasalar = repository.GetAll()
                .OrderBy(x => x.KasaTipID)
                .ToPagedList(page, pageSize);
            
            return (PagedList<ViewKasaBilgisi>)kasalar;
        }

        public Kasa getKasaByID(int id)
        {
            var repository = _unitOfWork.Repository<Kasa>();

            return repository.GetById(id);
        }

     

        public void KasaBakiyeDus(int kasaId, double tutar)
        {
            var repository = _unitOfWork.Repository<Kasa>();
            var kasa = repository.GetById(kasaId);
           
                kasa.Tutar -= tutar;
                repository.Update(kasa);
                _unitOfWork.SaveChanges();         
                      
        }

        public void KasaBakiyeEkle(int kasaID, double tutar ,string aciklama ,int ?userID = null)
        {
            var repository = _unitOfWork.Repository<Kasa>();
            var kasa = repository.GetById(kasaID);
           
            kasa.Tutar += tutar;
           
                repository.Update(kasa);
                _unitOfWork.SaveChanges();

            aciklama = aciklama + " KasaID : " + kasaID;
            _logService.AddLog(islemTurId: 18, aciklama: aciklama, userId: userID);
        }

        public void Transfer(int AlıcıID, int VericiID, double tutar)
        {
            var repository = _unitOfWork.Repository<Kasa>();

            var alıcı = repository.GetById(AlıcıID);
            var verici = repository.GetById(VericiID);

            alıcı.Tutar += tutar;
            repository.Update(alıcı);
           
            verici.Tutar -= tutar;
            repository.Update(verici);

            _unitOfWork.SaveChanges();


        }
    }
}
