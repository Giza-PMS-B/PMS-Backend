using System;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Site.Infrastrcure.Persistent.Repositories;

public class GenericRepo<T> : IRepo<T> where T : class
{
    private readonly DbContext _dbContext;
    public GenericRepo(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
    }

    public Task DeleteAsync(T entity)
    {
        return Task.FromResult(_dbContext.Set<T>().Remove(entity));
    }

    public IQueryable<T> GetAll()
    {
        return _dbContext.Set<T>().AsQueryable();
    }

    public async Task UpdateAsync(T entity)
    {
        await Task.FromResult(_dbContext.Set<T>().Update(entity));
    }
}
