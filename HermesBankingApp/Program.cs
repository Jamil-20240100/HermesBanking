using HermesBanking.Core.Application;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Application.Services;
using HermesBanking.Infrastructure.Identity;
using HermesBanking.Infrastructure.Identity.Contexts;
using HermesBanking.Infrastructure.Identity.Entities;
using HermesBanking.Infrastructure.Identity.Services;
using HermesBanking.Infrastructure.Persistence;
using HermesBanking.Infrastructure.Persistence.Contexts;
using HermesBanking.Infrastructure.Persistence.Repositories;
using HermesBanking.Infrastructure.Shared;
using HermesBanking.Infrastructure.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();  // Añadir el proveedor de TempData

builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
});

// Registrar servicios de identidad (UserManager, SignInManager)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Opciones adicionales si las necesitas
})
.AddEntityFrameworkStores<HermesBankingContext>()  // Usar el contexto de la base de datos
.AddDefaultTokenProviders(); // Proveedores de tokens para funcionalidades como recuperación de contraseñas

// Registrar los servicios de la aplicación
builder.Services.AddDbContext<HermesBankingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAccountServiceForWebApp, AccountServiceForWebApp>();
builder.Services.AddScoped<ICashierService, CashierService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmailService, EmailService>();  // Registrar EmailService

// Registrar repositorios y otros servicios
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  // Si tienes un IdentityContext

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();  // Si necesitas hacer un seeding de los usuarios

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();  // Middleware para usar TempData

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
