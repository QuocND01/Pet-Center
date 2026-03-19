using AutoMapper;
using PromotionAPI.DTOs;
using PromotionAPI.Models;

public class VoucherProfile : Profile
{
    public VoucherProfile()
    {
        CreateMap<Voucher, VoucherResponseDTO>();
        CreateMap<CreateVoucherDTO, Voucher>();
    }

}