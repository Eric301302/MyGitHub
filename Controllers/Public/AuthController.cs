using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyAPI.Helpers;
using MyAPI.Models;
using MyAPI.Services;
using System.Data;
using Microsoft.Data.SqlClient;

using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;


namespace MyAPI.Controllers.Public
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly DbHelper _db; // 假設你已經在 Program.cs 注入
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger, IConfiguration config, DbHelper db)
        {
            _logger = logger;
            _db = db;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 測試hash密碼的產生與驗證
            // 1. 設定你的原始密碼
            //string password = "123456";

            // 2. 產生雜湊值 (每次執行結果都會不同，這是正常的，因為有加「鹽」Salt)
            //string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // 3. 印出結果 (請將這串字串複製到資料庫的 PasswordHash 欄位)
            //Console.WriteLine($"原始密碼: {password}");
            //Console.WriteLine($"產生的 Hash: {passwordHash}");

            // 4. 測試驗證 (模擬登入時的比對)
            //bool isMatch = BCrypt.Net.BCrypt.Verify("123456", passwordHash);
            //Console.WriteLine($"驗證結果: {isMatch}"); // 應該要回傳 True



            // 1. 使用你的 dbHelper 查詢 User
            string sql = "SELECT * FROM Users WHERE Account = @Acc AND IsActive = 1";
            SqlParameter[] paras = { new SqlParameter("@Acc", request.Account) };

            DataTable dt = _db.ExecuteDataTable(sql, paras);

            if (dt.Rows.Count == 0)
            {
                return Unauthorized("帳號不存在或已停用");
            }

            DataRow row = dt.Rows[0];
            string dbPasswordHash = row["PasswordHash"].ToString();

            // 2. 驗證密碼 (使用 BCrypt)
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, dbPasswordHash.Trim());

            if (!isValid) return Unauthorized("密碼錯誤");

            // 3. 密碼正確，準備產生 Token
            var token = GenerateJwtToken(row);

            return Ok(new
            {
                token,
                userName = row["UserName"].ToString(),
                role = row["Role"].ToString()
            });
        }

        private string GenerateJwtToken(DataRow userRow)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 將資料表欄位封裝進 Claims
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userRow["Account"].ToString()),
            new Claim(ClaimTypes.Name, userRow["UserName"].ToString()),
            new Claim(ClaimTypes.Role, userRow["Role"].ToString()),
            new Claim("EmpId", userRow["EmpId"]?.ToString() ?? "")
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}