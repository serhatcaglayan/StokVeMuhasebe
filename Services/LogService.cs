using coreAden.Core.Interfaces;
using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace coreAden.Services
{
    public class LogService : ILogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedList<ViewLog> GetLoglar(int page, int pageSize, int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null)
        {
            var repository = _unitOfWork.Repository<ViewLog>();
            var query = repository.GetAll().AsQueryable();

            if (logTurId.HasValue)
            {
                query = query.Where(x => x.IslemTurID == logTurId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserID == userId.Value);
            }

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.LogTarihi >= baslangic.Value);
            }

            if (bitis.HasValue)
            {
                query = query.Where(x => x.LogTarihi <= bitis.Value);
            }

            var list = query.OrderByDescending(x => x.LogTarihi).ToPagedList(page, pageSize);
            return (PagedList<ViewLog>)list;
        }

        public List<LogIslemTur> GetLogIslemTurleri()
        {
            var repository = _unitOfWork.Repository<LogIslemTur>();
            return repository.GetAll().ToList();
        }

        public List<Users> GetUsers()
        {
            var repository = _unitOfWork.Repository<Users>();
            return repository.GetAll().ToList();
        }

        public byte[] ExportLoglarToExcel(int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null)
        {
            var repository = _unitOfWork.Repository<ViewLog>();
            var query = repository.GetAll().AsQueryable();

            if (logTurId.HasValue)
            {
                query = query.Where(x => x.IslemTurID == logTurId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserID == userId.Value);
            }

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.LogTarihi >= baslangic.Value);
            }

            if (bitis.HasValue)
            {
                query = query.Where(x => x.LogTarihi <= bitis.Value);
            }

            var data = query.OrderByDescending(x => x.LogTarihi).ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Tarih,İşlem Türü,Kullanıcı,Açıklama");

            foreach (var log in data)
            {
                csv.AppendLine($"{log.LogTarihi:dd.MM.yyyy HH:mm},{log.IslemAd},{log.username},\"{log.LogAcıklama}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public void AddLog(int islemTurId, string aciklama, int? userId = null)
        {
            try
            {
                var repository = _unitOfWork.Repository<Loglar>();
                repository.Add(new Loglar
                {
                    LogTarihi = DateTime.Now,
                    IslemTurID = islemTurId,
                    LogAcıklama = aciklama,
                    UserID = userId ?? 1
                });
                _unitOfWork.SaveChanges();
            }
            catch
            {
                // Log hatası görmezden gel
            }
        }

        public List<ViewLog> GetLogsWithLogType(int logType, int count = 5)
        {
            return _unitOfWork.Repository<ViewLog>().GetAll().Where(x=>x.IslemTurID == logType).Take(count).OrderByDescending(x=>x.LogTarihi).ToList();
        }
    }
}
