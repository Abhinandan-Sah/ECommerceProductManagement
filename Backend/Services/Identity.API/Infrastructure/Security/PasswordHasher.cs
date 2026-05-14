using BCrypt.Net;

namespace Identity.API.Infrastructure.Security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // BCrypt stores its own salt and work factor in the hash, which keeps verification simple later.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }



    }
}
