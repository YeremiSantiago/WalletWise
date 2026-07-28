using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Domain.Entities
{
     public class Transaction : BaseEntity<int>
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TypeTransaction Type { get; set; }
        public string? Comment { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }
        public int WalletId { get; set; }

        //Navigations Property
        public Category? Category { get; set; }
        public Wallet? Wallet { get; set; }
    }
}
