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
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }
        // GET: BrandController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
           var result = await _brandService.GetAllBrandAsync(search, page);
            return View("~/Views/CustomerViews/Home/HomePage.cshtml", result);
        }


        public async Task<ActionResult> Indexadmin(string? search, int page = 1)
        {
            var result = await _brandService.GetAllBrandAsync(search, page);
            return View("~/Views/AdminViews/Brand/Index.cshtml", result);
        }


        // GET: BrandController/Details/5
        public async Task<ActionResult> DetailsAsync(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readBrand = await _brandService.GetBrandByIdAsync(id);
            if (readBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/CustomerView/Brand/_Details.cshtml", readBrand);
        }

        public async Task<ActionResult> DetailsAsyncAdmin(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var readBrand = await _brandService.GetBrandByIdAsync(id);
            if (readBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Brand/_Details.cshtml", readBrand);
        }


        // GET: BrandController/Create
        public ActionResult Create()
        {
            return PartialView("~/Views/AdminViews/Brand/_Create.cshtml");
        }

        // POST: BrandController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBrandDTOs model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return PartialView("~/Views/AdminViews/Brand/_Create.cshtml", model);
            }
            try
            {
                await _brandService.AddBrandAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(new { success = true });
        }

        // GET: BrandController/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateBrand = await _brandService.GetBrandByIdAsync(id);
            if (updateBrand == null)
            {
                return NotFound();
            }
            return PartialView("~/Views/AdminViews/Brand/_Edit.cshtml", updateBrand);
        }

        // POST: BrandController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateBrandDTOs model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _brandService.UpdateBrandAsync(id, model);

            return Json(new { success = true });
        }

        // GET: ReadProdutDTOs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _brandService.GetBrandByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return PartialView("~/Views/AdminViews/Brand/_Delete.cshtml", model);
        }
        // GET: BrandController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);

            if (brand != null)
            {
                await _brandService.DeleteBrandAsync(id);
            }

            return Json(new { success = true });
        }
    }
}
