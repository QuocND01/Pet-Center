using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PetCenterAPI.DTOs;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;

namespace PetCenterAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // EntitySet = endpoint chính
            builder.EntitySet<ReadProductDTOForCustomer>("Products");

            builder.EntitySet<ReadBrandDTOForCustomer>("Brands");

            builder.EntitySet<ReadCategoryDTOForCustomer>("Categories");

            builder.EntitySet<ReadServiceDTOForCustomer>("Services");

            builder.EntityType<ReadProductDTOForCustomer>()
               .HasKey(p => p.ProductId);

            builder.EntityType<ReadBrandDTOForCustomer>()
               .HasKey(p => p.BrandId);

            builder.EntityType<ReadCategoryDTOForCustomer>()
              .HasKey(p => p.CategoryId);

            builder.EntityType<ReadServiceDTOForCustomer>()
              .HasKey(p => p.ServiceId);

            return builder.GetEdmModel();
        }
    }
}
