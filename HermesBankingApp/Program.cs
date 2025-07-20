using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Mappings.DTOsAndViewModels;
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
    .AddSessionStateTempDataProvider();

// Configuración para manejar sesiones
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60); // Tiempo de sesión
    opt.Cookie.HttpOnly = true;  // Seguridad en el uso de cookies
});

// Configuración de Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Aquí podrías agregar configuraciones personalizadas de Identity
    // Ejemplo: options.Password.RequireDigit = true;
})
.AddEntityFrameworkStores<IdentityContext>()
.AddDefaultTokenProviders();

// Configuración del DbContext principal para la base de datos
builder.Services.AddDbContext<HermesBankingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuración del DbContext para Identity
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));


builder.Services.AddScoped<IAccountServiceForWebApp, AccountServiceForWebApp>();
builder.Services.AddScoped<ICashierService, CashierService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<ISavingsAccountService, SavingsAccountService>();
builder.Services.AddScoped<ICreditCardService, CreditCardService>();
builder.Services.AddScoped<ICashAdvanceService, CashAdvanceService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<ICashierTransactionService, CashierTransactionService>();
builder.Services.AddScoped<IGenericService<TransactionDTO>, GenericService<Transaction, TransactionDTO>>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IAmortizationInstallmentRepository, AmortizationInstallmentRepository>();
builder.Services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();




builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(SavingsAccountDTOMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(SavingsAccountViewModelMappingProfile).Assembly);
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


var app = builder.Build();

app.UseStaticFiles();

await app.Services.RunIdentitySeedAsync(); 

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
