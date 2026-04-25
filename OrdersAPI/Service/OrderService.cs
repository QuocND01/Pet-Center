using AutoMapper;
using OrdersAPI.DTOs;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderResponseDTO>>(orders);
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(Guid id)
        {
            var order = await _repo.GetByIdAsync(id);
            return _mapper.Map<OrderResponseDTO>(order);
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO dto)
        {
            var order = _mapper.Map<Order>(dto);
            order.OrderId = Guid.NewGuid();
            order.OrderDate = DateTime.Now; // Tự động gán ngày tạo

            await _repo.AddAsync(order);
            await _repo.SaveChangesAsync();
            return _mapper.Map<OrderResponseDTO>(order);
        }

        public async Task<bool> UpdateOrderAsync(Guid id, OrderRequestDTO dto)
        {
            var existingOrder = await _repo.GetByIdAsync(id);
            if (existingOrder == null) return false;

            _mapper.Map(dto, existingOrder);

 
            if (dto.StaffId.HasValue)
            {
                existingOrder.StaffId = dto.StaffId;
            }
            if (dto.DeliveredDate.HasValue)
            {
                existingOrder.DeliveredDate = dto.DeliveredDate;
            }



            _repo.Update(existingOrder);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return false;

            // Soft delete
            order.Status = 0;
   

            // gọi repo để lưu
            return await _repo.SaveChangesAsync();
        }

    }
}
