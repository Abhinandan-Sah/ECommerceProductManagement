using Identity.API.Application.Interfaces.Repositories;
using Identity.API.Application.Interfaces.Services;
using Identity.API.Application.Services;
using Identity.API.Infrastructure.Data;
using Identity.API.Infrastructure.Repositories;
using Identity.API.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// Database
// ─────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDBContext>(options =>
    options.UseSqlServer(connectionString));

// ─────────────────────────────────────────
// Repositories
// ─────────────────────────────────────────
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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
            ValidateIssuer           = false,
            ValidateAudience         = false,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ClockSkew                = TimeSpan.Zero,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// Application Services
// ─────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddControllers();

// ─────────────────────────────────────────
// CORS
// ─────────────────────────────────────────
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod());
//});

// ─────────────────────────────────────────
// Swagger with JWT bearer support
// ─────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });

});

// ─────────────────────────────────────────
// Build & Configure Pipeline
// ─────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();