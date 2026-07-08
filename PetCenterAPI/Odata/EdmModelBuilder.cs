using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;
using static PetCenterAPI.DTOs.Requests.DiseaseDTO;
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;
using static PetCenterAPI.DTOs.Requests.VetPetRequestDTO;

namespace PetCenterAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // ================== KHAI BÁO ENTITY SET ==================
            builder.EntitySet<ReadProductDTOForCustomer>("Products");
            builder.EntitySet<ReadBrandDTOForCustomer>("Brands");
            builder.EntitySet<ReadCategoryDTOForCustomer>("Categories");
            builder.EntitySet<ReadServiceDTOForCustomer>("Services");
            builder.EntitySet<ReadOrderListDTO>("Orders");
            builder.EntitySet<ReadDiseaseDTO>("Diseases");
            builder.EntitySet<ReadPetListDTO>("Pets");
            builder.EntitySet<ReadVetPetListDTO>("VetPets");


            // ================== KHAI BÁO KHÓA CHÍNH (KEY) ==================
            builder.EntityType<ReadProductDTOForCustomer>().HasKey(p => p.ProductId);
            builder.EntityType<ReadBrandDTOForCustomer>().HasKey(p => p.BrandId);
            builder.EntityType<ReadCategoryDTOForCustomer>().HasKey(p => p.CategoryId);
            builder.EntityType<ReadServiceDTOForCustomer>().HasKey(p => p.ServiceId);
            builder.EntityType<ReadOrderListDTO>().HasKey(o => o.OrderId);
            builder.EntityType<ReadPetListDTO>().HasKey(p => p.PetId);
            builder.EntityType<ReadVetPetListDTO>().HasKey(p => p.PetId);
            builder.EntityType<ReadDiseaseDTO>().HasKey(d => d.DiseaseId);
            return builder.GetEdmModel();
        }
    }
}