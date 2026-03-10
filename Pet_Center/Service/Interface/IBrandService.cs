using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IBrandService
    {
       // Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync();

        IQueryable<ReadBrandDTOs> GetAllBrand();

        Task<ReadBrandDTOs?> GetBrandByIdAsync(Guid id);
        Task AddBrandAsync(CreateBrandDTOs brand);
        Task UpdateBrandAsync(Guid id , UpdateBrandDTOs brand);
        Task DeleteBrandAsync(Guid id);
    }
}
