using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetCenterClient.DTOs;
using PetCenterClient.Services.Interface;

namespace PetCenterClient.Controllers
{
    public class AddressController : Controller
    {
        private readonly IAddressServiceClient _addressService;
        private readonly ICustomerService _customerService;

        public AddressController(IAddressServiceClient addressService, ICustomerService customerService)
        {
            _addressService = addressService;
            _customerService = customerService;
        }

        // 1. LIST (READ ALL)
        public async Task<IActionResult> Index()
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            // CALL GetByCustomerIdAsync SO IT FILTERS DIRECTLY FROM THE DATABASE ON THE BACKEND
            var myAddresses = await _addressService.GetByCustomerIdAsync(profile.CustomerId);

            return View("~/Views/CustomerViews/Address/Index.cshtml", myAddresses);
        }

        // 2. DETAILS (READ ONE)
        public async Task<IActionResult> Details(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();

            return View("~/Views/CustomerViews/Address/Details.cshtml", address);
        }

        // 3. CREATE (GET)
        public IActionResult Create()
        {
            return View("~/Views/CustomerViews/Address/Create.cshtml", new AddressCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressCreateDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;
            ModelState.Remove("CustomerId");

            if (ModelState.IsValid)
            {
                var success = await _addressService.CreateAsync(dto);
                if (success)
                {
                    TempData["Success"] = "Address added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "API rejected the request. It might be due to duplicate data or a database error.");
            }
            // Return to Create view if there are errors
            return View("~/Views/CustomerViews/Address/Create.cshtml", dto);
        }

        // 5. EDIT (GET)
        public async Task<IActionResult> Edit(Guid id)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            var address = await _addressService.GetByIdAsync(id);

            // Check if this address belongs to the currently logged-in user
            if (address == null || address.CustomerId != profile.CustomerId)
                return Forbid();

            var editDto = new AddressCreateDTO
            {
                CustomerId = address.CustomerId,
                AddressDetails = address.AddressDetails,
                Province = address.Province,
                District = address.District,
                Ward = address.Ward,
                IsDefault = address.IsDefault
            };
            return View("~/Views/CustomerViews/Address/Edit.cshtml", editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressCreateDTO dto)
        {
            var profile = await _customerService.GetProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Auth");

            dto.CustomerId = profile.CustomerId;

            if (ModelState.IsValid)
            {
                var success = await _addressService.UpdateAsync(id, dto);
                if (success)
                {
                    TempData["Success"] = "Address updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View("~/Views/CustomerViews/Address/Edit.cshtml", dto);
        }

        // 7. DELETE (GET)
        public async Task<IActionResult> Delete(Guid id)
        {
            var address = await _addressService.GetByIdAsync(id);
            if (address == null) return NotFound();

            return View("~/Views/CustomerViews/Address/Delete.cshtml", address);
        }

        // 8. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var success = await _addressService.DeleteAsync(id);
            if (success) TempData["Success"] = "Address deleted successfully.";
            else TempData["Error"] = "Failed to delete address.";

            return RedirectToAction(nameof(Index));
        }
    }
}