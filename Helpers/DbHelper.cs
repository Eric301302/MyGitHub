using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// my add
using System.Data;
using Microsoft.Data.SqlClient;

namespace MyAPI.Helpers
{
    public class DbHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DbHelper> _logger;

        public DbHelper(string connectionString, ILogger<DbHelper> logger)
        {
            _connectionString = connectionString;
        }

        // 1. 執行查詢並回傳 DataTable (用於查詢 ID 或全部)
        public DataTable ExecuteDataTable(string sql, SqlParameter[] parameters = null, SqlTransaction transaction = null)
        {
         DataTable dt = new DataTable();
         try
          {
             using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
             {
               using (SqlCommand cmd = new SqlCommand(sql, conn))
               {
                  if (transaction != null) cmd.Transaction = transaction;
                  if (parameters != null) cmd.Parameters.AddRange(parameters);
                
                  if (conn.State != ConnectionState.Open) conn.Open(); // 連線錯誤通常噴在這裡
                
                  using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                  {
                    adapter.Fill(dt);
                  }
                }
             }
          }
          catch (SqlException ex)
          {
            // 這裡可以 Log 到 NLog, Serilog 等
            // 寫錯連線資訊會觸發：Login failed 或 Network-related error
            Console.Error.WriteLine($"SQL 錯誤：{ex.Message}");
            // 這裡會自動寫入檔案，檔名如 Logs/log-20231027.txt
            // 內容會包含 [2023-10-27 14:30:05.123] [ERR] ...
             _logger.LogError(ex, "資料庫執行失敗。SQL: {Sql}", sql);
             throw new Exception($"資料庫連線或執行失敗：{ex.Message}", ex); 
          }
          return dt;
        }  



        // 2. 執行指令並回傳受影響筆數 (用於 C/U/D)
        public int ExecuteNonQuery(string sql, SqlParameter[] parameters = null, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (transaction != null) cmd.Transaction = transaction;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);

                    if (conn.State != ConnectionState.Open) conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, List<SqlParameter> parameters)
       {
              using (var conn = new SqlConnection(_connectionString))
           {
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
          }
       }

        // 3. 執行指令並回傳第一項結果 (用於查 Count 或 Max ID)
        public object ExecuteScalar(string sql, SqlParameter[] parameters = null, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (transaction != null) cmd.Transaction = transaction;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);

                    if (conn.State != ConnectionState.Open) conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}