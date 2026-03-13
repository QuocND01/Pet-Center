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

        public async Task<(bool Success, string Message)> UpdateProfileAsync (Guid customerId, UpdateCustomerProfileRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 100)
                return (false, "Invalid full name");
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Length > 15)
                return (false, "Invalid phone number");
            if (!new[] { "Male", "Female", "Other" }.Contains(request.Gender))
                return (false, "Invalid gender");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.BirthDay > today)
                return (false, "Date of birth cannot be in the future");

            var age = today.Year - request.BirthDay.Year;
            if (request.BirthDay > today.AddYears(-age)) age--;
            if (age < 16)
                return (false, "You must be at least 16 years old");
            if (age > 120) return (false, "Invalid date of birth");

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return (false, "Customer not found");

            if (customer.IsActive != true)
                return (false, "Account is deactivated");

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
