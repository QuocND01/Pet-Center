using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.AI;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class AIClassifyService : IAIClassifyService
    {
        private readonly HttpClient _httpClient;

        public AIClassifyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AIResultViewModel?> ClassifyAsync(IFormFile image)
        {
            using var form = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();

            var content = new StreamContent(stream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(image.ContentType);

            form.Add(content, "file", image.FileName);

            var response = await _httpClient.PostAsync("http://127.0.0.1:5000/predict", form);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AIResultViewModel>();
        }
    }
}
