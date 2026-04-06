using Microsoft.EntityFrameworkCore;
using Reporting.API.Domain.Entities;

namespace Reporting.API.Infrastructure.Data
{
    public class ReportingDbContext : DbContext
    {
        public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);

                // Store JSON blobs as unlimited text (NVARCHAR(MAX) in SQL Server)
                entity.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
                entity.Property(a => a.NewValues).HasColumnType("nvarchar(max)");
            });
        }
    }
}