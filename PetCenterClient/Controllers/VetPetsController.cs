using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class VetPetsController : Controller
    {
        private readonly IPetAPIClient _apiClient;

        public VetPetsController(IPetAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Vet" && role != "Admin") return RedirectToAction("AdminLogin", "Auth");

            var pets = await _apiClient.GetAllPetsForVetAsync();
            return View("~/Views/AdminViews/Pets/Index.cshtml", pets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsForVetAsync(id);
            if (pet == null) return Content("<div class='text-danger p-3'>Pet details not found.</div>");
            return PartialView("~/Views/AdminViews/Pets/_PetDetailsPartial.cshtml", pet);
        }
    }
}