using Microsoft.AspNetCore.Mvc;

using PetCenterClient.Services.Interface;
using System.Linq;
using Microsoft.Extensions.Logging;
using PetCenterClient.ViewModels;

namespace PetCenterClient.Controllers
{
    [Route("[controller]")]
    public class AddressesController : Controller
    {
        private readonly IAddressAPIClient _apiClient;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(IAddressAPIClient apiClient, ILogger<AddressesController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang giao diện quản lý địa chỉ của khách hàng
        /// </summary>
        /// <returns>View Index cùng với danh sách địa chỉ</returns>
        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role != "Customer")
                {
                    _logger.LogWarning("Truy cập trái phép vào trang Addresses. Bắt buộc role Customer. Chuyển hướng về trang chủ.");
                    return RedirectToAction("Index", "Home");
                }

                _logger.LogInformation("Đang lấy danh sách địa chỉ cho người dùng hiện tại.");
                var addresses = await _apiClient.GetMyAddressesAsync();

                if (addresses == null)
                {
                    _logger.LogWarning("Không tìm thấy dữ liệu địa chỉ hoặc API trả về null.");
                    addresses = new List<ReadAddressViewModel>(); // Tránh lỗi NullReferenceException ở View
                }

                return View("~/Views/CustomerViews/Addresses/Index.cshtml", addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi tải trang Index của AddressesController.");
                // Trong thực tế có thể trả về một trang báo lỗi chung
                return View("Error");
            }
        }

        /// <summary>
        /// Thêm mới một địa chỉ
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] MutateAddressViewModel dto)
        {
            // Trim incoming values to avoid whitespace-only values
            dto.AddressDetails = dto.AddressDetails?.Trim();
            dto.Province = dto.Province?.Trim();
            dto.District = dto.District?.Trim();
            dto.Ward = dto.Ward?.Trim();

            // Re-validate after trimming
            ModelState.Clear();
            TryValidateModel(dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dữ liệu đầu vào không hợp lệ khi tạo địa chỉ mới.");
                var errors = ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, errors });
            }

            try
            {
                var res = await _apiClient.AddAddressAsync(dto);
                if (res.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Tạo địa chỉ mới thành công.");
                    return Json(new { success = true });
                }

                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var errors = new Dictionary<string, string[]>();
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("errors", out var errElem) && errElem.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            foreach (var prop in errElem.EnumerateObject())
                            {
                                var list = prop.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();
                                // add original key
                                errors[prop.Name] = list;
                                // also add camelCase key for JS consumers that use camelCase
                                var camel = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                                if (!errors.ContainsKey(camel)) errors[camel] = list;
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }

                    // If errors dictionary is empty, try to extract message or fallback to a generic field error
                    if (errors.Count == 0)
                    {
                        try
                        {
                            using var doc2 = System.Text.Json.JsonDocument.Parse(json);
                            if (doc2.RootElement.TryGetProperty("message", out var msgElem) && msgElem.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var msg = msgElem.GetString() ?? "";
                                errors["addressDetails"] = new[] { msg };
                                errors["AddressDetails"] = new[] { msg };
                            }
                            else
                            {
                                // fallback: if response contains word 'address' try to use it
                                var low = json.ToLowerInvariant();
                                if (low.Contains("address") || low.Contains("địa chỉ") || low.Contains("dia chi"))
                                {
                                    errors["addressDetails"] = new[] { "Address already exists" };
                                    errors["AddressDetails"] = new[] { "Address already exists" };
                                }
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    return Json(new { success = false, errors });
                }

                _logger.LogWarning("Add address failed from API client.");
                return Json(new { success = false, message = "Failed to add address" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when calling API to add address.");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin địa chỉ hiện có
        /// </summary>
        [HttpPost("Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] MutateAddressViewModel dto)
        {
            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            // Trim incoming values to avoid whitespace-only values
            dto.AddressDetails = dto.AddressDetails?.Trim();
            dto.Province = dto.Province?.Trim();
            dto.District = dto.District?.Trim();
            dto.Ward = dto.Ward?.Trim();

            // Re-validate after trimming
            ModelState.Clear();
            TryValidateModel(dto);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, errors });
            }

            try
            {
                var res = await _apiClient.UpdateAddressAsync(id, dto);
                if (res.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Cập nhật địa chỉ {id} thành công.");
                    return Json(new { success = true });
                }

                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var errors = new Dictionary<string, string[]>();
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("errors", out var errElem) && errElem.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            foreach (var prop in errElem.EnumerateObject())
                            {
                                var list = prop.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();
                                // add original key
                                errors[prop.Name] = list;
                                // also add camelCase key for JS consumers that use camelCase
                                var camel = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                                if (!errors.ContainsKey(camel)) errors[camel] = list;
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }

                    // If errors dictionary is empty, try to extract message or fallback to a generic field error
                    if (errors.Count == 0)
                    {
                        try
                        {
                            using var doc2 = System.Text.Json.JsonDocument.Parse(json);
                            if (doc2.RootElement.TryGetProperty("message", out var msgElem) && msgElem.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                var msg = msgElem.GetString() ?? "";
                                errors["addressDetails"] = new[] { msg };
                                errors["AddressDetails"] = new[] { msg };
                            }
                            else
                            {
                                // fallback: if response contains vietnamese word 'địa chỉ' try to use it
                                var low = json.ToLowerInvariant();
                                if (low.Contains("address") || low.Contains("địa chỉ") || low.Contains("dia chi"))
                                {
                                    errors["addressDetails"] = new[] { "Address already exists" };
                                    errors["AddressDetails"] = new[] { "Address already exists" };
                                }
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    return Json(new { success = false, errors });
                }

                _logger.LogWarning($"Update address {id} failed from API client.");
                return Json(new { success = false, message = "Failed to update address" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception when updating address ID: {id}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Xóa địa chỉ khỏi hệ thống
        /// </summary>
        [HttpPost("Delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            try
            {
                var res = await _apiClient.DeleteAddressAsync(id);
                if (res.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Xóa địa chỉ {id} thành công.");
                    return Json(new { success = true });
                }

                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var errors = new Dictionary<string, string[]>();
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("errors", out var errElem) && errElem.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            foreach (var prop in errElem.EnumerateObject())
                            {
                                var list = prop.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();
                                errors[prop.Name] = list;
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }

                    return Json(new { success = false, errors });
                }
                _logger.LogWarning($"Delete address {id} failed from API client.");
                return Json(new { success = false, message = "Failed to delete address" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception when deleting address ID: {id}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }
    }
}