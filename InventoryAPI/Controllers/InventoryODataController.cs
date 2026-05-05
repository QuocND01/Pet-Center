using InventoryAPI.DTOs;
using InventoryAPI.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace InventoryAPI.Controllers
{
    [Route("odata/[controller]")]
    public class InventoryODataController : ODataController
    {
        private readonly IInventoryService _service;

        public InventoryODataController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get(ODataQueryOptions<ReadInventoryDto> options)
        {
            var query = _service.GetInventories();

            var settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            };

            var result = options.ApplyTo(query, settings);

            return Ok(result);
        }
    }
}
