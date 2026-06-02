using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KurumsalPDKS.Models
{
    public class GecisLog
    {
        [Key]
        public int Id { get; set; }

        // Hangi personel okuttu? (Personel tablosuna bağlanıyor)
        public int PersonelId { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }

        public DateTime GecisZamani { get; set; } = DateTime.Now;

        // "Giriş" veya "Çıkış"
        public string IslemTipi { get; set; } 
    }
}