using ClosedXML.Excel;
using coreAden.Core.Interfaces;
using coreAden.Models;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace coreAden.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;
        public StockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logService = new LogService(unitOfWork);
        }

        public PagedList<Malzemeler> GetMalzemeler(int page, int pageSize, DateTime? baslangic = null, DateTime? bitis = null)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            var query = repository.GetAll().AsQueryable();

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.AlısTarihi >= baslangic.Value);
            }
            if (bitis.HasValue)
            {
                query = query.Where(x => x.AlısTarihi <= bitis.Value);
            }

            var list = query.OrderBy(x => x.StokMiktari).ToPagedList(page, pageSize);
            return (PagedList<Malzemeler>)list;
        }

        public Malzemeler GetMalzemeById(int id)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            return repository.GetById(id);
        }

        public void AddMalzeme(Malzemeler malzeme)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            repository.Add(malzeme);
            _unitOfWork.SaveChanges();
        }

        public void UpdateMalzeme(Malzemeler malzeme)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            var existingMalzeme = repository.GetById(malzeme.MalzemeID);
            
            if (existingMalzeme != null)
            {
                existingMalzeme.MalzemeAdı = malzeme.MalzemeAdı;
                existingMalzeme.AlısFiyati = malzeme.AlısFiyati;
                repository.Update(existingMalzeme);
                _unitOfWork.SaveChanges();
            }
        }

        public void DeleteMalzeme(int id)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            var malzeme = repository.GetById(id);
            if (malzeme != null)
            {
                repository.Remove(malzeme);
                _unitOfWork.SaveChanges();
            }
        }

        public void StokEkle(int malzemeId, double miktar, bool kasayaYansitilsinMi = false)
        {
            var malzemeRepository = _unitOfWork.Repository<Malzemeler>();
            var malzeme = malzemeRepository.GetById(malzemeId);
            var msg = "";
            try
            {
                if(malzeme!=null)
                {                  
                    malzeme.StokMiktari = (malzeme.StokMiktari ?? 0) + miktar;
                    malzeme.AlısTarihi = DateTime.Now;
                    malzeme.EklemeYapanUserID = 1;
                    malzemeRepository.Update(malzeme);
                    _unitOfWork.SaveChanges();
                    // Stok sayısı değiştiğinde trigger tetiklenir ve log kaydı oluşur. [dbo].[tr_Malzemeler_Stok_Update_Log]
                }
                else
                {
                    msg += $" Malzeme Bulunumadı : {malzeme.MalzemeAdı}. Stok Ekleme İşlemi Başarısız ";                    
                }

            }catch(Exception ex)
            {
                
                msg += $" Stok Ekleme İşlemi Başarısız. {malzeme.MalzemeAdı} adet : {miktar}";
                _logService.AddLog(islemTurId:2 ,msg,userId:1);
            }

          
        }

        public byte[] ExportMalzemelerToCsv(DateTime? baslangic = null, DateTime? bitis = null)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            var query = repository.GetAll().AsQueryable();
            var toplamMaliyet = 0.0;

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.AlısTarihi >= baslangic.Value);
            }
            if (bitis.HasValue)
            {
                query = query.Where(x => x.AlısTarihi <= bitis.Value);
            }

            var data = query.OrderBy(x => x.MalzemeAdı).ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Giderler");

                // Başlık satırı
                string[] headers = { "ID", "Malzeme_Adı", "Stok", "Alış_Fiyatı", "Alış_Tarihi", "User_ID" };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                    ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Data satırları
                int row = 2;
                foreach (var x in data)
                {


                    ws.Cell(row, 1).Value = x.MalzemeID;
                    ws.Cell(row, 2).Value = x.MalzemeAdı;
                    ws.Cell(row, 3).Value = x.StokMiktari;
                    if (x.StokMiktari < 100)
                    {
                        ws.Cell(row, 3).Style.Font.Bold = true;
                        ws.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.Red;
                    }
                        
                    ws.Cell(row, 4).Value = x.AlısFiyati;
                    ws.Cell(row, 5).Value = x.AlısTarihi;
                    ws.Cell(row, 6).Value = x.EklemeYapanUserID;
                    


                    row++;
                }

                toplamMaliyet = data.Sum(x => x.AlısFiyati) ?? 0;

                ws.Cell(row + 1, 4).Value = "Toplam Stok Maliyeti";
                ws.Cell(row + 1, 5).Value = " = ";
                ws.Cell(row + 1, 6).Value = toplamMaliyet + " ₺";

                ws.Cell(row + 1, 4).Style.Fill.BackgroundColor = XLColor.LightGreen;
                ws.Cell(row + 1, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;
                ws.Cell(row + 1, 6).Style.Fill.BackgroundColor = XLColor.LightGreen;

                ws.Cell(row + 1, 4).Style.Font.Bold = true;
                ws.Cell(row + 1, 5).Style.Font.Bold = true;
                ws.Cell(row + 1, 6).Style.Font.Bold = true;


                ws.Columns().AdjustToContents(); // Otomatik kolon genişliği

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return ms.ToArray();
                }
            }

        }

        
    }
}
