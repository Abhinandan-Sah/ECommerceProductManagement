using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Reporting.API.Application.Consumers;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Application.Interfaces.Services;
using Reporting.API.Application.Services;
using Reporting.API.Infrastructure.Data;
using Reporting.API.Infrastructure.Middleware;
using Reporting.API.Infrastructure.Repositories;
using Serilog;
using System.Text;

// ─────────────────────────────────────────
// Serilog Bootstrap
// ─────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// ─────────────────────────────────────────
// Controllers
// ─────────────────────────────────────────
builder.Services.AddControllers();

// ─────────────────────────────────────────
// Swagger / OpenAPI
// ─────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reporting API",
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// ─────────────────────────────────────────
// Database
// ─────────────────────────────────────────
builder.Services.AddDbContext<ReportingDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────
// Repositories & Services
// ─────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IReportingRepository, ReportingRepository>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

// ─────────────────────────────────────────
// RabbitMQ / MassTransit
// ─────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductStatusChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitPort = builder.Configuration.GetValue<ushort>("RabbitMq:Port", 5672);

        cfg.Host(rabbitHost, rabbitPort, "/", h => {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // Creates a specific listener queue just for this microservice
        cfg.ReceiveEndpoint("reporting_audit_queue", e =>
        {
            e.ConfigureConsumer<ProductStatusChangedConsumer>(context);
        });
    });
});

// ─────────────────────────────────────────
// Build & Configure Pipeline
// ─────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
