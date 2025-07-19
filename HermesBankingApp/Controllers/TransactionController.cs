using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.Services;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

public class TransactionController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ISavingsAccountService _savingsAccountService;
    private readonly ICreditCardService _creditCardService;
    private readonly ILoanService _loanService;

    public TransactionController(
        ITransactionService transactionService,
        IBeneficiaryService beneficiaryService,
        ISavingsAccountService savingsAccountService,
        ICreditCardService creditCardService,
        ILoanService loanService)
    {
        _transactionService = transactionService;
        _beneficiaryService = beneficiaryService;
        _savingsAccountService = savingsAccountService;
        _creditCardService = creditCardService;
        _loanService = loanService;
    }

    // Mostrar formulario para Transferencia Express
    [HttpGet]
    public IActionResult ExpressTransfer()
    {
        var model = new TransactionViewModel
        {
            Beneficiaries = _beneficiaryService.GetAllBeneficiaries(),
            SavingsAccounts = _savingsAccountService.GetAllActiveAccounts()
        };

        return View(model);
    }

    // Procesar Transferencia Express
    [HttpPost]
    public async Task<IActionResult> ExpressTransfer(TransactionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _transactionService.ProcessExpressTransfer(model);

        if (success)
            return RedirectToAction("TransactionSuccess");

        ModelState.AddModelError(string.Empty, "Error al procesar la transacción");
        return View(model);
    }

    // Otras acciones similares para cada tipo de transacción (Tarjeta de Crédito, Préstamo, Beneficiario)
}
