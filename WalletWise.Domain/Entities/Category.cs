using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;

namespace WalletWise.Domain.Entities
{
    public class Category : AuditEntity
    {
        public string Name { get; set; }
        public int? UserId { get; set; }

        // Navigation Property
        public Transaction transaction { get; set; }
    }
}
