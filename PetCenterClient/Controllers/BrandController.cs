using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetCenterClient.Common;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using PetCenterClient.ViewModels.Brand;

namespace PetCenterClient.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandAPIClient _brandService;

        public BrandController(IBrandAPIClient brandService)
        {
            _brandService = brandService;
        }
        // GET: BrandController
        public async Task<IActionResult> IndexAsync()
        {
            var result = await _brandService.GetAllBrandAsync();

            return View("~/Views/CustomerViews/Home/ProductPage.cshtml", result);
        }

        public async Task<IActionResult> IndexAdminAsync(
            string? search, Status? status, int page = 1)
        {
            var result = await _brandService.GetAllBrandAdminAsync(search, status, page);

            ViewBag.CurrentPage = result.CurrentPage;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalBrands = result.TotalCount;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View("~/Views/AdminViews/Brand/Index.cshtml", result.Data);
        }


        // GET: BrandController/Details/5
        public async Task<IActionResult> DetailsAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readBrand = await _brandService.DetailsBrandAsync(id);
            if (readBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/CustomerView/Brand/_Details.cshtml", readBrand);
        }

        public async Task<IActionResult> DetailsAdminAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readBrand = await _brandService.DetailsBrandAsync(id);
            if (readBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Brand/_Details.cshtml", readBrand);
        }


        // GET: BrandController/Create
        public IActionResult CreateAsync()
        {
            return PartialView("~/Views/AdminViews/Brand/_Create.cshtml");
        }

        // POST: BrandController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateBrandViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/AdminViews/Brand/_Create.cshtml", model);
            }

            try
            {
                await _brandService.AddBrandAsync(model);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView("~/Views/AdminViews/Brand/_Create.cshtml", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong: " + ex.Message);
                return PartialView("~/Views/AdminViews/Brand/_Create.cshtml", model);
            }
        }

        // GET: BrandController/Edit/5
        public async Task<IActionResult> EditAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _brandService.DetailsBrandAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            var model = new UpdateBrandViewModel
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                BrandDescription = brand.BrandDescription,
                ExistingBrandLogo = brand.BrandLogo,
                Status = brand.Status
            };

            return PartialView("~/Views/AdminViews/Brand/_Edit.cshtml", model);
        }

        // POST: BrandController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid BrandId, UpdateBrandViewModel model)
        {
            Console.WriteLine("ĐÃ VÀO EDIT");
            Console.WriteLine($"ID: {BrandId}");
            if (!ModelState.IsValid)
                return View(model);

            await _brandService.UpdateBrandAsync(BrandId, model);

            return Json(new { success = true });
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> ChangeStatusAsync(Guid id, Status status)
        {
            var model = await _brandService.DetailsBrandAsync(id);

            if (model == null)
                return NotFound();

            ViewBag.Status = status;

            return PartialView("~/Views/AdminViews/Brand/_Delete.cshtml", model);
        }

        [HttpPost]
        [ActionName("ChangeStatusConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusConfirmedAsync(Guid id, Status status)
        {
            var brand = await _brandService.DetailsBrandAsync(id);

            if (brand == null)
                return NotFound();

            await _brandService.ChangeBrandStatusAsync(id, status);

            return Json(new
            {
                success = true
            });
        }
    }
}
