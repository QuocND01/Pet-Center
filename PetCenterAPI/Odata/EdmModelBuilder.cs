using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

// 👇 1. THÊM 2 DÒNG NÀY ĐỂ TRỎ TỚI DTO CỦA PET 👇
using static PetCenterAPI.DTOs.Requests.CustomerProfile.PetRequestDTO;
using static PetCenterAPI.DTOs.Requests.VetPetRequestDTO;
// 👆 ========================================== 👆

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

            // 👇 2. KHAI BÁO ENTITY CHO PET (Customer & Vet) 👇
            builder.EntitySet<ReadPetListDTO>("Pets");
            builder.EntitySet<ReadVetPetListDTO>("VetPets");


            // ================== KHAI BÁO KHÓA CHÍNH (KEY) ==================
            builder.EntityType<ReadProductDTOForCustomer>().HasKey(p => p.ProductId);
            builder.EntityType<ReadBrandDTOForCustomer>().HasKey(p => p.BrandId);
            builder.EntityType<ReadCategoryDTOForCustomer>().HasKey(p => p.CategoryId);
            builder.EntityType<ReadServiceDTOForCustomer>().HasKey(p => p.ServiceId);
            builder.EntityType<ReadOrderListDTO>().HasKey(o => o.OrderId);

            // 👇 3. KHAI BÁO KHÓA CHÍNH (KEY) CHO PET 👇
            builder.EntityType<ReadPetListDTO>().HasKey(p => p.PetId);
            builder.EntityType<ReadVetPetListDTO>().HasKey(p => p.PetId);

            return builder.GetEdmModel();
        }
    }
}