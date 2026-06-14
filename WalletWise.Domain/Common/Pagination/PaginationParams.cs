using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Domain.Common.Pagination
{
    public class PaginationParams
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 20;
        public int Page { get; set; } = 1;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }

            set
            {
                _pageSize = value > MaxPageSize ? MaxPageSize : value;
            }
        }
    }
}
