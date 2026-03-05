using AutoMapper;
using AddressAPI.Models; // Thay bằng namespace chứa class Address của bạn

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map từ Entity sang DTO (để trả về client)
        CreateMap<Address, AddressResponseDTO>();

        // Map từ DTO sang Entity (để lưu vào DB)
        CreateMap<AddressCreateDTO, Address>()
            .ForMember(dest => dest.AddressId, opt => opt.Ignore()); // Không map ID vì ID tự sinh hoặc giữ nguyên
    }
}