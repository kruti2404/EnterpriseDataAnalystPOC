using EnterpriseDataAnalyst.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseDataAnalyst.Infrastructure.Data
{
    public static class DatabaseConfiguration
    {
        public static void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            modelBuilder.Entity<Product>()
                .Property(p => p.Category)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50);

            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Email)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Region)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50);

            modelBuilder.Entity<Sales>()
                .Property(s => s.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sales>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Sales)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sales>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Sales>()
                .HasIndex(s => s.Date);
            modelBuilder.Entity<Sales>()
                .HasIndex(s => s.Region);
        }
    }
}
