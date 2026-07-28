using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Reports;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Interfaces
{
    public interface IReportService
    {
        Task<Result<ReportSummaryDto>> GetSummaryAsync();
        Task<Result<IEnumerable<MonthlySummaryDto>>> GetMonthlySummaryAsync(int year);
        Task<Result<IEnumerable<CategoryReportItemDto>>> GetByCategoryAsync(DateTime start, DateTime end, TypeTransaction? type);
        Task<Result<IEnumerable<TopCategoryReportItemDto>>> GetTopCategoriesAsync(DateTime start, DateTime end, int top);
        Task<Result<ComparisonReportDto>> GetComparisonAsync(DateTime startA, DateTime endA, DateTime startB, DateTime endB);
        Task<Result<IEnumerable<TransactionResponseDto>>> ExportAsync(DateTime? start, DateTime? end, TypeTransaction? type, int? categoryId, string? search);

    }
}
