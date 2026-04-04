using Identity.API.Domain.Enums;

namespace Identity.API.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    }
}
