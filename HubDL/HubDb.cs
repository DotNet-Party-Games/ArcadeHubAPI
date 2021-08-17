using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace HubDL {
    public class HubDB<T>: IDatabase<T> where T: class {
        private readonly HubDbContext _context;

        public HubDB(HubDbContext context) {
            _context = context;
        }

        public async Task<T> Create(T model) {
            _context.Set<T>().Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> Delete(T model) {
            _context.Set<T>().Attach(model);
            _context.Set<T>().Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<T> FindSingle(QueryOptions<T> options) {
            var query = BuildQuery(options);
            return await Task.FromResult(query.SingleOrDefault());
        }

        public void FlagForRemoval(T model) {
            _context.Set<T>().Attach(model);
            _context.Set<T>().Remove(model);
        }

        public async Task<IList<T>> Query(QueryOptions<T> options) {
            var query = BuildQuery(options);
            return await Task.FromResult(query.ToList());
        }

        private IQueryable<T> BuildQuery(QueryOptions<T> options) {
            IQueryable<T> queryableQuery = _context.Set<T>().AsQueryable();
            if (options.Includes != null) {
                foreach (string inc in options.Includes) {
                    queryableQuery = queryableQuery.Include(inc);
                }
            }

            // Add conditions
            var enumerableQuery = queryableQuery.AsEnumerable<T>();
            if (options.Conditions != null) {
                foreach (Func<T, bool> cond in options.Conditions) {
                    enumerableQuery = enumerableQuery.Where(cond);
                }
            }

            queryableQuery = enumerableQuery.AsQueryable();

            // Add paging
            if (options.PageSize > 0) {
                queryableQuery = queryableQuery.Page(options.PageNumber, options.PageSize);
            }

            // Add OrderBy
            if (options.OrderBy != null) {
                (string propertyName, bool desc) = ParseSortOrder(options.OrderBy);
                if (propertyName != null) {
                    queryableQuery = (desc)
                        ? queryableQuery.OrderBy(propertyName + " desc")
                        : queryableQuery.OrderBy(propertyName);
                }
            }

            return queryableQuery;
        }

        private static (string, bool) ParseSortOrder(string orderBy) {
            if (orderBy == null) return (null, false);
            string[] tokens = orderBy.Split("_");
            if (tokens.Length == 0 || tokens[0] == "") return (null, false);

            IEnumerable<string> propNames = typeof(T).GetProperties().Select(p => p.Name);
            string propName = (propNames.Contains(tokens[0])) ? tokens[0]: null;

            bool desc = (tokens.Length >= 2 && tokens[1] == "desc"); 

            return (propName, desc);
        }

        public async Task<bool> Save() {
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Update(T model) {
            _context.Set<T>().Attach(model);

            var entry = _context.Entry(model);
            entry.State = EntityState.Modified;

            foreach (var prop in entry.Properties.Where(p => !p.Metadata.IsKey())) {
                if (prop.CurrentValue == null) {
                    prop.IsModified = false;
                }
            }

            await _context.SaveChangesAsync();
            entry.State = EntityState.Detached;
            return true;
        }
    }
}