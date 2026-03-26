namespace Identity.API.Application.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; } 
        public string FullName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Role { get; set; } = String.Empty;
    }
}
