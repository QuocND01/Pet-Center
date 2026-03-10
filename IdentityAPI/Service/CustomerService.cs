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
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return false;

            _mapper.Map(request, customer);
            customer.UpdatedAt = DateTime.UtcNow;

            return await _customerRepository.UpdateAsync(customer);
        }
    }
}
