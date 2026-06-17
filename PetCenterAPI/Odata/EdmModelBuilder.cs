using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using static PetCenterAPI.DTOs.Requests.Brand.BrandRequestDTO;
using static PetCenterAPI.DTOs.Requests.Category.CategoryRequestDTO;
using static PetCenterAPI.DTOs.Requests.Product.ProductRequestDTO;
using static PetCenterAPI.DTOs.Requests.Service.ServiceRequestDTO;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

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

            builder.EntitySet<ReadOrderListDTO>("Orders");
            builder.EntityType<ReadOrderListDTO>().HasKey(o => o.OrderId);

            builder.EntitySet<ReadOrderListDTO>("Orders");
            builder.EntityType<ReadOrderListDTO>().HasKey(o => o.OrderId);

            builder.EntityType<ReadServiceDTOForCustomer>()
              .HasKey(p => p.ServiceId);

            return builder.GetEdmModel();
        }
    }
}
