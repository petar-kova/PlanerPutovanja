using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;
using PlanerPutovanja.Services;
using QuestPDF.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("gm", c =>
{
    c.Timeout = TimeSpan.FromSeconds(8);
});

builder.Services.AddHttpClient("weather", c =>
{
    c.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
    c.Timeout = TimeSpan.FromSeconds(8);
});

builder.Services.AddScoped<GoogleMapsService>();
builder.Services.AddScoped<WeatherService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("hr-HR") };
    options.DefaultRequestCulture = new RequestCulture("hr-HR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

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
app.UseRequestLocalization();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
