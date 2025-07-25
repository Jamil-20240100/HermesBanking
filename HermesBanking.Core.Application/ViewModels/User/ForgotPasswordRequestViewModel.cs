﻿using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.User
{
    public class ForgotPasswordRequestViewModel
    {
        [Required(ErrorMessage = "You must enter the username of user")]
        [DataType(DataType.Text)]
        public required string UserName { get; set; }
    }
}
