namespace WalletWise.Domain.Reports
{
    public class TopCategoryReportItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}