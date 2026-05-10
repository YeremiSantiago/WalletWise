namespace WalletWise.Application.Dtos.Reports
{
    public class ComparisonReportDto
    {
        public DateTime Period1Start { get; set; }
        public DateTime Period1End { get; set; }
        public DateTime Period2Start { get; set; }
        public DateTime Period2End { get; set; }

        public decimal Period1Income { get; set; }
        public decimal Period1Expense { get; set; }
        public decimal Period1Balance { get; set; }

        public decimal Period2Income { get; set; }
        public decimal Period2Expense { get; set; }
        public decimal Period2Balance { get; set; }

        public decimal IncomeDifference { get; set; }
        public decimal ExpenseDifference { get; set; }
        public decimal BalanceDifference { get; set; }
    }
}