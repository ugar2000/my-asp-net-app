// The entry point wires together the minimal API stack, real-time hubs, ML services, database, and
// static asset pipeline so the same executable can serve Blazor WASM, SignalR, and REST endpoints.
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetAppForVika.Server.Abstractions;
using NetAppForVika.Server.Configuration;
using NetAppForVika.Server.Data;
using NetAppForVika.Server.Hubs;
using NetAppForVika.Server.ML;
using NetAppForVika.Server.Models;
using NetAppForVika.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Compress JSON payloads and WASM assets to keep realtime updates and static files snappy.
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/octet-stream",
        "application/wasm"
    });
});

// Allow the WebAssembly client and SignalR connections to originate from known hosts during dev/prod.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                         new[] { "https://localhost:5001", "https://localhost:5003" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.WriteIndented = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Central EF Core context hooking into PostgreSQL; falls back to local defaults for developer machines.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("Postgres") ??
        builder.Configuration["DATABASE_URL"] ??
        "Host=localhost;Database=net_app_for_vika;Username=postgres;Password=postgres";
    options.UseNpgsql(connectionString);
});

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

// Redis cache drives collaborative state fan-out and accelerates algorithm snapshots.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis") ??
        builder.Configuration["REDIS_URL"] ??
        "localhost:6379";
    options.InstanceName = "NetAppForVika:";
});

builder.Services.AddMemoryCache();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 64 * 1024;
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ??
               builder.Configuration["DATABASE_URL"] ??
               "Host=localhost;Database=net_app_for_vika;Username=postgres;Password=postgres")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ??
              builder.Configuration["REDIS_URL"] ??
              "localhost:6379");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<TokenService>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Register domain services so minimal APIs and hubs depend on small, testable abstractions.
builder.Services.AddScoped<IDigitPredictionService, MlDigitPredictionService>();
builder.Services.AddSingleton<IAlgorithmVisualizer, AlgorithmVisualizer>();
builder.Services.AddScoped<IClubSessionCoordinator, ClubSessionCoordinator>();
builder.Services.AddSingleton<ICompilerAnalysisService, CompilerAnalysisService>();
builder.Services.AddHostedService<ModelWarmupHostedService>();

builder.Services.Configure<MLModelOptions>(builder.Configuration.GetSection("MachineLearning"));

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

await SeedIdentityAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseResponseCompression();

var staticFileProvider = new FileExtensionContentTypeProvider();
staticFileProvider.Mappings[".onnx"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileProvider
});
app.UseRouting();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/ready");
app.MapControllers().RequireCors("Client");

// Bind the realtime hubs that stream algorithms and collaborative coding state.
app.MapHub<AlgorithmHub>("/hubs/algorithm");
app.MapHub<ClubSessionHub>("/hubs/club");

await app.RunAsync();

static async Task SeedIdentityAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roles = new[] { "Admin", "Member" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Name = role, Description = role == "Admin" ? "Administrative access" : "Standard member" });
        }
    }

    var adminEmail = app.Configuration["AdminUser:Email"] ?? "admin@codex.club";
    var adminPassword = app.Configuration["AdminUser:Password"] ?? "Admin!234";
    var adminDisplay = app.Configuration["AdminUser:DisplayName"] ?? "Club Admin";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new AppUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            DisplayName = adminDisplay,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create admin user: " + string.Join(",", createResult.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(admin, "Admin");
    }
    else if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
