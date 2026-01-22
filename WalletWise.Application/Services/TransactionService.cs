using WalletWise.Application.Interfaces;
using WalletWise.Domain.Interfaces;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common;
using WalletWise.Application.Constants;


namespace WalletWise.Application.Services
{
    public class TransactionService : GenericService<Transaction>, ITransactionService
    {

        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository ) : base(transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public override async Task<Result<Transaction>> AddAsync(Transaction transaction)
        {
            if(transaction.Amount < 1)
            {
                return Result<Transaction>.Failure("El monto tiene que ser mayor a cero");
            }

            if(transaction.Date > DateTime.Now)
            {
                return Result<Transaction>.Failure("La fecha no puede futura para posibles gastos");
            }
            transaction.UserId = DefaultUser.Id;

            return Result<Transaction>.Success(
                await _transactionRepository.AddAsync(transaction));
        }

        public override async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return transactions;

        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            
            var transactions = await _transactionRepository.GetAllAsync();

            var filter = transactions.Where(x => x.Date >= start && x.Date <= end);

            return filter;

        }

        public async Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(TypeTransaction type) 
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return transactions.Where(x => x.Type == type);
                
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAsync(int id)
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return transactions.Where(x => x.CategoryId == id); 
        }

    }
}
