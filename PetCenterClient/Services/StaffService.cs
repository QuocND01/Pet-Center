using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Services
{
    public class StaffService : IStaffService
    {
        private readonly HttpClient _httpClient;
        public StaffService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<List<StaffDto>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<List<StaffDto>>("api/staff") ?? new();

        public async Task<StaffDto?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<StaffDto>($"api/staff/{id}");

        public async Task<bool> CreateAsync(StaffDto dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/staff", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Guid id, StaffDto dto)
        {
            var res = await _httpClient.PutAsJsonAsync($"api/staff/{id}", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
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