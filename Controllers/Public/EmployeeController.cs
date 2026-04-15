using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyAPI.Models;

using Microsoft.AspNetCore.Authorization;
using MyAPI.Services.Interfaces;


namespace MyAPI.Controllers.Public
{
    //[Authorize] // 加上這個標籤，現在這整個 Controller 都被 JWT 保護了
    [ApiController] // 💡 增加這個，會自動幫您處理 [FromBody] 驗證
    [Route("api/[controller]")] // 💡 建議加上 api/ 前綴，這是業界標準
    public class EmployeeController : ControllerBase // 💡 改為繼承 ControllerBase
    {
        private readonly IConfiguration  _config;
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeeController> _logger;
         
        
        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService service, IConfiguration config   )
        {
            _logger = logger;
            _service = service;
        }

        // 💡 移除 Index() 和 Error()，因為 API 不需要回傳 HTML 網頁

        // GET api/employee/A12345678
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var result = _service.GetEmployee(id);
            if (result == null) return NotFound(new { message = "找不到該員工" });
            return Ok(result);
        }


        // POST api/employee
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("輸入資料格式不正確");


            try
            {
                var success = await _service.CreateEmployeeAsync(dto);
                if (success) return Ok(new { message = "新增成功" });

                return StatusCode(500, "資料新增失敗");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"伺服器內部錯誤: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] EmployeeUpdateDto dto)
        {

            if (!ModelState.IsValid) return BadRequest("輸入資料格式不正確");

            try
            {
                var result = await _service.UpdateEmployeeAsync(dto);
                if (!result) return NotFound(new { message = $"找不到編號為 {dto.EmpId} 的員工" });

                return Ok(new { message = "資料更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新員工 {EmpId} 時發生錯誤", dto.EmpId);
                return StatusCode(500, "資料庫更新失敗");
            }
        }

        // DELETE api/employee/PMA42628M
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _service.DeleteEmployeeAsync(id);

                if (success)
                {
                    return Ok(new { message = $"員工 {id} 已成功刪除" });
                }

                // 如果找不到 ID，回傳 404
                return NotFound(new { message = "找不到該員工，無法刪除" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除員工時發生錯誤: {Id}", id);
                return StatusCode(500, "資料庫刪除操作失敗");
            }
        }

        // GET api/employee?pageNumber=1&pageSize=20
       // [Authorize]
        [HttpGet]        
        public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
        {
            var result = await _service.GetEmployeesAsync(filter);
            return Ok(result);
        }

        [HttpGet("title") ]   
        public async Task<IActionResult> GetTitleView(string id)
        {
            var result = await _service.GetTitleViewAsync(id);
            if (result == null) return NotFound(new { message = "找不到Title ID" });
            return Ok(result);
        }
     
        
    }


}