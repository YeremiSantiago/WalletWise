using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Infrastructure.Time
{
    public class ClockSystem : IClock
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}

