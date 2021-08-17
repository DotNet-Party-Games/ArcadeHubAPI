using System.Collections.Generic;
using System.Threading.Tasks;

namespace HubDL {
    public interface IDatabase<T> where T: class {
        Task<T> Create(T model);
        Task<bool> Delete(T model);
        Task<IList<T>> Query(QueryOptions<T> options);
        Task<T> FindSingle(QueryOptions<T> options);
        Task<bool> Update(T model);
        Task<bool> Save();
        void FlagForRemoval(T model);
    }
}