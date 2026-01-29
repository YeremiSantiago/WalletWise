using Microsoft.Extensions.Logging;
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
        private readonly ILogger<T> _logger;

        public GenericService(IGenericRepository<T> repository, ILogger<T> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<Result<T?>> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                {
                    return Result<T?>.Failure($"La entidad con el Id {id} no pudo ser encontrada");

                }
                return Result<T?>.Success(entity);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex + " Ha ocurrido un error al obtener al entidad con el id " + typeof(T).Name);
                return Result<T?>.Failure("No se ha podido obtener la entidad");
            }

        }

        public virtual async Task<Result<IEnumerable<T>>> GetAllAsync()
        {
            try
            {
                return Result<IEnumerable<T>>.Success(await _repository.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex + "A ocurrido un error al obtener todas entidades");
                return Result<IEnumerable<T>>.Failure("No se a podido listar todas las entidades");
            }
        }

        public virtual async Task<Result<T>> AddAsync(T entity)
        {
            try
            {
                return Result<T>.Success(
                    await _repository.AddAsync(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex + " A ocurrido un error inesperado al crear una entidad ", typeof(T).Name);
                return Result<T>.Failure("No se a podido crear la entidad");
            }
        }


        public virtual async Task<Result<T>> UpdateAsync(T entity)
        {
            try
            {
                await _repository.UpdateAsync(entity);

                return Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex + " Fallo al actualizar la entidad ", typeof(T).Name);
                return Result<T>.Failure("No se a podido actualizar la entidad");
            }
        }

        public virtual async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {

                await _repository.RemoveAsync(id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " A ocurrido un fallo al borrar la entidad " + id);
                return Result<bool>.Failure("No se ha podido eliminar la entidad");
            }
        }


    }
}
