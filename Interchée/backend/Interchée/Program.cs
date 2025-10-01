using Interchée.Data;
using Interchée.Entities;
using Interchée.Repositories.Implementations;
using Interchée.Repositories.Interfaces;
using Interchée.Services.Implementations;
using Interchée.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// --- EF Core (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// --- Identity
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

// --- MVC + API docs
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


// --- REPOSITORY REGISTRATIONS
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();

// --- SERVICE REGISTRATIONS
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IGitIntegrationService, GitIntegrationService>();

// --- HTTP CLIENT for Git integration
builder.Services.AddHttpClient<IGitIntegrationService, GitIntegrationService>();

var app = builder.Build();

// --- Pipeline
app.UseSerilogRequestLogging();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar"); // Keep Scalar too if you want both
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