using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IGoogleClientService _googleClientService;

        public AuthController(IAuthService authService, IGoogleClientService googleClientService)
        {
            _authService = authService;
            _googleClientService = googleClientService;
        }

        public IActionResult Login()
        {
            var dto = _googleClientService.GetGoogleClientId();
            return View("~/Views/CustomerViews/Auth/Login.cshtml", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
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
                    ViewBag.Error = result?.message ?? "Email or password incorrect";
                }

                var model = _googleClientService.GetGoogleClientId();
                return View("~/Views/CustomerViews/Auth/Login.cshtml", model);
            }

            HttpContext.Session.SetString("JWT", result.token);

            // ✅ Decode JWT để lấy CustomerId lưu vào Session
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(result.token);

                // Lấy CustomerId từ claim "sub" hoặc "nameid"
                var customerId = jwt.Claims
                    .FirstOrDefault(c =>
                        c.Type == "sub" ||
                        c.Type == "nameid" ||
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    ?.Value ?? "";

                if (!string.IsNullOrEmpty(customerId))
                    HttpContext.Session.SetString("CustomerId", customerId);
            }
            catch
            {
                // Nếu decode lỗi thì vẫn login bình thường, chỉ không có CustomerId
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

        public IActionResult AdminLogin()
        {
            return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginDto dto, string selectedRole)
        {
            var result = await _authService.StaffLoginAsync(dto);
            if (result == null)
            {
                ViewBag.Error = "Email or password incorrect";
                return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
            }

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.token);
            var role = jwt.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value ?? "";
            var name = jwt.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                ?.Value ?? "";

            if (selectedRole == "Admin" && role != "Admin")
            {
                ViewBag.Error = "This account does not have Admin privileges";
                return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
            }

            if (selectedRole == "Staff" && role != "Staff")
            {
                ViewBag.Error = "This account does not have Staff privileges";
                return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
            }

            if (role != "Admin" && role != "Staff")
            {
                ViewBag.Error = "You do not have permission to access this area";
                return View("~/Views/AdminViews/Auth/AdminLogin.cshtml");
            }

            HttpContext.Session.SetString("JWT", result.token);
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("Name", name);

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

        // ================= REGISTER =================

        public IActionResult Register()
        {
            return View("~/Views/CustomerViews/Auth/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
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
                ViewBag.Error = result.Message;
                return View("~/Views/CustomerViews/Auth/Register.cshtml", dto);
            }

            HttpContext.Session.SetString("PendingEmail", dto.Email);
            TempData["PrefilledEmail"] = dto.Email;
            TempData["PrefilledPassword"] = dto.Password;

            return Json(new { success = true, redirectUrl = Url.Action("Verify", "Auth", new { email = dto.Email }) });
        }

        // ================= VERIFY EMAIL =================

        public IActionResult Verify(string email)
        {
            TempData.Keep("PrefilledEmail");
            TempData.Keep("PrefilledPassword");
            return View("~/Views/CustomerViews/Auth/Verify.cshtml", new VerifyEmailDto { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(VerifyEmailDto dto)
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

        // ================= RESEND OTP =================

        [HttpPost]
        public async Task<IActionResult> Resend(string email)
        {
            var result = await _authService.ResendOtpAsync(email);

            if (!result.Success)
                TempData["ResendError"] = result.Message;

            return RedirectToAction("Verify", new { email });
        }

        // POST: /Auth/GoogleLogin
        // Nhận idToken từ Google Identity Services JS SDK
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

            // Lưu JWT vào Session — nhất quán với Login thường
            HttpContext.Session.SetString("JWT", result.token!);

            // Decode JWT lấy thêm thông tin nếu cần
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
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleCallbackRequestDto dto)
        {
            var result = await _authService.GoogleCallbackAsync(dto.Code, dto.RedirectUri);

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

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Products")
            });
        }
    }


}
