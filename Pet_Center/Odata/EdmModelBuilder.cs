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

            builder.EntityType<ReadProductDTO>()
               .HasKey(p => p.ProductId);

            return builder.GetEdmModel();
        }
    }
}
