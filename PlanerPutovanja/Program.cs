using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanerPutovanja.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC (Controllers + Razor Views)
builder.Services.AddControllersWithViews();

// EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (Razor Pages pod Areas/Identity)
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Error handling + HSTS
if (app.Environment.IsDevelopment())
{
    // opcionalno, ali korisno dok radi� projekt
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

// MVC routing (Home je default, a "My trips" ide eksplicitno na /Trips/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages (/Identity/Account/Login, Register, ...)
app.MapRazorPages();

app.Run();
