using CustomerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace CustomerAPI.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    public class CustomersODataController : ODataController
    {
        private readonly PetCenterCustomerServiceContext _context; 

        public CustomersODataController(PetCenterCustomerServiceContext context)
        {
            _context = context;
        }

        // GET: odata/Customers
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Customer> Get()
        {
            return _context.Customers.AsQueryable();
        }

        // GET: odata/Customers({key})
        [HttpGet("{key}")]
        [Authorize(Roles = "Admin,Staff")]
        [EnableQuery]
        public SingleResult<Customer> Get([FromRoute] Guid key)
        {
            return SingleResult.Create(
                _context.Customers.Where(x => x.CustomerId == key));
        }
    }
}
