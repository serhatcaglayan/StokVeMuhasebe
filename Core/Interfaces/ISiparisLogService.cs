using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coreAden.Core.Interfaces
{
    internal interface ISiparisLogService
    {
        PagedList<SiparisLog> GetSiparisLogs(int page, int pageSize, int? logTurId = null, int? userId = null, DateTime? baslangic = null, DateTime? bitis = null, int? siparisID = null);
        List<LogIslemTur> GetLogIslemTurleri();
        List<Users> GetUsers();
        byte[] ExportSiparisLogsToExcel(int? logTurId = null, int? userId = null, int? SiparisID=null , DateTime? baslangic = null, DateTime? bitis = null);
        void AddSiparisLog(int islemTurId, string aciklama, int? userId = null, int? kasaID = null, int? siparisID = null, double? siparisTutari =null , double? malzemeTutari = null);
    }
}
