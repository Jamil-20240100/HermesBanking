// src/HermesBanking.Core.Application/ViewModels/Loan/AssignLoanPageViewModel.cs
using HermesBanking.Core.Application.ViewModels.User; // Asegúrate de que el using apunte a tu UserViewModel
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario si decides usar SelectList en el futuro, pero lo incluimos por consistencia.

namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class AssignLoanPageViewModel
    {
        // Contendrá los datos del formulario de asignación de préstamo
        public AssignLoanViewModel? LoanData { get; set; }

        // Contendrá la lista de clientes para la selección en la vista
        public List<UserViewModel>? Clients { get; set; }

        // Constructor para asegurar que las listas no sean null por defecto
        public AssignLoanPageViewModel()
        {
            LoanData = new AssignLoanViewModel();
            Clients = new List<UserViewModel>();
        }
    }
}