using ProductAPI.Common;
using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IBrandService
    {

        IQueryable<ReadBrandDTOForCustomer> GetAllBrand();

        Task<PagedResult<ReadBrandDTO>> GetAllBrandAdminAsync(
       BrandSpecification spec);

        Task<ReadBrandDTO?> GetBrandByIdAsync(Guid id);
        Task AddBrandAsync(CreateBrandDTO brand);
        Task UpdateBrandAsync(Guid id , UpdateBrandDTO brand);
        Task ChangeBrandStatusAsync(
     Guid id,
     Status status);
    }
}
