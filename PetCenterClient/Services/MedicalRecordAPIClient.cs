using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static PetCenterClient.ViewModels.MedicalRecord.MedicalRecordViewModel;

namespace PetCenterClient.Services
{
    public class MedicalRecordAPIClient : IMedicalRecordAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MedicalRecordAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedResponse<ReadMedicalRecordListViewModel>> GetAllAdminAsync(
            string? search, int? status, int page, int pageSize = 10)
        {
            var query = new List<string> { $"page={page}", $"pageSize={pageSize}" };
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            if (status.HasValue) query.Add($"status={status.Value}");

            var url = "api/MedicalRecords/admin?" + string.Join("&", query);
            return await _http.GetFromJsonAsync<PagedResponse<ReadMedicalRecordListViewModel>>(url, _jsonOptions)
                ?? new PagedResponse<ReadMedicalRecordListViewModel>();
        }

        public async Task<IEnumerable<ReadMedicalRecordListViewModel>> GetByCustomerAsync(
            Guid customerId, string? search)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");

            var url = $"api/MedicalRecords/customer/{customerId}";
            if (query.Any()) url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<IEnumerable<ReadMedicalRecordListViewModel>>(url, _jsonOptions)
                ?? Enumerable.Empty<ReadMedicalRecordListViewModel>();
        }

        public async Task<ReadMedicalRecordDetailViewModel?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<ReadMedicalRecordDetailViewModel>(
                $"api/MedicalRecords/{id}", _jsonOptions);
        }

        public async Task<IEnumerable<CompletedAppointmentViewModel>> GetCompletedAppointmentsAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<CompletedAppointmentViewModel>>(
                "api/MedicalRecords/completed-appointments", _jsonOptions)
                ?? Enumerable.Empty<CompletedAppointmentViewModel>();
        }

        public async Task CreateAsync(CreateMedicalRecordViewModel model)
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(new
            {
                model.AppointmentId,
                model.DiseaseId,
                model.CustomDiseaseName,
                model.Diagnosis,
                model.Treatment,
                model.Note
            });
            var response = await _http.PostAsync("api/MedicalRecords",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task UpdateAsync(Guid id, UpdateMedicalRecordViewModel model)
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(new
            {
                model.DiseaseId,
                model.CustomDiseaseName,
                model.Diagnosis,
                model.Treatment,
                model.Note
            });
            var response = await _http.PutAsync($"api/MedicalRecords/{id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task ChangeStatusAsync(Guid id, int status)
        {
            AddAuthorizationHeader();
            var json = JsonSerializer.Serialize(status);
            var response = await _http.PatchAsync($"api/MedicalRecords/{id}/status",
                new StringContent(json, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<ReadDiseaseViewModel>> GetDiseasesAsync(int? species)
        {
            var url = "api/MedicalRecords/diseases";
            if (species.HasValue) url += $"?species={species.Value}";
            return await _http.GetFromJsonAsync<IEnumerable<ReadDiseaseViewModel>>(url, _jsonOptions)
                ?? Enumerable.Empty<ReadDiseaseViewModel>();
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
