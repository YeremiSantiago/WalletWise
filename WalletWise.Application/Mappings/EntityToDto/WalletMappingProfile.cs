using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Mappings.EntityToDto
{
    public class WalletMappingProfile : Profile
    {
        public WalletMappingProfile()
        {
            CreateMap<Wallet, WalletResponseDto>();


            CreateMap<CreateWalletRequestDto, Wallet>()
                .ForMember(des => des.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(des => des.UserId, opt => opt.Ignore())
                .ForMember(des => des.Id, opt => opt.Ignore());

            CreateMap<UpdateWalletRequestDto, Wallet>()
                .ForMember(des => des.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(des => des.UserId, opt => opt.Ignore())
                .ForMember(des => des.Id, opt => opt.Ignore());

        }
    }
}
