using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Login;
using PetCenterClient.ViewModels.Register;

namespace PetCenterClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authService;
        private readonly IGoogleAPIClient _googleClientService;

        public AuthController(IAuthApiService authService, IGoogleAPIClient googleClientService)
        {
            _authService = authService;
            _googleClientService = googleClientService;
        }

        // ============================================================
        // LOGIN
        // ============================================================
        public IActionResult Login()
        {
            var dto = _googleClientService.GetGoogleClientId();
            return View("~/Views/CustomerViews/Auth/Login.cshtml", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null || !result.Success)
            {
                if (result?.ErrorType == "AccountInactive")
                {
                    ViewBag.Error = "Your account has been deactivated. Please contact support for assistance.";
                }
                else
                {
                    ViewBag.Error = result?.Message ?? "Email or password incorrect";
                }

                var model = _googleClientService.GetGoogleClientId();
                return View("~/Views/CustomerViews/Auth/Login.cshtml", model);
            }

            HttpContext.Session.SetString("JWT", result.Token);

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(result.Token);

                // Lấy CustomerId từ claim "sub" hoặc "nameid"
                var customerId = jwt.Claims
                    .FirstOrDefault(c =>
                        c.Type == "sub" ||
                        c.Type == "nameid" ||
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    ?.Value ?? "";

                if (!string.IsNullOrEmpty(customerId))
                    HttpContext.Session.SetString("CustomerId", customerId);

                var fullName = jwt.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value ?? "";
                HttpContext.Session.SetString("FullName", fullName);

                // Customers are not linked to the Roles table; mark them as "Customer"
                // (used client-side by pages such as Orders).
                HttpContext.Session.SetString("Role", "Customer");
            }
            catch
            {
            }

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Products");
        }

        [HttpGet]
        public IActionResult CheckAuth()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Json(new { isAuthenticated = false });

            return Json(new { isAuthenticated = true, token = token });
        }

        // ============================================================
        // LOGIN — STAFF / ADMIN
        // ============================================================
        public IActionResult AdminLogin()
        {
            return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(StaffLoginViewModel dto, string selectedRole)
        {
            var result = await _authService.StaffLoginAsync(dto);

            if (result == null || !result.Success || string.IsNullOrEmpty(result.Token))
            {
                ViewBag.Error = result?.ErrorType switch
                {
                    "AccountInactive" => "Your account has been deactivated. Please contact admin.",
                    "NoPermission" => "This account does not have permission to access the system.",
                    _ => result?.Message ?? "Email or password incorrect"
                };
                return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
            }

            var roles = result.Roles ?? new List<string>();
            var primaryRole = result.PrimaryRole ?? "";

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            var staffId = jwt.Claims
                .FirstOrDefault(c =>
                    c.Type == "sub" ||
                    c.Type == "nameid" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?.Value ?? "";

            var fullName = jwt.Claims
                .FirstOrDefault(c => c.Type == "fullName")
                ?.Value ?? "";

            if (selectedRole == "Admin")
            {
                if (!roles.Contains("Admin"))
                {
                    ViewBag.Error = "This account does not have Admin privileges.";
                    return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
                }
            }
            else if (selectedRole == "Staff")
            {
                var staffRoles = new[] { "Sale Staff", "Inventory Staff", "Vet" };
                if (!roles.Any(r => staffRoles.Contains(r)))
                {
                    ViewBag.Error = "This account does not have Staff privileges.";
                    return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
                }

                if (roles.Contains("Admin") && !roles.Any(r => staffRoles.Contains(r)))
                {
                    ViewBag.Error = "Please use the Admin tab to sign in.";
                    return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
                }
            }

            HttpContext.Session.SetString("JWT", result.Token);
            HttpContext.Session.SetString("Role", primaryRole);
            HttpContext.Session.SetString("Name", fullName);

            if (!string.IsNullOrEmpty(staffId))
                HttpContext.Session.SetString("StaffId", staffId);

            return RedirectToAction("Indexadmin", "Products");
        }

        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("AdminLogin");
        }

        [HttpGet]
        public IActionResult CheckAdminAuth()
        {
            var token = HttpContext.Session.GetString("JWT");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(token) || (role != "Admin" && role != "Staff"))
                return Json(new { isAuthenticated = false });

            return Json(new { isAuthenticated = true, role });
        }

        // ============================================================
        // REGISTER
        // ============================================================
        public IActionResult Register()
        {
            return View("~/Views/CustomerViews/Auth/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            HttpContext.Session.SetString("PendingEmail", dto.Email);
            TempData["PrefilledEmail"] = dto.Email;
            TempData["PrefilledPassword"] = dto.Password;

            return Json(new { success = true, redirectUrl = Url.Action("Verify", "Auth", new { email = dto.Email }) });
        }

        // ============================================================
        // OTP — VERIFY
        // ============================================================
        public IActionResult Verify(string email)
        {
            TempData.Keep("PrefilledEmail");
            TempData.Keep("PrefilledPassword");
            return View("~/Views/CustomerViews/Auth/Verify.cshtml", new VerifyEmailViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(VerifyEmailViewModel dto)
        {
            var result = await _authService.VerifyOtpAsync(dto.Email, dto.Code);

            if (!result.Success)
            {
                var attempts = HttpContext.Session.GetInt32("OtpAttempts_" + dto.Email) ?? 0;
                attempts++;
                HttpContext.Session.SetInt32("OtpAttempts_" + dto.Email, attempts);

                ViewBag.AttemptCount = attempts;
                ViewBag.Error = result.Message;
                return View("~/Views/CustomerViews/Auth/Verify.cshtml", dto);
            }

            HttpContext.Session.Remove("OtpAttempts_" + dto.Email);
            HttpContext.Session.Remove("PendingEmail");

            TempData["Success"] = "Email verified successfully. You can now login.";
            return RedirectToAction("Login");
        }

        // ============================================================
        // OTP — RESEND
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Resend(string email)
        {
            var result = await _authService.ResendOtpAsync(email);

            if (!result.Success)
                TempData["ResendError"] = result.Message;

            return RedirectToAction("Verify", new { email });
        }

        // ============================================================
        // GOOGLE LOGIN
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto?.IdToken))
                return Json(new { success = false, message = "Invalid request." });

            var result = await _authService.GoogleLoginAsync(dto.IdToken);

            if (result == null || !result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result?.message ?? "Google login failed.",
                    errorType = result?.ErrorType ?? "GoogleLoginFailed"
                });
            }

            HttpContext.Session.SetString("JWT", result.token!);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.token);
            var role = jwt.Claims
                .FirstOrDefault(c => c.Type ==
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value ?? "Customer";
            var name = jwt.Claims
                .FirstOrDefault(c => c.Type ==
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                ?.Value ?? "";

            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("Name", name);

            var customerId = jwt.Claims
                .FirstOrDefault(c =>
                    c.Type == "sub" ||
                    c.Type == "nameid" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?.Value ?? "";

            if (!string.IsNullOrEmpty(customerId))
                HttpContext.Session.SetString("CustomerId", customerId);

            return Json(new
            {
                success = true,
                message = "Google login successful!",
                redirectUrl = Url.Action("Index", "Products")
            });
        }

        // GET: /Auth/GoogleCallback — nhận code từ Google redirect
        [HttpGet]
        public IActionResult GoogleCallback()
        {
            return View("~/Views/CustomerViews/Auth/GoogleCallback.cshtml");
        }

        // POST: /Auth/GoogleCallback — frontend gọi sau khi lấy được code
        [HttpPost]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleCallbackViewModel dto)
        {
            var result = await _authService.GoogleCallbackAsync(dto.Code, dto.RedirectUri);

            if (result == null || !result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result?.Message ?? "Google login failed.",
                    errorType = result?.ErrorType ?? "GoogleLoginFailed"
                });
            }

            HttpContext.Session.SetString("JWT", result.Token!);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);
            var role = jwt.Claims
                .FirstOrDefault(c => c.Type ==
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value ?? "Customer";
            var name = jwt.Claims
                .FirstOrDefault(c => c.Type ==
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                ?.Value ?? "";

            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("Name", name);
            HttpContext.Session.SetString("FullName",                                  
    jwt.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value ?? "");

            var customerId = jwt.Claims
    .FirstOrDefault(c =>
        c.Type == "sub" ||
        c.Type == "nameid" ||
        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
    ?.Value ?? "";

            if (!string.IsNullOrEmpty(customerId))
                HttpContext.Session.SetString("CustomerId", customerId);

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Products")
            });
        }

        // ============================================================
        // FORGOT PASSWORD — SEND RESET LINK
        // ============================================================

        // GET: /Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View("~/Views/CustomerViews/Auth/ForgotPassword.cshtml");
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Please enter your email address." });

            var result = await _authService.ForgotPasswordAsync(email);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new { success = true, message = result.Message });
        }

        // ============================================================
        // FORGOT PASSWORD — VALIDATE TOKEN & DISPLAY RESET FORM
        // ============================================================

        // GET: /Auth/ResetPassword?email=...&token=...
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction("ForgotPassword");

            var validation = await _authService.ValidateResetTokenAsync(email, token);

            ViewBag.Email = email;
            ViewBag.Token = token;
            ViewBag.TokenValid = validation.Success;
            ViewBag.TokenError = validation.Success ? null : validation.Message;

            return View("~/Views/CustomerViews/Auth/ResetPassword.cshtml");
        }

        // ============================================================
        // FORGOT PASSWORD — SUBMIT NEW PASSWORD
        // ============================================================

        // POST: /Auth/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel dto)
        {
            if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword != dto.ConfirmPassword)
                return Json(new { success = false, message = "Passwords do not match." });

            if (dto.NewPassword.Length < 8)
                return Json(new { success = false, message = "Password must be at least 8 characters." });

            var result = await _authService.ResetPasswordAsync(dto);
            return Json(new { success = result.Success, message = result.Message });
        }
    }


}
