using ElsSaleWallet.Services;
using Microsoft.EntityFrameworkCore;
using RazorWithSQLiteApp.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// =====================================
// Environment-based settings
// =====================================
var frontendUrl = config["FRONTEND_URL"] ?? "http://localhost:3000";
var walletFrontendUrl = config["WALLET_FRONTEND_URL"] ?? "http://localhost:5253";
var apiGatewayUrl = config["API_GATEWAY_URL"] ?? "http://localhost:3001";

// =====================================
// Add services to the container
// =====================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging(); // Optional: remove in production
});

// ✅ Add support for both APIs and Razor Views
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(frontendUrl, walletFrontendUrl, apiGatewayUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Dependency Injection
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ElsSaleWallet API",
        Version = "v1",
        Description = "Digital Wallet Management System API"
    });
});

var app = builder.Build();

// =====================================
// Middleware Pipeline
// =====================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElsSaleWallet v1");
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Optional for production:
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");

// Uncomment if you add auth:
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute(); // 👈 Needed for Razor View (MVC-style) controllers like WalletController.Index()

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

await InitializeDatabaseAsync(app);

await app.RunAsync();

// =====================================
// DB Initialization
// =====================================
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (context.Database.IsSqlite())
        {
            await context.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to initialize database");
        throw;
    }
}
