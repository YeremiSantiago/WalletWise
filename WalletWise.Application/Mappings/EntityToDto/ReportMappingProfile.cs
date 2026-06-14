using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Reports;
using WalletWise.Domain.Reports;

namespace WalletWise.Application.Mappings.EntityToDto
{
    public class ReportMappingProfile : Profile
    {
        public ReportMappingProfile()
        {
            CreateMap<ReportSummary, ReportSummaryDto>()
                .ForMember(des => des.TotalIncome, opt => opt.MapFrom(s => s.TotalIncome))
                .ForMember(des => des.TotalExpense, opt => opt.MapFrom(s => s.TotalExpense))
                .ForMember(des => des.Balance, opt => opt.MapFrom(s => s.Balance));

            CreateMap<MonthlySummary, MonthlySummaryDto>()
                .ForMember(des => des.Year, opt => opt.MapFrom(s => s.Year))
                .ForMember(des => des.Month, opt => opt.MapFrom(s => s.Month))
                .ForMember(des => des.TotalIncome, opt => opt.MapFrom(s => s.TotalIncome))
                .ForMember(des => des.TotalExpense, opt => opt.MapFrom(s => s.TotalExpense))
                .ForMember(des => des.Balance, opt => opt.MapFrom(s => s.Balance));

            CreateMap<CategoryReportItem, CategoryReportItemDto>()
                .ForMember(des => des.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(des => des.CategoryName, opt => opt.MapFrom(s => s.CategoryName))
                .ForMember(des => des.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(des => des.TransactionsCount, opt => opt.MapFrom(s => s.TransactionsCount))
                .ForMember(des => des.TotalAmount, opt => opt.MapFrom(s => s.TotalAmount));

            CreateMap<ComparisonReport, ComparisonReportDto>()
                .ForMember(des => des.Period1Start, opt => opt.MapFrom(s => s.Period1Start))
                .ForMember(des => des.Period1End, opt => opt.MapFrom(s => s.Period1End))
                .ForMember(des => des.Period2Start, opt => opt.MapFrom(s => s.Period2Start))
                .ForMember(des => des.Period2End, opt => opt.MapFrom(s => s.Period2End))
                .ForMember(des => des.Period1Income, opt => opt.MapFrom(s => s.Period1Income))
                .ForMember(des => des.Period1Expense, opt => opt.MapFrom(s => s.Period1Expense))
                .ForMember(des => des.Period1Balance, opt => opt.MapFrom(s => s.Period1Balance))
                .ForMember(des => des.Period2Income, opt => opt.MapFrom(s => s.Period2Income))
                .ForMember(des => des.Period2Expense, opt => opt.MapFrom(s => s.Period2Expense))
                .ForMember(des => des.Period2Balance, opt => opt.MapFrom(s => s.Period2Balance))
                .ForMember(des => des.IncomeDifference, opt => opt.MapFrom(s => s.IncomeDifference))
                .ForMember(des => des.ExpenseDifference, opt => opt.MapFrom(s => s.ExpenseDifference))
                .ForMember(des => des.BalanceDifference, opt => opt.MapFrom(s => s.BalanceDifference));

            CreateMap<TopCategoryReportItem, TopCategoryReportItemDto>()
                .ForMember(des => des.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
                .ForMember(des => des.CategoryName, opt => opt.MapFrom(s => s.CategoryName))
                .ForMember(des => des.TotalAmount, opt => opt.MapFrom(s => s.TotalAmount));
        }
    }
}
