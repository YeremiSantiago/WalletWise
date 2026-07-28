using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Domain.Common.Pagination
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        // Metadatos
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }

        public PagedResult(List<T> items, int totalRecords, int currentPage, int pageSize)
        {
            Items = items;
            TotalRecords = totalRecords;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
