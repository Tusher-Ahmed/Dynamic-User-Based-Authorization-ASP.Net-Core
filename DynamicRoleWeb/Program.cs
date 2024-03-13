using DynamicRoleWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Identity;
using DynamicRoleWeb.Services;
using DynamicRoleWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDataAccessService,DataAccessService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(AuthorizeAccess));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
               name: "areaRoute",
               pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "PermissionDenied",
    pattern: "Home/PermissionDenied",
    defaults: new { controller = "Home", action = "PermissionDenied" }
);

app.MapControllerRoute(
    name: "Login",
    pattern: "Account/Login",
    defaults: new { controller = "Account", action = "Login" }
);

app.MapControllerRoute(
    name: "LogOff",
    pattern: "Account/LogOff",
    defaults: new { controller = "Account", action = "LogOff" }
);

app.MapRazorPages();
app.Run();
