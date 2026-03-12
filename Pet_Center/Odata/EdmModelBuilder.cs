using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ProductAPI.DTOs;

namespace ProductAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // EntitySet = endpoint chính
            builder.EntitySet<ReadProductDTO>("Products");

            builder.EntitySet<ReadBrandDTOs>("Brands");

            builder.EntitySet<ReadCategoryDTOs>("Categories");

            builder.EntityType<ReadProductDTO>()
               .HasKey(p => p.ProductId);

            builder.EntityType<ReadBrandDTOs>()
               .HasKey(p => p.BrandId);

            builder.EntityType<ReadCategoryDTOs>()
              .HasKey(p => p.CategoryId);

            return builder.GetEdmModel();
        }
    }
}
