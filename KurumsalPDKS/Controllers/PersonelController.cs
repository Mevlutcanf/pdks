using Microsoft.AspNetCore.Mvc;
using KurumsalPDKS.Models;
using System.Linq;

namespace KurumsalPDKS.Controllers
{
    public class PersonelController : Controller
    {
        // Veritabanı bağlantısı
        AppDbContext _db = new AppDbContext();

        // Personelleri Listeleme
        public IActionResult Index()
        {
            var personelListesi = _db.Personeller
                                     .Where(p => p.AktifMi == true)
                                     .ToList();

            return View(personelListesi);
        }

        // Yeni Personel Ekleme Formu
        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        // Yeni Personel Kaydetme (Kart Çakışma Kontrolü ile)
        [HttpPost]
        public IActionResult Ekle(Personel yeniPersonel)
        {
            if (!ModelState.IsValid)
            {
                return View(yeniPersonel);
            }

            // Aynı kart numarası var mı?
            bool kartVarMi = _db.Personeller
                                .Any(p => p.KartNo == yeniPersonel.KartNo);

            if (kartVarMi)
            {
                ViewBag.Hata = "KART ÇAKIŞMASI: Bu RFID numarası sistemde başka bir personele kayıtlı!";
                return View(yeniPersonel);
            }

            _db.Personeller.Add(yeniPersonel);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // Düzenleme Formunu Aç
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var personel = _db.Personeller.Find(id);

            if (personel == null)
            {
                return RedirectToAction("Index");
            }

            return View(personel);
        }

        // Düzenlenen Bilgileri Kaydet (Kart Çakışma Kontrolü ile)
        [HttpPost]
        public IActionResult Duzenle(Personel guncelPersonel)
        {
            if (!ModelState.IsValid)
            {
                return View(guncelPersonel);
            }

            // Kendisi hariç aynı kart numarasını kullanan başka personel var mı?
            bool kartVarMi = _db.Personeller.Any(
                p => p.KartNo == guncelPersonel.KartNo &&
                     p.Id != guncelPersonel.Id);

            if (kartVarMi)
            {
                ViewBag.Hata = "KART ÇAKIŞMASI: Bu kart numarası başka birine ait, değişiklik yapılamaz!";
                return View(guncelPersonel);
            }

            var eskiPersonel = _db.Personeller.Find(guncelPersonel.Id);

            if (eskiPersonel != null)
            {
                eskiPersonel.Ad = guncelPersonel.Ad;
                eskiPersonel.Soyad = guncelPersonel.Soyad;
                eskiPersonel.KartNo = guncelPersonel.KartNo;
                eskiPersonel.Departman = guncelPersonel.Departman;

                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Personeli Pasife Alma
        public IActionResult PasifYap(int id)
        {
            var personel = _db.Personeller.Find(id);

            if (personel != null)
            {
                personel.AktifMi = false;
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}