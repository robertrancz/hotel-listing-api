using AutoMapper;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<HotelsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<HotelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotels()
        {
            var hotels = await _unitOfWork.Hotels.GetAllAsync();
            var results = _mapper.Map<IList<HotelDto>>(hotels);
            return Ok(results);
        }

        [HttpGet("{id:int}", Name = "GetHotel")]
        [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotel(int id)
        {
            var hotel = await _unitOfWork.Hotels.GetAsync(q => q.Id == id, new List<string> { "Country" });
            var result = _mapper.Map<HotelDto>(hotel);

            if (hotel == null)
                return StatusCode(StatusCodes.Status204NoContent);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHotel([FromBody] HotelDtoCreate model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hotel = _mapper.Map<Hotel>(model);
            await _unitOfWork.Hotels.InsertAsync(hotel);
            await _unitOfWork.SaveAsync();

            // sets the 'Location' header with the route to the GET /hotel/{id} route
            return CreatedAtRoute("GetHotel", new { id = hotel.Id }, hotel);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] HotelDtoUpdate model)
        {
            if (!ModelState.IsValid || id < 1)
            {
                return BadRequest(ModelState);
            }

            var hotel = await _unitOfWork.Hotels.GetAsync(q => q.Id == id);
            if (hotel == null)
            {
                _logger.LogError($"Invalid update attempt in {nameof(UpdateHotel)}: resource with id={id} was not found.");
                return BadRequest("Invalid id");
            }

            _mapper.Map(model, hotel);
            _unitOfWork.Hotels.Update(hotel);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            if (id < 1)
            {
                _logger.LogError($"Invalid delete attempt in {nameof(DeleteHotel)}: resource with id={id} was not found.");
                return BadRequest("Invalid id");
            }

            var hotel = _unitOfWork.Hotels.GetAsync(q => q.Id == id);
            if (hotel == null)
            {
                _logger.LogError($"Invalid delete attempt in {nameof(DeleteHotel)}: resource with id={id} was not found.");
                return BadRequest($"Hotel with id={id} does not exist");
            }

            await _unitOfWork.Hotels.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
