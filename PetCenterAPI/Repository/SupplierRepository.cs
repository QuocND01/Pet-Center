using PetCenterAPI.Repository.Interface;

using PetCenterAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace PetCenterAPI.Repository
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly PetCenterContext _context;

        public SupplierRepository(PetCenterContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(x => x.SupplierId == id && x.IsActive);
        }
        public async Task<Supplier?> FindDuplicateAsync(
    string? taxId,
    string supplierName,
    string supplierEmail,
    string supplierPhoneNumber,
    Guid? excludeSupplierId = null)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(x =>
                x.IsActive == true
                &&
                    (!excludeSupplierId.HasValue ||
                     x.SupplierId != excludeSupplierId.Value)
                    &&
                    (
                        (!string.IsNullOrWhiteSpace(taxId) && x.TaxId == taxId) ||
                        x.SupplierName == supplierName ||
                        x.SupplierEmail == supplierEmail ||
                        x.SupplierPhoneNumber == supplierPhoneNumber
                    ));
        }
        public async Task<bool> IsUsedInImportStockAsync(Guid supplierId)
        {
            return await _context.ImportStocks
                .AnyAsync(x => x.SupplierId == supplierId);
        }
        public async Task AddAsync(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
        }

        public void Update(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
