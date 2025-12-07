using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LifePlannerSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Database – InMemory (s'merr më SQL Server fare)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("LifePlannerDB"));

// Identity (login/registration bazik)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI();

// MVC controllers + views
builder.Services.AddControllersWithViews();

// 🔴 SHUMË E RËNDËSISHME: shto Razor Pages që Identity UI të punojë
builder.Services.AddRazorPages();

var app = builder.Build();

// Krijo "databazën" InMemory
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rrugët për MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rrugët për Identity Razor Pages (Register/Login)
app.MapRazorPages();

app.Run();
