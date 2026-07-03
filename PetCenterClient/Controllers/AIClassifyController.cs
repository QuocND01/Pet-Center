using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AIClassifyController : Controller
    {
        private readonly IAIClassifyService _aIClassifyService;

        public AIClassifyController(IAIClassifyService aIClassifyService)
        {
            _aIClassifyService = aIClassifyService;
           
        }

        [HttpPost]
        public async Task<IActionResult> ClassifyAI(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError("", "Please select an image.");
                return View("Index");
            }

            var result = await _aIClassifyService.ClassifyAsync(image);

            if (result == null)
            {
                ModelState.AddModelError("", "Unable to classify image.");
                return View("Index");
            }

            //var disease = await _diseaseService.GetByNameAsync(result.DiseaseName);

            //if (disease != null)
            //{
            //    result.Description = disease.Description;
            //    result.Recommendation = disease.Recommendation;
            //    result.DiseaseId = disease.DiseaseId;
            //}
            //else
            //{
            //    result.Description = "No description available.";
            //    result.Recommendation = "Please consult a veterinarian.";
            //}

            return View("Index", result);
        }
    }
}
