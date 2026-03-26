using MyAPI.Helpers;
using MyAPI.Repositories;
using MyAPI.Services;
using Serilog; // 記得引用

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 設定記錄等級
    .WriteTo.Console()          // 同時輸出到控制台
    .WriteTo.File(
        path: "Logs/log-.txt",   // 檔名中間會自動插入日期
        rollingInterval: RollingInterval.Day, // 每天產生一個新檔案
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}" // 包含精確時間
    )
    .CreateLogger();

builder.Host.UseSerilog(); // 告訴 .NET 使用 Serilog 替代預設 Log

// --- Swagger 註冊 (Swashbuckle) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Controller 註冊 ---
builder.Services.AddControllers();

// 1. 取得連線字串
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("連線字串讀取失敗，請檢查 appsettings.json");
}

// 2. 註冊 DI 服務 原本的做法（手動 new 會漏掉參數）
//builder.Services.AddSingleton(new DbHelper(connectionString));
// ✅ 建議做法：註冊實例化的邏輯
builder.Services.AddSingleton<DbHelper>(sp => 
{
    // 從 DI 容器中取得 Logger
    var logger = sp.GetRequiredService<ILogger<DbHelper>>();
    return new DbHelper(connectionString, logger);
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

// --- 2. 配置中間件 (Middleware Pipeline) ---

// [關鍵] 全域異常處理：攔截所有連線錯誤或程式碼錯誤
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        
        // 這裡可以寫 Log 記錄連線錯誤
        var error = new { 
            Message = "伺服器內部錯誤，請聯繫管理員。", 
            Detail = app.Environment.IsDevelopment() ? exceptionHandlerPathFeature?.Error.Message : null 
        };
        await context.Response.WriteAsJsonAsync(error);
    });
});

// --- 中間件配置 ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPubsAPI v1");
        c.RoutePrefix = string.Empty; // 讓首頁直接就是 Swagger
    });
    
    // 這裡原本的 app.MapOpenApi(); 必須刪除 ❌
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

