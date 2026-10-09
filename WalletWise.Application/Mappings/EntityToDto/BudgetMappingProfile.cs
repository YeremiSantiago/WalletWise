using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Mappings.EntityToDto
{
    public class BudgetMappingProfile : Profile
    {
        public BudgetMappingProfile()
        {
            CreateMap<CreateBudgetRequestDto, Budget>();

            CreateMap<UpdateBudgetRequestDto, Budget>();

            CreateMap<Budget, BudgetResponseDto>()
                .ForMember(dest => dest.SpentAmount, opt => opt.Ignore())
                .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore())
                .ForMember(dest => dest.UsagePercentage, opt => opt.Ignore())
                .ForMember(dest => dest.IsExceeded, opt => opt.Ignore());
        }
    }
}
