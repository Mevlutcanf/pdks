using Microsoft.AspNetCore.Mvc;
using KurumsalPDKS.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace KurumsalPDKS.Controllers
{
    public class GecisController : Controller
    {
        AppDbContext _db = new AppDbContext();

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string kartNo)
        {
            if (string.IsNullOrWhiteSpace(kartNo)) return View();
            kartNo = kartNo.Trim();

            var personel = _db.Personeller.FirstOrDefault(p => p.KartNo == kartNo && p.AktifMi == true);
            if (personel == null)
            {
                ViewBag.Mesaj = "ERİŞİM ENGELLENDİ: Geçersiz/Pasif Kart!";
                ViewBag.Durum = "danger";
                return View();
            }

            var sonHareket = _db.GecisLoglari
                                .Where(g => g.PersonelId == personel.Id)
                                .OrderByDescending(g => g.GecisZamani)
                                .FirstOrDefault();

            // 4 Aşamalı Akıllı State Döngüsü (Giriş -> Mola Çıkış -> Mola Giriş -> Çıkış)
            string yeniIslemTipi = "Giriş";
            if (sonHareket != null)
            {
                if (sonHareket.IslemTipi == "Giriş") yeniIslemTipi = "Mola";
                else if (sonHareket.IslemTipi == "Mola") yeniIslemTipi = "Mola Dönüş";
                else if (sonHareket.IslemTipi == "Mola Dönüş") yeniIslemTipi = "Çıkış";
                else yeniIslemTipi = "Giriş";
            }

            // Vardiya Kontrolü (Örn: Sabah 08:30'dan sonra gelenler GEÇ KALDI sayılır)
            if (yeniIslemTipi == "Giriş" && DateTime.Now.TimeOfDay > new TimeSpan(8, 30, 0))
            {
                yeniIslemTipi = "Giriş (Geç)";
            }

            GecisLog yeniLog = new GecisLog
            {
                PersonelId = personel.Id,
                GecisZamani = DateTime.Now,
                IslemTipi = yeniIslemTipi
            };

            _db.GecisLoglari.Add(yeniLog);
            _db.SaveChanges();

            ViewBag.Mesaj = $"{yeniIslemTipi.ToUpper()} BAŞARILI: {personel.Ad} {personel.Soyad}";
            
            // Renk Kodları (Giriş=Yeşil, Çıkış=Kırmızı, Mola=Mavi, Geç Kalma=Turuncu)
            if (yeniIslemTipi.Contains("Giriş") && !yeniIslemTipi.Contains("Geç")) ViewBag.Durum = "success";
            else if (yeniIslemTipi.Contains("Geç")) ViewBag.Durum = "warning";
            else if (yeniIslemTipi == "Mola" || yeniIslemTipi == "Mola Dönüş") ViewBag.Durum = "info";
            else ViewBag.Durum = "danger";

            return View();
        }

        // DEVASE Gelişmiş Arama, Departman Filtreleme ve Tarih Sorgu Motoru
        public IActionResult Rapor(string aramaMetni, string departman, string islemTipi, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            var sorgu = _db.GecisLoglari.Include(g => g.Personel).AsQueryable();

            // 1. İsim / Soyisim / Kart No Arama Filtresi
            if (!string.IsNullOrEmpty(aramaMetni))
            {
                sorgu = sorgu.Where(g => g.Personel.Ad.Contains(aramaMetni) || 
                                         g.Personel.Soyad.Contains(aramaMetni) || 
                                         g.Personel.KartNo.Contains(aramaMetni));
            }

            // 2. Departman Filtresi
            if (!string.IsNullOrEmpty(departman))
            {
                sorgu = sorgu.Where(g => g.Personel.Departman == departman);
            }

            // 3. İşlem Tipi Filtresi (Giriş, Çıkış, Mola vb.)
            if (!string.IsNullOrEmpty(islemTipi))
            {
                sorgu = sorgu.Where(g => g.IslemTipi.Contains(islemTipi));
            }

            // 4. Dinamik Tarih Aralığı Filtresi
            if (baslangicTarihi.HasValue)
            {
                sorgu = sorgu.Where(g => g.GecisZamani >= baslangicTarihi.Value);
            }
            if (bitisTarihi.HasValue)
            {
                // Günün son saniyesine kadar dahil etmek için .AddDays(1) mantığı
                sorgu = sorgu.Where(g => g.GecisZamani <= bitisTarihi.Value.AddDays(1));
            }

            // İK için hazır benzersiz departman listesini dropdown'a dolduruyoruz
            ViewBag.Departmanlar = _db.Personeller.Select(p => p.Departman).Distinct().Where(d => d != null).ToList();

            var loglar = sorgu.OrderByDescending(g => g.GecisZamani).ToList();
            return View(loglar);
        }
    }
}