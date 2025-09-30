using Interch�e.Data;
using Interch�e.Entities;
using Interch�e.Repositories.Implementations;
using Interch�e.Repositories.Interfaces;
using Interch�e.Services.Implementations;  // ADD THIS
using Interch�e.Services.Interfaces;      // ADD THIS
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog (console + file, configured via appsettings.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// --- EF Core (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// --- Identity (Users + Roles, using GUID keys)
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection.GetValue<string>("Issuer")!;
var jwtAudience = jwtSection.GetValue<string>("Audience")!;
var jwtKey = jwtSection.GetValue<string>("Key")!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();

// --- Service Registrations (ADD THESE)
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IGitIntegrationService, GitIntegrationService>();
builder.Services.AddHttpClient<IGitIntegrationService, GitIntegrationService>();

var app = builder.Build();

// --- pipeline
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/scalar");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed on startup
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await SeedData.RunAsync(sp);
}

await app.RunAsync();