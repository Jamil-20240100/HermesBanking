using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Cashier")]
public class CashierHomeController : Controller
{
    private readonly ICashierService _cashierService;
    private readonly UserManager<AppUser> _userManager;

    public CashierHomeController(ICashierService cashierService, UserManager<AppUser> userManager)
    {
        _cashierService = cashierService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var vm = await _cashierService.GetTodaySummaryAsync(userId);

        // Depuración
        Console.WriteLine($"Total Transactions: {vm.TotalTransactions}");
        Console.WriteLine($"Total Deposits: {vm.TotalDeposits}");
        Console.WriteLine($"Total Withdrawals: {vm.TotalWithdrawals}");
        Console.WriteLine($"Total Payments: {vm.TotalPayments}");

        return View(vm);
    }
}
