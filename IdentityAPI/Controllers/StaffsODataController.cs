using IdentityAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace IdentityAPI.Controllers
{
    [Route("odata/[controller]")]
    [ApiController]
    public class StaffsODataController : ODataController
    {
        private readonly PetCenterIdentityServiceDBContext _context;

        public StaffsODataController(PetCenterIdentityServiceDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery(MaxTop = 100)]
        public IQueryable<Staff> Get()
        {
            return _context.Staffs
                .Where(s => s.Roles.Any(r => r.RoleName == "Staff"))
                .AsQueryable();
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public SingleResult<Staff> Get([FromRoute] Guid key)
        {
            return SingleResult.Create(
                _context.Staffs.Where(s => s.StaffId == key));
        }
    }
}