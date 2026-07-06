using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Controllers
{
    public class VetPetsController : Controller
    {
        private readonly IPetAPIClient _apiClient;

        public VetPetsController(IPetAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Vet" && role != "Admin") return RedirectToAction("AdminLogin", "Auth");

            string odataQuery = "";
            if (!string.IsNullOrEmpty(search))
            {
                // OData query: Tìm trong Breed, Species HOẶC OwnerName
                odataQuery = $"?$filter=contains(tolower(Breed), '{search.ToLower()}') or contains(tolower(Species), '{search.ToLower()}') or contains(tolower(OwnerName), '{search.ToLower()}')";
            }

            var pets = await _apiClient.GetAllPetsForVetAsync(odataQuery);
            ViewBag.SearchKeyword = search; // Giữ lại từ khóa trên ô tìm kiếm

            return View("~/Views/AdminViews/Pets/Index.cshtml", pets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsForVetAsync(id);
            if (pet == null) return Content("<div class='text-danger p-3'>Pet details not found.</div>");
            return PartialView("~/Views/AdminViews/Pets/_PetDetailsPartial.cshtml", pet);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid customerId, [FromBody] MutatePetViewModel dto)
        {
            var success = await _apiClient.AddPetForVetAsync(customerId, dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromBody] MutatePetViewModel dto)
        {
            var success = await _apiClient.UpdatePetForVetAsync(id, dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeletePetForVetAsync(id);
            return Json(new { success });
        }
    }
}