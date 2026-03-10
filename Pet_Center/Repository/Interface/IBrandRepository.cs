using ProductAPI.Models;

namespace ProductAPI.Repository.Interface
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllBrandAsync();

        IQueryable<Brand> GetAllBrand();

        Task<Brand?> GetBrandByIdAsync(Guid id);
        Task AddBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(Guid id);

        Task<bool> CheckBrandExist(string brandName);

    }
}
