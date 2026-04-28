using CustomerAPI.Models;

namespace CustomerAPI.Repository.Interface;

public interface IPetRepository
{
    /// <summary>Trả IQueryable để OData filter/sort/page.</summary>
    IQueryable<Pet> GetAll();

    /// <summary>Lấy pet theo ID (kể cả IsActive = false).</summary>
    Task<Pet?> GetByIdAsync(Guid petId);

    /// <summary>Lấy tất cả pet của một customer (IsActive = true).</summary>
    Task<List<Pet>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Tìm kiếm pet theo tên loài hoặc giống.</summary>
    Task<List<Pet>> SearchAsync(Guid customerId, string keyword);

    Task<Pet> AddAsync(Pet pet);
    Task<Pet> UpdateAsync(Pet pet);

    /// <summary>Soft delete: set IsActive = false.</summary>
    Task<Pet?> SoftDeleteAsync(Guid petId);
}