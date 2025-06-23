using ElsSaleWallet.Services;
using Microsoft.EntityFrameworkCore;
using RazorWithSQLiteApp.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Environment URLs
var frontendUrl = config["FRONTEND_URL"] ?? "http://localhost:3000";
var walletFrontendUrl = config["WALLET_FRONTEND_URL"] ?? "http://localhost:5253";
var apiGatewayUrl = config["API_GATEWAY_URL"] ?? "http://localhost:3001";

// Configure EF Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Razor + API Controllers
builder.Services.AddControllersWithViews().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendUrl, walletFrontendUrl, apiGatewayUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// DI
builder.Services.AddScoped<IWalletService, WalletService>();

// Swagger
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

// Middleware
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

// app.UseHttpsRedirection(); // Optional
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/home", () => Results.Ok(new
{
    Message = "Welcome to ElsSaleWallet API",
    Version = "1.0.0",
    Endpoints = new[] { "/swagger", "/health", "/api/wallet" }
}));
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

await InitializeDatabaseAsync(app);
await app.RunAsync();

// ===========================
// DB Initialization
// ===========================
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (context.Database.IsSqlite())
        {
            var pending = await context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                Console.WriteLine("Applying pending migrations...");
                await context.Database.MigrateAsync();
            }
            else
            {
                Console.WriteLine("No pending migrations.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to initialize database");
        throw;
    }
}
