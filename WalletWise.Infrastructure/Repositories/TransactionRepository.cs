using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common.Pagination;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Context;
namespace WalletWise.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsTransactionByCategoryAsync(int IdCategory, string userId)
        {
            return await _context.Transactions.
                AnyAsync(x => x.CategoryId == IdCategory && x.UserId == userId);

        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsByCategoryAsync(string userId, int idCategory)
        {
            return await _context.Transactions.Where(x => x.CategoryId == idCategory && x.UserId == userId)
                                                        .AsNoTracking()
                                                        .OrderByDescending(x => x.Date)
                                                        .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(string userId, DateTime start, DateTime end)
        {
            return await _context.Transactions.Where(x => x.Date >= start && x.Date <= end && x.UserId == userId)
               .AsNoTracking()
               .OrderByDescending(x => x.Date)
               .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(string userId, TypeTransaction typeTransaction)
        {
            return await _context.Transactions.Where(x => x.Type == typeTransaction && x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task<IEnumerable<Transaction>> GetAllByUserAsync(string userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PagedResult<Transaction>> GetPagedTransactionsAsync(string userId, TransactionFilterParams filterParams)
        {
            var query = BuildFilteredQuery(userId, filterParams);

            var totalRecords = await query.CountAsync();
            bool isDesc = filterParams.OrderDir?.ToLower() == "desc";

            if (filterParams.OrderBy?.ToLower() == "amount")
            {
                query = isDesc ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount);
            }
            else
            {
                query = isDesc ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date);
            }

            var items = await query
                .Skip((filterParams.Page - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            return new PagedResult<Transaction>(items, totalRecords, filterParams.Page, filterParams.PageSize);
        }

        public async Task<(decimal TotalIncome, decimal TotalExpense, decimal Balance)> GetSummaryAsync(string userId, TransactionFilterParams filterParams)
        {
            var query = BuildFilteredQuery(userId, filterParams);

            var totals = await query
                .GroupBy(x => x.Type)
                .Select(g => new
                {
                    type = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            decimal totalIncome = totals.FirstOrDefault(t => t.type == TypeTransaction.Income)?.Total ?? 0m;
            decimal totalExpense = totals.FirstOrDefault(t => t.type == TypeTransaction.Expense)?.Total ?? 0m;
            decimal balance = totalIncome - totalExpense;

            return (totalIncome, totalExpense, balance);
        }

        private IQueryable<Transaction> BuildFilteredQuery(string userId, TransactionFilterParams filterParams)
        {
            var query = _context.Transactions
                .Where(x => x.UserId == userId)
                .AsNoTracking();

            if (filterParams.DateFrom.HasValue)
                query = query.Where(x => x.Date >= filterParams.DateFrom.Value);

            if (filterParams.DateTo.HasValue)
                query = query.Where(x => x.Date <= filterParams.DateTo.Value);

            if (filterParams.Type.HasValue)
                query = query.Where(x => x.Type == filterParams.Type.Value);

            if (filterParams.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filterParams.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filterParams.Search))
                query = query.Where(x => x.Comment != null && x.Comment.Contains(filterParams.Search));

            return query;

        }
    }
}

