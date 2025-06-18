using Microsoft.EntityFrameworkCore;
using RazorWithSQLiteApp.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Configure services
// -----------------------------------------------------------------------------

// Use SQLite and bind connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MVC support
builder.Services.AddControllersWithViews();

// Swagger (for API testing/documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -----------------------------------------------------------------------------
// Configure middleware pipeline
// -----------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Swagger available only in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElsSaleWallet API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enable HSTS in production
}

app.UseHttpsRedirection();
app.UseStaticFiles();     // Serve CSS/JS/images from wwwroot
app.UseRouting();
// app.UseAuthorization(); // Enable only if you use authentication

// -----------------------------------------------------------------------------
// Routes
// -----------------------------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Wallet}/{action=Index}/{id?}");

app.MapControllers(); // For API endpoints

// -----------------------------------------------------------------------------
// Database Initialization (Ensure created)
// -----------------------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Create the SQLite DB if not exists
}

// -----------------------------------------------------------------------------
// Run the app
// -----------------------------------------------------------------------------

app.Run();
