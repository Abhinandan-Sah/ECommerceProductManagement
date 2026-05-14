using Microsoft.EntityFrameworkCore;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Infrastructure.Data
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }

        public DbSet<Price> Prices { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Approval> Approvals { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Apply audit timestamps before EF Core writes tracked entities.
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store monetary values with fixed precision.
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MRP).HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.GSTPercent).HasColumnType("decimal(5,2)");
            });

            // Keep warehouse text bounded for predictable storage.
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.WarehouseLocation).HasMaxLength(100);
            });

            // Persist approval status as text so reports and database reads stay understandable.
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Status)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(50);
                entity.Property(a => a.Remarks).HasMaxLength(500);
            });
        }
    }
}
