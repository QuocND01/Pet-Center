using AutoMapper;
using CustomerAPI.DTOs.Request;
using CustomerAPI.DTOs.Response;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Service.Interface;

namespace CustomerAPI.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;

        public CustomerService(ICustomerRepository customerRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        // ── Customer ───────────────────────────────────
        public async Task<CustomerProfileResponseDto?> GetProfileAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return null;
            return _mapper.Map<CustomerProfileResponseDto>(customer);
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            Guid customerId, UpdateCustomerProfileRequestDto request)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return (false, "Customer not found");

            if (customer.IsActive != true)
                return (false, "Account is deactivated");

            // Kiểm tra phone đã dùng bởi account khác chưa
            if (customer.PhoneNumber != request.PhoneNumber)
            {
                var existingPhone = await _customerRepository.GetByPhoneAsync(request.PhoneNumber);
                if (existingPhone != null)
                    return (false, "Phone number is already in use by another account.");
            }

            _mapper.Map(request, customer);
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);

            return (true, "Profile updated successfully");
        }

        // ── Staff/Admin ────────────────────────────────
        public async Task<List<CustomerResponseDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            return _mapper.Map<List<CustomerResponseDto>>(customers);
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            return customer == null ? null : _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return false;

            customer.IsActive = isActive;
            customer.UpdatedAt = DateTime.UtcNow;
            return await _customerRepository.UpdateAsync(customer);
        }

        public async Task<CustomerInternalDto?> GetInternalAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdInternalAsync(customerId);
            if (customer == null) return null;
            return new CustomerInternalDto
            {
                FullName = customer.FullName,
                Email = customer.Email
            };
        }
    }
}
