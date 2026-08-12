using PetCenterClient.ViewModels;

namespace PetCenterClient.Services.Interface
{
    public interface IPetAPIClient
    {
        // ================= CUSTOMER METHODS =================

        // Hỗ trợ truyền tham số OData query để Search/Filter/Sort
        Task<List<ReadPetListViewModel>?> GetMyPetsAsync(string query = "");

        Task<ReadPetDetailViewModel?> GetPetDetailsAsync(Guid id);

        Task<System.Net.Http.HttpResponseMessage> AddPetAsync(MutatePetViewModel dto);

        Task<System.Net.Http.HttpResponseMessage> UpdatePetAsync(Guid id, MutatePetViewModel dto);

        Task<bool> DeletePetAsync(Guid id);


        // ================= VET / ADMIN METHODS =================

        // Hỗ trợ truyền tham số OData query để Search/Filter/Sort
        Task<List<ReadVetPetListViewModel>?> GetAllPetsForVetAsync(string query = "");

        Task<ReadVetPetDetailViewModel?> GetPetDetailsForVetAsync(Guid id);

        Task<System.Net.Http.HttpResponseMessage> AddPetForVetAsync(Guid customerId, MutatePetViewModel dto);
        Task<System.Net.Http.HttpResponseMessage> UpdatePetForVetAsync(Guid id, MutatePetViewModel dto);
        Task<bool> DeletePetForVetAsync(Guid id);
    }
}