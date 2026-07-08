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

        public async Task<IActionResult> Index(string search = "")
        {
            // Chỉ Vet/Admin được vào
            var role = HttpContext.Session.GetString("Role");
            if (role != "Vet" && role != "Admin") return RedirectToAction("AdminLogin", "Auth");

            string odataQuery = "";
            if (!string.IsNullOrEmpty(search))
            {
                // Tự động build chuỗi OData tìm theo tên bệnh
                odataQuery = $"?$filter=contains(tolower(Name), '{search.ToLower()}')";
            }

            var diseases = await _apiClient.GetAllDiseasesAsync(odataQuery);
            ViewBag.SearchKeyword = search;

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
            var success = await _apiClient.AddDiseaseAsync(dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, [FromBody] MutateDiseaseViewModel dto)
        {
            var success = await _apiClient.UpdateDiseaseAsync(id, dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeleteDiseaseAsync(id);
            return Json(new { success });
        }
    }
}