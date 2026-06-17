using PetCenterAPI.Common;
using PetCenterAPI.Models;
using System.Linq.Expressions;

namespace PetCenterAPI.Repository.Interface
{
    public interface IServiceRepository
    {
        IQueryable<Models.Service> GetAllService();

        Task<(IEnumerable<Models.Service> Items, int Total)> GetAllServiceAdminAsync(
    ServiceSpecification spec);

        Task<Models.Service?> GetServiceByIdAsync(Guid id);
        Task AddServiceAsync(Models.Service Service);
        Task UpdateServiceAsync(Models.Service Service);
        Task ChangeServiceStatusAsync(
     Guid id,
     Status status,
     bool hardDeleteImages = false);
        Task<bool> CheckServiceExistAsync(string ServiceName, Guid? excludeId = null);
        Task SaveAsync();
        void DeleteServiceImage(ServiceImage image);
    }
}
