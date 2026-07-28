using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WalletWise.Domain.Common.Enums;

namespace WalletWise.Domain.Common.Pagination
{
    public class TransactionFilterParams : PaginationParams
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public TypeTransaction? Type { get; set; }
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public string? OrderDir { get; set; }
    }
}
