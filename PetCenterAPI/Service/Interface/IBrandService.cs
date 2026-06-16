using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Responses.Brand.BrandResposeDTO;

namespace PetCenterAPI.Service.Interface
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
