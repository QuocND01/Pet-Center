using Microsoft.AspNetCore.OData.Query;
using PetCenterAPI.Common;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Responses.Service.ServiceResponseDTO;

namespace PetCenterAPI.Service.Interface
{
    public interface IServiceService
    {
        Task<List<ReadServiceDTOForCustomer>> GetAllServiceAsync(ODataQueryOptions<ReadServiceDTOForCustomer> queryOptions);

        Task<PagedResult<ReadServiceDTO>> GetAllServiceAdminAsync(
    ServiceSpecification spec);
        Task<ReadServiceDTO> GetServiceByIdAsync(Guid id);
        Task AddServiceAsync(CreateServiceDTO createService);
        Task UpdateServiceAsync(Guid id, UpdateServiceDTO updateService);
        Task ChangeServiceStatusAsync(Guid id, Status status);
    }
}
