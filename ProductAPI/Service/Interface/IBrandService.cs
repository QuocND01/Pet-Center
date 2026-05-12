using ProductAPI.Common;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IBrandService
    {

        IQueryable<ReadBrandDTOs> GetAllBrand();

        Task<PagedResult<ReadBrandDTOs>> GetAllBrandAdminAsync(
       BrandSpecification spec);

        Task<ReadBrandDTOs?> GetBrandByIdAsync(Guid id);
        Task AddBrandAsync(CreateBrandDTOs brand);
        Task UpdateBrandAsync(Guid id , UpdateBrandDTOs brand);
        Task DeleteBrandAsync(Guid id);
    }
}
