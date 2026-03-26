using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Models
{
    public class EmployeeUpdateDto
    {
        public string EmpId { get; set; } = string.Empty; // 必要，用來定位資料
        public string Name { get; set; } = string.Empty;
        public string? Minit { get; set; }
        public short? JobId { get; set; }
        public byte? JobLvl { get; set; }
        public string? PubId { get; set; }
        public DateTime? HireDate { get; set; } // 修改時不一定會動到日期

    }
}