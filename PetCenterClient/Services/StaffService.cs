using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class StaffService : IStaffService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public StaffService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            _httpClient.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
        }

        // ============================================================
        // READ
        // ============================================================
        public async Task<List<StaffListItemDto>> GetAllAsync()
        {
            AddAuthHeader();
            return await _httpClient.GetFromJsonAsync<List<StaffListItemDto>>("api/staff")
                   ?? new List<StaffListItemDto>();
        }

        public async Task<StaffDetailDto?> GetByIdAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/staff/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StaffDetailDto>(json, JsonOpts);
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            AddAuthHeader();
            return await _httpClient.GetFromJsonAsync<List<RoleDto>>("api/staff/roles")
                   ?? new List<RoleDto>();
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<(bool Success, string Message)> CreateAsync(CreateStaffDto dto)
        {
            AddAuthHeader();

            using var form = new MultipartFormDataContent
            {
                { new StringContent(dto.FullName), "FullName" },
                { new StringContent(dto.Email), "Email" },
                { new StringContent(dto.PhoneNumber), "PhoneNumber" },
                { new StringContent(dto.Gender), "Gender" },
                { new StringContent(dto.RoleId.ToString()), "RoleId" },
                { new StringContent(dto.Password), "Password" }
            };

            AddDate(form, "BirthDate", dto.BirthDate);
            AddDate(form, "HireDate", dto.HireDate);

            if (!string.IsNullOrWhiteSpace(dto.LicenseNumber))
                form.Add(new StringContent(dto.LicenseNumber), "LicenseNumber");
            if (!string.IsNullOrWhiteSpace(dto.Description))
                form.Add(new StringContent(dto.Description), "Description");
            if (dto.ExperienceYears.HasValue)
                form.Add(new StringContent(dto.ExperienceYears.Value.ToString(CultureInfo.InvariantCulture)),
                    "ExperienceYears");

            AddFile(form, "Avatar", dto.Avatar);

            var response = await _httpClient.PostAsync("api/staff", form);
            return await ReadResultAsync(response, "Staff created successfully.");
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, UpdateStaffDto dto)
        {
            AddAuthHeader();

            using var form = new MultipartFormDataContent
            {
                { new StringContent(dto.FullName), "FullName" },
                { new StringContent(dto.Email), "Email" },
                { new StringContent(dto.PhoneNumber), "PhoneNumber" },
                { new StringContent(dto.Gender), "Gender" },
                { new StringContent(dto.RoleId.ToString()), "RoleId" },
                { new StringContent(dto.ResetPassword.ToString()), "ResetPassword" }
            };

            AddDate(form, "BirthDate", dto.BirthDate);
            AddDate(form, "HireDate", dto.HireDate);

            if (!string.IsNullOrWhiteSpace(dto.Description))
                form.Add(new StringContent(dto.Description), "Description");

            AddFile(form, "Avatar", dto.Avatar);

            var response = await _httpClient.PutAsync($"api/staff/{id}", form);
            return await ReadResultAsync(response, "Staff updated successfully.");
        }

        // ============================================================
        // DELETE (soft)
        // ============================================================
        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/staff/{id}");
            return await ReadResultAsync(response, "Staff deactivated successfully.");
        }

        // ============================================================
        // OTHER MODULES
        // ============================================================
        public async Task<List<StaffNameListDto>> GetStaffNameListAsync()
        {
            var staffs = await GetAllAsync();
            return staffs.Select(s => new StaffNameListDto
            {
                StaffId = s.StaffId,
                StaffName = s.FullName
            }).ToList();
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private static void AddDate(MultipartFormDataContent form, string name, DateTime? value)
        {
            if (value.HasValue)
                form.Add(new StringContent(value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)), name);
        }

        private static void AddFile(MultipartFormDataContent form, string name, IFormFile? file)
        {
            if (file == null || file.Length == 0) return;

            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(streamContent, name, file.FileName);
        }

        private static async Task<(bool Success, string Message)> ReadResultAsync(
            HttpResponseMessage response, string defaultSuccessMessage)
        {
            var body = await response.Content.ReadAsStringAsync();
            var message = ExtractMessage(body);

            if (response.IsSuccessStatusCode)
                return (true, string.IsNullOrWhiteSpace(message) ? defaultSuccessMessage : message);

            return (false, string.IsNullOrWhiteSpace(message)
                ? "An error occurred while contacting the server."
                : message);
        }

        private static string? ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("message", out var msg))
                {
                    return msg.ValueKind == JsonValueKind.String ? msg.GetString() : msg.ToString();
                }
            }
            catch (JsonException)
            {
                // body is not JSON; ignore
            }
            return null;
        }
    }
}
