using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using PetCenterClient.DTOs;
namespace PetCenterClient.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly HttpClient _httpClient;

        public SuppliersController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:5000"); // port gateway
        }

        public async Task<IActionResult> Index()
        {
            var res = await _httpClient.GetAsync("/inventory/suppliers");
            var data = await res.Content.ReadAsStringAsync();

            var suppliers = JsonSerializer.Deserialize<List<ReadSupplierDto>>(data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View("~/Views/AdminViews/Supplier/Index.cshtml", suppliers);
        }

        public IActionResult Create()
        {
            return View("~/Views/AdminViews/Supplier/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Create.cshtml", dto);
            }

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync("/inventory/suppliers", content);

            if (res.IsSuccessStatusCode)
                return Json(new { success = true });

            ModelState.AddModelError("", "Error creating supplier");

            return PartialView("~/Views/AdminViews/Supplier/Create.cshtml", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var res = await _httpClient.GetAsync($"/inventory/suppliers/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var data = await res.Content.ReadAsStringAsync();
            Console.WriteLine(data);
            var supplier = JsonSerializer.Deserialize<ReadSupplierDto>(data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
            }

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _httpClient.PutAsync($"/inventory/suppliers/{dto.SupplierId}", content);

            if (res.IsSuccessStatusCode)
                return Json(new { success = true });

            ModelState.AddModelError("", "Error updating supplier");
            return PartialView("~/Views/AdminViews/Supplier/Edit.cshtml", dto);
        }

        [HttpPost] // Chuyển Delete sang HttpPost để gọi từ Ajax an toàn
        public async Task<IActionResult> Delete(Guid id)
        {
            var res = await _httpClient.DeleteAsync($"/inventory/suppliers/{id}");
            if (res.IsSuccessStatusCode) return Json(new { success = true });
            return BadRequest();
        }
    }
}
