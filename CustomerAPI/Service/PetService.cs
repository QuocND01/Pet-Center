using AutoMapper;
using CustomerAPI.DTOs.Pet;
using CustomerAPI.Models;
using CustomerAPI.Repository.Interface;
using CustomerAPI.Service.Interface;

namespace CustomerAPI.Service;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepo;
    private readonly IMapper _mapper;

    public PetService(IPetRepository petRepo, IMapper mapper)
    {
        _petRepo = petRepo;
        _mapper = mapper;
    }


    public IQueryable<Pet> GetAllQueryable()
        => _petRepo.GetAll();


    public async Task<PetReadDto> GetByIdAsync(Guid petId)
    {
        var pet = await _petRepo.GetByIdAsync(petId)
            ?? throw new KeyNotFoundException($"Pet with ID {petId} was not found.");

        return _mapper.Map<PetReadDto>(pet);
    }


    public async Task<List<PetReadDto>> GetByCustomerIdAsync(Guid customerId)
    {
        var pets = await _petRepo.GetByCustomerIdAsync(customerId);
        return _mapper.Map<List<PetReadDto>>(pets);
    }


    public async Task<List<PetReadDto>> SearchAsync(Guid customerId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Keyword cannot be empty.");

        var pets = await _petRepo.SearchAsync(customerId, keyword);
        return _mapper.Map<List<PetReadDto>>(pets);
    }


    public async Task<PetReadDto> AddAsync(PetCreateDto dto)
    {
        var pet = _mapper.Map<Pet>(dto);
        pet.PetId = Guid.NewGuid();
        pet.IsActive = true;

        var created = await _petRepo.AddAsync(pet);
        return _mapper.Map<PetReadDto>(created);
    }


    public async Task<PetReadDto> UpdateAsync(Guid petId, PetUpdateDto dto)
    {
        var pet = await _petRepo.GetByIdAsync(petId)
            ?? throw new KeyNotFoundException($"Pet with ID {petId} was not found.");

        if (pet.IsActive == false)
            throw new KeyNotFoundException($"Pet with ID {petId} has been deleted.");

        // Chỉ cập nhật field không null trong dto
        if (dto.Species is not null) pet.Species = dto.Species;
        if (dto.Breed is not null) pet.Breed = dto.Breed;
        if (dto.Gender is not null) pet.Gender = dto.Gender;
        if (dto.Weight.HasValue) pet.Weight = dto.Weight;
        if (dto.Note is not null) pet.Note = dto.Note;
        if (dto.DateOfBirth.HasValue) pet.DateOfBirth = dto.DateOfBirth;

        var updated = await _petRepo.UpdateAsync(pet);
        return _mapper.Map<PetReadDto>(updated);
    }


    public async Task SoftDeleteAsync(Guid petId)
    {
        var pet = await _petRepo.SoftDeleteAsync(petId)
            ?? throw new KeyNotFoundException($"Pet with ID {petId} was not found.");
    }
}