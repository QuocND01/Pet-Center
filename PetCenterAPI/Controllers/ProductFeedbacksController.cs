using Microsoft.AspNetCore.Mvc;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductFeedbacksController : ControllerBase
    {
        private readonly IProductFeedbackService _productFeedbackService;

        public ProductFeedbacksController(IProductFeedbackService productFeedbackService)
        {
            _productFeedbackService = productFeedbackService;
        }

        // ============================================================
        // FEEDBACK — VIEW BY PRODUCT (CUSTOMER SIDE)
        // ============================================================
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetFeedbacksByProductId(Guid productId)
        {
            try
            {
                var feedbacks = await _productFeedbackService.GetFeedbacksByProductIdAsync(productId);
                return Ok(new { success = true, data = feedbacks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
