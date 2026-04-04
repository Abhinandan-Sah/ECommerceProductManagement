namespace Identity.API.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;

        // Foreign key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Expires in 30 minutes by default
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; } = false;

        // Computed helpers
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }
}
