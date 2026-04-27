using AutoMapper;
using InventoryAPI.Repository.Interface;
using InventoryAPI.Service.Interface;
using InventoryAPI.DTOs;
using AutoMapper.QueryableExtensions;

namespace InventoryAPI.Service
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IInventoryTransactionRepository _repo;
        private readonly IMapper _mapper;

        public InventoryTransactionService(IInventoryTransactionRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public IQueryable<ReadTransactionDto> GetTransactions()
        {
            return _repo.GetAll()
                .ProjectTo<ReadTransactionDto>(_mapper.ConfigurationProvider);
        }

        public async Task<List<ReadTransactionDto>> GetByInventoryId(Guid inventoryId)
        {
            var data = await _repo.GetByInventoryIdAsync(inventoryId);
            return _mapper.Map<List<ReadTransactionDto>>(data);
        }
    }
}
