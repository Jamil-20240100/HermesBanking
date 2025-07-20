using Asp.Versioning;
using HermesAPI.Controllers;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HermesAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize (Roles = "Admin, Commerce")]
    public class AccountController : BaseApiController
    {
        private readonly IAccountServiceForWebApi _accountServiceForWebApi;
        public AccountController(IAccountServiceForWebApi accountServiceForWebApi)
        {
            _accountServiceForWebApi = accountServiceForWebApi;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.UserName) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest("Username and password are required.");

            var response = await _accountServiceForWebApi.AuthenticateAsync(loginDto);

            if (response == null)
                return Unauthorized("Invalid credentials or account not confirmed.");

            return Ok(response);
        }


        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest("Token is required.");

            string? userId = User?.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token.");

            bool success = await _accountServiceForWebApi.ConfirmAccountAsync(userId, dto.Token);

            if (!success)
                return BadRequest("Invalid or expired token.");

            return NoContent();
        }

        [HttpPost("get-reset-token")]
        public async Task<IActionResult> GetResetToken([FromBody] ForgotPasswordWithTokenDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _accountServiceForWebApi.ForgotPasswordWithTokenAsync(request);

            if (response.HasError)
                return BadRequest(new { message = "Usuario inválido" });

            return NoContent();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                return BadRequest("Request body cannot be empty.");
            }

            var response = await _accountServiceForWebApi.ResetPasswordAsync(resetPasswordDto);

            if (response.HasError)
            {
                return BadRequest(new { Errors = response.Errors });
            }

            return NoContent();
        }
    }
}
