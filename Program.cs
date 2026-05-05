using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReadX.Api.Middlewares;
using ReadX.Api.Repositories.Implementations;
using ReadX.Api.Repositories.Interfaces;
using ReadX.Api.Seeders;
using ReadX.Api.Services.Implementations;
using ReadX.Api.Services.Interfaces;
using ReadX.Api.Settings;
using ReadX.Api.Validators;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Map environment variables to strongly-typed configuration sections
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "MongoDbSettings:ConnectionString", Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") },
    { "MongoDbSettings:DatabaseName", Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") },
    { "JwtSettings:Secret", Environment.GetEnvironmentVariable("JWT_SECRET") },
    { "JwtSettings:Issuer", Environment.GetEnvironmentVariable("JWT_ISSUER") },
    { "JwtSettings:Audience", Environment.GetEnvironmentVariable("JWT_AUDIENCE") },
    { "JwtSettings:ExpiryMinutes", Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") }
}.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value));

// Prevent standard JWT claim mapping to long XML namespaces (keep them clean as "sub", "role", etc.)
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Documentation setup with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReadX API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure Settings Classes
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Repositories
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // Disable automatic claim mapping
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        RoleClaimType = "role" // Identify the claim name for roles
    };
});

// Define Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "admin"));
    options.AddPolicy("MemberOnly", policy => policy.RequireClaim("role", "member"));
});

var app = builder.Build();

// --- Database Seeding ---

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// --- Middlewares Pipeline ---

// 1. Handle errors globally
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 2. Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// 3. Security Middlewares
app.UseAuthentication();
app.UseAuthorization();

// 4. Map API Routes
app.MapControllers();

app.Run();
