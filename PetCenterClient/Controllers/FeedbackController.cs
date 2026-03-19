using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _service;
        private readonly IOrderServiceClient _orderService;
        private readonly IOrderDetailServiceClient _detailService;

        public FeedbackController(IFeedbackService service, IOrderServiceClient orderService, IOrderDetailServiceClient detailService)
        {
            _service = service;
            _orderService = orderService;
            _detailService = detailService;
        }

        private Guid? GetUserIdFromToken()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims
                .FirstOrDefault(c =>
                    c.Type == "sub" ||
                    c.Type == "nameid" ||
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?.Value;

            if (Guid.TryParse(userId, out var guid))
                return guid;

            return null;
        }

        // Feedback
        public async Task<IActionResult> Index()
        {
            return await AdminList();
        }

        public async Task<IActionResult> AdminList()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Staff")
                return RedirectToAction("AdminLogin", "Auth");

            var feedbacks = await _service.GetAllAsync();

            return View("~/Views/AdminViews/Feedback/List.cshtml", feedbacks);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var feedback = await _service.GetDetailAsync(id);

            if (feedback == null)
                return NotFound();

            return View("~/Views/AdminViews/Feedback/Detail.cshtml", feedback);
        }

        public async Task<IActionResult> Toggle(Guid id)
        {
            await _service.ToggleVisibilityAsync(id);

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);

            return RedirectToAction(nameof(AdminList));
        }

        public async Task<IActionResult> MyFeedback(Guid customerId)
        {
            var feedbacks = await _service.GetByCustomerAsync(customerId);

            return View("~/Views/CustomerViews/Feedback/MyFeedback.cshtml", feedbacks);
        }

        public IActionResult Create(Guid productId)
        {
            var dto = new CreateFeedbackDTO
            {
                ProductId = productId
            };

            return View("~/Views/CustomerViews/Feedback/Create.cshtml", dto);
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(CreateFeedbackDTO dto)
        {
            var customerId = GetUserIdFromToken();
            if (customerId == null)
                return RedirectToAction("Login", "Auth");

            dto.CustomerId = customerId.Value;

            var orders = await _orderService.GetAllAsync();

            var hasBought = false;

            Console.WriteLine("ProductId: " + dto.ProductId);
            Console.WriteLine("CustomerId: " + dto.CustomerId);

            foreach (var order in orders.Where(o => o.CustomerId == customerId.Value && o.Status == 4))
            {
                var details = await _detailService.GetByOrderIdAsync(order.OrderId);

                if (details.Any(d => d.ProductId == dto.ProductId))
                {
                    hasBought = true;

                    dto.OrderId = order.OrderId;

                    break;
                }
            }

            //if (!hasBought)
            //{
            //    TempData["Error"] = "You  must purchase this product before writting a feedback!";
            //    return RedirectToAction("DetailsForcustomer", "Products", new { id = dto.ProductId });
            //}

            if (!ModelState.IsValid)
            {
                return View("~/Views/CustomerViews/Feedback/Create.cshtml", dto);
            }

            try
            {
                await _service.CreateAsync(dto);

                TempData["Success"] = "Feedback submitted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index", "Products");
        }


        public async Task<IActionResult> List(
    int? rating,
    Guid? productId,
    bool? isVisible,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var feedbacks = await _service.FilterAsync(
                rating, productId, isVisible, fromDate, toDate);

            return View("~/Views/AdminViews/Feedback/List.cshtml", feedbacks);
        }


        // Reply

        public async Task<IActionResult> Reply(Guid id)
        {
            var feedback = await _service.GetDetailAsync(id);

            return View("~/Views/AdminViews/Feedback/Reply.cshtml", feedback);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(Guid feedbackId, string reply)
        {
            var staffId = GetUserIdFromToken();
            if (staffId == null)
                return RedirectToAction("AdminLogin", "Auth");

            await _service.ReplyAsync(feedbackId, staffId.Value, reply);

            return RedirectToAction(nameof(AdminList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReply(Guid id)
        {
            var staffId = GetUserIdFromToken();
            if (staffId == null)
                return RedirectToAction("AdminLogin", "Auth");

            await _service.DeleteReplyAsync(id);

            return RedirectToAction(nameof(AdminList));
        }
    }
}