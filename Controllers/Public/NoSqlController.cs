using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Services.Interfaces;

namespace MyAPI.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoSqlController : ControllerBase // 💡 改為繼承 ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeeController> _logger;
        
    
    }
}
