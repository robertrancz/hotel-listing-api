using AutoMapper;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Models;
using Marvin.Cache.Headers;
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
    public class CountriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CountriesController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<CountryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries([FromQuery] RequestParams requestParams)
        {
            var countries = await _unitOfWork.Countries.GetAllPagedAsync(requestParams, new List<string> { "Hotels" });
            var results = _mapper.Map<IList<CountryDto>>(countries);
            return Ok(results);
        }

        [HttpGet("{id:int}", Name = "GetCountry")]
        [HttpCacheValidation(NoCache = true)]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountry(int id)
        {
            var country = await _unitOfWork.Countries.GetAsync(q => q.Id == id, new List<string> { "Hotels" });
            var result = _mapper.Map<CountryDto>(country);

            if (country == null)
                return StatusCode(StatusCodes.Status204NoContent); //, $"The request was successful, but there is no country with id={id}");

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CountryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCountry([FromBody] CountryDtoCreate model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var country = _mapper.Map<Country>(model);
            await _unitOfWork.Countries.InsertAsync(country);
            await _unitOfWork.SaveAsync();

            // sets the 'Location' header with the route to the GET /country/{id} route
            return CreatedAtRoute("GetCountry", new { id = country.Id }, country);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryDtoUpdate model)
        {
            if (!ModelState.IsValid || id < 1)
            {
                return BadRequest(ModelState);
            }

            var country = await _unitOfWork.Countries.GetAsync(q => q.Id == id);
            if (country == null)
            {
                _logger.LogError($"Invalid update attempt in {nameof(UpdateCountry)}: resource with id={id} was not found.");
                return BadRequest("Invalid id");
            }

            _mapper.Map(model, country);
            _unitOfWork.Countries.Update(country);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        /// <summary>Deletes the country and ALL HOTELS BELONGING TO THAT COUNTRY!!! Use with caution!</summary>        
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (id < 1)
            {
                _logger.LogError($"Invalid delete attempt in {nameof(DeleteCountry)}: resource with id={id} was not found.");
                return BadRequest("Invalid id");
            }

            var country = _unitOfWork.Countries.GetAsync(q => q.Id == id);
            if (country == null)
            {
                _logger.LogError($"Invalid delete attempt in {nameof(DeleteCountry)}: resource with id={id} was not found.");
                return BadRequest($"Country with id={id} does not exist");
            }

            await _unitOfWork.Countries.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
