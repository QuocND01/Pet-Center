using System.Net.Http.Headers;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Services
{
    public class PetAPIClient : IPetAPIClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PetAPIClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // ================= HÀM HỖ TRỢ ĐÓNG GÓI FORM-DATA =================
        private MultipartFormDataContent CreateMultipartContent(MutatePetViewModel dto)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Species ?? ""), "Species");
            content.Add(new StringContent(dto.Breed ?? ""), "Breed");
            content.Add(new StringContent(dto.Gender ?? ""), "Gender");
            // Pet name
            content.Add(new StringContent(dto.PetName ?? ""), "PetName");

            if (dto.Weight.HasValue)
                content.Add(new StringContent(dto.Weight.Value.ToString()), "Weight");

            if (dto.DateOfBirth.HasValue)
                content.Add(new StringContent(dto.DateOfBirth.Value.ToString("yyyy-MM-dd")), "DateOfBirth");

            if (!string.IsNullOrEmpty(dto.Note))
                content.Add(new StringContent(dto.Note), "Note");

            // Đính kèm File ảnh (nếu user có chọn file)
            if (dto.ImageFile != null)
            {
                var stream = dto.ImageFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(dto.ImageFile.ContentType);
                content.Add(fileContent, "ImageFile", dto.ImageFile.FileName);
            }

            return content;
        }

        // ================= CUSTOMER METHODS =================
        public async Task<List<ReadPetListViewModel>?> GetMyPetsAsync(string query = "")
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Pets/my-pets{query}");
            if (!res.IsSuccessStatusCode) return null;

            // OData endpoints often return an object with a 'value' array. Try to handle both shapes.
            var raw = await res.Content.ReadAsStringAsync();
            try
            {
                // Try OData wrapper: { "value": [ ... ] }
                var odata = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<ReadPetListViewModel>>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (odata != null && odata.Value != null) return odata.Value;
            }
            catch { /* ignore and try direct list */ }

            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<ReadPetListViewModel>>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return list;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ReadPetDetailViewModel?> GetPetDetailsAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/Pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadPetDetailViewModel>() : null;
        }

        public async Task<bool> AddPetAsync(MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            // Đã đổi sang PostAsync kèm FormData
            var res = await _http.PostAsync("api/Pets", CreateMultipartContent(dto));
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePetAsync(Guid id, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            // Đã đổi sang PutAsync kèm FormData
            var res = await _http.PutAsync($"api/Pets/{id}", CreateMultipartContent(dto));
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/Pets/{id}");
            return res.IsSuccessStatusCode;
        }

        // ================= VET METHODS =================
        public async Task<List<ReadVetPetListViewModel>?> GetAllPetsForVetAsync(string query = "")
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/vet/pets{query}");
            if (!res.IsSuccessStatusCode) return null;

            var raw = await res.Content.ReadAsStringAsync();
            try
            {
                var odata = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<ReadVetPetListViewModel>>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (odata != null && odata.Value != null) return odata.Value;
            }
            catch { }

            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<ReadVetPetListViewModel>>(raw, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return list;
            }
            catch { return null; }
        }

        public async Task<ReadVetPetDetailViewModel?> GetPetDetailsForVetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.GetAsync($"api/vet/pets/{id}");
            return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ReadVetPetDetailViewModel>() : null;
        }

        public async Task<bool> AddPetForVetAsync(Guid customerId, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            // Đã đổi sang PostAsync kèm FormData
            var res = await _http.PostAsync($"api/vet/pets/add-for-customer/{customerId}", CreateMultipartContent(dto));
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePetForVetAsync(Guid id, MutatePetViewModel dto)
        {
            AddAuthorizationHeader();
            // Đã đổi sang PutAsync kèm FormData
            var res = await _http.PutAsync($"api/vet/pets/{id}", CreateMultipartContent(dto));
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePetForVetAsync(Guid id)
        {
            AddAuthorizationHeader();
            var res = await _http.DeleteAsync($"api/vet/pets/{id}");
            return res.IsSuccessStatusCode;
        }
    }
}