using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

using WalletWise.Application.Common;
using WalletWise.Application.Exceptions;

namespace WalletWise.Application.Services
{
    public class WalletService : GenericService<Wallet, WalletResponseDto, CreateWalletRequestDto, UpdateWalletRequestDto>, IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrentUserService _currentUserService;
        public WalletService(IWalletRepository walletRepository, ILogger<Wallet> logger, IMapper mapper, ICurrentUserService currentUserService) : base(walletRepository, logger, mapper)
        {
            _walletRepository = walletRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IEnumerable<WalletResponseDto>>> GetAllWalletsAsync()
        {
            var wallets = await _walletRepository.GetAllByUserAsync(_currentUserService.UserId!);

            return Result<IEnumerable<WalletResponseDto>>.Success(_mapper.Map<IEnumerable<WalletResponseDto>>(wallets));
        }

        public async Task<Result<WalletResponseDto?>> GetWalletByIdAsync(int id)
        {
            var result = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (result is null)
            {
                throw new NotFoundException($"La category con el id {id} no existe");
            }

            return Result<WalletResponseDto?>.Success(_mapper.Map<WalletResponseDto>(result));
        }

        public async Task<Result<WalletResponseDto>> CreateWalletAsync(CreateWalletRequestDto walletDto)
        {
            var wallet = _mapper.Map<Wallet>(walletDto);

            wallet.UserId = _currentUserService.UserId!;

            try
            {
                var walletR = await _walletRepository.AddAsync(wallet);
                return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(walletR));
            }
            catch (UniqueConstraintViolationException ex)
            {
                return Result<WalletResponseDto>.Failure(BusinessErrorCodes.ERR_WALLET_NAME_EXISTS, "Ya existe una Wallet con ese mismo nombre");
            }
        }

        public async Task<Result<WalletResponseDto>> UpdateWalletAsync(int id, UpdateWalletRequestDto walletDto) 
        {
            var exist = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (exist == null)
            {
                throw new .NotFoundException($"La wallet con el id {id} no pudo ser encontrada");
            }

            _mapper.Map(walletDto, exist);

            await _walletRepository.UpdateAsync(exist);

            return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(exist));
        }

        public async Task<Result<bool>> DeleteWalletAsync(int id)
        {
            var exist = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

            if (exist == null)
            {
                 throw new NotFoundException($"La wallet con el id {id} no pudo ser encontrada");
            }

            await _walletRepository.RemoveAsync(id);

            return Result<bool>.Success(true);
        }
    }
}
