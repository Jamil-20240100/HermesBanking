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
        return View(vm);
    }
}
