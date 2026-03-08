using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetCenterClient.DTOs;
using System.Text;

namespace PetCenterClient.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly HttpClient _httpClient;

        public FeedbackController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:5001/api/");
        }

        public async Task<IActionResult> AdminList()
        {
            var res = await _httpClient.GetAsync("feedback");
            var json = await res.Content.ReadAsStringAsync();

            var feedbacks = JsonConvert.DeserializeObject<List<FeedbackDTO>>(json);

            return View("~/Views/AdminViews/Feedback/List.cshtml", feedbacks);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var res = await _httpClient.GetAsync($"feedback/{id}");
            var json = await res.Content.ReadAsStringAsync();

            var feedback = JsonConvert.DeserializeObject<FeedbackDTO>(json);

            return View("~/Views/AdminViews/Feedback/Detail.cshtml", feedback);
        }

        public async Task<IActionResult> MyFeedback(Guid customerId)
        {
            var res = await _httpClient.GetAsync($"feedback/customer/{customerId}");
            var json = await res.Content.ReadAsStringAsync();

            var feedbacks = JsonConvert.DeserializeObject<List<FeedbackDTO>>(json);

            return View("~/Views/CustomerViews/Feedback/MyFeedback.cshtml", feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(ReplyFeedbackDTO dto)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync("feedback/reply", content);

            return RedirectToAction("AdminList");
        }

        public async Task<IActionResult> Toggle(Guid id)
        {
            await _httpClient.PutAsync($"feedback/toggle/{id}", null);

            return RedirectToAction("AdminList");
        }
    }
}