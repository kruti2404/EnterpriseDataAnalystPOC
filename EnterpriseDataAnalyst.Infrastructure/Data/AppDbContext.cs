using EnterpriseDataAnalyst.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseDataAnalyst.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Sales> Sales { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            DatabaseConfiguration.ApplyConfiguration(modelBuilder);
        }
    }
}
