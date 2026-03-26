using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Models
{
    public class Employee
    {
        public string EmpId { get; set; }     // char(9)
        public string Name { get; set; }      // varchar(20)
        public string Minit { get; set; }     // char(1) - Allow Null
        public short? JobId { get; set; }     // smallint - Allow Null
        public byte? JobLvl { get; set; }     // tinyint - Allow Null
        public string PubId { get; set; }     // char(4) - Allow Null
        public DateTime HireDate { get; set; } // datetime
    }
}