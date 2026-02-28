
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Interfaces
{
    public interface ITransactionService : IGenericService<TransactionResponseDto, CreateTransactionRequestDto, UpdateTransactionRequestDto>
    {
        Task<Result<TransactionResponseDto>> CreateTransactionAsync(CreateTransactionRequestDto transactionDto);
        Task<Result<TransactionResponseDto>> UpdateTransactionAsync(int id, UpdateTransactionRequestDto transactionDto);
        Task<Result<bool>> DeleteTransactionAsync(int id);
        Task<Result<IEnumerable<TransactionResponseDto>>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<Result<IEnumerable<TransactionResponseDto>>> GetByTypeTransactionAsync(TypeTransaction typeTransaction);
        Task<Result<IEnumerable<TransactionResponseDto>>> GetAllTransactionsByCategoryAsync(int idCategory);

    }
}
