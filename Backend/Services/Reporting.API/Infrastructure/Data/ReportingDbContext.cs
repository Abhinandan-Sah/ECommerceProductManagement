using Microsoft.EntityFrameworkCore;
using Reporting.API.Domain.Entities;

namespace Reporting.API.Infrastructure.Data
{
    public class ReportingDbContext : DbContext
    {
        public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DashboardSnapshot> DashboardSnapshots { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DashboardSnapshot>(entity =>
            {
                entity.ToTable("DashboardSnapshots");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SnapshotDate).HasColumnType("DATE").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("DATETIME2").IsRequired().HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<ProductReport>(entity =>
            {
                entity.ToTable("ProductReports");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).HasMaxLength(300);
                entity.Property(e => e.SKU).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.PublishedAt).HasColumnType("DATETIME2");
                entity.Property(e => e.CategoryName).HasMaxLength(150);
                entity.Property(e => e.CreatedAt).HasColumnType("DATETIME2").IsRequired().HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.OldValues).HasColumnType("NVARCHAR(MAX)");
                entity.Property(a => a.NewValues).HasColumnType("NVARCHAR(MAX)");
                entity.Property(a => a.CreatedAt).HasColumnType("DATETIME2").IsRequired().HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}