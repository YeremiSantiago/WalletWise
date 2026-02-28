using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;

namespace WalletWise.Domain.Entities
{
    public class Wallet : BaseEntity<int>
    {
        public string Name { get; set; }
        public int? UserId { get; set; }

        // Navigation Property
        public IEnumerable<Transaction>? Transactions { get; set; }
    }
}
