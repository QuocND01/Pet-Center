using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.AI;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class AIClassifyAPIClient : IAIClassifyAPIClient
    {
        private readonly HttpClient _httpClient;

        public AIClassifyAPIClient(HttpClient httpClient)
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
            var response = await _httpClient.PostAsync("predict", form);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AIResultViewModel>();
        }
    }
}
