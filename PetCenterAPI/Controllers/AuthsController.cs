using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using PetCenterAPI.DTOs.Requests.Login;
using PetCenterAPI.DTOs.Responses;
using PetCenterAPI.DTOs.Responses.Login;
using PetCenterAPI.Security;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthsController : Controller
    {
        private readonly ICustomerAuthService _customerAuthService;
        // private readonly IGoogleAuthService _googleAuthService;
        private readonly IJwtService _jwtService;
        // private readonly IForgotPasswordService _forgotPasswordService;

        public AuthsController(
            ICustomerAuthService customerAuthService,
            // IGoogleAuthService googleAuthService,
            IJwtService jwtService
            // IForgotPasswordService forgotPasswordService
            )
        {
            _customerAuthService = customerAuthService;
            // _googleAuthService = googleAuthService;
            _jwtService = jwtService;
            //_forgotPasswordService = forgotPasswordService;
        }

        // ============================================================
        // LOGIN
        // ============================================================

        /// <summary>
        /// Authenticate a customer with email and password, return JWT on success
        /// </summary>
        [HttpPost("customer-login")]
        [AllowAnonymous]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, errorType, message) =
                await _customerAuthService.LoginAsync(request.Email, request.Password);

            if (!success)
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Success = false,
                    Message = message,
                    ErrorType = errorType,
                    Token = (string?)null
                });
            }

            return Ok(new LoginResponseDTO
            {
                Success = true,
                Message = "Login success",
                Token = token
            });
        }
    }
}
