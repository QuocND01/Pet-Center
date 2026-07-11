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

            var result = await _aIClassifyService.ClassifyAsync(image);

            if (result == null)
            {
                ModelState.AddModelError("", "Unable to classify image.");
                return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml");
            }

            if (AIDiseaseData.Diseases.TryGetValue(result.DiseaseName, out var diseaseInfo))
            {
                result.Description = diseaseInfo.Diagnosis;
                result.Recommendation = diseaseInfo.Treatment;

            }

            return View("~/Views/CustomerViews/AI/ClassifyAI.cshtml", result);
        }
    }

    }
