using Microsoft.EntityFrameworkCore;
using CLIR_InfoSystem.Data;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Add Session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expires after 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================= DATABASE CONNECTION SETUP =================

// Get MYSQL_URL from Render (Railway provides this)
var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");

MySqlConnectionStringBuilder connBuilder;

if (!string.IsNullOrEmpty(mysqlUrl))
{
    // Running on Render → use Railway MySQL
    var uri = new Uri(mysqlUrl);
    var userInfo = uri.UserInfo.Split(':');

    connBuilder = new MySqlConnectionStringBuilder
    {
        Server = uri.Host,
        Port = (uint)uri.Port,
        UserID = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = MySqlSslMode.Required
    };
}
else
{
    // Running locally → use appsettings.json
    connBuilder = new MySqlConnectionStringBuilder(
        builder.Configuration.GetConnectionString("DefaultConnection"));
}

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseMySql(connBuilder.ConnectionString,
        ServerVersion.AutoDetect(connBuilder.ConnectionString)));

// =============================================================

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 2. Enable Sessions (Must be before Authorization)
app.UseSession();

app.UseAuthorization();

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
