namespace Identity.API.Application.DTOs
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}
