using AutoMapper;
using InventoryAPI.DTOs;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service.Interface;
using AutoMapper.QueryableExtensions;

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

        public async Task<IEnumerable<ProductQuantityDTO>> GetProductStockAsync(List<Guid> productIds)
        {
            var stocks = await _inventoryRepository.GetByProductIds(productIds);
            return _mapper.Map<IEnumerable<ProductQuantityDTO>>(stocks);
        }

        public IQueryable<ReadInventoryDto> GetInventories()
        {
            return _inventoryRepository.GetAll()
                .ProjectTo<ReadInventoryDto>(_mapper.ConfigurationProvider);
        }

        public async Task<ReadInventoryDto?> GetInventoryById(Guid id)
        {
            var entity = await _inventoryRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ReadInventoryDto>(entity);
        }
    }
}
