using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Domain.Interfaces
{
    public interface IClock
    {
         DateTime UtcNow();
    }
}
