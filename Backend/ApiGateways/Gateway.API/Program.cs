// Configures the API Gateway host, Ocelot routing, Swagger aggregation, authentication, and gateway middleware.
using Gateway.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Ocelot reads its route table from ocelot.json. Keep reloadOnChange enabled so local route changes
// rebuild the gateway configuration without restarting the host.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// The gateway validates the same JWT issuer and audiences as the downstream services before
// Ocelot forwards requests that require authentication.
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured");
var jwtAudiences = builder.Configuration.GetSection("JwtSettings:Audiences").Get<string[]>()
    ?? throw new InvalidOperationException("JwtSettings:Audiences is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudiences = jwtAudiences,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// CORS is restricted at the gateway so browser clients do not need separate cross-origin policy
// decisions from every downstream service.
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("CorsSettings:AllowedOrigins is not configured");

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("CorsSettings:AllowedOrigins cannot be empty");
}

if (allowedOrigins.Any(origin => origin.Contains("*")))
{
    throw new InvalidOperationException("CorsSettings:AllowedOrigins cannot contain wildcard origins");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Ocelot handles upstream-to-downstream route matching, while Polly adds circuit-breaker and timeout
// behavior from each route's QoSOptions section.
builder.Services.AddOcelot(builder.Configuration).AddPolly();

// SwaggerForOcelot exposes one gateway Swagger UI backed by the downstream SwaggerEndPoints
// configured in ocelot.json.
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// The UI calls /swagger/docs, where SwaggerForOcelot merges service OpenAPI documents by SwaggerKey.
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// Handle gateway-level failures before the request reaches Ocelot or while Ocelot routes downstream.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

app.UseCors("RestrictedOrigins");

app.UseAuthentication();
app.UseAuthorization();

// Ocelot must run at the end of the request pipeline because it selects the downstream route and
// forwards the request to the configured service.
await app.UseOcelot();

app.Run();
