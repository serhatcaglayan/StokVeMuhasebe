using coreAden.Core.Interfaces;
using coreAden.Models;


using iTextSharp.text.pdf;
using iTextSharp.text;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;

using Document = iTextSharp.text.Document;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace coreAden.Controllers
{
    public class TeklifController : Controller
    {
        
        private readonly ISiparisService _siparisService;

        public TeklifController()
        {
            var unitOfWork = new coreAden.Data.Repositories.UnitOfWork(new adenEntities());
            _siparisService = new coreAden.Services.SiparisService(unitOfWork);
        }

        public ActionResult Index(string q = null)
        {
            
           var list = new List<ViewSiparis>().ToPagedList(1, 1); // başlangıçta boş liste döndürsün , veritabanı sorgusu yapmasın

            if (!string.IsNullOrEmpty(q))
            {
                list = _siparisService.GetSiparisler(1, 20, q, SprsTklf: false); // sadece teklifler
            }
           

            return View(list);
        }
        [HttpPost]
        public ActionResult ExportPdf(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Error"] = "Lütfen en az bir kayıt seçin!";
                return RedirectToAction("Index");
            }

            // Seçili siparişleri getir
            var selectedSiparisler = new List<ViewSiparis>();

            foreach (var id in selectedIds)
            {
                var allSiparisler = _siparisService.GetSiparisler(1, 1000);
                var siparis = allSiparisler.FirstOrDefault(s => s.SiparisID == id);

                if (siparis != null)
                {
                    selectedSiparisler.Add(siparis);
                }
            }

            if (selectedSiparisler.Count == 0)
            {
                TempData["Error"] = "Seçili kayıtlar bulunamadı!";
                return RedirectToAction("Index");
            }

            // PDF oluştur
            var pdfBytes = CreatePdf(selectedSiparisler);

            // PDF'i indir
            string fileName = "TeklifListesi_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private byte[] CreatePdf(List<ViewSiparis> siparisler)
        {
            using (var memoryStream = new MemoryStream())
            {
                // A4 yatay sayfa
                var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 20, 20, 20, 20);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Türkçe karakter desteği için font tanımla
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont baseFont;

                try
                {
                    baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
                catch
                {
                    // Arial bulunamazsa varsayılan font kullan
                    baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED);
                }

                // Font tanımları
                var titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                var dateFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, BaseColor.DARK_GRAY);
                var headerFont = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                var cellFont = new iTextSharp.text.Font(baseFont, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                var toplamFont = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                var footerFont = new iTextSharp.text.Font(baseFont, 6, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                var mfotter = new iTextSharp.text.Font(baseFont, 4, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);

                // Tarih bilgisi
                var dateText = new iTextSharp.text.Paragraph("Tarih: " + DateTime.Now.ToString("dd.MM.yyyy"), dateFont);
                dateText.Alignment = Element.ALIGN_LEFT;
                dateText.SpacingAfter = 0;
                document.Add(dateText);

                // LOGO
             
                string logoPath = Server.MapPath("~/Logo/logo.jpg");
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);

                logo.ScaleAbsolute(140f, 60f); // genişlik, yükseklik (piksel)

                // Pozisyon belirleme (sağ üst)
                float xPos = document.PageSize.Width - 140f - 40f; // sağdan  boşluk bırak
                float yPos = document.PageSize.Height - 75f;     // yukarıdan  aşağıda
                logo.SetAbsolutePosition(xPos, yPos);

                PdfContentByte cb = writer.DirectContent;
                cb.AddImage(logo);

                //// Başlık
                //var title = new iTextSharp.text.Paragraph("ADEN AHŞAP", titleFont);
                //title.Alignment = Element.ALIGN_CENTER;
                //title.SpacingAfter = 10;
                //title.SpacingBefore = 0;
                //document.Add(title);

                //Müşteri
                var Ad = siparisler.FirstOrDefault().MusteriAdSoyad;
                var Musteri = new iTextSharp.text.Paragraph("Müşteri: " + Ad, toplamFont);
                Musteri.Alignment = Element.ALIGN_LEFT;
                Musteri.SpacingAfter = 0;
                document.Add(Musteri);

                // Teslim Adresi
                var Adres = new iTextSharp.text.Paragraph("Teslim Adresi:", toplamFont);
                Adres.Alignment = Element.ALIGN_LEFT;
                Adres.SpacingAfter = 40;
                
                document.Add(Adres);
                document.Add(new iTextSharp.text.Paragraph(""));


                // Tablo oluştur (6 sütun)
                var table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.5f, 1.5f, 2.5f, 1f, 1.2f, 1.2f });

                // Başlık satırı
                string[] headers = { "#", "Ürün", "Ürün Tanımı", "Birim", "Birim Fiyat", "Tutar" };

                foreach (string header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = new BaseColor(52, 73, 94);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.Padding = 8;
                    cell.BorderWidth = 1;
                    table.AddCell(cell);
                }

                // Veri satırları
                int sira = 1;
                decimal toplamTutar = 0;

                foreach (var siparis in siparisler)
                {
                    // Sıra
                    AddCell(table, sira.ToString(), cellFont, Element.ALIGN_CENTER);

                    // Kategori
                    AddCell(table, siparis.KategoriAdı ?? "-", cellFont, Element.ALIGN_LEFT);

                    // Sipariş Tanımı
                    AddCell(table, siparis.SiparisTanimi ?? "-", cellFont, Element.ALIGN_LEFT);

                    // Birim
                    string birim = "";
                    if (siparis.Birim.HasValue)
                    {
                        birim = siparis.Birim.Value.ToString("N2");
                    }
                    AddCell(table, birim, cellFont, Element.ALIGN_CENTER);

                    // Birim Fiyat
                    string birimFiyat = "-";
                    if (siparis.BirimFiyat.HasValue)
                    {
                        birimFiyat = siparis.BirimFiyat.Value.ToString("N2");
                    }
                    AddCell(table, birimFiyat, cellFont, Element.ALIGN_RIGHT);

                    // Sipariş Tutarı - DÜZELTME: Operatör önceliği sorunu
                    decimal siparisTutar = 0;
                    if (siparis.Birim.HasValue && siparis.BirimFiyat.HasValue)
                    {
                        siparisTutar = (decimal)(siparis.Birim.Value * siparis.BirimFiyat.Value);
                    }

                    AddCell(table, siparisTutar.ToString("N2") + " ₺", cellFont, Element.ALIGN_RIGHT);

                    toplamTutar += siparisTutar;
                    sira++;
                }

                // Toplam satırı - DÜZELTME: Doğru sütun sayısı
                // İlk 4 sütun için boş hücreler
                for (int i = 0; i < 4; i++)
                {
                    var emptyCell = new PdfPCell(new Phrase("", toplamFont));
                    emptyCell.Border = Rectangle.NO_BORDER;
                    emptyCell.Padding = 5;
                    table.AddCell(emptyCell);
                }

                // "TOPLAM:" etiketi
                var toplamLabelCell = new PdfPCell(new Phrase("TOPLAM:", toplamFont));
                toplamLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                toplamLabelCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                toplamLabelCell.Padding = 8;
                toplamLabelCell.BorderWidth = 1;
                table.AddCell(toplamLabelCell);

                // Toplam değeri
                var toplamValueCell = new PdfPCell(new Phrase(toplamTutar.ToString("N2") + " ₺", toplamFont));
                toplamValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                toplamValueCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                toplamValueCell.Padding = 8;
                toplamValueCell.BorderWidth = 1;
                table.AddCell(toplamValueCell);

                document.Add(table);

                /// Uyarı
                var txt = " *FİYATLARIMIZA KDV DAHİL DEĞİLDİR.\r\n " +
               "*LED AYDINLATMA VE ELEKTRİK AKSAMLARI FİYATLARIMIZA DAHİL DEĞİLDİR.\r\n " +
               "*FİYATLARIMIZA TEZGAH, LAVABO, EVYE, ASPİRATÖR, BATARYA VE ANKASTRE CİHAZLAR HARİÇTİR.\r\n " +
               "TEKLİF SÜRESİ 15 GÜNDÜR.";

                var uyarı = new iTextSharp.text.Paragraph(txt, footerFont);
                uyarı.Alignment = Element.ALIGN_LEFT;                
                document.Add(uyarı);

                //  Footer
                var ftxt = " Aden Ahşap\r\n " +
                    "Mobilya Dekorasyon\r\n " +
                    "email: proje@adenahsap.com\r\n " +
                    "Kadıkendi Mah. 8732. cad. No:26/C Mobilyacılkar sitesi Eyübiye/Şanlıurfa";

                var f = new iTextSharp.text.Paragraph(ftxt,mfotter);
               
               

                // Sayfa sınırlarını kontrol et
                var pageHeight = document.PageSize.Height;
                var currentY = writer.GetVerticalPosition(false);
                var remainingSpace = currentY - document.BottomMargin;

                // Footer'ın yüksekliğini tahmin et (yaklaşık 80-100 point)
                var estimatedFooterHeight = 80;

                // Eğer kalan alan footer için yeterli değilse, yeni sayfaya geç
                if (remainingSpace < estimatedFooterHeight)
                {
                    document.NewPage();
                    f.SpacingBefore = 0;
                }
                else
                {
                    // Footer'ı mevcut sayfanın altına yerleştir
                    f.SpacingBefore = remainingSpace - estimatedFooterHeight;
                }
                f.Alignment = Element.ALIGN_CENTER;
                document.Add(f);
                document.Close();
                writer.Close();

                return memoryStream.ToArray();
            }
        }

        private void AddCell(PdfPTable table, string text, iTextSharp.text.Font font, int alignment)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            cell.BorderWidth = 0.5f;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

    }

   
   
}