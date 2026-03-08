using AutoMapper;
using OrdersAPI.DTOs;
using OrdersAPI.Models;
using OrdersAPI.Repository.Interface;
using OrdersAPI.Service.Interface;

namespace OrdersAPI.Service
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _repo;
        private readonly IMapper _mapper;

        // FIX: Inject thêm IMapper để sử dụng MappingProfile bạn đã tạo
        public OrderDetailService(IOrderDetailRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // 1. LẤY DANH SÁCH THEO ĐƠN HÀNG
        public async Task<IEnumerable<OrderDetailResponseDTO>> GetDetailsByOrderAsync(Guid orderId)
        {
            var list = await _repo.GetByOrderIdAsync(orderId);

            // AutoMapper sẽ tự động xử lý logic Guid? -> Guid dựa trên MappingProfile
            return _mapper.Map<IEnumerable<OrderDetailResponseDTO>>(list);
        }

        // 2. THÊM MỚI CHI TIẾT ĐƠN HÀNG
        public async Task<bool> CreateDetailAsync(OrderDetailRequestDTO dto)
        {
            // Map từ DTO sang Entity
            var detail = _mapper.Map<OrderDetail>(dto);

            await _repo.AddAsync(detail);
            return await _repo.SaveChangesAsync();
        }

        // 3. CẬP NHẬT CHI TIẾT
        public async Task<bool> UpdateDetailAsync(Guid id, OrderDetailRequestDTO dto)
        {
            var detail = await _repo.GetByIdAsync(id);
            if (detail == null) return false;

            // Map dữ liệu từ DTO đè vào Entity đang có
            _mapper.Map(dto, detail);

            _repo.Update(detail);
            return await _repo.SaveChangesAsync();
        }

        // 4. XÓA CHI TIẾT
        public async Task<bool> DeleteDetailAsync(Guid id)
        {
            var detail = await _repo.GetByIdAsync(id);
            if (detail == null) return false;

            _repo.Delete(detail);
            return await _repo.SaveChangesAsync();
        }
    }
}