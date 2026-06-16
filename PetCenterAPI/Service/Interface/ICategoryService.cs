using PetCenterAPI.Common;
using PetCenterAPI.DTOs;
using PetCenterAPI.Models;
using static PetCenterAPI.DTOs.Requests.Category.CategoryAttributeRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Responses.Category.CategoryResponseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface ICategoryService
    {
        IQueryable<ReadCategoryDTOForCustomer> GetAllCategory();

        Task<PagedResult<ReadCategoryDTO>> GetAllCategoryAdminAsync(
    CategorySpecification spec);

        Task<ReadCategoryDTO?> GetCategoryByIdAsync(Guid id);
        Task AddCategoryAsync(CreateCategoryDTO category);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryDTO category);
        Task ChangeCategoryStatusAsync(
       Guid id,
       Status status);
        Task<IEnumerable<ReadCategoryAttributeDTO>> GetAllCategoryAttributeByCategoryIDAsync(Guid id);
    }
}
