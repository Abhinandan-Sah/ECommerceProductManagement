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

        // ─── THE MAGIC INTERCEPTOR ──────────────────────────────────
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Look at all the entities EF Core is about to save...
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                // If we are INSERTING a new record, ensure CreatedAt is right now
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                // If we are UPDATING an existing record, stamp the UpdatedAt time
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Continue with the normal save process
            return base.SaveChangesAsync(cancellationToken);
        }

        // ─── FLUENT API CONFIGURATION ───────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Price Configuration
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MRP).HasColumnType("decimal(18,2)");
                entity.Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.GSTPercent).HasColumnType("decimal(5,2)");
            });

            // Inventory Configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.WarehouseLocation).HasMaxLength(100);
            });

            // Approval Configuration
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