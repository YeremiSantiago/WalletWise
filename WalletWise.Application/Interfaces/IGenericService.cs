using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;

namespace WalletWise.Application.Interfaces
{
    public interface IGenericService<ResDto, CreatDto, UpdaDto> 
        where ResDto : class
        where CreatDto : class
        where UpdaDto : class
    {
        Task<Result<ResDto?>> GetByIdAsync(int id);
        Task<Result<IEnumerable<ResDto>>> GetAllAsync();
        Task<Result<ResDto>> AddAsync(CreatDto entity);
        Task<Result<ResDto>> UpdateAsync(int id, UpdaDto entity);
        Task<Result<bool>> DeleteAsync(int id);
        
    }
}
