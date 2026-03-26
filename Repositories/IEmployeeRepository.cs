using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MyAPI.Models;
using Microsoft.Data.SqlClient;

namespace MyAPI.Repositories
{
    public interface IEmployeeRepository
    {
        Employee GetById(string id);
        int Create(Employee emp, SqlTransaction trans = null);
        // 未來可擴充 Update, Delete...
        Task<bool> CreateEmployeeAsync(EmployeeCreateDto dto);
        Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto);
        Task<bool> DeleteEmployeeAsync(string id);
        Task<IEnumerable<Employee>> GetPagedEmployeesAsync(int pageNumber, int pageSize);

    }
}