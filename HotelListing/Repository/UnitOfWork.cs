using HotelListing.Data;
using HotelListing.IRepository;
using System;
using System.Threading.Tasks;

namespace HotelListing.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext context;
        private IGenericRepository<Country> countries;
        private IGenericRepository<Hotel> hotels;

        public UnitOfWork(DatabaseContext databaseContext)
        {
            context = databaseContext;
        }
        public IGenericRepository<Country> Countries => countries ??= new GenericRepository<Country>(context);

        public IGenericRepository<Hotel> Hotels => hotels ??= new GenericRepository<Hotel>(context);

        public void Dispose()
        {
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
