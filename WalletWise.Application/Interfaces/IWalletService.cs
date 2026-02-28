using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Interfaces
{
    public interface IWalletService : IGenericService<WalletResponseDto, CreateWalletRequestDto, UpdateWalletRequestDto>
    {
        Task<Result<WalletResponseDto>> CreateWalletAsync(CreateWalletRequestDto walletDto);
        Task<Result<WalletResponseDto>> UpdateWalletAsync(int id, UpdateWalletRequestDto walletDto);
        Task<Result<bool>> DeleteWalletAsync(int id);

    }
}
