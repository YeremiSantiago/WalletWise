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
            try
            {
                var wallets = await _walletRepository.GetAllByUserAsync(_currentUserService.UserId!);

                return Result<IEnumerable<WalletResponseDto>>.Success(_mapper.Map<IEnumerable<WalletResponseDto>>(wallets));
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "A ocurrido un fallo inesperado al obtener todas las wallets");
                return Result<IEnumerable<WalletResponseDto>>.Failure("No se ha podido obtener todas las wallets");
            }
        }

        public async Task<Result<WalletResponseDto?>> GetWalletByIdAsync(int id)
        {
            try
            {
                var result = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

                if (result is null)
                {
                    throw new WalletWise.Application.Exceptions.NotFoundException($"La category con el id {id} no existe");
                }

                return Result<WalletResponseDto?>.Success(_mapper.Map<WalletResponseDto>(result));

            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo inesperado al obtener la wallet con el id {Id} ", id);
                return Result<WalletResponseDto?>.Failure($"No se ha podido obtener la wallet con el id {id}");
            }
        }

        public async Task<Result<WalletResponseDto>> CreateWalletAsync(CreateWalletRequestDto walletDto)
        {
            try
            {
                var wallet = _mapper.Map<Wallet>(walletDto);

                wallet.UserId = _currentUserService.UserId!;
                var walletR = await _walletRepository.AddAsync(wallet);

                return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(walletR));
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al crear la wallet");
                return Result<WalletResponseDto>.Failure("No se ha podido crear la wallet");
            }
        }

        public async Task<Result<WalletResponseDto>> UpdateWalletAsync(int id, UpdateWalletRequestDto walletDto) 
        {
            try
            {

                var exist = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

                if (exist == null)
                {
                    throw new WalletWise.Application.Exceptions.NotFoundException("$La wallet con el id {id} no pudo ser encontrada");
                }

                _mapper.Map(walletDto, exist);

                await _walletRepository.UpdateAsync(exist);

                return Result<WalletResponseDto>.Success(_mapper.Map<WalletResponseDto>(exist));
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al actualizar la wallet con id {Id}", id);
                return Result<WalletResponseDto>.Failure("No se ha podido actualizar la wallet");
            }
        }

        public async Task<Result<bool>> DeleteWalletAsync(int id)
        {
            try
            {
                var exist = await _walletRepository.GetByIdForUserAsync(id, _currentUserService.UserId!);

                if (exist == null)
                {
                     throw new WalletWise.Application.Exceptions.NotFoundException($"La wallet con el id {id} no pudo ser encontrada");
                }

                await _walletRepository.RemoveAsync(id);

                return Result<bool>.Success(true);
            }
            catch (Exception ex) when (ex is not WalletWise.Application.Exceptions.NotFoundException && ex is not WalletWise.Application.Exceptions.ForbiddenAccessException)
            {
                _logger.LogError(ex, "Ha ocurrido un error inesperado al eliminar la wallet con el id {Id}", id);
                return Result<bool>.Failure("No se ha podido eliminar la wallet");
            }
        }

        
    }
}
