using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service.Interface;

namespace InventoryAPI.Service
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public InventoryService(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductInventoryDTO>> GetProductStockAsync(List<Guid> productIds)
        {
            var stocks = await _inventoryRepository.GetByProductIds(productIds);
            return _mapper.Map<IEnumerable<ProductInventoryDTO>>(stocks);
        }
    }
}
