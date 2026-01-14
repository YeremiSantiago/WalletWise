using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace WalletWise.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
    }
}
