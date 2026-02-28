using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Dtos.Transaction
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TypeTransaction Type { get; set; }
        public string? Comment { get; set; }
        public int? UserId { get; set; }
        public int CategoryId { get; set; }
        public int WalletId { get; set; }

    }
}
