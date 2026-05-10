using WalletWise.Domain.Common.Enums;

namespace WalletWise.Domain.Reports
{
    public class CategoryReportItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public TypeTransaction Type { get; set; }
        public int TransactionsCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}