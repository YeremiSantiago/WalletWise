using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class WalletService : GenericService<Wallet>, IWalletService
    {
        private readonly IWalletRepository _walletService;
        public WalletService(IWalletRepository walletService) : base(walletService)
        {
            _walletService = walletService;
        }
    }
}
