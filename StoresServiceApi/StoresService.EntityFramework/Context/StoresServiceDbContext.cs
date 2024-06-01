using Microsoft.EntityFrameworkCore;
using StoresService.EntityFramework.Entities;

namespace StoresService.EntityFramework.Context
{
    public class StoresServiceDbContext : DbContext
    {
        public StoresServiceDbContext(DbContextOptions<StoresServiceDbContext> options) : base(options)
        { }

        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StoreProduct> StoreProducts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreProduct>()
                .HasKey(sp => new { sp.StoreId, sp.ProductId });

            modelBuilder.Entity<StoreProduct>()
                .HasOne(sp => sp.Store)
                .WithMany(s => s.StoreProducts)
                .HasForeignKey(sp => sp.StoreId);

            modelBuilder.Entity<StoreProduct>()
                .HasOne(sp => sp.Product)
                .WithMany(p => p.StoreProducts)
                .HasForeignKey(sp => sp.ProductId);
        }
    }
}
