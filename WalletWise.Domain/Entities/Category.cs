using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;

namespace WalletWise.Domain.Entities
{
    public class Category : BaseEntity<int>
    {
        public string Name { get; set; }
        public int IdUser { get; set; }
    }
}
