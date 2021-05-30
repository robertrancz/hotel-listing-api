using HotelListing.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelListing.Controllers.V2
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private DatabaseContext _context;

        public CountriesController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            return Ok(_context.Countries);
        }
    }
}
