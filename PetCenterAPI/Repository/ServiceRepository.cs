using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PetCenterAPI.Common;
using PetCenterAPI.Models;
using PetCenterAPI.Repository.Interface;
using System.Linq.Expressions;

namespace PetCenterAPI.Repository
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly PetCenterContext _db;
        private readonly IMapper _mapper;
        public ServiceRepository(PetCenterContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        public async Task AddServiceAsync(Models.Service Service)
        {
            _db.Services.Add(Service);
            await _db.SaveChangesAsync();
        }

        public async Task ChangeServiceStatusAsync(
     Guid id,
     Status status,
     bool hardDeleteImages = false)
        {
            var Service = await _db.Services
                .Include(p => p.ServiceImages)
                .FirstOrDefaultAsync(x => x.ServiceId == id);

            if (Service == null)
                throw new Exception("Service not found");

            await _db.Services
                .Where(p => p.ServiceId == id)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(p => p.Status, status));

            if (status == Status.Deleted)
            {
                var imageIds = Service.ServiceImages
                    .Select(i => i.ImageId)
                    .ToList();

                if (!imageIds.Any())
                    return;

                if (hardDeleteImages)
                {
                    await _db.ServiceImages
                        .Where(i => imageIds.Contains(i.ImageId))
                        .ExecuteDeleteAsync();
                }
            }
        }

        public IQueryable<Models.Service> GetAllService()
        {
            try
            {
                return _db.Services.Where(s => s.Status == Status.Active)
                .Include(p => p.ServiceImages.Where(i => i.IsActive == true))
                .AsQueryable();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<(IEnumerable<Models.Service> Items, int Total)> GetAllServiceAdminAsync(
    ServiceSpecification spec)
        {
            var query = _db.Services.Where(s => s.Status != Status.Deleted)
                .Include(p => p.ServiceImages.Where(i => i.IsActive == true))
                .Where(spec.ToExpression());

            var total = await query.CountAsync();
            var items = await query
                .Skip((spec.Page - 1) * spec.PageSize)
                .Take(spec.PageSize)
                .ToListAsync();

            return (items, total);
        }


        public Task<Models.Service?> GetServiceByIdAsync(Guid id)
        {
            return _db.Services
                .Include(p => p.ServiceImages.Where(i => i.IsActive == true))
                .FirstOrDefaultAsync(x => x.ServiceId == id);
        }



        public async Task UpdateServiceAsync(Models.Service Service)
        {
            _db.Services.Update(Service);
            await _db.SaveChangesAsync();
        }


        public async Task<bool> CheckServiceExistAsync(
      string ServiceName,
      Guid? excludeId = null)
        {
            return await _db.Services.Where(s => s.Status != Status.Deleted).AnyAsync(p =>
                p.ServiceName == ServiceName &&
                p.Status == Status.Active &&
                (!excludeId.HasValue || p.ServiceId != excludeId.Value));
        }


        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void DeleteServiceImage(ServiceImage image)
        {
            _db.ServiceImages.Remove(image);
        }
        //Vinh
        public async Task<IEnumerable<Models.Service>> GetAllActiveServicesAsync()
        {
            try
            {
                return await _db.Services
                    .Where(s => s.Status == Status.Active)
                    .Include(s => s.ServiceImages.Where(i => i.IsActive == true))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
