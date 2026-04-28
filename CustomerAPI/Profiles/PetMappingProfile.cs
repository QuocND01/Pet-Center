using AutoMapper;
using CustomerAPI.DTOs.Pet;
using CustomerAPI.Models;

namespace CustomerAPI.Profiles;

public class PetMappingProfile : Profile
{
    public PetMappingProfile()
    {
        CreateMap<Pet, PetReadDto>();
        CreateMap<PetCreateDto, Pet>();
        CreateMap<PetUpdateDto, Pet>()
            .ForAllMembers(opt => opt.Condition((_, _, srcMember) => srcMember != null));
    }
}