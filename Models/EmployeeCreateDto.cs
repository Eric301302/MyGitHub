using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;   // 用來驗證輸入資料的屬性

namespace MyAPI.Models
{
    public class EmployeeCreateDto
    {
        
       [Required]
       [RegularExpression(@"^[A-Z]{3}[1-9][0-9]{4}[FM]$|^[A-Z]-[A-Z][1-9][0-9]{4}[FM]$" , 
        ErrorMessage = "員工編號格式不符 (例如: PMA42628M)")] 
       public string EmpId { get; set; } = string.Empty; // 手動輸入 ID
       public string Name { get; set; } = string.Empty;
       public string lname { get; set; } = string.Empty;
       public string? Minit { get; set; }
       public short? JobId { get; set; }
       public byte? JobLvl { get; set; }
       public string? PubId { get; set; }
       public DateTime HireDate { get; set; } = DateTime.Now; // 預設給今天

    }
}