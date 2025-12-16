using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.Persistent.Abstraction
{
    public interface IRepo<T> where T : class
    {
        public Task AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(T entity);
        public IQueryable<T> GetAll();
    }
}
