using Microsoft.EntityFrameworkCore;
using JawadContractingApp.Models;

namespace JawadContractingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<AccountingEntry> AccountingEntries { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.PhoneNumber);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AccountingEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Paid).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Statement).HasMaxLength(200);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.SerialNumber);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}