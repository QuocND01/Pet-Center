using ProductAPI.Models;

namespace ProductAPI.Repository.Interface
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllBrandAsync();

    }
}
