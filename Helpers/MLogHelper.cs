namespace MyAPI.Helpers
{
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    public static class MLogHelper
    {
        /// <summary>
        /// 模擬 NLog 的 PushScope 功能。
        /// 在 ILogger 中，我們使用 BeginScope 來達到類似效果。
        /// </summary>
        public static IDisposable Log_PushCaseScope(string functionName)
        {
            // 這裡回傳一個空的 Disposable 或者實作邏輯
            // 如果你有注入 ILogger，可以使用 _logger.BeginScope
            return new NoopDisposable();
        }

        /// <summary>
        /// 執行業務邏輯並計算執行時間
        /// </summary>
        public static async Task<T> Log_RunWithSpendTimeScopeAsync<T>(
            Func<Task<T>> func,
            ILogger logger,
            string functionName)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                logger.LogInformation($"[Start] 執行方法: {functionName}");

                T result = await func();

                sw.Stop();
                logger.LogInformation($"[Success] 方法 {functionName} 執行完畢，耗時: {sw.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogError($"[Error] 方法 {functionName} 執行失敗，耗時: {sw.ElapsedMilliseconds}ms, 錯誤: {ex.Message}");
                throw; // 讓原本 BaseController 的 catch 繼續處理
            }
        }

        // 私有類別，用來處理不需要動作的 IDisposable
        private class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
