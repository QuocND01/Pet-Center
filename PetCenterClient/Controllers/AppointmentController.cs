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
    }
}