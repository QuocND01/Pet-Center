using PetCenterAPI.Common;
using PetCenterAPI.Models;

namespace PetCenterAPI.Repository.Interface
{
    public interface IBrandRepository
    {
       // Task<IEnumerable<Brand>> GetAllBrandAsync();

        IQueryable<Brand> GetAllBrand();
        Task<(IEnumerable<Brand> Items, int Total)> GetAllBrandAdminAsync(
      BrandSpecification spec);

        Task<Brand?> GetBrandByIdAsync(Guid id);
        Task AddBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task ChangeBrandStatusAsync(
      Guid id,
      Status status);

        Task<bool> CheckBrandExistAsync(string brandName);

    }
}
