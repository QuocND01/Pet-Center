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
            if (role != "Vet" && role != "Admin" && role != "Groomer") return RedirectToAction("AdminLogin", "Auth");

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

        // Return pet JSON (server-side proxy) so browser JS does not need to call protected API directly
        [HttpGet]
        public async Task<IActionResult> GetJson(Guid id)
        {
            var pet = await _apiClient.GetPetDetailsForVetAsync(id);
            if (pet == null) return NotFound();
            return Json(pet);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid customerId, [FromForm] MutatePetViewModel dto)
        {
            try
            {
                var form = Request.HasFormContentType ? Request.Form : null;
                if (form != null)
                {
                    Console.WriteLine("[VetPetsController.Add] Received form keys:");
                    foreach (var k in form.Keys)
                    {
                        var v = form[k];
                        Console.WriteLine($"  {k} = {v}");
                    }
                }
                Console.WriteLine($"[VetPetsController.Add] Bound DTO: PetName='{dto.PetName}', Breed='{dto.Breed}', Species='{dto.Species}', Gender='{dto.Gender}', Weight='{dto.Weight}', DateOfBirth='{dto.DateOfBirth}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VetPetsController.Add] Logging error: {ex.Message}");
            }

            var res = await _apiClient.AddPetForVetAsync(customerId, dto);
            if (res.IsSuccessStatusCode) return Json(new { success = true });

            var raw = await res.Content.ReadAsStringAsync();
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("errors", out var errs))
                {
                    var dict = new Dictionary<string, string[]>();
                    foreach (var prop in errs.EnumerateObject())
                    {
                        var arr = prop.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
                        dict[prop.Name] = arr;
                    }
                    return Json(new { success = false, errors = dict });
                }
                if (doc.RootElement.TryGetProperty("message", out var msg))
                {
                    return Json(new { success = false, message = msg.GetString() });
                }
            }
            catch { }
            return Json(new { success = false, message = raw });
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromForm] MutatePetViewModel dto)
        {
            var res = await _apiClient.UpdatePetForVetAsync(id, dto);
            if (res.IsSuccessStatusCode) return Json(new { success = true });

            var raw = await res.Content.ReadAsStringAsync();
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("errors", out var errs))
                {
                    var dict = new Dictionary<string, string[]>();
                    foreach (var prop in errs.EnumerateObject())
                    {
                        var arr = prop.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();
                        dict[prop.Name] = arr;
                    }
                    return Json(new { success = false, errors = dict });
                }
                if (doc.RootElement.TryGetProperty("message", out var msg))
                {
                    return Json(new { success = false, message = msg.GetString() });
                }
            }
            catch { }
            return Json(new { success = false, message = raw });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeletePetForVetAsync(id);
            return Json(new { success });
        }
    }
}