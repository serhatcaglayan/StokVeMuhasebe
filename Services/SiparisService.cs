using ClosedXML.Excel;
using coreAden.Core.Interfaces;
using coreAden.Data.Repositories;
using coreAden.Models;

using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class SiparisService : ISiparisService
    {
        private readonly IUnitOfWork _unitOfWork;
       

        public SiparisService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }

        public PagedList<ViewSiparis> GetSiparisler(int page, int pageSize,string q=null, bool? SprsTklf = null,DateTime ? baslangic = null, DateTime? bitis = null)
        {
            var repository = _unitOfWork.Repository<ViewSiparis>();
            var query = repository.GetAll().AsQueryable();

            if (baslangic.HasValue)
            {
                query = query.Where(x => x.SiparisTarihi >= baslangic.Value);
            }
            if (bitis.HasValue)
            {
                query = query.Where(x => x.SiparisTarihi <= bitis.Value);
            }
            if(!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x => x.MusteriAdSoyad.ToUpper().Contains(q.ToUpper()));
            }
            if(SprsTklf.HasValue)
            {
                query = query.Where(x => x.SiparisTeklifDurumu == SprsTklf);
            }

            var list = query.OrderByDescending(x => x.SiparisTarihi).ToPagedList(page, pageSize);
            return (PagedList<ViewSiparis>)list;
        }

        public PagedList<Siparisler> GetMusteriSiparisleri(int musteriId, int page, int pageSize, bool? siparisTeklifDurumu = null, bool? SiparisAktifMi = null)
        {
            var repository = _unitOfWork.Repository<Siparisler>();
            var list = repository.Find(x => x.MusteriID == musteriId);
            if (siparisTeklifDurumu.HasValue)
                list = list.Where(x => x.SiparisTeklifDurumu == siparisTeklifDurumu);
            if (SiparisAktifMi.HasValue)
                list = list.Where(x => x.SiparisAktifMi == SiparisAktifMi);

            list = list.OrderByDescending(x => x.SiparisTarihi).ToPagedList(page, pageSize);            
            return (PagedList<Siparisler>)list;
        }

        public ViewSiparis GetSiparisDetay(int id)
        {
            var repository = _unitOfWork.Repository<ViewSiparis>();
            return repository.SingleOrDefault(x => x.SiparisID == id);
        }

        public Siparisler GetSiparisById(int id)
        {
            var repository = _unitOfWork.Repository<Siparisler>();
            return repository.GetById(id);
        }

        public void CreateSiparis(Siparisler siparis)
        {
                    
            var repository = _unitOfWork.Repository<Siparisler>();
            repository.Add(siparis);
            _unitOfWork.SaveChanges();
        }

        public void UpdateSiparis(Siparisler siparis)
        {
            var repository = _unitOfWork.Repository<Siparisler>();
            var existingSiparis = repository.GetById(siparis.SiparisID);
            
            if (existingSiparis != null)
            {
                existingSiparis.MusteriID = siparis.MusteriID;
                existingSiparis.UrunKategoriID = siparis.UrunKategoriID;
                existingSiparis.SiparisTanimi = siparis.SiparisTanimi;
                existingSiparis.Birim = siparis.Birim;
                existingSiparis.BirimFiyat = siparis.BirimFiyat;
                existingSiparis.SiparisTeklifDurumu = siparis.SiparisTeklifDurumu;
                existingSiparis.OdemeTurID = siparis.OdemeTurID;
                existingSiparis.SiparisAktifMi = siparis.SiparisAktifMi;
                existingSiparis.GuncellemeTarihi = DateTime.Now;
                existingSiparis.SiparisTutari = existingSiparis.Birim * existingSiparis.BirimFiyat;

                repository.Update(existingSiparis);
                _unitOfWork.SaveChanges();

                
            }
        }

        public void DeleteSiparis(int id)
        {
            var repository = _unitOfWork.Repository<Siparisler>();
            var siparis = repository.GetById(id);
            if (siparis != null)
            {
                repository.Remove(siparis);
                _unitOfWork.SaveChanges();
            }
        }

        public List<SelectListItem> GetKategoriler()
        {
            var repository = _unitOfWork.Repository<UrunKategori>();
            return repository.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.KategoriAdı,
                    Value = x.UrunKategoriID.ToString()
                }).ToList();
        }

        public List<SelectListItem> GetUsers()
        {
            var repository = _unitOfWork.Repository<Users>();
            return repository.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.UserAd,
                    Value = x.UserID.ToString()
                }).ToList();
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




        public byte[] ExportSiparislerToExcel(DateTime? baslangic = null, DateTime? bitis = null, string q = null, bool? SprsTklf = null)
        {
            var repository = _unitOfWork.Repository<ViewSiparis>();
            var query = repository.GetAll().AsQueryable();

            if (baslangic.HasValue)
                query = query.Where(x => x.SiparisTarihi >= baslangic.Value);

            if (bitis.HasValue)
                query = query.Where(x => x.SiparisTarihi <= bitis.Value);

            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.MusteriAdSoyad.ToLower().Contains(q.ToLower()));

            if (SprsTklf.HasValue)
                query = query.Where(x => x.SiparisTeklifDurumu == SprsTklf);

            var data = query.OrderByDescending(x => x.SiparisTarihi).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Siparisler");

                // Başlık satırı
                string[] headers = { "Siparis_ID", "Musteri", "Kategori", "Tanim", "Birim", "Birim_Fiyat", "Tutar", "Tarih", "Durum" };
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
                    var durum = x.SiparisTeklifDurumu ? "Siparis" : "Teklif";

                    ws.Cell(row, 1).Value = x.SiparisID;
                    ws.Cell(row, 2).Value = x.MusteriAdSoyad;
                    ws.Cell(row, 3).Value = x.KategoriAdı;
                    ws.Cell(row, 4).Value = x.SiparisTanimi;
                    ws.Cell(row, 5).Value = x.Birim ?? 0;
                    ws.Cell(row, 6).Value = x.BirimFiyat ?? 0;
                    ws.Cell(row, 7).Value = x.SiparisTutari ?? 0;
                    ws.Cell(row, 8).Value = x.SiparisTarihi.ToShortDateString();
                    ws.Cell(row, 9).Value = durum;

                    row++;
                }

                ws.Columns().AdjustToContents(); // Otomatik kolon genişliği

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }





        private void AddLog(int islemTurId, string aciklama)
        {
            try
            {
                var repository = _unitOfWork.Repository<Loglar>();
                repository.Add(new Loglar
                {
                    LogTarihi = DateTime.Now,
                    IslemTurID = islemTurId,
                    LogAcıklama = aciklama
                });
                _unitOfWork.SaveChanges();
            }
            catch
            {
                // Log hatası görmezden gel
            }
        }

        public double toplamSiparisMalzemeTutari(int siparisId)
        {       
            var malzemeListesi = SiparisMalzemeListesi(siparisId);
            double tutar = (double)malzemeListesi.Sum(x => x.AlısFiyati * x.Birim); 
           return tutar;
        }

        public List<ViewSiparisMalzemeleri> SiparisMalzemeListesi(int siparisId)
        {
            var repository = _unitOfWork.Repository<ViewSiparisMalzemeleri>();
            var malzemeListesi = repository.Find(x => x.SiparisID == siparisId).ToList();
            return malzemeListesi;
        }
    }
}
