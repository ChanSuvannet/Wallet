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
    var connectionString = config.GetConnectionString("DefaultConnection");
    
    // Ensure the connection string points to the correct path in Docker
    if (connectionString?.Contains("wallet.db") == true && !connectionString.Contains("/app/data/"))
    {
        connectionString = "Data Source=/app/data/wallet.db";
    }
    
    options.UseSqlite(connectionString);
    
    // Enable detailed errors and sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
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
// Initialize Database BEFORE middleware
// =====================================
await InitializeDatabaseAsync(app);

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

// Add a root route that redirects to your main page
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/home", () => Results.Ok(new { 
    Message = "Welcome to ElsSaleWallet API", 
    Version = "1.0.0",
    Endpoints = new[] { "/swagger", "/health", "/api/wallet" }
}));

app.MapGet("/health", async (ApplicationDbContext context) => 
{
    try
    {
        // Test database connection by checking if we can access it
        await context.Database.CanConnectAsync();
        return Results.Ok(new { 
            Status = "Healthy", 
            Database = "Connected",
            Timestamp = DateTime.UtcNow 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"Database connection failed: {ex.Message}",
            statusCode: 503,
            title: "Service Unavailable"
        );
    }
});

await app.RunAsync();

// =====================================
// Enhanced DB Initialization
// =====================================
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // Ensure data directory exists
        var dbPath = "/app/data";
        if (!Directory.Exists(dbPath))
        {
            Directory.CreateDirectory(dbPath);
            logger.LogInformation("Created data directory: {DbPath}", dbPath);
        }

        if (context.Database.IsSqlite())
        {
            // Check if database exists
            var canConnect = await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection test: {CanConnect}", canConnect);
            
            // Apply migrations (this will create the database if it doesn't exist)
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
                
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("No pending migrations found");
            }

            // Ensure database is created
            var created = await context.Database.EnsureCreatedAsync();
            if (created)
            {
                logger.LogInformation("Database created successfully");
            }
            else
            {
                logger.LogInformation("Database already exists");
            }

            // Test that we can query the database
            var walletsTableExists = await TableExistsAsync(context, "Wallets");
            var transactionsTableExists = await TableExistsAsync(context, "Transactions");
            var paymentRequestsTableExists = await TableExistsAsync(context, "PaymentRequests");
            
            logger.LogInformation("Tables exist - Wallets: {Wallets}, Transactions: {Transactions}, PaymentRequests: {PaymentRequests}", 
                walletsTableExists, transactionsTableExists, paymentRequestsTableExists);
            
            if (!walletsTableExists)
            {
                logger.LogWarning("Wallets table does not exist after migration. Creating manually...");
                
                // Create tables manually based on your actual models
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS Wallets (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL UNIQUE,
                        Email TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        Balance DECIMAL(18,2) NOT NULL DEFAULT 0.00
                    )");
                    
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WalletId INTEGER NOT NULL,
                        Type TEXT NOT NULL,
                        Date DATETIME NOT NULL,
                        Amount DECIMAL(18,2) NOT NULL,
                        Status TEXT NOT NULL,
                        FOREIGN KEY (WalletId) REFERENCES Wallets(Id) ON DELETE CASCADE
                    )");
                    
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS PaymentRequests (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        WalletId INTEGER NOT NULL,
                        Recipientor TEXT NOT NULL,
                        Amount DECIMAL(18,2) NOT NULL,
                        Description TEXT,
                        Status TEXT NOT NULL,
                        CreatedAt DATETIME NOT NULL,
                        FOREIGN KEY (WalletId) REFERENCES Wallets(Id) ON DELETE CASCADE
                    )");
                    
                logger.LogInformation("Tables created manually");
                
                // Insert seed data
                await SeedDataManually(context, logger);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database");
        
        // Don't throw in production, log the error and continue
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
        
        logger.LogError("Database initialization failed, but continuing startup in production mode");
    }
}

static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName)
{
    try
    {
        var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name={0}";
        var result = await context.Database.ExecuteSqlRawAsync(sql, tableName);
        return result > 0;
    }
    catch
    {
        return false;
    }
}

static async Task SeedDataManually(ApplicationDbContext context, ILogger logger)
{
    try
    {
        logger.LogInformation("Seeding initial data...");
        
        // Check if data already exists
        var walletsExist = await context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM Wallets") > 0;
        
        if (!walletsExist)
        {
            // Insert seed wallets
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Wallets (Id, UserId, Email, Name, Balance) VALUES 
                (1, 1, 'kimsreng@gmail.com', 'kimsreng', 1234.56),
                (2, 2, 'admin2@gmail.com', 'admin', 500.00)");
            
            // Insert seed transactions
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Transactions (Id, WalletId, Type, Date, Amount, Status) VALUES 
                (1, 1, 'Payment Received', '2025-06-17 14:30:00', 125.00, 'Completed'),
                (2, 1, 'Product Purchase', '2025-06-16 16:15:00', -89.99, 'Completed')");
            
            // Insert seed payment requests
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO PaymentRequests (Id, WalletId, Recipientor, Amount, Description, Status, CreatedAt) VALUES 
                (1, 1, 'net@gmail.com', 100.00, 'Test Payment', 'Pending', '2025-06-17 11:00:00')");
            
            logger.LogInformation("Seed data inserted successfully");
        }
        else
        {
            logger.LogInformation("Seed data already exists, skipping...");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to seed data manually, but continuing...");
    }
}