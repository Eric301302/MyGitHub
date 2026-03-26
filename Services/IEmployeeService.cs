using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MyAPI.Models;

namespace MyAPI.Services
{
    public interface IEmployeeService
    {
        Employee GetEmployee(string id);
        ApiResponse<bool> CreateEmployee(Employee emp);
        // 定義非同步的新增方法
        Task<bool> CreateEmployeeAsync(EmployeeCreateDto dto);
        Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto);
        Task<bool> DeleteEmployeeAsync(string id);

        Task<PagedResult<Employee>> GetEmployeesAsync(PaginationFilter filter);
    }
}