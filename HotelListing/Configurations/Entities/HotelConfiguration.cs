using HotelListing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Configurations.Entities
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasData(
                new Hotel
                {
                    Id = 1,
                    Name = "Sandals Resort And Spa",
                    Address = "Negril",
                    CountryId = 4,
                    Rating = 4.5
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Grand Palladium",
                    Address = "Nassua",
                    CountryId = 3,
                    Rating = 4.0
                },
                new Hotel
                {
                    Id = 3,
                    Name = "Bella Muzica",
                    Address = "Brasov",
                    CountryId = 1,
                    Rating = 4.5
                },
                new Hotel
                {
                    Id = 4,
                    Name = "Aro Palace",
                    Address = "Brasov",
                    CountryId = 1,
                    Rating = 4.0
                });
        }
    }
}
