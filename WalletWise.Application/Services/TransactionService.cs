using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Xml.Linq;
using WalletWise.Application.Constants;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace WalletWise.Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionResponseDto, CreateTransactionRequestDto, UpdateTransactionRequestDto>, ITransactionService
    {

        private readonly ITransactionRepository _transactionRepository;
        private readonly IClock _clock;


        public TransactionService(ITransactionRepository transactionRepository, IClock clock, ILogger<Transaction> logger, IMapper mapper) : base(transactionRepository, logger, mapper)
        {
            _transactionRepository = transactionRepository;
            _clock = clock;
        }


        public async Task<Result<TransactionResponseDto>> CreateTransactionAsync(CreateTransactionRequestDto transactionDto)
        {
            try
            {

                var transaction = _mapper.Map<Transaction>(transactionDto);

                transaction.UserId = DefaultUser.Id;

                if (transaction.Amount <= 0)
                {
                    return Result<TransactionResponseDto>.Failure("El monto tiene que ser mayor a cero");
                }

                if (transaction.Date <= _clock.UtcNow())
                {
                    return Result<TransactionResponseDto>.Failure("La fecha no puede futura para posibles gastos");
                }

                await _transactionRepository.AddAsync(transaction);

                return Result<TransactionResponseDto>.Success(_mapper.Map<TransactionResponseDto>(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo a la hora de crear un transaccion");
                return Result<TransactionResponseDto>.Failure("No se ha podido crear la transaccion");
            }
        }

        public async Task<Result<TransactionResponseDto>> UpdateTransactionAsync(int id, UpdateTransactionRequestDto transactionDto)
        {
            try
            {

                var exist = await _transactionRepository.GetByIdAsync(id);

                if (exist is null)
                {
                    return Result<TransactionResponseDto>.Failure($"La transaction con el id {id} no existe");
                }

                _mapper.Map(transactionDto, exist);

                return Result<TransactionResponseDto>.Success(_mapper.Map<TransactionResponseDto>(exist));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un error inesperado al actualizar la transaccion {Id}", id);
                return Result<TransactionResponseDto>.Failure("No se ha podido actualizar la transaccion con el id" + id);
            }
        }

        public async Task<Result<bool>> DeleteTransactionAsync(int id)
        {
            try
            {
                var exist = await _transactionRepository.GetByIdAsync(id);

                if (exist is null)
                {
                    return Result<bool>.Failure($"La transaction con el id {id} no existe");
                }

                await _transactionRepository.RemoveAsync(id);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un error inespesperado al borrar la transaccion con el id {Id}", id);
                return Result<bool>.Failure($"No se ha podido eliminar la transaccion con el id {id} ");
            }
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                var transactions = await _transactionRepository.GetByDateRangeAsync(start, end);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se han podido filtar las transacciones por fecha debido a un error inesperado");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se ha podido obtener las transacciones filtradas por fecha");
            }

        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetByTypeTransactionAsync(TypeTransaction type)
        {
            try
            {

                var transactions = await _transactionRepository.GetByTypeTransactionAsync(type);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se ha podido filtar las transacciones por su tipo debido a un fallo inesperado");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se ha podido obtener las transacciones por su tipo");
            }

        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetAllTransactionsByCategoryAsync(int id)
        {
            try
            {
                var transactions = await _transactionRepository.GetAllTransactionsByCategoryAsync(id);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al filtrar las transacciones por categoria");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se han podido obtener las transacciones por categoria");
            }
        }

    }
}
