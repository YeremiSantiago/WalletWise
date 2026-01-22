using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Constants;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class WalletService : GenericService<Wallet>, IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        public WalletService(IWalletRepository walletRepository) : base(walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public override async Task<Result<Wallet>> AddAsync(Wallet wallet)
        {
            wallet.UserId = DefaultUser.Id;
            return Result<Wallet>.Success(await _walletRepository.AddAsync(wallet));
        }

    }
}
