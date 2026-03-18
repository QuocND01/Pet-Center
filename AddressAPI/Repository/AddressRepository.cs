using AddressAPI.Models;
using AddressAPI.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddressAPI.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly PetCenterIdentityServiceDBContext _context;

        public AddressRepository(PetCenterIdentityServiceDBContext context)
            => _context = context;

        public async Task<IEnumerable<Address>> GetAllAsync()
            => await _context.Addresses.ToListAsync();

        // Filter thẳng tại DB — không load toàn bộ rồi filter
        public async Task<IEnumerable<Address>> GetByCustomerIdAsync(Guid customerId)
            => await _context.Addresses
                .Where(a => a.CustomerId == customerId && a.IsActive == true)
                .OrderByDescending(a => a.IsDefault)   // default address lên đầu
                .ThenBy(a => a.AddressDetails)
                .ToListAsync();

        public async Task<Address?> GetByIdAsync(Guid id)
            => await _context.Addresses.FindAsync(id);

        public async Task AddAsync(Address address)
        {
            // QUY TẮC 1: Nếu địa chỉ mới thêm là Mặc định (Default)
            if (address.IsDefault == true)
            {
                // Tìm các địa chỉ cũ đang là Default của khách hàng này
                var existingDefaults = await _context.Addresses
                    .Where(a => a.CustomerId == address.CustomerId && a.IsDefault == true)
                    .ToListAsync();

                // Gỡ bỏ cờ Default của tụi nó
                foreach (var oldAddr in existingDefaults)
                {
                    oldAddr.IsDefault = false;
                }

                _context.Addresses.UpdateRange(existingDefaults);
            }

            await _context.Addresses.AddAsync(address);
        }

        public void Update(Address address)
        {
            // QUY TẮC 1: Nếu khách update một địa chỉ bình thường thành Mặc định
            if (address.IsDefault == true)
            {
                // Tìm các địa chỉ mặc định cũ (loại trừ chính cái đang update ra)
                var existingDefaults = _context.Addresses
                    .Where(a => a.CustomerId == address.CustomerId
                             && a.AddressId != address.AddressId
                             && a.IsDefault == true)
                    .ToList();

                foreach (var oldAddr in existingDefaults)
                {
                    oldAddr.IsDefault = false;
                }

                _context.Addresses.UpdateRange(existingDefaults);
            }

            _context.Addresses.Update(address);
        }

        public void Delete(Address address)
        {
            // QUY TẮC 2: Chặn xóa từ trong "trứng nước" nếu nó là địa chỉ mặc định
            if (address.IsDefault == true)
            {
                // Quăng Exception để Service/Controller bắt lỗi và báo về Client
                throw new InvalidOperationException("Không thể xóa địa chỉ mặc định. Vui lòng thiết lập địa chỉ khác làm mặc định trước.");
            }

            _context.Addresses.Remove(address);
        }

        public async Task<bool> SaveChangesAsync()
            => await _context.SaveChangesAsync() > 0;
    }
}