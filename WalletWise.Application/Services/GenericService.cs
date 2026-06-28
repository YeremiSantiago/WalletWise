using AutoMapper;
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
    public class GenericService<T, ResDto, CreatDto, UpdaDto> : IGenericService<ResDto, CreatDto, UpdaDto> 
        where T : class 
        where ResDto : class
        where CreatDto : class
        where UpdaDto : class
    {
        private readonly IGenericRepository<T> _repository;
        protected readonly ILogger<T> _logger;
        protected readonly IMapper _mapper;

        public GenericService(IGenericRepository<T> repository, ILogger<T> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public virtual async Task<Result<ResDto?>> GetByIdAsync(int id)
        {
            try
            {
                T? entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                {
                    return Result<ResDto?>.Failure($"La entidad con el Id {id} no existe");

                }

                return Result<ResDto?>.Success(_mapper.Map<ResDto>(entity));

            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Ha ocurrido un error al obtener la entidad {T} con id {Id}", typeof(T).Name, id);
                return Result<ResDto?>.Failure("No se ha podido obtener la entidad");
            }

        }

        public virtual async Task<Result<IEnumerable<ResDto>>> GetAllAsync()
        {
            try
            {
                IEnumerable<T> values = await _repository.GetAllAsync();

                var a = _mapper.Map<IEnumerable<ResDto>>(values);

                return Result<IEnumerable<ResDto>>.Success(a);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "A ocurrido un error al obtener todas entidades");
                return Result<IEnumerable<ResDto>>.Failure("No se a podido listar todas las entidades");
            }
        }

        public virtual async Task<Result<ResDto>> AddAsync(CreatDto DtoRequest)
        {
            try
            {
                T entity = _mapper.Map<T>(DtoRequest);

                T values = await _repository.AddAsync(entity);

                return Result<ResDto>.Success(_mapper.Map<ResDto>(values));
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "A ocurrido un error inesperado al crear una entidad {T}", typeof(T).Name);
                return Result<ResDto>.Failure("No se a podido crear la entidad");
            }
        }


        public virtual async Task<Result<ResDto>> UpdateAsync(int id, UpdaDto dtoRequest)
        {
            try
            {
                T? exist = await _repository.GetByIdAsync(id);

                if (exist == null)
                {
                    return Result<ResDto>.Failure($"La entidad con el Id {id} no existe");
                }

                T entity = _mapper.Map<T>(dtoRequest);

                await _repository.UpdateAsync(entity);

                return Result<ResDto>.Success(_mapper.Map<ResDto>(entity));
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Fallo al actualizar la entidad {T}", typeof(T).Name);
                return Result<ResDto>.Failure("No se a podido actualizar la entidad");
            }
        }

        public virtual async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var exist = await _repository.GetByIdAsync(id);

                if (exist == null)
                {
                    return Result<bool>.Failure($"La entidad con el Id {id} no pudo ser encontrada");
                }

                await _repository.RemoveAsync(id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "A ocurrido un fallo al borrar la entidad con el id {Id}", id);
                return Result<bool>.Failure("No se ha podido eliminar la entidad");
            }
        }


    }
}
