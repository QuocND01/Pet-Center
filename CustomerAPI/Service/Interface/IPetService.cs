using CustomerAPI.DTOs.Pet;
using CustomerAPI.Models;

namespace CustomerAPI.Service.Interface;

public interface IPetService
{
    /// <summary>Trả IQueryable để OData xử lý.</summary>
    IQueryable<Pet> GetAllQueryable();

    /// <summary>Lấy chi tiết một pet.</summary>
    Task<PetReadDto> GetByIdAsync(Guid petId);

    /// <summary>Lấy danh sách pet của customer.</summary>
    Task<List<PetReadDto>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Tìm kiếm pet theo keyword (species/breed).</summary>
    Task<List<PetReadDto>> SearchAsync(Guid customerId, string keyword);

    Task<PetReadDto> AddAsync(PetCreateDto dto);
    Task<PetReadDto> UpdateAsync(Guid petId, PetUpdateDto dto);
    Task SoftDeleteAsync(Guid petId);
}