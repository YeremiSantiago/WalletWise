using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;

namespace WalletWise.Domain.Entities
{
    public class User : BaseEntity<int>
    {
        public string Username { get; set; }
    }
}
