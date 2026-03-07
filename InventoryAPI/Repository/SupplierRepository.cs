using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;

namespace InventoryAPI.Repository
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly InventoryContext _context;

        public SupplierRepository(InventoryContext context)
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
