using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;

namespace coreAden.Core.Interfaces
{
    public interface ILogService
    {
        PagedList<ViewLog> GetLoglar(int page, int pageSize, int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null);
        List<ViewLog> GetLogsWithLogType(int logType, int count=5);
        List<LogIslemTur> GetLogIslemTurleri();
        List<Users> GetUsers();
        byte[] ExportLoglarToExcel(int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null);
        void AddLog(int islemTurId, string aciklama, int? userId = null);
    }
}
