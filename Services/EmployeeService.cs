using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using MyAPI.Models;
using MyAPI.Repositories;
using System.Data;

namespace MyAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly string _connectionString;

        public EmployeeService(IEmployeeRepository repo, IConfiguration config)
        {
            _repo = repo;
            _connectionString = config.GetConnectionString("DefaultConnection");
            Console.WriteLine($"[EmployeeService] 我的連線字串: {_connectionString}"); // 確認連線字串  
        }

        // 取得單一員工
        public Employee GetEmployee(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _repo.GetById(id);
        }

        // 新增員工 (含驗證與交易)
        public ApiResponse<bool> CreateEmployee(Employee emp)
        {
            var response = new ApiResponse<bool>();

            // [欄位驗證]
            if (string.IsNullOrEmpty(emp.EmpId) || emp.EmpId.Length != 9)
            {
                response.Message = "驗證失敗：員工編號必須為 9 碼";
                return response;
            }

            // [交易控制] 
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 檢查重複
                        if (_repo.GetById(emp.EmpId) != null)
                        {
                            response.Message = $"員工編號 {emp.EmpId} 已存在";
                            return response;
                        }

                        _repo.Create(emp, trans);

                        trans.Commit(); // 成功才確認
                        response.Success = true;
                        response.Message = "新增成功";
                    }
                    catch (SqlException ex)
                    {
                        trans.Rollback();
                        response.Message = $"資料庫異常 (Error {ex.Number}): {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        response.Message = $"系統發生未預期錯誤: {ex.Message}";
                    }
                }
            }
            return response;
        }


        public async Task<bool> CreateEmployeeAsync(EmployeeCreateDto dto)
        {
            // 1. 商業邏輯檢查：例如檢查 Email 是否重複 (假設 Repo 有此方法)
            // var isExist = await _repo.CheckEmailExistsAsync(dto.Email);
            // if (isExist) return false;

            // 2. 呼叫 Repository 執行資料庫動作 (方式 B)
            //_logger.LogInformation("開始新增員工: {Email}", dto.Email);
            return await _repo.CreateEmployeeAsync(dto);
        }

        public async Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto)
        {
            if (string.IsNullOrEmpty(dto.EmpId)) return false;

            //_logger.LogInformation("準備更新員工: {EmpId}", dto.EmpId);

            // 呼叫 Repository
            return await _repo.UpdateEmployeeAsync(dto);
        }

        public async Task<bool> DeleteEmployeeAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            //_logger.LogInformation("正在執行刪除員工，ID: {EmpId}", id);

            return await _repo.DeleteEmployeeAsync(id);
        }

        public async Task<PagedResult<Employee>> GetEmployeesAsync(PaginationFilter filter)
        {
            //_logger.LogInformation("查詢第 {Page} 頁資料", filter.PageNumber);

            var data = await _repo.GetPagedEmployeesAsync(filter.PageNumber, filter.PageSize);

            // 實務上還會去 SELECT COUNT(*) 算出總筆數
            return new PagedResult<Employee>
            {
                Items = data,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }


    }
}