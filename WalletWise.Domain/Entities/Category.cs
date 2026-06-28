using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Domain.Entities
{
    public class Category : AuditEntity
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public TypeTransaction Type { get; set; }
        public string? Description { get; set; }

        // Navigation Property
        public IEnumerable<Transaction>? Transactions { get; set; }
    }
}
