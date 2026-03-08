using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Login()
        {
            return View("~/Views/CustomerViews/Auth/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                ViewBag.Error = "Email or password incorrect";
                return View("~/Views/CustomerViews/Auth/Login.cshtml");
            }

            HttpContext.Session.SetString("JWT", result.token);

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

            // Decode JWT lấy role
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.token);
            var role = jwt.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value ?? "";
            var name = jwt.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                ?.Value ?? "";

            // Kiểm tra role có khớp với tab đang chọn không
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

            // Nếu không phải Admin cũng không phải Staff thì chặn
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
    }
}
