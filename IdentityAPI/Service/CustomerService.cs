using IdentityAPI.DTOs.Response;
using IdentityAPI.DTOs.Resquest;
using IdentityAPI.Repository.Interface;
using IdentityAPI.Service.Interface;

namespace IdentityAPI.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerResponseDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();

            return customers.Select(c => new CustomerResponseDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                BirthDay = c.BirthDay,
                Gender = c.Gender,
                EmailVerified = c.EmailVerified,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return null;

            return new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                BirthDay = customer.BirthDay,
                Gender = customer.Gender,
                EmailVerified = customer.EmailVerified,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerProfileResponseDto?> GetProfileAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return null;

            return new CustomerProfileResponseDto
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                BirthDay = customer.BirthDay,
                Gender = customer.Gender,
                EmailVerified = customer.EmailVerified,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid customerId, UpdateCustomerProfileRequestDto request)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
                return false;

            customer.FullName = request.FullName;
            customer.PhoneNumber = request.PhoneNumber;
            customer.BirthDay = request.BirthDay;
            customer.Gender = request.Gender;
            customer.UpdatedAt = DateTime.UtcNow;

            return await _customerRepository.UpdateAsync(customer);
        }
    }
}
