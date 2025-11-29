using coreAden.Core.Interfaces;
using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class SiparisMalzemeleriService : ISiparisMalzemeleri
    {

        private readonly IUnitOfWork _unitOfWork;

        public SiparisMalzemeleriService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void AddSiparisMalzeme(SiparisMalzemeler siparisMalzeme)
        {
           
                
                  
                    var malzemeRepository = _unitOfWork.Repository<Malzemeler>();
                    var malzeme = malzemeRepository.GetById(siparisMalzeme.MalzemeID);

                    if (siparisMalzeme.StoktanDus)
                    {
                        if (malzeme == null)
                            throw new InvalidOperationException("Malzeme bulunamadı.");

                        var mevcutStok = malzeme.StokMiktari ?? 0;
                        if (mevcutStok < (siparisMalzeme.Birim ?? 0))
                            throw new InvalidOperationException($"Yetersiz stok. Mevcut: {mevcutStok}, İstenen: {siparisMalzeme.Birim}");

                        malzeme.StokMiktari = mevcutStok - (siparisMalzeme.Birim ?? 0);
                        malzemeRepository.Update(malzeme);
                    }

                    var repository = _unitOfWork.Repository<SiparisMalzemeler>();
                    repository.Add(siparisMalzeme);

                    _unitOfWork.SaveChanges();

                  //  _logService.AddLog(1, $"Sipariş malzemesi eklendi: SiparisID={siparisMalzeme.SiparisID}, MalzemeID={siparisMalzeme.MalzemeID} [{malzeme?.MalzemeAdı}]");
             
            

        }

        public void DeleteSiparisMalzeme(int id)
        {
            var repo = _unitOfWork.Repository<SiparisMalzemeler>();
            var malzeme = repo.GetById(id);
            repo.Remove(malzeme);
            _unitOfWork.SaveChanges();        
                       
        }

     

        public Malzemeler getMalzemeByID(int id)
        {
            var repo = _unitOfWork.Repository<Malzemeler>();
            return repo.GetById(id);
        }

        public List<SelectListItem> GetMalzemeler()
        {
            
            var repository = _unitOfWork.Repository<Malzemeler>();
            return repository.GetAll()
                .Select(m => new SelectListItem
                {
                    Value = m.MalzemeID.ToString(),
                    Text = m.MalzemeAdı + " [Stok: " + (m.StokMiktari ?? 0) + "]"
                })
                .ToList();
        }

        public SiparisMalzemeler GetSiparisMalzemeler(int id)
        {
            var repo = _unitOfWork.Repository<SiparisMalzemeler>();
            return repo.GetById(id);
        }

        public IEnumerable<ViewSiparisMalzemeleri> GetSiparisMalzemeleriView(int siparisId, string q = null, bool? stokFlag = null)
        {
            var repository = _unitOfWork.Repository<ViewSiparisMalzemeleri>();
            
            var query = repository.GetAll().AsQueryable();
            query = query.Where(x => x.SiparisID == siparisId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x => x.MalzemeAdı.ToUpper().Contains(q.ToUpper()));
            }
           if(stokFlag.HasValue)
            {
                query = query.Where(x => x.StoktanDus == stokFlag);
            }

            return query;
        }

        public void StokDus(int MalzemeID,double miktar)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            var malzeme = repository.GetById(MalzemeID);          
            malzeme.StokMiktari = (malzeme.StokMiktari ?? 0) - miktar;
            _unitOfWork.SaveChanges();


        }


        //Siparişteki malzeme silindiğinde stoga iade etme işlemi
        public void StoktanEkle(int malzemeID, double miktar)
        {

            var repository = _unitOfWork.Repository<Malzemeler>();
            var malzeme = repository.GetById(malzemeID);
            malzeme.StokMiktari = (malzeme.StokMiktari ?? 0) + miktar;
            _unitOfWork.SaveChanges();
        }


        // Export Excel

        public byte[] ExportMalzemelerToCsv(int siparisId, string q = null, bool? stokFlag = null)
        {
            var repository = _unitOfWork.Repository<ViewSiparisMalzemeleri>();
            var query = repository.GetAll().AsQueryable();

            var count = query.Count();
            query = query.Where(x => x.SiparisID == siparisId);

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(x => x.MalzemeAdı.ToLower().Contains(q.ToLower()));

                count = query.Count();
            }

            if (stokFlag.HasValue)
            {
                query = query.Where(x => x.StoktanDus == stokFlag.Value);
                count = query.Count();
            }
            count = query.Count();
            var data = query.OrderBy(x => x.MalzemeAdı).ToList();
            
            var csv = new StringBuilder();
            csv.AppendLine("Malzeme_Adi;Stok_Durumu;Miktar;Birim_Fiyat");

            foreach (var x in data)
            {
               
                    var stok = x.StoktanDus ? "Dustu" : "Dusmedi";
               
                
                csv.AppendLine(string.Join(";", new string[]
                {
                x.MalzemeAdı,
                stok,
                (x.Birim ?? 0).ToString(),
                (x.AlısFiyati ?? 0).ToString()
                }));
            }
            var toplam = data.Sum(x=>x.Birim * x.AlısFiyati).Value.ToString();
            csv.AppendLine("");
            string add = $"Toplam;Malzeme;Tutari;===;{toplam}";
            
            csv.AppendLine(add);

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

    }
}