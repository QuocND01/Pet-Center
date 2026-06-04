using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PetCenterAPI.DTOs;

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

            builder.EntityType<ReadProductDTOForCustomer>()
               .HasKey(p => p.ProductId);

            builder.EntityType<ReadBrandDTOForCustomer>()
               .HasKey(p => p.BrandId);

            builder.EntityType<ReadCategoryDTOForCustomer>()
              .HasKey(p => p.CategoryId);

            return builder.GetEdmModel();
        }
    }
}
