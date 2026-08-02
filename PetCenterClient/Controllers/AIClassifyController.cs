using Humanizer;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.AI;
using static PetCenterClient.ViewModels.MedicalRecord.MedicalRecordViewModel;

namespace PetCenterClient.Controllers
{
    public class AIClassifyController : Controller
    {
        private readonly IAIClassifyAPIClient _aIClassifyService;

        public AIClassifyController(IAIClassifyAPIClient aIClassifyService)
        {
            _aIClassifyService = aIClassifyService;

        }

        public IActionResult ClassifyAI()
        {
            return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ClassifyAI(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("", "Please select an image.");
                return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml");
            }

            // Tạo thư mục uploads/temp nếu chưa có
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "temp");

            Directory.CreateDirectory(uploadFolder);

            // Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Gọi AI
            var result = await _aIClassifyService.ClassifyAsync(image);

            if (result == null)
            {
                ModelState.AddModelError("", "Unable to classify image.");
                return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml");
            }

            // Trả đường dẫn ảnh về View
            result.UploadedImageUrl = $"/uploads/temp/{fileName}";

            return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml", result);
        }
    }

    }
