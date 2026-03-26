using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//my add
using System.Data;
using Microsoft.Data.SqlClient;
using MyAPI.Helpers;
using MyAPI.Models;


namespace MyAPI.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DbHelper _db;
        private readonly ILogger<EmployeeRepository>? _logger;

        private readonly string _connectionString;
        public EmployeeRepository(DbHelper db,IConfiguration configuration, ILogger<EmployeeRepository>? logger = null)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("找不到連線字串");
            _db = db;
            _logger = logger;
        }

        public Employee GetById(string id)
        {

            try
            {

                // 事件加入時間 Log
                _logger?.LogInformation("{Time}: 正在查詢員工 ID: {Id}", DateTime.Now.ToString("HH:mm:ss"), id);


                string sql = "SELECT * FROM employee WHERE emp_id = @id";
                var parms = new[] { new SqlParameter("@id", id) };
                DataTable dt = _db.ExecuteDataTable(sql, parms);

                if (dt.Rows.Count == 0) return null;

                DataRow dr = dt.Rows[0];
                return new Employee
                {
                    EmpId = dr["emp_id"].ToString().Trim(),
                    Name = dr["fname"].ToString(), // pubs 原表是 fname, minit, lname
                    Minit = dr["minit"] == DBNull.Value ? null : dr["minit"].ToString(),
                    JobId = dr["job_id"] == DBNull.Value ? (short?)null : Convert.ToInt16(dr["job_id"]),
                    JobLvl = dr["job_lvl"] == DBNull.Value ? (byte?)null : Convert.ToByte(dr["job_lvl"]),
                    PubId = dr["pub_id"].ToString(),
                    HireDate = Convert.ToDateTime(dr["hire_date"])
                };
            }
            catch (Exception ex)
            {
                // 這裡可以 Log 到 NLog, Serilog 等
                _logger?.LogError(ex, "{Time}: 查詢過程發生異常", DateTime.Now.ToString("HH:mm:ss"));
                throw new Exception($"取得員工資料失敗：{ex.Message}", ex);
            }

        }

        public int Create(Employee emp, SqlTransaction trans = null)
        {
            string sql = @"INSERT INTO employee (empid, fname, minit, lname, job_id, job_lvl, pub_id, hire_date) 
                           VALUES (@id, @name, @minit, 'LName', @jobid, @joblvl, @pubid, @date)";

            var parms = new[] {
                new SqlParameter("@id", emp.EmpId),
                new SqlParameter("@name", emp.Name),
                new SqlParameter("@minit", (object)emp.Minit ?? DBNull.Value),
                new SqlParameter("@jobid", (object)emp.JobId ?? DBNull.Value),
                new SqlParameter("@joblvl", (object)emp.JobLvl ?? DBNull.Value),
                new SqlParameter("@pubid", (object)emp.PubId ?? DBNull.Value),
                new SqlParameter("@date", emp.HireDate)
            };
            return _db.ExecuteNonQuery(sql, parms, trans);
        }

        public async Task<bool> CreateEmployeeAsync(EmployeeCreateDto dto)
        {
            try
            {
                // 💡 注意：這裡的欄位名稱必須與 SQL Server Table 一模一樣
                // 假設你的 Table 欄位是 emp_id, fname, minit, job_id, job_lvl, pub_id, hire_date
                string sql = @"INSERT INTO Employee (emp_id, fname,lname, minit, job_id, job_lvl, pub_id, hire_date) 
                          VALUES (@Id, @Name,@lname, @Minit, @JobId, @JobLvl, @PubId, @HireDate)";

                var parameters = new List<SqlParameter>
                {
                   // 因為沒設 Identity，所以 @Id 必須由前端傳入 (符合 char(9))
                   new SqlParameter("@Id", SqlDbType.Char, 9) { Value = dto.EmpId },
                   new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = dto.Name },
                   new SqlParameter("@lname", SqlDbType.VarChar, 20) { Value = dto.lname },
                   new SqlParameter("@Minit", SqlDbType.Char, 1) { Value = (object?)dto.Minit ?? DBNull.Value },
                   new SqlParameter("@JobId", SqlDbType.SmallInt) { Value = (object?)dto.JobId ?? DBNull.Value },
                   new SqlParameter("@JobLvl", SqlDbType.TinyInt) { Value = (object?)dto.JobLvl ?? DBNull.Value },
                   new SqlParameter("@PubId", SqlDbType.Char, 4) { Value = (object?)dto.PubId ?? DBNull.Value },
                   new SqlParameter("@HireDate", SqlDbType.DateTime) { Value = dto.HireDate }
               };

                int rows = await _db.ExecuteNonQueryAsync(sql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增員工資料時發生錯誤: {EmpId}", dto.EmpId);
                throw; // 拋回給 Controller 處理回傳訊息
            }
        }

        public async Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto)
        {
            // 💡 SQL 指令：確保欄位名稱與你提供的 Entity 一致
            string sql = @"UPDATE Employee 
                           SET fname = @Name, 
                           minit = @Minit, 
                           job_id = @JobId, 
                           job_lvl = @JobLvl, 
                           pub_id = @PubId, 
                           hire_date = @HireDate 
                           WHERE emp_id = @EmpId";
            try
            {
                var parameters = new List<SqlParameter>
                {
                   new SqlParameter("@EmpId", SqlDbType.Char, 9) { Value = dto.EmpId },
                   new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = dto.Name },
                   // 處理 Allow Null 欄位
                   new SqlParameter("@Minit", SqlDbType.Char, 1) { Value = (object?)dto.Minit ?? DBNull.Value },
                   new SqlParameter("@JobId", SqlDbType.SmallInt) { Value = (object?)dto.JobId ?? DBNull.Value },
                   new SqlParameter("@JobLvl", SqlDbType.TinyInt) { Value = (object?)dto.JobLvl ?? DBNull.Value },
                   new SqlParameter("@PubId", SqlDbType.Char, 4) { Value = (object?)dto.PubId ?? DBNull.Value },
                   new SqlParameter("@HireDate", SqlDbType.DateTime) { Value = (object?)dto.HireDate ?? DBNull.Value }
            };

                // 呼叫方式 A 的 DbHelper
                int rows = await _db.ExecuteNonQueryAsync(sql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "編輯員工資料時發生錯誤: {EmpId}", dto.EmpId);
                throw; // 拋回給 Controller 處理回傳訊息    
            }



        }


        public async Task<bool> DeleteEmployeeAsync(string id)
        {
            // 💡 使用參數化查詢防止 SQL Injection
            string sql = "DELETE FROM employee WHERE emp_id = @Id";
            try
            {
                var parameters = new List<SqlParameter>
            {
                  // 配合資料庫 char(9) 型別
                  new SqlParameter("@Id", SqlDbType.Char, 9) { Value = id }
            };

                // 呼叫你的靜態方法 ExecuteNonQueryAsync
                int rows = await _db.ExecuteNonQueryAsync(sql, parameters);

                // 如果影響列數 > 0，代表刪除成功
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除員工資料時發生錯誤: {EmpId}", id);
                throw; // 拋回給 Controller 處理回傳訊息    
            }


        }

        public async Task<IEnumerable<Employee>> GetPagedEmployeesAsync(int pageNumber, int pageSize)
        {
            var list = new List<Employee>();
            string sql = @"SELECT emp_id, fname AS Name, minit, job_id AS JobId, 
                          job_lvl AS JobLvl, pub_id AS PubId, hire_date AS HireDate
                   FROM employee 
                   ORDER BY hire_date DESC 
                   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Employee
                        {
                            EmpId = reader["emp_id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Minit = reader["minit"]?.ToString(),
                            JobId = reader["JobId"] as short?,
                            JobLvl = reader["JobLvl"] as byte?,
                            PubId = reader["PubId"]?.ToString(),
                            HireDate = (DateTime)reader["HireDate"]
                        });
                    }
                }
            }
            return list;
        }


    }
}