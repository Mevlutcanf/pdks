using Microsoft.AspNetCore.Mvc;
using KurumsalPDKS.Models;
using System.Linq;
using System;

namespace KurumsalPDKS.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext _db = new AppDbContext();

        public IActionResult Index()
        {
            // 1. Personel İstatistikleri
            ViewBag.AktifPersonel = _db.Personeller.Count(p => p.AktifMi == true);
            ViewBag.PasifPersonel = _db.Personeller.Count(p => p.AktifMi == false);

            // 2. Bugünün Toplam Geçiş Hareketi
            var bugun = DateTime.Today;
            ViewBag.BugunGecis = _db.GecisLoglari.Count(g => g.GecisZamani.Date == bugun);

            // 3. İçerideki Personel Sayısı Algoritması (Akıllı Kontrol)
            int iceridekiSayisi = 0;
            var aktifPersoneller = _db.Personeller.Where(p => p.AktifMi == true).ToList();
            
            foreach(var p in aktifPersoneller) 
            {
                // Her personelin EN SON hareketine bakıyoruz
                var sonHareket = _db.GecisLoglari
                                    .Where(g => g.PersonelId == p.Id)
                                    .OrderByDescending(g => g.GecisZamani)
                                    .FirstOrDefault();
                                    
                // Eğer en son hareketi "Giriş" ise, adam şu an içeridedir!
                if(sonHareket != null && sonHareket.IslemTipi == "Giriş") 
                {
                    iceridekiSayisi++;
                }
            }
            ViewBag.IceridekiPersonel = iceridekiSayisi;

            return View();
        }
    }
}