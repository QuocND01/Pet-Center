namespace PetCenterClient.Services.Interface
{
    using PetCenterClient.DTOs;

    public interface IAddressServiceClient
    {
        Task<List<AddressResponseDTO>> GetAllAsync();

        // Lấy address active theo customerId
        Task<List<AddressResponseDTO>> GetByCustomerIdAsync(Guid customerId);

        Task<AddressResponseDTO?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(AddressCreateDTO dto);
        Task<bool> UpdateAsync(Guid id, AddressCreateDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}