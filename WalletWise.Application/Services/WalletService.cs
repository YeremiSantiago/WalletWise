using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class WalletService : GenericService<Wallet, WalletResponseDto, CreateWalletRequestDto, UpdateWalletRequestDto>, IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        public WalletService(IWalletRepository walletRepository, ILogger<Wallet> logger, IMapper mapper) : base(walletRepository, logger, mapper)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Result<WalletResponseDto>> CreateWalletAsync(CreateWalletRequestDto walletDto)
        {
            try
            {
                var wallet = _mapper.Map<Wallet>(walletDto);


                var walletR = await _walletRepository.AddAsync(wallet);

                return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(walletR));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al crear la wallet");
                return Result<WalletResponseDto>.Failure("No se ha podido crear la wallet");
            }
        }

        public async Task<Result<WalletResponseDto>> UpdateWalletAsync(int id, UpdateWalletRequestDto walletDto) 
        {
            try
            {

                var exist = await _walletRepository.GetByIdAsync(id);

                if (exist == null)
                {
                    return Result<WalletResponseDto>.Failure($"La wallet con el id {id} no pudo ser encontrada");
                }

                _mapper.Map(walletDto, exist);

                await _walletRepository.UpdateAsync(exist);

                return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(exist));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al actualizar la wallet con id {Id}", id);
                return Result<WalletResponseDto>.Failure("No se ha podido actualizar la wallet");
            }
        }

        public async Task<Result<bool>> DeleteWalletAsync(int id)
        {
            try
            {
                var exist = await _walletRepository.GetByIdAsync(id);

                if (exist == null)
                {
                    return Result<bool>.Failure($"La wallet con el id {id} no pudo ser encontrada");
                }

                await _walletRepository.RemoveAsync(id);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un error inesperado al eliminar la wallet con el id {Id}", id);
                return Result<bool>.Failure("No se ha podido eliminar la wallet");
            }
        }
    }
}
