using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Models
{
    public class PaginationFilter
    {
         public int PageNumber { get; set; } = 1;  // 第幾頁
        public int PageSize { get; set; } = 20;   // 每頁幾筆 (你指定 20)        

    }
}