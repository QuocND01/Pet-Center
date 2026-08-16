using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static PetCenterClient.ViewModels.PrescriptionItem.PrescriptionItemViewModel;

namespace PetCenterClient.Services
{
    public class PrescriptionItemAPIClient : IPrescriptionItemAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PrescriptionItemAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ReadPrescriptionItemViewModel>> GetByRecordAsync(Guid recordId)
        {
            return await _http.GetFromJsonAsync<IEnumerable<ReadPrescriptionItemViewModel>>(
                $"api/PrescriptionItems/record/{recordId}", _jsonOptions)
                ?? Enumerable.Empty<ReadPrescriptionItemViewModel>();
        }

        public async Task<ReadPrescriptionItemViewModel?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<ReadPrescriptionItemViewModel>(
                $"api/PrescriptionItems/{id}", _jsonOptions);
        }

        public async Task CreateAsync(CreatePrescriptionItemViewModel model)
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(new
            {
                model.RecordId,
                model.MedicineName,
                model.Dosage,
                model.Duration,
                model.Quantity,
                model.Note
            });
            var response = await _http.PostAsync("api/PrescriptionItems",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await GetErrorMessageAsync(response));
        }

        public async Task UpdateAsync(Guid id, UpdatePrescriptionItemViewModel model)
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(new
            {
                model.MedicineName,
                model.Dosage,
                model.Duration,
                model.Quantity,
                model.Note
            });
            var response = await _http.PutAsync($"api/PrescriptionItems/{id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await GetErrorMessageAsync(response));
        }

        public async Task DeleteAsync(Guid id)
        {
            AddAuthorizationHeader();
            var response = await _http.DeleteAsync($"api/PrescriptionItems/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception(await GetErrorMessageAsync(response));
        }

        private static async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return $"An error occurred (Status {response.StatusCode})";

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                    {
                        var msg = msgProp.GetString();
                        if (!string.IsNullOrWhiteSpace(msg)) return msg;
                    }
                    if (doc.RootElement.TryGetProperty("Message", out var msgPropPascal) && msgPropPascal.ValueKind == JsonValueKind.String)
                    {
                        var msg = msgPropPascal.GetString();
                        if (!string.IsNullOrWhiteSpace(msg)) return msg;
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON, fall back to returning content
            }

            return content;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
