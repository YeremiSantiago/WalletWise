using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Dtos.Budget
{
    public class CreateBudgetRequestDto
    {
        public int WalletId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BudgetMode Mode { get; set; }
    }
}
