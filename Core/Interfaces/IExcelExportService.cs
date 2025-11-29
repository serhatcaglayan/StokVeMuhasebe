using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coreAden.Core.Interfaces
{
    internal interface IExcelExportService
    {
        byte[] ExportToExcel<T>(IQueryable<T> query, string fileName = "export") where T : class;
    }
}
