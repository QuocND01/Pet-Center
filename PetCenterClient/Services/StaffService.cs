using System.Text.Json;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;
using System.Net.Http.Headers;

namespace PetCenterClient.Services
{
    public class StaffService : IStaffService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaffService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<OdataResponse<StaffDto>> GetAllODataAsync(
            string? search = null,
            bool? isActive = null,
            string? sortBy = null,
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            AddAuthHeader();

            // Gọi API thường
            var all = await _httpClient.GetFromJsonAsync<List<StaffDto>>("api/staff")
                      ?? new List<StaffDto>();

            // Filter
            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                all = all.Where(x =>
                    x.FullName.ToLower().Contains(s) ||
                    (x.Email?.ToLower().Contains(s) ?? false) ||
                    x.PhoneNumber.Contains(s)
                ).ToList();
            }

            if (isActive.HasValue)
                all = all.Where(x => x.IsActive == isActive.Value).ToList();

            // Sort
            all = sortBy switch
            {
                "name" => sortOrder == "desc"
                            ? all.OrderByDescending(x => x.FullName).ToList()
                            : all.OrderBy(x => x.FullName).ToList(),
                "email" => sortOrder == "desc"
                            ? all.OrderByDescending(x => x.Email).ToList()
                            : all.OrderBy(x => x.Email).ToList(),
                _ => all.OrderBy(x => x.FullName).ToList()
            };

            int total = all.Count;

            // Pagination
            var paged = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new OdataResponse<StaffDto>
            {
                Count = total,
                Values = paged
            };
        }

        public async Task<List<StaffDto>> GetAllAsync()
        {
            AddAuthHeader();
            return await _httpClient.GetFromJsonAsync<List<StaffDto>>("api/staff") ?? new();
        }

        public async Task<StaffDto?> GetByIdAsync(Guid id)
        {
            AddAuthHeader();
            return await _httpClient.GetFromJsonAsync<StaffDto>($"api/staff/{id}");
        }

        public async Task<bool> CreateAsync(StaffDto dto)
        {
            AddAuthHeader();
            var res = await _httpClient.PostAsJsonAsync("api/staff", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, StaffDto dto)
        {
            AddAuthHeader();
            var res = await _httpClient.PutAsJsonAsync($"api/staff/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            AddAuthHeader();
            var res = await _httpClient.DeleteAsync($"api/staff/{id}");
            return res.IsSuccessStatusCode;
        }
        public async Task<List<StaffNameListDto>> GetStaffNameListAsync()
        {

            var staffs = await GetAllAsync();

            return staffs.Select(s => new StaffNameListDto
            {
                StaffId = s.StaffId,
                StaffName = s.FullName,
            }).ToList();
        }
    }
}