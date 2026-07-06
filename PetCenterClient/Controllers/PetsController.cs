using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class PetsController : Controller
    {
        private readonly IPetAPIClient _apiClient;

        public PetsController(IPetAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Customer") return RedirectToAction("Index", "Home");
            var pets = await _apiClient.GetMyPetsAsync();
            return View("~/Views/CustomerViews/Pets/Index.cshtml", pets);
        }

        // Render Partial View cho Modal
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsAsync(id);
            if (pet == null) return Content("<div class='text-danger p-3'>Pet details not found.</div>");
            return PartialView("~/Views/CustomerViews/Pets/_PetDetailsPartial.cshtml", pet);
        }
    }
}