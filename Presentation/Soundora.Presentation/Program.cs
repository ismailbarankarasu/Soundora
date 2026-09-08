using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Soundora.Application.Authentication.Abstractions;
using Soundora.Infrastructure.Authentication;
using Soundora.Persistence;
using Soundora.Persistence.Seeds;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection bulunamadı.");
builder.Services.AddPersistence(connectionString);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Soundora.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT ayarları bulunamadı.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT Issuer ve Audience alanları zorunludur.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey en az 32 byte olmalıdır.");
}

if (jwtSettings.AccessTokenMinutes <= 0 ||
    jwtSettings.AccessTokenMinutes > 60)
{
    throw new InvalidOperationException(
        "JWT geçerlilik süresi 1–60 dakika arasında olmalıdır.");
}

builder.Services.AddSingleton(jwtSettings);

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services
    .AddAuthentication()
    .AddJwtBearer(
        JwtBearerDefaults.AuthenticationScheme,
        options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,

                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.HmacSha256
                },

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = "unique_name",
                RoleClaimType = "role"
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlaybackJwt", policy =>
    {
        policy.AddAuthenticationSchemes(
            JwtBearerDefaults.AuthenticationScheme);

        policy.RequireAuthenticatedUser();
        policy.RequireClaim("sub");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<DataSeeder>();

    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Categories}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
