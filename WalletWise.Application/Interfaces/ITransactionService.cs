using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace WalletWise.Application.Interfaces
{
    public interface ITransactionService : IGenericService<Transaction>
    {
    }
}
