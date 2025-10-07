using Interchée.Auth;
using Interchée.Data;
using Interchée.Entities;
using Interchée.Services;
using Interchée.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var MyAllowedOrigins = "_myAllowOrigins";


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
    // you can tighten password rules later if needed
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowedOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// --- JWT Authentication (reads from appsettings: Jwt:Issuer/Audience/Key)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection.GetValue<string>("Issuer")!;
var jwtAudience = jwtSection.GetValue<string>("Audience")!;
var jwtKey = jwtSection.GetValue<string>("Key")!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

    });


builder.Services.AddAuthorization();

// --- MVC + API docs (Swagger) + Scalar explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Bind JwtOptions from appsettings.json and register as a singleton
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddSingleton(jwtOptions);

// Token services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleAssignmentService>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentRoleHandler>();
builder.Services.AddSingleton<IEmailSender, DevEmailSender>();



builder.Services.AddHttpClient<SimpleGitService>();

var app = builder.Build();

// --- pipeline
app.UseSerilogRequestLogging();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();        // serves /openapi/v1.json

    // Scalar v2 style: pass the route as the first arg, then configure pattern
    app.MapScalarApiReference("/scalar", options =>
    {
        options
            .WithTitle("InternAttache API")
            .WithOpenApiRoutePattern("/openapi/{documentName}.json"); // default docName is "v1"
    }).AllowAnonymous();
}


app.UseHttpsRedirection();
app.UseCors(MyAllowedOrigins);

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
