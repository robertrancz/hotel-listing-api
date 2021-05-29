using System;
using System.ComponentModel.DataAnnotations;

namespace HotelListing.Models
{
    public class HotelDtoCreate
    {
        [Required]
        [StringLength(maximumLength: 150, ErrorMessage = "Hotel name is too long")]
        public string Name { get; set; }

        [Required]
        [StringLength(maximumLength: 250, ErrorMessage = "Hotel address is too long")]
        public string Address { get; set; }

        [Required]
        [Range(1,5)]
        public double Rating { get; set; }

        [Required]
        public int CountryId { get; set; }        
    }

    public class HotelDto : HotelDtoCreate
    {
        public int Id { get; set; }
        public CountryDto Country { get; set; }
    }
}
