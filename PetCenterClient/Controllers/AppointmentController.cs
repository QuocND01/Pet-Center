using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Appointment;
using System;
using System.Threading.Tasks;

namespace PetCenterClient.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentApiService _appointmentApiService;

        public AppointmentController(IAppointmentApiService appointmentApiService)
        {
            _appointmentApiService = appointmentApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Book(Guid? serviceId = null)
        {   

            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return RedirectToAction("Login", "Auth");
            }
            var bookingData = await _appointmentApiService.GetBookingDataAsync();

            var vm = new BookingPageViewModel
            {
                AppointmentStart = DateTime.Now.AddHours(1),
                Pets = bookingData!.Pets,
                Staffs = bookingData.Staffs,
                Services = bookingData.Services
            };
            ViewBag.AutoSelectServiceId = serviceId;
            return View("~/Views/CustomerViews/Appointment/Book.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel vm)
        {
            // Nếu Form không hợp lệ (ví dụ: thiếu thông tin bắt buộc)
            if (!ModelState.IsValid)
            {
                // Gọi hàm helper để nạp lại danh sách Pets, Staffs, Services cho giao diện
                var errorVm = await PopulateBookingPageViewModelAsync(vm);
                return View("~/Views/CustomerViews/Appointment/Book.cshtml", errorVm);
            }

            try
            {
                // Vì vm đã là BookAppointmentViewModel rồi, bạn có thể truyền thẳng 'vm' vào ApiService 
                // không cần mất công tạo 'new dto' thủ công để gán lại từng trường nữa nhé.
                var result = await _appointmentApiService.BookAppointmentAsync(vm);

                TempData["Success"] = "Book appointment successfully.";

                return RedirectToAction(nameof(Detail), new { id = result!.AppointmentId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                // Khi API báo lỗi (Trùng lịch, bác sĩ bận...), nạp lại giao diện an toàn
                var errorVm = await PopulateBookingPageViewModelAsync(vm);
                return View("~/Views/CustomerViews/Appointment/Book.cshtml", errorVm);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> GetAvailableSlots(
    [FromBody] GetAvailableSlotsRequestViewModel request)
        {
            try
            {
                var slots = await _appointmentApiService
                    .GetAvailableSlotsAsync(
                        request.StaffId,
                        request.Date,
                        request.ServiceIds);

                return Json(new
                {
                    status = true,
                    data = slots
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        /// <summary>
        /// Hàm Helper dùng để biến đổi dữ liệu từ Form Input (BookAppointmentViewModel)
        /// thành Dữ liệu hiển thị giao diện (BookingPageViewModel) khi xảy ra lỗi.
        /// </summary>
        private async Task<BookingPageViewModel> PopulateBookingPageViewModelAsync(BookAppointmentViewModel inputModel)
        {
            var bookingData = await _appointmentApiService.GetBookingDataAsync();

            return new BookingPageViewModel
            {
                // Giữ lại các giá trị user đã nhập/chọn trên form trước đó để họ không phải điền lại
                PetId = inputModel.PetId,
                StaffId = inputModel.StaffId,
                AppointmentStart = inputModel.AppointmentStart,
                Note = inputModel.Note,

                // Tái nạp lại danh sách dữ liệu từ API cho dropdown/checkbox hiển thị lại
                Pets = bookingData?.Pets ?? [],
                Staffs = bookingData?.Staffs ?? [],
                Services = bookingData?.Services ?? []
            };
        }
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            try
            {
                var model =
                    await _appointmentApiService.GetMyAppointmentsAsync();

                return View("~/Views/CustomerViews/Appointment/MyAppointment.cshtml",model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("~/Views/CustomerViews/Appointment/MyAppointment.cshtml",new List<AppointmentListViewModel>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var model =
                await _appointmentApiService
                    .GetAppointmentDetailAsync(id);

            if (model == null)
                return NotFound();

            return View("~/Views/CustomerViews/Appointment/Detail.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                await _appointmentApiService
                    .CancelAppointmentAsync(id);

                TempData["Success"] =
                    "Appointment cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyAppointments));
        }
        [HttpPost]
        public async Task<IActionResult> SubmitReview(
    SubmitReviewViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Detail),
                    new { id = model.AppointmentId });

            try
            {
                await _appointmentApiService
                    .SubmitReviewAsync(model);

                TempData["Success"] =
                    "Review submitted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Detail),
                new { id = model.AppointmentId });
        }

        [HttpGet]
        public async Task<IActionResult> StaffAppointments()
        {
            try
            {
                var model = await _appointmentApiService.GetAllAppointmentsAsync();
                return View("~/Views/AdminViews/Appointment/StaffAppointments.cshtml",model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("~/Views/AdminViews/Appointment/StaffAppointments.cshtml", new List<AppointmentListViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> StaffAppointmentDetail(Guid id)
        {
            try
            {
                var model = await _appointmentApiService.GetAppointmentDetailAsync(id);

                if (model == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(StaffAppointments));
                }

                return View("~/Views/AdminViews/Appointment/StaffAppointmentDetail.cshtml", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(StaffAppointments));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardAppointment(Guid appointmentId)
        {
            try
            {
                await _appointmentApiService
                    .ForwardAppointmentStatusAsync(appointmentId);

                TempData["Success"] = "Appointment status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(StaffAppointmentDetail), new
            {
                id = appointmentId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointmentService(
            [FromForm] Guid appointmentId,
            [FromForm] Guid appointmentServiceId)
        {
            try
            {
                await _appointmentApiService
                    .CompleteAppointmentServiceAsync(appointmentServiceId);

                TempData["Success"] = "Service completed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(StaffAppointmentDetail), new
            {
                id = appointmentId
            });
        }
        /// <summary>
        /// Action xử lý gửi request khởi tạo thanh toán từ AJAX View
        /// Route: POST /Appointment/CreatePayment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] AppointmentPaymentRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                // Lấy Token từ Session/Cookie/Claims của Client
                var token = HttpContext.Session.GetString("JWToken")
                         ?? User.FindFirst("JWToken")?.Value
                         ?? string.Empty;

                // Lấy IP Client nếu không có
                if (string.IsNullOrEmpty(request.ClientIpAddress))
                {
                    var ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                    request.ClientIpAddress = (ip == "0.0.0.1" || string.IsNullOrEmpty(ip)) ? "127.0.0.1" : ip;
                }

                var paymentResponse = await _appointmentApiService.CreatePaymentUrlAsync(request, token);

                if (paymentResponse != null && !string.IsNullOrEmpty(paymentResponse.PaymentUrl))
                {
                    return Json(new
                    {
                        status = true,
                        message = "Initialization payment successful.",
                        data = paymentResponse
                    });
                }

                return Json(new { status = false, message = "Cannot create payment URL.Please try again" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Trang hiển thị kết quả trả về sau khi khách thanh toán xong trên VNPay / MoMo
        /// Route: GET /Appointment/PaymentReturn
        /// </summary>
        [HttpGet]
        public IActionResult PaymentReturn([FromQuery] bool success, [FromQuery] string? message, [FromQuery] Guid? appointmentId)
        {
            ViewBag.Success = success;
            ViewBag.Message = message ?? (success ? "Thanh toán lịch hẹn thành công!" : "Thanh toán thất bại.");
            ViewBag.AppointmentId = appointmentId;

            return View("~/Views/CustomerViews/Appointment/PaymentReturn.cshtml");
        }
        /// <summary>
        /// GET: /Appointment/Update/{id}
        /// Lấy chi tiết lịch hẹn (Pending = 1) và Master Data để dựng UI Stepper Cập nhật
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // 1. Lấy chi tiết lịch hẹn hiện tại
                var appointmentDetail = await _appointmentApiService.GetAppointmentDetailAsync(id);
                if (appointmentDetail == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin lịch hẹn.";
                    return RedirectToAction("MyAppointments");
                }

                // 2. Chỉ cho phép chỉnh sửa nếu Status = 1 (Giữ chỗ / Pending)
                if (appointmentDetail.Status != 1)
                {
                    TempData["Error"] = "Chỉ có thể chỉnh sửa lịch hẹn đang ở trạng thái Giữ chỗ (Pending).";
                    return RedirectToAction("Detail", new { id });
                }

                // 3. Lấy Master Data (Pets, Staffs, Services)
                var bookingData = await _appointmentApiService.GetBookingDataAsync();

                // 4. Tìm StaffId theo tên Bác sĩ hiện tại trong Master Data (nếu có)
                Guid? matchedStaffId = bookingData?.Staffs
                    .FirstOrDefault(s => string.Equals(s.FullName, appointmentDetail.VetName, StringComparison.OrdinalIgnoreCase))
                    ?.StaffId;

                // 5. Tìm danh sách ServiceIds đã được đặt dựa vào tên dịch vụ
                var selectedServiceIds = bookingData?.Services
                    .Where(s => appointmentDetail.AppointmentServices.Any(aps => aps.ServiceName == s.ServiceName))
                    .Select(s => s.ServiceId)
                    .ToList() ?? new List<Guid>();

                // 6. Bind dữ liệu vào BookingPageViewModel
                var vm = new BookingPageViewModel
                {
                    PetId = bookingData?.Pets.FirstOrDefault(p => p.PetName == appointmentDetail.PetName)?.PetId,
                    StaffId = matchedStaffId,
                    ServiceIds = selectedServiceIds,
                    AppointmentStart = appointmentDetail.AppointmentStart,
                    Note = appointmentDetail.Note,

                    Pets = bookingData?.Pets ?? new List<BookingPetViewModel>(),
                    Staffs = bookingData?.Staffs ?? new List<BookingStaffViewModel>(),
                    Services = bookingData?.Services ?? new List<BookingServiceViewModel>()
                };

                ViewBag.AppointmentId = id;
                return View("~/Views/CustomerViews/Appointment/Update.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Detail", new { id });
            }
        }

        /// <summary>
        /// POST: /Appointment/Update
        /// Tiếp nhận dữ liệu cập nhật từ UI gửi về API
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateAppointmentViewModel request)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Customer")
            {
                return Json(new { status = false, message = "Your session has expired. Please log in again." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { status = false, message = "Invalid input" });
            }

            try
            {
                var result = await _appointmentApiService.UpdateReservedAppointmentAsync(request);

                if (result)
                {
                    return Json(new
                    {
                        status = true,
                        message = "Appointment update successful",
                        redirectUrl = Url.Action("Detail", "Appointment", new { id = request.AppointmentId })
                    });
                }

                return Json(new { status = false, message = "Update appointment fail! Please try again" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
    }
}