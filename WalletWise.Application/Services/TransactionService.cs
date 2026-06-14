using AutoMapper;
using Microsoft.Extensions.Logging;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Pagination;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;


namespace WalletWise.Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionResponseDto, CreateTransactionRequestDto, UpdateTransactionRequestDto>, ITransactionService
    {

        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IClock _clock;
        private readonly ICurrentUserService _currentUserService;


        public TransactionService(ITransactionRepository transactionRepository, IClock clock, ILogger<Transaction> logger, IMapper mapper, IWalletRepository walletRepository, ICategoryRepository categoryRepository, ICurrentUserService currentUserService) : base(transactionRepository, logger, mapper)
        {
            _transactionRepository = transactionRepository;
            _clock = clock;
            _walletRepository = walletRepository;
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetAllTransactionsAsync()
        {
            try
            {
                var transactions = await _transactionRepository.GetAllByUserAsync(_currentUserService.UserId!);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A ocurrido un fallo inesperado al obtener todas las transacciones");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se ha podido obtener todas las transacciones");
            }
        }

        public async Task<Result<TransactionResponseDto?>> GetTransactionByIdAsync(int id)
        {
            try
            {
                var result = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

                if (result is null)
                {
                    return Result<TransactionResponseDto?>.Failure($"La transaccion con el id {id} no existe");
                }

                return Result<TransactionResponseDto?>.Success(_mapper.Map<TransactionResponseDto>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo inesperado al obtener la transaccion con el id {Id} ", id);
                return Result<TransactionResponseDto?>.Failure($"No se ha podido obtener la transaccion con el id {id}");
            }
        }

        public async Task<Result<TransactionResponseDto>> CreateTransactionAsync(CreateTransactionRequestDto transactionDto)
        {
            try
            {
                var transaction = _mapper.Map<Transaction>(transactionDto);


                if (transaction.Amount <= 0)
                {
                    return Result<TransactionResponseDto>.Failure("El monto tiene que ser mayor a cero");
                }


                if (transaction.Date > _clock.UtcNow())
                {
                    return Result<TransactionResponseDto>.Failure("La fecha no puede ser futura para posibles gastos");
                }


                var walletExists = await _walletRepository.GetByIdForUserAsync(transaction.WalletId, _currentUserService.UserId!);

                if (walletExists == null)
                {
                    return Result<TransactionResponseDto>.Failure($"La wallet con ID {transaction.WalletId} no existe");
                }

                var categoryExists = await _categoryRepository.GetCategoryActiveByIdAsync(transaction.CategoryId, _currentUserService.UserId!);

                if (categoryExists == null)
                {
                    return Result<TransactionResponseDto>.Failure($"La categoría con ID {transaction.CategoryId} no existe");
                }

                transaction.UserId = _currentUserService.UserId!;

                await _transactionRepository.AddAsync(transaction);

                return Result<TransactionResponseDto>.Success(_mapper.Map<TransactionResponseDto>(transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo a la hora de crear una transaccion");
                return Result<TransactionResponseDto>.Failure("No se ha podido crear la transaccion");
            }
        }

        public async Task<Result<TransactionResponseDto>> UpdateTransactionAsync(int id, UpdateTransactionRequestDto transactionDto)
        {
            try
            {

                var exist = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

                if (exist is null)
                {
                    return Result<TransactionResponseDto>.Failure($"La transaction con el id {id} no existe");
                }

                _mapper.Map(transactionDto, exist);

                await _transactionRepository.UpdateAsync(exist);

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
                var exist = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

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
                var transactions = await _transactionRepository.GetByDateRangeAsync(_currentUserService.UserId!, start, end);

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

                var transactions = await _transactionRepository.GetByTypeTransactionAsync(_currentUserService.UserId!, type);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se ha podido filtrar las transacciones por su tipo debido a un fallo inesperado");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se ha podido obtener las transacciones por su tipo");
            }

        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetAllTransactionsByCategoryAsync(int id)
        {
            try
            {
                var transactions = await _transactionRepository.GetAllTransactionsByCategoryAsync(_currentUserService.UserId!, id);

                return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al filtrar las transacciones por categoria");
                return Result<IEnumerable<TransactionResponseDto>>.Failure("No se han podido obtener las transacciones por categoria");
            }
        }

        public async Task<Result<PagedResult<TransactionResponseDto>>> GetPagedTransactionsAsync(TransactionFilterParams filterParams)
        {
            try
            {
                
                var pagedResult = await _transactionRepository.GetPagedTransactionsAsync(_currentUserService.UserId!, filterParams);
                
                var dtos = _mapper.Map<List<TransactionResponseDto>>(pagedResult.Items);
                
                var resultDto = new PagedResult<TransactionResponseDto>(
                    dtos, 
                    pagedResult.TotalRecords, 
                    pagedResult.CurrentPage, 
                    pagedResult.PageSize);
                return Result<PagedResult<TransactionResponseDto>>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un error inesperado al obtener las transacciones paginadas.");
                return Result<PagedResult<TransactionResponseDto>>.Failure("No se han podido obtener las transacciones.");
            }
        }

    }
}
