using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return null;

            }
            return entity;

        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public virtual async Task<Result<T>> AddAsync(T entity)
        {
            return Result<T>.Success(
                await _repository.AddAsync(entity));
        }


        public virtual async Task<Result<T>> UpdateAsync(T entity)
        {
            await _repository.UpdateAsync(entity);
           
           return Result<T>.Success(entity);
        }

        public virtual async Task<Result<bool>> DeleteAsync(int id)
        {
            await _repository.RemoveAsync(id);
            return Result<bool>.Success(true);

        }


    }
}
