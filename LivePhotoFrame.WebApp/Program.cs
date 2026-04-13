using LivePhotoFrame.WebApp.Data;
using LivePhotoFrame.WebApp.Models;
using LivePhotoFrame.WebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database provider selection: "postgres" (default) or "sqlserver"
var databaseProvider = (builder.Configuration["DatabaseProvider"] ?? "postgres").ToLowerInvariant();
var connectionStringName = databaseProvider == "sqlserver" ? "SqlServerConnection" : "DefaultConnection";
var connectionString = builder.Configuration.GetConnectionString(connectionStringName)
    ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "sqlserver")
    {
        options.UseSqlServer(connectionString);
    }
    else if (databaseProvider == "postgres")
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        throw new InvalidOperationException($"Unsupported DatabaseProvider '{databaseProvider}'. Expected 'postgres' or 'sqlserver'.");
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Email confirmation disabled — EmailSender is a no-op placeholder.
    // Replace EmailSender with a real provider (SendGrid, SMTP, etc.) before enabling.
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

var app = builder.Build();

// Log the active database provider at startup
app.Logger.LogInformation("Database provider: {Provider}", databaseProvider);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/healthz");

// Serve Vite SPA from /app
app.MapGet("/app", context =>
{
    context.Response.Redirect("/app/index.html");
    return Task.CompletedTask;
});
app.MapFallbackToFile("/app/{*path:nonfile}", "/app/index.html");

app.MapFallbackToController(
    action: "Index",
    controller: "Home");

app.Run();

// Make the implicit Program class public so test projects can reference it
public partial class Program { }
