using AutoMapper;
using PetCenterAPI.DTOs.Requests.Inventory;
using PetCenterAPI.DTOs.Responses.Inventory;
using PetCenterAPI.Repository.Interface;
using PetCenterAPI.Service.Interface;

namespace PetCenterAPI.Service
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repo;
        private readonly IMapper _mapper;

        public InventoryService(
            IInventoryRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InventoryListResponseDTO> GetPagedAsync(
            InventoryQueryRequestDTO request)
        {
            var (items, totalRecords) =
                await _repo.GetPagedAsync(request);

            return new InventoryListResponseDTO
            {
                Items = _mapper.Map<List<InventoryItemResponseDTO>>(items),

                TotalRecords = totalRecords,

                Page = request.Page,

                PageSize = request.PageSize,

                TotalPages = (int)Math.Ceiling(
                    totalRecords / (double)request.PageSize)
            };
        }

        public async Task<InventoryDetailResponseDTO?> GetByIdAsync(
            Guid inventoryId)
        {
            var entity = await _repo.GetByIdAsync(inventoryId);

            if (entity == null)
                return null;

            return _mapper.Map<InventoryDetailResponseDTO>(entity);
        }
    }
}