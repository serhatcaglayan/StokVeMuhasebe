using coreAden.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace coreAden.Controllers
{
    public class UrunKategoriController : Controller
    {
        adenEntities db = new adenEntities();
        public ActionResult UrunKategoriListesi(int? page)
        {
            int pageSize = 10; //sayfada görünecek kişi sayısı
            int pagenumber = (page ?? 1); //sayfa  numarası null ise 1 kabul edecek

            var lst = db.UrunKategori.ToList().ToPagedList(pagenumber, pageSize);

            return View(lst);           
        }

        public ActionResult KategoriSil(int id)
        {
            var urunkategori = db.UrunKategori.FirstOrDefault(x => x.UrunKategoriID == id);
            if (urunkategori != null)
            {
                db.UrunKategori.Remove(urunkategori);
                db.SaveChanges();
            }

            return RedirectToAction("UrunKategoriListesi");
        }

        public ActionResult KategoriEkle (UrunKategori urunKategori)
        {
            if (ModelState.IsValid)
            {             

                db.UrunKategori.Add(urunKategori);
                db.SaveChanges();

                return RedirectToAction("UrunKategoriListesi");
            }

            return RedirectToAction("UrunKategoriListesi");
        }

    }
}