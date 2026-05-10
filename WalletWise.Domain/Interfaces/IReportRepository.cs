using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Reports;

namespace WalletWise.Domain.Interfaces
{
    public interface IReportRepository
    {
        Task<ReportSummary> GetSummaryAsync(string userId);
        Task<IEnumerable<MonthlySummary>> GetMonthlySummaryAsync(string userId, int year);
        Task<IEnumerable<CategoryReportItem>> GetByCategoryAsync(string userId, DateTime start, DateTime end, TypeTransaction? type);
        Task<IEnumerable<TopCategoryReportItem>> GetTopCategoriesAsync(string userId, DateTime start, DateTime end, int top);
        Task<ComparisonReport> GetComparisonAsync(string userId, DateTime startA, DateTime endA, DateTime startB, DateTime endB);
        Task<IEnumerable<Transaction>> GetExportAsync(string userId, DateTime? start, DateTime? end, TypeTransaction? type, int? categoryId, string? search);

    }
}
