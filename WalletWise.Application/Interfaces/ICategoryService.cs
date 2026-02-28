using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Interfaces
{
    public interface ICategoryService : IGenericService<CategoryResponseDto, CreateCategoryRequestDto, UpdateCategoryRequestDto>
    {
        Task<Result<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryRequestDto updateCategoryDto);
        Task<Result<CategoryResponseDto>> UpdateCategoryAsync(int id, UpdateCategoryRequestDto updateCategoryDto);
        Task<Result<bool>> DeleteCategoryAsync(int id);

    }
}
