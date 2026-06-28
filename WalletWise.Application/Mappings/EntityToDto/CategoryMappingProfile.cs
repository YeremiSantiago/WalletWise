using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Mappings.EntityToDto
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.TransactionCount, opt => opt.MapFrom(src => src.Transactions != null ? src.Transactions.Count() : 0));

            CreateMap<UpdateCategoryRequestDto, Category>()
                .ForMember(des => des.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(des => des.Id, opt => opt.Ignore())
                .ForMember(des => des.UserId, opt => opt.Ignore());

            CreateMap<CreateCategoryRequestDto, Category>()
                .ForMember(des => des.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(des => des.UserId, opt => opt.Ignore())
                .ForMember(des => des.Id, opt => opt.Ignore());
    
        }
    }
}
