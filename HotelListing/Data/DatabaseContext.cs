using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        { }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Hotel> Hotels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Country>().HasData(
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

            builder.Entity<Hotel>().HasData(
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

            base.OnModelCreating(builder);
        }
    }
}
