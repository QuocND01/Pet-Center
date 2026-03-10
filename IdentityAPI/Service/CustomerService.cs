using AutoMapper;
using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;
using IdentityAPI.Repository.Interface;
using IdentityAPI.Service.Interface;

namespace IdentityAPI.Service
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

        public async Task<List<CustomerResponseDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            return _mapper.Map<List<CustomerResponseDto>>(customers);
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return null;

            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<CustomerProfileResponseDto?> GetProfileAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return null;

            return _mapper.Map<CustomerProfileResponseDto>(customer);
        }

        public async Task<bool> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto request)
        {
            // ✅ Double-check validation ở service layer
            if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 100)
                throw new ArgumentException("Invalid full name");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length > 15)
                throw new ArgumentException("Invalid phone number");

            if (!new[] { "Male", "Female", "Other" }.Contains(request.Gender))
                throw new ArgumentException("Invalid gender");

            // ✅ Validate tuổi - không dùng .Value vì DateOnly không nullable
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (request.BirthDay > today)
                throw new ArgumentException("Date of birth cannot be in the future");

            // ✅ Tính tuổi đúng cách
            var age = today.Year - request.BirthDay.Year;

            // Kiểm tra nếu chưa qua sinh nhật năm nay
            if (request.BirthDay > today.AddYears(-age))
                age--;

            if (age < 13)
                throw new ArgumentException("You must be at least 13 years old");

            if (age > 120)
                throw new ArgumentException("Invalid date of birth");

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return false;

            // ✅ Kiểm tra account có bị deactivate không
            if (customer.IsActive != true)
                throw new InvalidOperationException("Account is deactivated");

            _mapper.Map(request, customer);
            customer.UpdatedAt = DateTime.UtcNow;

            return await _customerRepository.UpdateAsync(customer);
        }

        public async Task<bool> ChangeCustomerStatusAsync(Guid customerId, bool isActive)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return false;

            customer.IsActive = isActive;
            customer.UpdatedAt = DateTime.UtcNow;

            return await _customerRepository.UpdateAsync(customer);
        }
    }
}
