using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class TransactionService : GenericService<Transaction>, ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        public TransactionService(ITransactionRepository transactionRepository) : base(transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
    }
}
