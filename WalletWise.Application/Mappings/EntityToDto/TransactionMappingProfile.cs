using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Mappings.EntityToDto
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<CreateTransactionRequestDto, Transaction>()
                .ForMember(des => des.Amount, opt => opt.MapFrom(s => s.Amount))
                .ForMember(des => des.Date, opt => opt.MapFrom(s => s.Date))
                .ForMember(des => des.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(des => des.Comment, opt => opt.MapFrom(s => s.Comment))
                .ForMember(des => des.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(des => des.WalletId, opt => opt.MapFrom(s => s.WalletId))
                .ForMember(des => des.UserId, opt => opt.Ignore())
                .ForMember(des => des.Id, opt => opt.Ignore());

            CreateMap<UpdateTransactionRequestDto, Transaction>()
                .ForMember(des => des.Amount, opt => opt.MapFrom(s => s.Amount))
                .ForMember(des => des.Date, opt => opt.MapFrom(s => s.Date))
                .ForMember(des => des.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(des => des.Comment, opt => opt.MapFrom(s => s.Comment))
                .ForMember(des => des.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(des => des.WalletId, opt => opt.MapFrom(s => s.WalletId))
                .ForMember(des => des.UserId, opt => opt.Ignore())
                .ForMember(des => des.Id, opt => opt.Ignore());


            CreateMap<Transaction, TransactionResponseDto>();
   
        }
    }
}
