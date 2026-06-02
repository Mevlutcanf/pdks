using System.ComponentModel.DataAnnotations;

namespace KurumsalPDKS.Models
{
    public class Personel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Ad { get; set; }
        
        [Required]
        public string Soyad { get; set; }
        
        // RFID cihazından okuyacağımız ID
        public string KartNo { get; set; } 
        
        public string Departman { get; set; }
        
        // İşten çıkan personelin verisini silmeyip pasife çekeceğiz
        public bool AktifMi { get; set; } = true; 
    }
}