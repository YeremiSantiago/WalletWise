using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Context;
using WalletWise.Application.Exceptions;

namespace WalletWise.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {

            return await _context.Set<T>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                .AsNoTracking().
                ToListAsync();

        }

        public virtual async Task<T> AddAsync(T entity)
        {

            await _context.Set<T>().AddAsync(entity);

            try
            {

            await _context.SaveChangesAsync();

            }
            catch (DbUpdateException ex)
            {
                throw new UniqueConstraintViolationException("Se violó una restriccion de unicidad en la base de datos.", ex);
            }

            return entity;

        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(int id)
        {
            var exist = await _context.Set<T>().FindAsync(id);

            if (exist != null)
            {
                _context.Set<T>().Remove(exist);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var exist = await _context.Set<T>().AnyAsync(predicate);

            return exist;
        }


    }
}

