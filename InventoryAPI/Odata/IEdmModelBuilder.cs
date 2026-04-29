using InventoryAPI.DTOs;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace InventoryAPI.Odata
{
    public static class IEdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ReadInventoryDto>("Inventory");
            builder.EntitySet<ReadTransactionDto>("InventoryTransactions");

            return builder.GetEdmModel();
        }
    }
}
