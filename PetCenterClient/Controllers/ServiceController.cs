using Microsoft.AspNetCore.Mvc;
using PetCenterClient.Common;
using PetCenterClient.Services.Interface;
using static PetCenterClient.ViewModels.Service.ServiceViewModel;

namespace PetCenterClient.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceAPIClient _ServiceService;

        public ServiceController(IServiceAPIClient ServiceService)

        {
            _ServiceService = ServiceService;
        }

        // GET: ReadProdutDTOs
        public async Task<IActionResult> IndexAsync(
        string? search,
                decimal? minPrice,
                decimal? maxPrice,
                int? serviceType,
                int page = 1)
        {
            int pagesize = 24;
            var result = await _ServiceService.GetAllServiceAsync(
                search,
                minPrice,
                maxPrice,
                serviceType,
                page);

            int totalItems = result?.Count ?? 0;
            var totalPages = (int)Math.Ceiling((double)totalItems / pagesize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pagesize;
            ViewBag.Search = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.ServiceType = serviceType;
            return View(
                "~/Views/CustomerViews/Home/ServicePage.cshtml",
                result);
        }


        public async Task<IActionResult> IndexAdminAsync(
     string? search,
    Status? status,
     decimal? minPrice,
     decimal? maxPrice,
     int? serviceType,
     int page = 1,
     int pageSize = 10)
        {
            var result = await _ServiceService.GetAllServiceAdminAsync(
                search, status, minPrice, maxPrice, serviceType, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalServices = result.TotalCount;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.Search = search;
            ViewBag.status = status;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.ServiceType = serviceType;
           

            return View("~/Views/AdminViews/Service/Index.cshtml", result.Data);
        }


        // GET: ReadProdutDTOs/Details/5
        public async Task<IActionResult> DetailsAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _ServiceService.DetailsServiceAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Service/_Details.cshtml", readProdutDTOs);
        }

        public async Task<IActionResult> DetailsForcustomerAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readProdutDTOs = await _ServiceService.DetailsServiceAsync(id);
            if (readProdutDTOs == null)
            {
                return NotFound();
            }          

            return View("~/Views/CustomerViews/Service/Details.cshtml", readProdutDTOs);
        }

        // GET: ReadProdutDTOs/Create
        public async Task<IActionResult> CreateAsync()
        {
            return PartialView("~/Views/AdminViews/Service/_Create.cshtml");
        }

        // POST: ReadProdutDTOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateServiceViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
       .SelectMany(v => v.Errors)
       .Select(e => e.ErrorMessage)
       .ToList();
                return PartialView("~/Views/AdminViews/Service/_Create.cshtml", model);
            }

            try
            {
                await _ServiceService.AddServiceAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("~/Views/AdminViews/Service/_Create.cshtml", model);
            }
        }

        // GET: ReadProdutDTOs/Edit/5
        public async Task<IActionResult> EditAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Service = await _ServiceService.DetailsServiceAsync(id);

            if (Service == null)
            {
                return NotFound();
            }

            var model = new UpdateServiceViewModel
            {
                ServiceId = Service.ServiceId,
                ServiceName = Service.ServiceName,
                Price = Service.Price,
                ServiceDescription = Service.ServiceDescription,
                Duration = Service.Duration,
                ServiceType = Service.ServiceType,
                ExistingImages = Service.ImageFiles,
            };
            return PartialView("~/Views/AdminViews/Service/_Edit.cshtml", model);
        }

        // POST: ReadProdutDTOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid ServiceId, UpdateServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Key = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    });

                foreach (var err in errors)
                {
                    Console.WriteLine($"FIELD: {err.Key}");
                    foreach (var msg in err.Errors)
                    {
                        Console.WriteLine($"  ERROR: {msg}");
                    }
                }

                return PartialView("~/Views/AdminViews/Service/_Edit.cshtml", model);
            }

            try
            {
                await _ServiceService.UpdateServiceAsync(ServiceId, model);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("~/Views/AdminViews/Service/_Edit.cshtml", model);
            }
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> ChangeStatusAsync(
      Guid? id,
      Status status)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _ServiceService.DetailsServiceAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            ViewBag.Status = status;

            return PartialView(
                "~/Views/AdminViews/Service/_Delete.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusConfirmedAsync(
            Guid id,
            Status status)
        {
            await _ServiceService.ChangeServiceStatusAsync(
                id,
                status);

            return Json(new
            {
                success = true
            });
        }

    }
}
