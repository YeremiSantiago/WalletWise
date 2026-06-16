using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Domain.Reports;
using WalletWise.Infrastructure.Context;

namespace WalletWise.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReportSummary> GetSummaryAsync(string userId)
        {
            var data = await _context.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => 1)
                .Select(g => new 
                {
                    Income = g.Where(t => t.Type == TypeTransaction.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TypeTransaction.Expense).Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync();

            var income = data?.Income ?? 0m;
            var expense = data?.Expense ?? 0m;

            return new ReportSummary
            {
                TotalIncome = income,
                TotalExpense = expense,
                Balance = income - expense
            };
        }

        public async Task<IEnumerable<MonthlySummary>> GetMonthlySummaryAsync(string userId, int year)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Year == year)
                .GroupBy(t => t.Date.Month)
                .Select(g => new MonthlySummary
                {
                    Year = year,
                    Month = g.Key,
                    TotalIncome = g.Where(t => t.Type == TypeTransaction.Income).Sum(t => (decimal?)t.Amount) ?? 0m,
                    TotalExpense = g.Where(t => t.Type == TypeTransaction.Expense).Sum(t => (decimal?)t.Amount) ?? 0m,
                    Balance = (g.Where(t => t.Type == TypeTransaction.Income).Sum(t => (decimal?)t.Amount) ?? 0m) -
                              (g.Where(t => t.Type == TypeTransaction.Expense).Sum(t => (decimal?)t.Amount) ?? 0m)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryReportItem>> GetByCategoryAsync(string userId, DateTime start, DateTime end, TypeTransaction? type)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.Category!.IsDeleted == false && t.Date >= start && t.Date <= end);

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            return await query
                .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Type })
                .Select(g => new CategoryReportItem
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Type = g.Key.Type,
                    TransactionsCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TopCategoryReportItem>> GetTopCategoriesAsync(string userId, DateTime start, DateTime end, int top)
        {
            var data = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.Type == TypeTransaction.Expense && t.Category!.IsDeleted == false && t.Date >= start && t.Date <= end)
                .GroupBy(t => new { t.CategoryId, t.Category!.Name })
                .Select(g => new TopCategoryReportItem
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            return data
                .OrderByDescending(x => x.TotalAmount)
                .Take(top)
                .ToList();
        }

        public async Task<ComparisonReport> GetComparisonAsync(string userId, DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            var p1Task = GetSummaryInRangeAsync(userId, startA, endA);
            var p2Task = GetSummaryInRangeAsync(userId, startB, endB);

            await Task.WhenAll(p1Task, p2Task);
            var p1 = p1Task.Result;
            var p2 = p2Task.Result;

            return new ComparisonReport
            {
                Period1Start = startA, Period1End = endA,
                Period2Start = startB, Period2End = endB,
                Period1Income = p1.TotalIncome, Period1Expense = p1.TotalExpense, Period1Balance = p1.Balance,
                Period2Income = p2.TotalIncome, Period2Expense = p2.TotalExpense, Period2Balance = p2.Balance,
                IncomeDifference = p2.TotalIncome - p1.TotalIncome,
                ExpenseDifference = p2.TotalExpense - p1.TotalExpense,
                BalanceDifference = p2.Balance - p1.Balance
            };
        }

        public async Task<IEnumerable<Transaction>> GetExportAsync(string userId, DateTime? start, DateTime? end, TypeTransaction? type, int? categoryId, string? search)
        {
            var query = _context.Transactions.AsNoTracking().Where(t => t.UserId == userId);

            if (start.HasValue) query = query.Where(t => t.Date >= start.Value);
            if (end.HasValue) query = query.Where(t => t.Date <= end.Value);
            if (type.HasValue) query = query.Where(t => t.Type == type.Value);
            if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(t => t.Comment != null && t.Comment.Contains(search));

            return await query.OrderByDescending(t => t.Date).ToListAsync();
        }

        private async Task<ReportSummary> GetSummaryInRangeAsync(string userId, DateTime start, DateTime end)
        {
             var data = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Income = g.Where(t => t.Type == TypeTransaction.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TypeTransaction.Expense).Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync();

            var income = data?.Income ?? 0m;
            var expense = data?.Expense ?? 0m;

            return new ReportSummary { TotalIncome = income, TotalExpense = expense, Balance = income - expense };
        }
    }
}

