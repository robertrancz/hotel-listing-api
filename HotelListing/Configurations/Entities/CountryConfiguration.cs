using HotelListing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Configurations.Entities
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasData(
                new Country
                {
                    Id = 1,
                    Name = "Romania",
                    ShortName = "RO"
                },
                new Country
                {
                    Id = 2,
                    Name = "United States",
                    ShortName = "US"
                },
                new Country
                {
                    Id = 3,
                    Name = "Bahamas",
                    ShortName = "BS"
                },
                new Country
                {
                    Id = 4,
                    Name = "Jamaica",
                    ShortName = "JM"
                });
        }
    }
}
