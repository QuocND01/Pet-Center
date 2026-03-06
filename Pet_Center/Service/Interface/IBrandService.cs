using ProductAPI.DTOs;
using ProductAPI.Models;

namespace ProductAPI.Service.Interface
{
    public interface IBrandService
    {
        Task<IEnumerable<ReadBrandDTOs>> GetAllBrandAsync();
    }
}
