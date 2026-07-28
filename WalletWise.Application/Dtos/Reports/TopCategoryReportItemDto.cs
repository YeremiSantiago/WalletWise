namespace WalletWise.Application.Dtos.Reports
{
    public class TopCategoryReportItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}