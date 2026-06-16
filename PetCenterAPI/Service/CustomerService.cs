using AutoMapper;
using PetCenterAPI.DTOs.Requests.CustomerProfile;
using PetCenterAPI.DTOs.Responses.CustomerProfile;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
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

        // ============================================================
        // CUSTOMER — PROFILE
        // ============================================================
        public async Task<CustomerProfileResponseDTO?> GetProfileAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null) return null;

            return _mapper.Map<CustomerProfileResponseDTO>(customer);
        }

        // ============================================================
        // CUSTOMER — UPDATE PROFILE
        // ============================================================
        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            Guid customerId, UpdateCustomerProfileRequestDTO request)
        {
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
    }
}
