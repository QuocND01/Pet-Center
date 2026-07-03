using Microsoft.AspNetCore.Mvc;

namespace PetCenterClient.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult ServiceUnavailable(string? message)
        {
            ViewBag.Message = message;
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
