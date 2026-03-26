using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
         public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        // 之後可以補上 TotalCount (總筆數)

    }
}