using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Common;
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
            T? entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                throw new WalletWise.Application.Exceptions.NotFoundException($"La entidad con el Id {id} no existe");
            }

            return Result<ResDto?>.Success(_mapper.Map<ResDto>(entity));
        }

        public virtual async Task<Result<IEnumerable<ResDto>>> GetAllAsync()
        {
            IEnumerable<T> values = await _repository.GetAllAsync();

            var a = _mapper.Map<IEnumerable<ResDto>>(values);

            return Result<IEnumerable<ResDto>>.Success(a);
        }

        public virtual async Task<Result<ResDto>> AddAsync(CreatDto DtoRequest)
        {
            T entity = _mapper.Map<T>(DtoRequest);

            T values = await _repository.AddAsync(entity);

            return Result<ResDto>.Success(_mapper.Map<ResDto>(values));
        }


        public virtual async Task<Result<ResDto>> UpdateAsync(int id, UpdaDto dtoRequest)
        {
            T? exist = await _repository.GetByIdAsync(id);

            if (exist == null)
            {
                throw new WalletWise.Application.Exceptions.NotFoundException($"La entidad con el Id {id} no existe");
            }

            T entity = _mapper.Map<T>(dtoRequest);

            await _repository.UpdateAsync(entity);

            return Result<ResDto>.Success(_mapper.Map<ResDto>(entity));
        }

        public virtual async Task<Result<bool>> DeleteAsync(int id)
        {
            var exist = await _repository.GetByIdAsync(id);

            if (exist == null)
            {
                throw new WalletWise.Application.Exceptions.NotFoundException($"La entidad con el Id {id} no pudo ser encontrada");
            }

            await _repository.RemoveAsync(id);
            return Result<bool>.Success(true);
        }
    }
}
