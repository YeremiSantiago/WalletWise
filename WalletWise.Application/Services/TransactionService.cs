using WalletWise.Application.Interfaces;
using WalletWise.Domain.Interfaces;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common;
using WalletWise.Application.Constants;
using Microsoft.Extensions.Logging;


namespace WalletWise.Application.Services
{
    public class TransactionService : GenericService<Transaction>, ITransactionService
    {

        private readonly ITransactionRepository _transactionRepository;
        private readonly IClock _clock;

        public TransactionService(ITransactionRepository transactionRepository, IClock clock, ILogger<Transaction> logger) : base(transactionRepository, logger)
        {
            _transactionRepository = transactionRepository;
            _clock = clock;
        }

        public override async Task<Result<Transaction>> AddAsync(Transaction transaction)
        {
            if(transaction.Amount < 1)
            {
                return Result<Transaction>.Failure("El monto tiene que ser mayor a cero");
            }

            if(transaction.Date > _clock.UtcNow())
            {
                return Result<Transaction>.Failure("La fecha no puede futura para posibles gastos");
            }
            transaction.UserId = DefaultUser.Id;

            return Result<Transaction>.Success(
                await _transactionRepository.AddAsync(transaction));
        }

        public override async Task<Result<IEnumerable<Transaction>>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return Result<IEnumerable<Transaction>>.Success(transactions);

        }

        public async Task<Result<IEnumerable<Transaction>>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            
            var transactions = await _transactionRepository.GetAllAsync();

            var filter = transactions.Where(x => x.Date >= start && x.Date <= end);

            return Result<IEnumerable<Transaction>>.Success(filter);

        }

        public async Task<Result<IEnumerable<Transaction>>> GetByTypeTransactionAsync(TypeTransaction type) 
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return Result<IEnumerable<Transaction>>.Success(transactions.Where(x => x.Type == type));
                
        }

        public async Task<Result<IEnumerable<Transaction>>> GetByCategoryAsync(int id)
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return Result<IEnumerable<Transaction>>.Success(transactions.Where(x => x.CategoryId == id)); 
        }

    }
}
