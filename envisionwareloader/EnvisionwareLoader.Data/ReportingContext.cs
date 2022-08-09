using EnvisionwareLoader.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EnvisionwareLoader.Data
{
    public class ReportingContext : DbContext
    {
        private readonly string _connectionString;
        public ReportingContext(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HourlySummary>()
                .HasKey(_ => new { _.Date, _.Branch});
            modelBuilder.Entity<DailySummary>()
                .HasKey(_ => new { _.Date, _.Branch});
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public IEnumerable<string> GetPendingMigrations()
        {
            return Database.GetPendingMigrations();
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<DailySummary> DailySummaries { get; set; }
        public DbSet<UsageDetail> Details { get; set; }
        public DbSet<HourlySummary> HourlySummaries { get; set; }
    }
}
