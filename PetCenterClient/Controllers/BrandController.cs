using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetCenterClient.DTOs;
using PetCenterClient.Services;
using PetCenterClient.Services.Interface;

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
        public async Task<ActionResult> IndexAsync(string? search, int page = 1)
        {
            var result = await _brandService.GetAllBrandAsync(search, page);

            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View("~/Views/CustomerViews/Home/HomePage.cshtml", result);
        }


        public async Task<ActionResult> IndexAdminAsync(string? search, int page = 1)
        {
            var result = await _brandService.GetAllBrandAsync(search, page);

            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View("~/Views/AdminViews/Brand/Index.cshtml", result);
        }


        // GET: BrandController/Details/5
        public async Task<ActionResult> DetailsAsync(Guid id)
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

        public async Task<ActionResult> DetailsAdminAsync(Guid id)
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
        public ActionResult CreateAsync()
        {
            return PartialView("~/Views/AdminViews/Brand/_Create.cshtml");
        }

        // POST: BrandController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateBrandDTOs model)
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

            var updateBrand = await _brandService.DetailsBrandAsync(id);
            if (updateBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Brand/_Edit.cshtml", updateBrand);
        }

        // POST: BrandController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid BrandId, UpdateBrandDTOs model)
        {
            Console.WriteLine("ĐÃ VÀO EDIT");
            Console.WriteLine($"ID: {BrandId}");
            if (!ModelState.IsValid)
                return View(model);

            await _brandService.UpdateBrandAsync(BrandId, model);

            return Json(new { success = true });
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> DeleteAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _brandService.DetailsBrandAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/AdminViews/Brand/_Delete.cshtml", model);
        }
        // GET: BrandController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync(Guid id)
        {
            var brand = await _brandService.DetailsBrandAsync(id);

            if (brand != null)
            {
                await _brandService.DeleteBrandAsync(id);
            }

            return Json(new { success = true });
        }
    }
}
