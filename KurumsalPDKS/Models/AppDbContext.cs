using Microsoft.EntityFrameworkCore;

namespace KurumsalPDKS.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<GecisLog> GecisLoglari { get; set; }

        // SQLite bağlantı ayarı
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=pdks.db");
        }
    }
}