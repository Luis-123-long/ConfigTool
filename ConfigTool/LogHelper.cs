using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigTool
{

    // 工具类型使用静态类
    public static class LogHelper
    {
        // 日志文件存放的位置
        private static readonly string LogDir = @"D:\SystemLogs";

        // 记录普通的信息
        public static void Info(string message)
        {
            WriteToFile("INFO", message);
        }

        // 2.记录错误的信息
        public static void Error(string message, Exception ex = null)
        {
            string content = message;
            if (ex != null)
            {
                content += $" | Error: {ex.Message}";
            }
            WriteToFile("ERROR", content);
        }
        
        private static void WriteToFile(string level, string message)
        {
            try
            {
                // 确保日志文件夹是否存在
                if(!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }

                // 按日期生成文件名
                string fileName = Path.Combine(LogDir,$"log_{ DateTime.Now:yyyyMMdd}.txt");

                //日志格式: [时间] [级别] 内容

                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

                // 追加写入
                File.AppendAllText(fileName, logContent);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
