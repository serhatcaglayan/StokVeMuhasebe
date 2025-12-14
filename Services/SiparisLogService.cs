using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace coreAden.Services
{
    public class SiparisLogService : ISiparisLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SiparisLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void AddSiparisLog(int islemTurId, string aciklama, int? userId = null, int? kasaID = null, int? siparisID = null, double? siparisTutari = null, double? malzemeTutari = null)
        {
            try
            {
                var repository = _unitOfWork.Repository<SiparisLog>();
                repository.Add(new SiparisLog
                {
                    Tarih = DateTime.Now,
                    LogIslemTurID = islemTurId,
                    Acıklama = aciklama,
                    UserID = userId ?? 1,
                    KasaID = kasaID,
                    SiparisTutari = siparisTutari,
                    MalzemeTutari = malzemeTutari,
                    SiparisID = siparisID                    
                });
                _unitOfWork.SaveChanges();
            }
            catch
            {
                // Log hatası görmezden gel
            }
        }

        public byte[] ExportSiparisLogsToExcel(int? logTurId = null, int? userId = null, int? SiparisID = null, DateTime? baslangic = null, DateTime? bitis = null)
        {
            throw new NotImplementedException();
        }

        public List<LogIslemTur> GetLogIslemTurleri()
        {
            var repository = _unitOfWork.Repository<LogIslemTur>();
            return repository.GetAll().ToList();
        }

        public PagedList<SiparisLog> GetSiparisLogs(int page, int pageSize, int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null, int? siparisID = null)
        {
            var repository = _unitOfWork.Repository<SiparisLog>();
            var query = repository.GetAll().AsQueryable();

            if (logTurId.HasValue)
            {
                query = query.Where(x => x.LogIslemTurID == logTurId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserID == userId.Value);
            }

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.Tarih >= baslangic.Value);
            }

            if (bitis.HasValue)
            {
                query = query.Where(x => x.Tarih <= bitis.Value);
            }
            if(siparisID.HasValue)
            {
                query = query.Where(x => x.SiparisID == siparisID.Value);
            }

            var list = query.OrderByDescending(x => x.Tarih).ToPagedList(page, pageSize);
            return (PagedList<SiparisLog>)list;
        }

        public List<Users> GetUsers()
        {
            var repository = _unitOfWork.Repository<Users>();
            return repository.GetAll().ToList();
        }

    }
}