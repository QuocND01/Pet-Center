using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Controllers
{
    public class DiseasesController : Controller
    {
        private readonly IDiseaseAPIClient _apiClient;

        public DiseasesController(IDiseaseAPIClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index(string search = "", int? species = null)
        {
            // Chỉ Vet/Admin được vào
            var role = HttpContext.Session.GetString("Role");
            if (role != "Vet" && role != "Admin") return RedirectToAction("AdminLogin", "Auth");

            // Tạo một list chứa các điều kiện lọc
            var filterConditions = new List<string>();

            if (!string.IsNullOrEmpty(search))
            {
                filterConditions.Add($"contains(tolower(Name), '{search.ToLower()}')");
            }

            if (species.HasValue && species.Value > 0)
            {
                filterConditions.Add($"Species eq {species.Value}");
            }

            // Gộp các điều kiện lại bằng chữ " and " chuẩn OData
            string odataQuery = "";
            if (filterConditions.Any())
            {
                odataQuery = "?$filter=" + string.Join(" and ", filterConditions);
            }

            var diseases = await _apiClient.GetAllDiseasesAsync(odataQuery);

            // Giữ lại trạng thái trên UI
            ViewBag.SearchKeyword = search;
            ViewBag.SelectedSpecies = species;

            return View("~/Views/AdminViews/Diseases/Index.cshtml", diseases);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var disease = await _apiClient.GetDiseaseDetailsAsync(id);
            if (disease == null) return Content("<div class='text-danger p-3'>Disease not found.</div>");
            return PartialView("~/Views/AdminViews/Diseases/_DiseaseDetailsPartial.cshtml", disease);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MutateDiseaseViewModel dto)
        {
            var result = await _apiClient.AddDiseaseAsync(dto);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromBody] MutateDiseaseViewModel dto)
        {
            var result = await _apiClient.UpdateDiseaseAsync(id, dto);
            return Json(new { success = result.success, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _apiClient.DeleteDiseaseAsync(id);
            return Json(new { success = result.success, message = result.message });
        }
    }
}