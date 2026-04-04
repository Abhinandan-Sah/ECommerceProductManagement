using Identity.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Data
{
    public class IdentityDBContext : DbContext
    {
        public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users table configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired();
            });

            // RefreshTokens table configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Token).IsUnique();
                entity.Property(r => r.Token).IsRequired();

                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetTokens table configuration
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Token).IsUnique();
                entity.Property(p => p.Token).IsRequired();

                entity.HasOne(p => p.User)
                      .WithMany(u => u.PasswordResetTokens)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

