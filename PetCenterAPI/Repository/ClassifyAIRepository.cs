using NuGet.Protocol.Core.Types;
using PetCenterAPI.DTOs.Requests.AI;
using PetCenterAPI.Repository.Interface;
using System.Net.Http.Headers;

namespace PetCenterAPI.Repository
{
    public class ClassifyAIRepository : IClassifyAIRepository
    {
        private readonly HttpClient _httpClient;

        public ClassifyAIRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AIRequestDTO?> PredictAsync(IFormFile image)
        {
            using var form = new MultipartFormDataContent();

            using var stream = image.OpenReadStream();

            var content = new StreamContent(stream);

            content.Headers.ContentType =
                new MediaTypeHeaderValue(image.ContentType);

            form.Add(content, "file", image.FileName);

            var response = await _httpClient.PostAsync("predict", form);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<AIRequestDTO>();
        }
    }
}
