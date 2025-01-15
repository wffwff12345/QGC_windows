using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UavApp
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        { // 配置Serilog  
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 设置日志的最小级别  
                .WriteTo.File("logs/UavApp-.txt", rollingInterval: RollingInterval.Day) // 配置文件接收器  
                .CreateLogger();
            try
            {
                Log.Information("Hello, Serilog!");
                // 应用程序逻辑...  
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                // 处理异常（可选）  
                Log.Fatal(ex, "应用程序启动过程中发生了一个致命错误");
            }
            finally
            {
                // 应用程序关闭时刷新并关闭日志  
                Log.CloseAndFlush();
            }
        }
    }
}
