using ElsSaleWallet.Services;
using Microsoft.EntityFrameworkCore;
using RazorWithSQLiteApp.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// Configuration & Services
// =====================================
var config = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve);

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

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (app.Environment.IsProduction())
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});

app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("default", "{controller=Wallet}/{action=Index}/{id?}");
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

    if (context.Database.IsSqlite())
        await context.Database.MigrateAsync();
}
