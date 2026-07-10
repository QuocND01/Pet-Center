using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Controllers
{
    public class PetsController : Controller
    {
        private readonly IPetAPIClient _apiClient;

        public PetsController(IPetAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            if (HttpContext.Session.GetString("Role") != "Customer") return RedirectToAction("Index", "Home");

            // Xử lý Search với chuẩn OData
            string odataQuery = "";
            if (!string.IsNullOrEmpty(search))
            {
                // OData query: Tìm trong Breed HOẶC Species
                odataQuery = $"?$filter=contains(tolower(Breed), '{search.ToLower()}') or contains(tolower(Species), '{search.ToLower()}')";
            }

            var pets = await _apiClient.GetMyPetsAsync(odataQuery);
            ViewBag.SearchKeyword = search; // Giữ lại từ khóa trên ô tìm kiếm

            return View("~/Views/CustomerViews/Pets/Index.cshtml", pets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsAsync(id);
            if (pet == null) return Content("<div class='text-danger p-3'>Pet not found.</div>");
            return PartialView("~/Views/CustomerViews/Pets/_PetDetailsPartial.cshtml", pet);
        }

        // Return pet JSON (server-side proxy) so browser JS does not need to call protected API directly
        [HttpGet]
        public async Task<IActionResult> GetJson(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsAsync(id);
            if (pet == null) return NotFound();
            return Json(pet);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] MutatePetViewModel dto)
        {
            // Debug: log incoming form keys and bound DTO values to help diagnose mapping issues
            try
            {
                var form = Request.HasFormContentType ? Request.Form : null;
                if (form != null)
                {
                    Console.WriteLine("[PetsController.Add] Received form keys:");
                    foreach (var k in form.Keys)
                    {
                        var v = form[k];
                        Console.WriteLine($"  {k} = {v}");
                    }
                }
                Console.WriteLine($"[PetsController.Add] Bound DTO: PetName='{dto.PetName}', Breed='{dto.Breed}', Species='{dto.Species}', Gender='{dto.Gender}', Weight='{dto.Weight}', DateOfBirth='{dto.DateOfBirth}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PetsController.Add] Logging error: {ex.Message}");
            }

            var success = await _apiClient.AddPetAsync(dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromForm] MutatePetViewModel dto)
        {
            var success = await _apiClient.UpdatePetAsync(id, dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeletePetAsync(id);
            return Json(new { success });
        }
    }
}