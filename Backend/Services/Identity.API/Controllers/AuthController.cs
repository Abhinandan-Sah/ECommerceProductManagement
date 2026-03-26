using Identity.API.Application.DTOs;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Identity.API.Infrastructure.Data;
using Identity.API.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IdentityDBContext _context;
        private readonly JwtTokenGenerator _jwtGenerator;

        public AuthController(IdentityDBContext context, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtGenerator = jwtTokenGenerator;
        }
  
        [HttpPost("register")]
        public IActionResult Register(RegisterRequestDto request)
        {
            if (_context.Users.Any(u=> u.Email == request.Email))
            {
                return BadRequest("User already exists.");
            }

            var hashedPassword = PasswordHasher.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = Role.Customer,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if(user == null){
                return BadRequest("User not found.");
            }

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash)){
                return BadRequest("Incorrect Password");
            }

            //var jwtGenerator = new JwtTokenGenerator(IConfiguration.GetValue<string>("JwtSettings:Secert");

            string token = _jwtGenerator.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            return Ok("Authorize user");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok("Only Admin can access");
        }
    }
}
