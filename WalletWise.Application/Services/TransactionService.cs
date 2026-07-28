using AutoMapper;
using Microsoft.Extensions.Logging;
using WalletWise.Application.Common;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common.Pagination;
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
            var transactions = await _transactionRepository.GetAllByUserAsync(_currentUserService.UserId!);

            return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
        }

        public async Task<Result<TransactionResponseDto?>> GetTransactionByIdAsync(int id)
        {
            var result = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (result is null)
            {
                throw new Exceptions.NotFoundException($"La transaccion con el id {id} no existe");
            }

            return Result<TransactionResponseDto?>.Success(_mapper.Map<TransactionResponseDto>(result));
        }

        public async Task<Result<TransactionResponseDto>> CreateTransactionAsync(CreateTransactionRequestDto transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);


            if (transaction.Amount <= 0)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_INVALID_AMOUNT, "El monto tiene que ser mayor a cero");
            }


            if (transaction.Date > _clock.UtcNow())
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_FUTURE_DATE_NOT_ALLOWED, "La fecha no puede ser futura para posibles gastos");
            }


            var walletExists = await _walletRepository.GetByIdForUserAsync(transaction.WalletId, _currentUserService.UserId!);

            if (walletExists == null)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_WALLET_NOT_FOUND, $"La wallet con ID {transaction.WalletId} no existe");
            }

            var categoryExists = await _categoryRepository.GetCategoryActiveByIdAsync(transaction.CategoryId, _currentUserService.UserId!);

            if (categoryExists == null)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_NOT_FOUND, $"La categoría con ID {transaction.CategoryId} no existe");
            }

            if (categoryExists.Type != transaction.Type)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_TYPE_MISMATCH, "El tipo de la transacción no coincide con el de la categoría");
            }

            transaction.UserId = _currentUserService.UserId!;

            await _transactionRepository.AddAsync(transaction);

            return Result<TransactionResponseDto>.Success(_mapper.Map<TransactionResponseDto>(transaction));
        }

        public async Task<Result<TransactionResponseDto>> UpdateTransactionAsync(int id, UpdateTransactionRequestDto transactionDto)
        {

            var exist = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (exist is null)
            {
                throw new Exceptions.NotFoundException($"La transaction con el id {id} no existe");
            }

            var categoryExists = await _categoryRepository.GetCategoryActiveByIdAsync(transactionDto.CategoryId, _currentUserService.UserId!);
            if (categoryExists == null)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_NOT_FOUND, $"La categoría con ID {transactionDto.CategoryId} no existe");
            }

            if (categoryExists.Type != transactionDto.Type)
            {
                return Result<TransactionResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_TYPE_MISMATCH, "El tipo de la transacción no coincide con el de la categoría");
            }

            _mapper.Map(transactionDto, exist);

            await _transactionRepository.UpdateAsync(exist);

            return Result<TransactionResponseDto>.Success(_mapper.Map<TransactionResponseDto>(exist));
        }

        public async Task<Result<bool>> DeleteTransactionAsync(int id)
        {
            var exist = await _transactionRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (exist is null)
            {
                throw new Exceptions.NotFoundException($"La transaction con el id {id} no existe");
            }

            await _transactionRepository.RemoveAsync(id);

            return Result<bool>.Success(true);
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var transactions = await _transactionRepository.GetByDateRangeAsync(_currentUserService.UserId!, start, end);

            return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetByTypeTransactionAsync(TypeTransaction type)
        {

            var transactions = await _transactionRepository.GetByTypeTransactionAsync(_currentUserService.UserId!, type);

            return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
        }

        public async Task<Result<IEnumerable<TransactionResponseDto>>> GetAllTransactionsByCategoryAsync(int id)
        {
            var transactions = await _transactionRepository.GetAllTransactionsByCategoryAsync(_currentUserService.UserId!, id);

            return Result<IEnumerable<TransactionResponseDto>>.Success(_mapper.Map<IEnumerable<TransactionResponseDto>>(transactions));
        }

        public async Task<Result<PagedResult<TransactionResponseDto>>> GetPagedTransactionsAsync(TransactionFilterParams filterParams)
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

        public async Task<Result<TransactionSummaryResponseDto>> GetSummaryAsync(TransactionFilterParams filterParams)
        {
            var userId = _currentUserService.UserId;

            if (userId == null)
                return Result<TransactionSummaryResponseDto>.Failure(BusinessErrorCodes.ERR_UNAUTHORIZED, "Usuario no autenticado");

            var (totalIncome, totalExpense, balance) = await _transactionRepository.GetSummaryAsync(userId, filterParams);

            return Result<TransactionSummaryResponseDto>.Success(new TransactionSummaryResponseDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance
            });
        }
    }
}
