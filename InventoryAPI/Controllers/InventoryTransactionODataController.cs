using InventoryAPI.Service.Interface;
using InventoryAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace InventoryAPI.Controllers
{
    [Route("odata/[controller]")]
    public class InventoryTransactionODataController : ODataController
    {
        private readonly IInventoryTransactionService _service;

        public InventoryTransactionODataController(IInventoryTransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get(ODataQueryOptions<ReadTransactionDto> options)
        {
            var query = _service.GetTransactions();

            var result = options.ApplyTo(query);

            return Ok(result);
        }
    }
}
