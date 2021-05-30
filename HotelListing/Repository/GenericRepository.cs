using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace HotelListing.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DatabaseContext context;
        private readonly DbSet<T> db;

        public GenericRepository(DatabaseContext databaseContext)
        {
            context = databaseContext;
            db = context.Set<T>();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await db.FindAsync(id);
            db.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            db.RemoveRange(entities);
        }

        /// <summary>Gets one entity.</summary>
        /// <param name="expression">The lambda expression to define a query condition</param>
        /// <param name="includes">Specify related entities (foreign key(s)) to include additional details into the query result</param>
        /// <returns>
        ///   The first element that satisfies the specified condition or a default value if no such element is found
        /// </returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> expression = null, List<string> includes = null)
        {
            IQueryable<T> query = db;

            if(includes != null)
            {
                foreach (var includeProperety in includes)
                {
                    query = query.Include(includeProperety);
                }
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        /// <summary>Gets all entities.</summary>
        /// <param name="expression">The lambda expression to define a query condition</param>
        /// <param name="orderBy">The sorting operation.</param>
        /// <param name="includes">Specify related entities (foreign key(s)) to include additional details into the query result</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<IList<T>> GetAllAsync(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, List<string> includes = null)
        {
            IQueryable<T> query = db;

            // Filter
            if(expression != null)
            {
                query = query.Where(expression);
            }

            // Include additional details (related entities)
            if (includes != null)
            {
                foreach (var includeProperety in includes)
                {
                    query = query.Include(includeProperety);
                }
            }

            // Sort
            if(orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IPagedList<T>> GetAllPagedAsync(RequestParams requestParams, List<string> includes = null)
        {
            IQueryable<T> query = db;

            if (includes != null)
            {
                foreach (var includeProperety in includes)
                {
                    query = query.Include(includeProperety);
                }
            }

            return await query.AsNoTracking().ToPagedListAsync(requestParams.PageNumber, requestParams.PageSize);
        }

        public async Task InsertAsync(T entity)
        {
            await db.AddAsync(entity);
        }

        public async Task InsertRangeAsync(IEnumerable<T> entities)
        {
            await db.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            db.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
