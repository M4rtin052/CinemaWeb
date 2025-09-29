using Cinema.Controllers;
using Cinema.Database;
using Cinema.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MyDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));

builder.Services.AddDistributedMemoryCache(); // U¿ywa pamiêci na serwerze do przechowywania sesji
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Czas trwania sesji - 30 minut
    options.Cookie.HttpOnly = true; // Zabezpieczenie, widocznoœæ tylko od strony serwera, nie klienta
});
// Us³uga autoryzacji u¿ytkownika poprzez wykorzystanie ciasteczek, automatycznie wylogowanie po godzinie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options 
    => options.ExpireTimeSpan = TimeSpan.FromHours(1));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Dodanie sesji

app.UseAuthentication(); // Dodanie uwierzytelniania
app.UseAuthorization(); // Dodanie autoryzacji

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
