using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Load the Ocelot Configuration File
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ─────────────────────────────────────────
// JWT Authentication
// ─────────────────────────────────────────
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});


// 2. Register Ocelot Services
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 3. Execute Ocelot Middleware
await app.UseOcelot();

app.Run();